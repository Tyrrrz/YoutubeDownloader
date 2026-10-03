using System;
using System.Globalization;
using Avalonia.Data.Converters;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Converters;

public class VideoSortOptionToStringConverter : IValueConverter
{
    public static VideoSortOptionToStringConverter Instance { get; } = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is VideoSortOption option ? option.GetDisplayName() : default;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
