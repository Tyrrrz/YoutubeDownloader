using System;
using System.Globalization;
using System.Windows.Data;
using YoutubeDownloader.Models;

namespace YoutubeDownloader.Converters
{
    [ValueConversion(typeof(VideoQualityPreference), typeof(string))]
    public class VideoQualityPreferenceToStringConverter : IValueConverter
    {
        public static VideoQualityPreferenceToStringConverter Instance { get; } = new();

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VideoQualityPreference preference)
                return preference.GetDisplayName();

            return default(string);
        }

        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}