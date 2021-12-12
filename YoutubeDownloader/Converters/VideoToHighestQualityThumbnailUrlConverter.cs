using System;
using System.Globalization;
using System.Windows.Data;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Converters;

[ValueConversion(typeof(IVideo), typeof(string))]
public class VideoToHighestQualityThumbnailUrlConverter : IValueConverter
{
    public static VideoToHighestQualityThumbnailUrlConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is IVideo video
            ? video.Thumbnails.TryGetWithHighestResolution()?.Url
            : null;

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}