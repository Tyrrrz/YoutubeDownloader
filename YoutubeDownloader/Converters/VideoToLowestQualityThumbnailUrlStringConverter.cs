using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Converters;

public class VideoToLowestQualityThumbnailUrlStringConverter : IValueConverter
{
    public static VideoToLowestQualityThumbnailUrlStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is IVideo video ? video.Thumbnails.MinBy(t => t.Resolution.Area)?.Url : null;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
