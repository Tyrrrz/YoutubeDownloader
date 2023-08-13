using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace YoutubeDownloader.Converters;

public class IsEqualConverter : IValueConverter
{
    public bool Inverted { get; init; } = false;

    public static IsEqualConverter IsEqual { get; } = new IsEqualConverter();
    public static IsEqualConverter IsNotEqual { get; } = new IsEqualConverter { Inverted = true };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return EqualityComparer<object>.Default.Equals(value, parameter) != Inverted;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
