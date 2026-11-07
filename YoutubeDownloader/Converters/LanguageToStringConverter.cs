using System;
using System.Globalization;
using Avalonia.Data.Converters;
using YoutubeDownloader.Localization;

namespace YoutubeDownloader.Converters;

public class LanguageToStringConverter : IValueConverter
{
    public static LanguageToStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) =>
        value is Language language
            ? language switch
            {
                Language.ChineseSimplified => "Simplified Chinese",
                _ => language.ToString(),
            }
            : default;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
