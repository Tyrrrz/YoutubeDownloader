using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading;

public partial record VideoDownloadOption(
    Container Container,
    bool IsAudioOnly,
    IReadOnlyList<IStreamInfo> StreamInfos
)
{
    public VideoQuality? VideoQuality { get; } =
        StreamInfos.OfType<IVideoStreamInfo>().MaxBy(s => s.VideoQuality)?.VideoQuality;
}

public partial record VideoDownloadOption
{
    internal static IReadOnlyList<VideoDownloadOption> ResolveAll(
        StreamManifest manifest,
        bool includeLanguageSpecificAudioStreams = true
    )
    {
        IEnumerable<VideoDownloadOption> GetVideoAndAudioOptions()
        {
            var videoStreamInfos = manifest
                .GetVideoStreams()
                .OrderByDescending(v => v.VideoQuality);

            foreach (var videoStreamInfo in videoStreamInfos)
            {
                // Muxed stream
                if (videoStreamInfo is MuxedStreamInfo)
                {
                    yield return new VideoDownloadOption(
                        videoStreamInfo.Container,
                        false,
                        [videoStreamInfo]
                    );
                }
                // Separate audio + video stream
                else
                {
                    var audioStreamInfos = manifest
                        .GetAudioStreams()
                        // Prefer audio streams with the same container
                        .OrderByDescending(s => s.Container == videoStreamInfo.Container)
                        .ThenByDescending(s => s is AudioOnlyStreamInfo)
                        .ThenByDescending(s => s.Bitrate)
                        .ToArray();

                    // Prefer language-specific audio streams, if available and if allowed
                    var languageSpecificAudioStreamInfos = includeLanguageSpecificAudioStreams
                        ? audioStreamInfos
                            .Where(s => s.AudioLanguage is not null)
                            .DistinctBy(s => s.AudioLanguage)
                            // Default language first so it's encoded as the first audio track in the output file
                            .OrderByDescending(s => s.IsAudioLanguageDefault)
                            .ToArray()
                        : [];

                    // If there are language-specific streams, include them all
                    if (languageSpecificAudioStreamInfos.Any())
                    {
                        yield return new VideoDownloadOption(
                            videoStreamInfo.Container,
                            false,
                            [videoStreamInfo, .. languageSpecificAudioStreamInfos]
                        );
                    }
                    // If there are no language-specific streams, download the single best quality audio stream
                    else
                    {
                        var audioStreamInfo = audioStreamInfos
                            // Prefer audio streams in the default language (or non-language-specific streams)
                            .OrderByDescending(s => s.IsAudioLanguageDefault ?? true)
                            .FirstOrDefault();

                        if (audioStreamInfo is not null)
                        {
                            yield return new VideoDownloadOption(
                                videoStreamInfo.Container,
                                false,
                                [videoStreamInfo, audioStreamInfo]
                            );
                        }
                    }
                }
            }
        }

        IEnumerable<VideoDownloadOption> GetAudioOnlyOptions()
        {
            // WebM-based audio-only containers
            {
                var audioStreamInfo = manifest
                    .GetAudioStreams()
                    // Prefer audio streams in the default language (or non-language-specific streams)
                    .OrderByDescending(s => s.IsAudioLanguageDefault ?? true)
                    // Prefer audio streams with the same container
                    .ThenByDescending(s => s.Container == Container.WebM)
                    .ThenByDescending(s => s is AudioOnlyStreamInfo)
                    .ThenByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                if (audioStreamInfo is not null)
                {
                    yield return new VideoDownloadOption(Container.WebM, true, [audioStreamInfo]);

                    yield return new VideoDownloadOption(Container.Mp3, true, [audioStreamInfo]);

                    yield return new VideoDownloadOption(
                        new Container("ogg"),
                        true,
                        [audioStreamInfo]
                    );
                }
            }

            // Mp4-based audio-only containers
            {
                var audioStreamInfo = manifest
                    .GetAudioStreams()
                    // Prefer audio streams in the default language (or non-language-specific streams)
                    .OrderByDescending(s => s.IsAudioLanguageDefault ?? true)
                    // Prefer audio streams with the same container
                    .ThenByDescending(s => s.Container == Container.Mp4)
                    .ThenByDescending(s => s is AudioOnlyStreamInfo)
                    .ThenByDescending(s => s.Bitrate)
                    .FirstOrDefault();

                if (audioStreamInfo is not null)
                {
                    yield return new VideoDownloadOption(Container.Mp4, true, [audioStreamInfo]);
                }
            }
        }

        // Deduplicate download options by video quality and container
        var comparer = EqualityComparer<VideoDownloadOption>.Create(
            (x, y) => x?.VideoQuality == y?.VideoQuality && x?.Container == y?.Container,
            x => HashCode.Combine(x.VideoQuality, x.Container)
        );

        var options = new HashSet<VideoDownloadOption>(comparer);

        options.AddRange(GetVideoAndAudioOptions());
        options.AddRange(GetAudioOnlyOptions());

        return options.ToArray();
    }
}
