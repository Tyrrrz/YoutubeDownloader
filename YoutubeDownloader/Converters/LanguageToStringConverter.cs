using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;
using YoutubeDownloader.Localization;

namespace YoutubeDownloader.Converters;

public class LanguageToStringConverter : IValueConverter
{
    public static LanguageToStringConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Language language)
        {
            var memberInfo = typeof(Language).GetMember(language.ToString());
            if (memberInfo.Length > 0)
            {
                var descAttr = memberInfo[0].GetCustomAttribute<DescriptionAttribute>();
                if (descAttr is not null)
                    return descAttr.Description;
            }

            return language.ToString();
        }

        return default(string);
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
