using System;
using Avalonia;
using Avalonia.WebView.Desktop;
using GLib;

namespace YoutubeDownloader;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // prepare and run your App here
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            // TODO how to show messagebox here? As MessageBox.Avalonia needs running application
            //MessageBox.Show(
            //    args.Exception.ToString(),
            //    "Error occured",
            //    MessageBoxButton.OK,
            //    MessageBoxImage.Error
            //);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseDesktopWebView();
}
