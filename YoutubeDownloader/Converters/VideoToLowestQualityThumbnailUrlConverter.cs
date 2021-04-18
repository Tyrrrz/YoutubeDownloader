using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Converters
{
    [ValueConversion(typeof(IVideo), typeof(string))]
    public class VideoToLowestQualityThumbnailUrlConverter : IValueConverter
    {
        public static VideoToLowestQualityThumbnailUrlConverter Instance { get; } = new();

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IVideo video)
                return video.Thumbnails.OrderBy(t => t.Resolution.Area).FirstOrDefault()?.Url;

            return default(string);
        }

        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}