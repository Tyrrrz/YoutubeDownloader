using System;
using System.Globalization;
using System.Windows.Data;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.Converters;

[ValueConversion(typeof(string), typeof(string))]
public class ShortUrlConverter : IValueConverter
{
    public static ShortUrlConverter Instance { get; } = new();
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is string valueString && Uri.TryCreate(valueString, UriKind.Absolute, out var uri)
            ? uri.GetHostWithoutWww() + uri.PathAndQuery
            : value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
        throw new NotSupportedException();
}