using System;
using System.Globalization;
using System.Windows.Data;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Converters
{
    [ValueConversion(typeof(IVideo), typeof(string))]
    public class VideoToHighestQualityThumbnailUrlConverter : IValueConverter
    {
        public static VideoToHighestQualityThumbnailUrlConverter Instance { get; } = new();

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IVideo video)
                return video.Thumbnails.TryGetWithHighestResolution()?.Url;

            return default(string);
        }

        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}