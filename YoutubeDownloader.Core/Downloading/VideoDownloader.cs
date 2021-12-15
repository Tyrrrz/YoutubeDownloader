using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Downloading.Tagging;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Downloading;

public class VideoDownloader
{
    private readonly YoutubeClient _youtube = new(Http.Client);
    private readonly MediaTagInjector _tagInjector = new();

    public async Task<IReadOnlyList<VideoDownloadOption>> GetVideoDownloadOptionsAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
        return VideoDownloadOption.ResolveAll(manifest);
    }

    public async Task<IReadOnlyList<SubtitleDownloadOption>> GetSubtitleDownloadOptionsAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        var manifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(videoId, cancellationToken);
        return SubtitleDownloadOption.ResolveAll(manifest);
    }

    public async Task DownloadAsync(
        string filePath,
        IVideo video,
        VideoDownloadOption videoOption,
        SubtitleDownloadOption? subtitleOption,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: split progress in 3 parts

        // Download video
        {
            await _youtube.Videos.DownloadAsync(
                videoOption.StreamInfos,
                new ConversionRequestBuilder(filePath)
                    .SetFormat(videoOption.Container.Name)
                    .SetPreset(ConversionPreset.Medium)
                    .Build(),
                progress,
                cancellationToken
            );
        }

        // Download subtitles
        if (subtitleOption is not null)
        {
            var subtitleFilePath = Path.ChangeExtension(filePath, ".srt");

            await _youtube.Videos.ClosedCaptions.DownloadAsync(
                subtitleOption.TrackInfo,
                subtitleFilePath,
                null,
                cancellationToken
            );
        }

        // Inject media tags
        try
        {
            await _tagInjector.InjectTagsAsync(filePath, video, cancellationToken);
        }
        catch
        {
            // Not a critical operation, ignore errors
        }
    }
}