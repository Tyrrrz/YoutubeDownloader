using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YoutubeDownloader.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public partial class BoolToVisibilityConverter(
    Visibility trueVisibility,
    Visibility falseVisibility
) : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? trueVisibility : falseVisibility;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) =>
        value is Visibility visibility
            ? visibility == trueVisibility
            : throw new NotSupportedException();
}

public partial class BoolToVisibilityConverter
{
    public static BoolToVisibilityConverter VisibleOrCollapsed { get; } =
        new(Visibility.Visible, Visibility.Collapsed);

    public static BoolToVisibilityConverter VisibleOrHidden { get; } =
        new(Visibility.Visible, Visibility.Hidden);

    public static BoolToVisibilityConverter CollapsedOrVisible { get; } =
        new(Visibility.Collapsed, Visibility.Visible);

    public static BoolToVisibilityConverter HiddenOrVisible { get; } =
        new(Visibility.Hidden, Visibility.Visible);
}
