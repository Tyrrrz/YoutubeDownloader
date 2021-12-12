using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contextual;
using Contextual.Contexts;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core;

public static class YoutubeExtensions
{


    public static async Task<VideoDownloadOption?> TryGetPreferredVideoDownloadOptionAsync(
        this IVideo video,
        Container container,
        VideoQualityPreference qualityPreference)
    {
        var options = await GetDownloadOptionsAsync(video);

        // Audio-only
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

    public static async Task DownloadAsync(this )
}