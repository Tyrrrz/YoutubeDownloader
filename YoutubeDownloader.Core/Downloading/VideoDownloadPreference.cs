using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading;

public record VideoDownloadPreference(
    Container PreferredContainer,
    VideoQualityPreference PreferredVideoQuality)
{
    public VideoDownloadOption? TryGetBestOption(IReadOnlyList<VideoDownloadOption> options)
    {
        // Short-circuit for audio-only formats
        if (PreferredContainer.IsAudioOnly)
            return options.FirstOrDefault(o => o.Container == PreferredContainer);

        var orderedOptions = options
            .OrderBy(o => o.VideoQuality)
            .ToArray();

        var preferredOption = PreferredVideoQuality switch
        {
            VideoQualityPreference.Highest => orderedOptions
                .LastOrDefault(o => o.Container == PreferredContainer),

            VideoQualityPreference.UpTo1080p => orderedOptions
                .Where(o => o.VideoQuality?.MaxHeight <= 1080)
                .LastOrDefault(o => o.Container == PreferredContainer),

            VideoQualityPreference.UpTo720p => orderedOptions
                .Where(o => o.VideoQuality?.MaxHeight <= 720)
                .LastOrDefault(o => o.Container == PreferredContainer),

            VideoQualityPreference.UpTo480p => orderedOptions
                .Where(o => o.VideoQuality?.MaxHeight <= 480)
                .LastOrDefault(o => o.Container == PreferredContainer),

            VideoQualityPreference.Lowest => orderedOptions
                .LastOrDefault(o => o.Container == PreferredContainer),

            _ => throw new InvalidOperationException($"Unknown video quality preference '{PreferredVideoQuality}'.")
        };

        return
            preferredOption ??
            orderedOptions.FirstOrDefault(o => o.Container == PreferredContainer);
    }
}