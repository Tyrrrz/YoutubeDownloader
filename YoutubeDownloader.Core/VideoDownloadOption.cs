using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core;

public partial record VideoDownloadOption(Container Container, IReadOnlyList<IStreamInfo> StreamInfos)
{
    public string Label => VideoQuality?.Label ?? "Audio";

    public VideoQuality? VideoQuality => Memo.Cache(this, () =>
        StreamInfos
            .OfType<IVideoStreamInfo>()
            .Select(s => s.VideoQuality)
            .OrderByDescending(q => q)
            .FirstOrDefault()
    );

    public bool IsAudioOnly => VideoQuality is null;

    public async Task DownloadAsync(
        string filePath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default) =>
        await Youtube.Client.Videos.DownloadAsync(
            StreamInfos,
            new ConversionRequestBuilder(filePath)
                .SetFormat(Container.Name)
                .SetPreset(ConversionPreset.Medium)
                .Build(),
            progress,
            cancellationToken
        );
}

public partial record VideoDownloadOption
{
    public static IReadOnlyList<VideoDownloadOption> ResolveAll(StreamManifest manifest)
    {
        static IEnumerable<VideoDownloadOption> GetVideoAndAudioOptions(StreamManifest streamManifest)
        {
            var videoStreams = streamManifest
                .GetVideoStreams()
                .OrderByDescending(v => v.VideoQuality);

            foreach (var videoStreamInfo in videoStreams)
            {
                // Muxed stream
                if (videoStreamInfo is MuxedStreamInfo)
                {
                    yield return new VideoDownloadOption(
                        videoStreamInfo.Container,
                        new[] { videoStreamInfo }
                    );
                }
                // Separate audio + video stream
                else
                {
                    // Prefer audio stream with the same container
                    var audioStreamInfo = streamManifest
                        .GetAudioStreams()
                        .OrderByDescending(s => s.Container == videoStreamInfo.Container)
                        .ThenByDescending(s => s.Bitrate)
                        .FirstOrDefault();

                    if (audioStreamInfo is not null)
                    {
                        yield return new VideoDownloadOption(
                            videoStreamInfo.Container,
                            new IStreamInfo[] { videoStreamInfo, audioStreamInfo }
                        );
                    }
                }
            }
        }

        static IEnumerable<VideoDownloadOption> GetAudioOnlyOptions(StreamManifest streamManifest)
        {
            // WebM-based audio-only containers
            {
                var audioStreamInfo = streamManifest
                    .GetAudioStreams()
                    .OrderByDescending(s => s.Container == Container.WebM)
                    .ThenByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                if (audioStreamInfo is not null)
                {
                    yield return new VideoDownloadOption(new Container("mp3"), new[] { audioStreamInfo });
                    yield return new VideoDownloadOption(new Container("ogg"), new[] { audioStreamInfo });
                }
            }

            // Mp4-based audio-only containers
            {
                var audioStreamInfo = streamManifest
                    .GetAudioStreams()
                    .OrderByDescending(s => s.Container == Container.Mp4)
                    .ThenByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                if (audioStreamInfo is not null)
                {
                    yield return new VideoDownloadOption(new Container("m4a"), new[] { audioStreamInfo });
                }
            }
        }

        // Deduplicate download options by label and container
        var comparer = new DelegateEqualityComparer<VideoDownloadOption>(
            (x, y) => StringComparer.OrdinalIgnoreCase.Equals(x.Label, y.Label) && x.Container == y.Container,
            x => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(x.Label), x.Container)
        );

        var options = new HashSet<VideoDownloadOption>(comparer);

        options.AddRange(GetVideoAndAudioOptions(manifest));
        options.AddRange(GetAudioOnlyOptions(manifest));

        return options.ToArray();
    }

    public static async Task<IReadOnlyList<VideoDownloadOption>> ResolveAllAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        var manifest = await Youtube.Client.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
        return ResolveAll(manifest);
    }

    public static VideoDownloadOption ResolveBest(StreamManifest manifest, Container container) =>
        ResolveAll(manifest)
            // Prioritize audio-only options for audio-only containers
            .OrderByDescending(o => o.IsAudioOnly || !container.IsAudioOnly())
            // Avoid transcoding, even at the expense of video quality
            .ThenByDescending(o => o.Container == container)
            .ThenByDescending(o => o.VideoQuality)
            .FirstOrDefault() ??
        throw new ApplicationException("No video download options available.");

    public static async Task<VideoDownloadOption> ResolveBestAsync(
        VideoId videoId,
        Container container,
        CancellationToken cancellationToken = default)
    {
        var manifest = await Youtube.Client.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
        return ResolveBest(manifest, container);
    }
}