using System;
using System.Globalization;
using Avalonia.Data.Converters;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Converters;

public class VideoToHighestQualityThumbnailUrlStringConverter : IValueConverter
{
    public static VideoToHighestQualityThumbnailUrlStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is IVideo video ? video.Thumbnails.TryGetWithHighestResolution()?.Url : null;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
