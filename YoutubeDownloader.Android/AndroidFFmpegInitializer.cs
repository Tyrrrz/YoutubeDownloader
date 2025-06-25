using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Java.Lang;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Android;

public static class AndroidFFmpegInitializer
{
    public static async Task InitializeAsync()
    {
        var extractedPath = await ExtractFFmpegFromAssetsAsync();
        if (!string.IsNullOrEmpty(extractedPath))
        {
            FFmpeg.SetCustomPath(extractedPath);
        }
    }

    private static async Task<string?> ExtractFFmpegFromAssetsAsync()
    {
        try
        {
            await Task.Delay(0);
            JavaSystem.LoadLibrary("ffmpeg");
            var appInfo = global::Android.App.Application.Context.ApplicationInfo;
            var path = Path.Combine(appInfo!.NativeLibraryDir!, "libffmpeg.so");
            return path;
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine($"FFmpeg extraction failed: {ex.Message}");
            return null;
        }
    }
}