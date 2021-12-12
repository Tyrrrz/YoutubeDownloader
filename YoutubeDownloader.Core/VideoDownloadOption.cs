using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core;

public partial record VideoDownloadOption(string Label, Container Container, IReadOnlyList<IStreamInfo> StreamInfos)
{
    public VideoQuality? VideoQuality => StreamInfos
        .OfType<IVideoStreamInfo>()
        .Select(s => s.VideoQuality)
        .OrderByDescending(q => q)
        .FirstOrDefault();

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
    public static IReadOnlyList<VideoDownloadOption> GetAll(StreamManifest manifest)
    {
        static IEnumerable<VideoDownloadOption> GetVideoAndAudioOptions(StreamManifest streamManifest)
        {
            var videoStreams = streamManifest
                .GetVideoStreams()
                .OrderByDescending(v => v.VideoQuality);

            foreach (var streamInfo in videoStreams)
            {
                // Muxed stream
                if (streamInfo is MuxedStreamInfo)
                {
                    yield return new VideoDownloadOption(
                        streamInfo.VideoQuality.Label,
                        streamInfo.Container,
                        new[] { streamInfo }
                    );
                }
                // Separate audio + video stream
                else
                {
                    // Prefer audio stream with the same container
                    var audioStreamInfo = streamManifest
                        .GetAudioStreams()
                        .OrderByDescending(s => s.Container == streamInfo.Container)
                        .ThenByDescending(s => s.Bitrate)
                        .FirstOrDefault();

                    if (audioStreamInfo is not null)
                    {
                        yield return new VideoDownloadOption(
                            streamInfo.VideoQuality.Label,
                            streamInfo.Container,
                            new IStreamInfo[] { streamInfo, audioStreamInfo }
                        );
                    }
                }
            }
        }

        static IEnumerable<VideoDownloadOption> GetAudioOnlyOptions(StreamManifest streamManifest)
        {
            // WebM-based audio-only containers
            {
                var streamInfo = streamManifest
                    .GetAudioStreams()
                    .OrderByDescending(s => s.Container == Container.WebM)
                    .ThenByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                if (streamInfo is not null)
                {
                    yield return new VideoDownloadOption("Audio", new Container("mp3"), new[] { streamInfo });
                    yield return new VideoDownloadOption("Audio", new Container("ogg"), new[] { streamInfo });
                }
            }

            // Mp4-based audio-only containers
            {
                var streamInfo = streamManifest
                    .GetAudioStreams()
                    .OrderByDescending(s => s.Container == Container.Mp4)
                    .ThenByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                if (streamInfo is not null)
                {
                    yield return new VideoDownloadOption("Audio", new Container("m4a"), new[] { streamInfo });
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

    public static VideoDownloadOption Get(
        StreamManifest manifest,
        Container container,
        VideoQualityPreference preference)
    {
        var options = GetAll(manifest);

        // Video quality preference is ignored for audio-only containers
        if (container.IsAudioOnly())
        {
            return options.FirstOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase));
        }

        var orderedOptions = options
            .OrderBy(o => o.VideoQuality)
            .ToArray();

        var preferredOption = qualityPreference switch
        {
            VideoQualityPreference.Maximum => orderedOptions
                .LastOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase)),

            VideoQualityPreference.High => orderedOptions
                .Where(o => o.VideoQuality?.MaxHeight <= 1080)
                .LastOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase)),

            VideoQualityPreference.Medium => orderedOptions
                .Where(o => o.VideoQuality?.MaxHeight <= 720)
                .LastOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase)),

            VideoQualityPreference.Low => orderedOptions
                .Where(o => o.VideoQuality?.MaxHeight <= 480)
                .LastOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase)),

            VideoQualityPreference.Minimum => orderedOptions
                .FirstOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase)),

            _ => throw new ArgumentOutOfRangeException(nameof(qualityPreference))
        };

        return
            preferredOption ??
            orderedOptions.FirstOrDefault(o => string.Equals(o.Format, format, StringComparison.OrdinalIgnoreCase));
    }
}