using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace YoutubeDownloader.Converters;

/// <summary>
/// Scales a length by the fraction given as the converter parameter.
/// </summary>
/// <remarks>
/// Bindings cannot do arithmetic, so a dialog that wants to be a share of the window -
/// rather than a fixed number of pixels that happens to suit a desktop monitor - needs
/// this to express it.
/// </remarks>
public class FractionOfConverter : IValueConverter
{
    public static FractionOfConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double length || double.IsNaN(length) || double.IsInfinity(length))
            return null;

        var fraction = parameter switch
        {
            double d => d,
            string s
                when double.TryParse(
                    s,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var d
                ) => d,
            _ => double.NaN,
        };

        return double.IsNaN(fraction) ? null : length * fraction;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
