using System;
using System.Reflection;
using Avalonia;
using YoutubeDownloader.Utils;

namespace YoutubeDownloader;

public static class Program
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    public static string Name { get; } = Assembly.GetName().Name ?? "YoutubeDownloader";

    public static Version Version { get; } = Assembly.GetName().Version ?? new Version(0, 0, 0);

    public static string VersionString { get; } = Version.ToString(3);

    public static bool IsDevelopmentBuild { get; } = Version.Major is <= 0 or >= 999;

    public static string ProjectUrl { get; } = "https://github.com/Tyrrrz/YoutubeDownloader";

    public static string ProjectReleasesUrl { get; } = $"{ProjectUrl}/releases";

    // The members above are app metadata and are consumed on every platform. Everything
    // below is the desktop entry point: it depends on Avalonia.Desktop, which has no
    // Android target, so the Android head supplies its own entry point instead.
#if !ANDROID
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();

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
#endif
}
