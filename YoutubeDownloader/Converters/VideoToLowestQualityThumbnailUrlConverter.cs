using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Converters;

[ValueConversion(typeof(IVideo), typeof(string))]
public class VideoToLowestQualityThumbnailUrlConverter : IValueConverter
{
    public static VideoToLowestQualityThumbnailUrlConverter Instance { get; } = new();

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
