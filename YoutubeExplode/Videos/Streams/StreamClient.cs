using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PowerKit.Extensions;
using YoutubeExplode.Bridge;
using YoutubeExplode.Bridge.Cipher;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// Operations related to media streams of YouTube videos.
/// </summary>
public class StreamClient(HttpClient http)
{
    private readonly StreamController _controller = new(http);

    // Because we determine the player version ourselves, it's safe to cache the cipher manifest
    // for the entire lifetime of the client.
    private CipherManifest? _cipherManifest;

    private async ValueTask<CipherManifest> ResolveCipherManifestAsync(
        CancellationToken cancellationToken
    )
    {
        if (_cipherManifest is not null)
            return _cipherManifest;

        var playerSource = await _controller.GetPlayerSourceAsync(cancellationToken);

        return _cipherManifest =
            playerSource.CipherManifest
            ?? throw new YoutubeExplodeException("Failed to extract the cipher manifest.");
    }

    private async ValueTask<long?> TryGetContentLengthAsync(
        IStreamData streamData,
        string url,
        CancellationToken cancellationToken = default
    )
    {
        var contentLength = streamData.ContentLength;

        // If content length is not available in the metadata, get it by
        // sending a HEAD request and parsing the Content-Length header.
        if (contentLength is null)
        {
            using var response = await http.HeadAsync(url, cancellationToken);
            contentLength = response.Content.Headers.ContentLength;

            // 404 error indicates that the stream is not available
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
        }

        if (contentLength is not null)
        {
            // Streams may have mismatched content length, so ensure that the obtained value is correct
            // https://github.com/Tyrrrz/YoutubeExplode/issues/759
            using var response = await http.GetAsync(
                // Try to access the last byte of the stream
                MediaStream.GetSegmentUrl(url, contentLength.Value - 2, contentLength.Value - 1),
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            // 404 error indicates that the stream has mismatched content length or is not available
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
        }

        return contentLength;
    }

    private async IAsyncEnumerable<IStreamInfo> GetStreamInfosAsync(
        IEnumerable<IStreamData> streamDatas,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        foreach (var streamData in streamDatas)
        {
            // SABR / server-side streams have no progressive URL we can download.
            // ANDROID in particular mixes one muxed itag-18 URL with SABR-only adaptive
            // formats, so skip those instead of failing the whole manifest.
            var url = streamData.Url;
            if (string.IsNullOrWhiteSpace(url))
                continue;

            var itag =
                streamData.Itag
                ?? throw new YoutubeExplodeException("Failed to extract the stream itag.");

            // Handle cipher-protected streams
            if (!string.IsNullOrWhiteSpace(streamData.Signature))
            {
                var cipherManifest = await ResolveCipherManifestAsync(cancellationToken);

                url = UrlEx.SetQueryParameter(
                    url,
                    streamData.SignatureParameter ?? "sig",
                    cipherManifest.Decipher(streamData.Signature)
                );
            }

            var contentLength = await TryGetContentLengthAsync(streamData, url, cancellationToken);
            if (contentLength is null)
                continue;

            var container =
                streamData.Container?.Pipe(s => new Container(s))
                ?? throw new YoutubeExplodeException("Failed to extract the stream container.");

            var bitrate =
                streamData.Bitrate?.Pipe(s => new Bitrate(s))
                ?? throw new YoutubeExplodeException("Failed to extract the stream bitrate.");

            var audioLanguage = !string.IsNullOrWhiteSpace(streamData.AudioLanguageCode)
                ? new Language(
                    streamData.AudioLanguageCode,
                    streamData.AudioLanguageName ?? streamData.AudioLanguageCode
                )
                : (Language?)null;

            // Muxed or video-only stream
            if (!string.IsNullOrWhiteSpace(streamData.VideoCodec))
            {
                var framerate = streamData.VideoFramerate ?? 24;

                var videoQuality = !string.IsNullOrWhiteSpace(streamData.VideoQualityLabel)
                    ? VideoQuality.FromLabel(streamData.VideoQualityLabel, framerate)
                    : VideoQuality.FromItag(itag, framerate);

                var videoResolution =
                    streamData.VideoWidth is not null && streamData.VideoHeight is not null
                        ? new Resolution(streamData.VideoWidth.Value, streamData.VideoHeight.Value)
                        : videoQuality.GetDefaultVideoResolution();

                // Muxed
                if (!string.IsNullOrWhiteSpace(streamData.AudioCodec))
                {
                    var streamInfo = new MuxedStreamInfo(
                        url,
                        container,
                        new FileSize(contentLength.Value),
                        bitrate,
                        streamData.AudioCodec,
                        audioLanguage,
                        streamData.IsAudioLanguageDefault,
                        streamData.VideoCodec,
                        videoQuality,
                        videoResolution,
                        streamData.IsVideoUpscaled
                    );

                    yield return streamInfo;
                }
                // Video-only
                else
                {
                    var streamInfo = new VideoOnlyStreamInfo(
                        url,
                        container,
                        new FileSize(contentLength.Value),
                        bitrate,
                        streamData.VideoCodec,
                        videoQuality,
                        videoResolution,
                        streamData.IsVideoUpscaled
                    );

                    yield return streamInfo;
                }
            }
            // Audio-only
            else if (!string.IsNullOrWhiteSpace(streamData.AudioCodec))
            {
                var streamInfo = new AudioOnlyStreamInfo(
                    url,
                    container,
                    new FileSize(contentLength.Value),
                    bitrate,
                    streamData.AudioCodec,
                    audioLanguage,
                    streamData.IsAudioLanguageDefault
                );

                yield return streamInfo;
            }
            else
            {
                throw new YoutubeExplodeException("Failed to extract the stream codec.");
            }
        }
    }

    private async ValueTask<IReadOnlyList<IStreamInfo>> GetStreamInfosAsync(
        VideoId videoId,
        PlayerResponse playerResponse,
        CancellationToken cancellationToken = default
    )
    {
        var streamInfos = new List<IStreamInfo>();

        // Video is pay-to-play
        if (!string.IsNullOrWhiteSpace(playerResponse.PreviewVideoId))
        {
            throw new VideoRequiresPurchaseException(
                $"Video '{videoId}' requires purchase and cannot be played.",
                playerResponse.PreviewVideoId
            );
        }

        // Video is unplayable
        if (!playerResponse.IsPlayable)
        {
            throw new VideoUnplayableException(
                $"Video '{videoId}' is unplayable. Reason: '{playerResponse.PlayabilityError}'."
            );
        }

        // Extract streams from the player response
        streamInfos.AddRange(await GetStreamInfosAsync(playerResponse.Streams, cancellationToken));

        // Extract streams from the DASH manifest
        if (!string.IsNullOrWhiteSpace(playerResponse.DashManifestUrl))
        {
            try
            {
                var dashManifest = await _controller.GetDashManifestAsync(
                    playerResponse.DashManifestUrl,
                    cancellationToken
                );

                streamInfos.AddRange(
                    await GetStreamInfosAsync(dashManifest.Streams, cancellationToken)
                );
            }
            // Some DASH manifest URLs return 404 for whatever reason
            // https://github.com/Tyrrrz/YoutubeExplode/issues/728
            catch (HttpRequestException) { }
        }

        // Error if no streams were found
        if (!streamInfos.Any())
        {
            throw new VideoUnplayableException(
                $"Video '{videoId}' does not contain any playable streams."
            );
        }

        return streamInfos;
    }

    private async ValueTask<IReadOnlyList<IStreamInfo>> GetStreamInfosAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Try to get player response from a cipher-less client
            var playerResponse = await _controller.GetPlayerResponseAsync(
                videoId,
                cancellationToken
            );

            return await GetStreamInfosAsync(videoId, playerResponse, cancellationToken);
        }
        // Retry with deciphering
        catch (VideoUnplayableException ex)
            // Only retry on videos that are unplayable for reasons other than being unavailable (deleted, private, etc)
            when (ex is not VideoUnavailableException)
        {
            var cipherManifest = await ResolveCipherManifestAsync(cancellationToken);

            // Try to get player response from a client with cipher
            var playerResponse = await _controller.GetPlayerResponseAsync(
                videoId,
                cipherManifest.SignatureTimestamp,
                cancellationToken
            );

            return await GetStreamInfosAsync(videoId, playerResponse, cancellationToken);
        }
    }

    /// <summary>
    /// Gets the manifest that lists available streams for the specified video.
    /// </summary>
    public async ValueTask<StreamManifest> GetManifestAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    )
    {
        for (var retriesRemaining = 5; ; retriesRemaining--)
        {
            try
            {
                return new StreamManifest(await GetStreamInfosAsync(videoId, cancellationToken));
            }
            // Retry on connectivity issues
            catch (Exception ex)
                when (ex is HttpRequestException or IOException && retriesRemaining > 0) { }
        }
    }

    /// <summary>
    /// Gets the HTTP Live Stream (HLS) manifest URL for the specified video (if it is a livestream).
    /// </summary>
    public async ValueTask<string> GetHttpLiveStreamUrlAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    )
    {
        var playerResponse = await _controller.GetPlayerResponseAsync(videoId, cancellationToken);
        if (!playerResponse.IsPlayable)
        {
            throw new VideoUnplayableException(
                $"Video '{videoId}' is unplayable. Reason: '{playerResponse.PlayabilityError}'."
            );
        }

        if (string.IsNullOrWhiteSpace(playerResponse.HlsManifestUrl))
        {
            throw new YoutubeExplodeException(
                $"Failed to extract the HTTP Live Stream manifest URL. Video '{videoId}' is likely not a live stream."
            );
        }

        return playerResponse.HlsManifestUrl;
    }

    /// <summary>
    /// Gets the stream identified by the specified metadata.
    /// </summary>
    public async ValueTask<Stream> GetAsync(
        IStreamInfo streamInfo,
        CancellationToken cancellationToken = default
    )
    {
        var stream = new MediaStream(http, streamInfo);
        await stream.InitializeAsync(cancellationToken);

        return stream;
    }

    /// <summary>
    /// Copies the stream identified by the specified metadata to the specified stream.
    /// </summary>
    public async ValueTask CopyToAsync(
        IStreamInfo streamInfo,
        Stream destination,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        using var input = await GetAsync(streamInfo, cancellationToken);
        await input.CopyToAsync(destination, progress, cancellationToken);
    }

    /// <summary>
    /// Downloads the stream identified by the specified metadata to the specified file.
    /// </summary>
    public async ValueTask DownloadAsync(
        IStreamInfo streamInfo,
        string filePath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        using var destination = File.Create(filePath);
        await CopyToAsync(streamInfo, destination, progress, cancellationToken);
    }
}
