using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace YoutubeDownloader.Converters;

public class EqualityConverter(bool isInverted) : IValueConverter
{
    public static EqualityConverter Equality { get; } = new(false);
    public static EqualityConverter IsNotEqual { get; } = new(true);

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => EqualityComparer<object>.Default.Equals(value, parameter) != isInverted;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
