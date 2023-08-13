using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PropertyChanged;

namespace YoutubeDownloader.Controls;

[DoNotNotify]
public class Hyperlink : InlineUIContainer
{
    private readonly Underline _underline;

    public Span Content => _underline;

    /// <summary>
    /// Defines the <see cref="Click"/> event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<Button, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);
    private Button _button;

    /// <summary>
    /// Raised when the user clicks the button.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => _button.AddHandler(Button.ClickEvent, value);
        remove => _button.RemoveHandler(Button.ClickEvent, value);
    }

    public Hyperlink()
    {
        _underline = new Underline();

        var textBlock = new TextBlock
        {
            Inlines = new InlineCollection
            {
                _underline
            }
        };


        _button = new Button
        {
            Background = Brushes.Transparent,
            Margin = new Thickness(),
            Padding = new Thickness(),
            Cursor = new Cursor(StandardCursorType.Hand),
            Content = textBlock
        };

        Child = _button;
    }

    //private static async Task OpenUrl(Uri? url)
    //{
    //    if (url != null)
    //    {
    //        await OpenBrowserAsync(url);
    //    }
    //}

    //public static async Task OpenBrowserAsync(Uri uri, bool external = false)
    //{
    //    var url = uri.ToString();

    //    try
    //    {
    //        Process.Start(url);
    //    }
    //    catch
    //    {
    //        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    //        {
    //            url = url.Replace("&", "^&");
    //            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    //        }
    //        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    //        {
    //            Process.Start("xdg-open", url);
    //        }
    //        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    //        {
    //            Process.Start("open", url);
    //        }
    //    }

    //    await Task.Yield();
    //}
}
