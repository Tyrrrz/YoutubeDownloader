using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YoutubeDownloader.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public partial class BoolToVisibilityConverter : IValueConverter
{
    private readonly Visibility _trueVisibility;
    private readonly Visibility _falseVisibility;

    public BoolToVisibilityConverter(Visibility trueVisibility, Visibility falseVisibility)
    {
        _trueVisibility = trueVisibility;
        _falseVisibility = falseVisibility;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true
            ? _trueVisibility
            : _falseVisibility;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Visibility visibility
            ? visibility == _trueVisibility
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