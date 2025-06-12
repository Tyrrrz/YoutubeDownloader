using System;
using Avalonia;
using Avalonia.WebView.Desktop;
using YoutubeDownloader.Utils;

namespace YoutubeDownloader;

public static class Program
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().UseDesktopWebView();

    [STAThread]
    public static int Main(string[] args)
    {
        // Build and run the app
        var builder = BuildAvaloniaApp();

        try
        {
            return builder.StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            if (OperatingSystem.IsWindows())
                _ = NativeMethods.Windows.MessageBox(0, ex.ToString(), "Fatal Error", 0x10);

            throw;
        }
        finally
        {
            // Clean up after application shutdown
            if (builder.Instance is IDisposable disposableApp)
                disposableApp.Dispose();
        }
    }
}
