using System;
using Avalonia.Controls;

namespace YoutubeDownloader.Framework;

public class Window<TDataContext> : Window
{
    public new TDataContext DataContext
    {
        get =>
            base.DataContext is TDataContext dataContext
                ? dataContext
                : throw new InvalidCastException(
                    $"DataContext is null or not of the expected type '{typeof(TDataContext).FullName}'."
                );
        set => base.DataContext = value;
    }
}
