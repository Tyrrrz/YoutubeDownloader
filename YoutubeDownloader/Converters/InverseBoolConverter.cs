using System;
using System.Globalization;
using System.Windows.Data;

namespace YoutubeDownloader.Converters;

[ValueConversion(typeof(bool), typeof(bool))]
public class InverseBoolConverter : IValueConverter
{
    public static InverseBoolConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is false;

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is false;
}