using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeDownloader.Android;

public static class AndroidFFmpegInitializer
{
    public static void Initialize()
    {
        var extractedPath = ExtractFFmpegFromAssetsAsync();
        if (!string.IsNullOrEmpty(extractedPath))
        {
            Core.Downloading.FFmpeg.SetCustomPath(extractedPath);
        }
    }

    private static string? ExtractFFmpegFromAssetsAsync()
    {
        try
        {
            var appInfo = global::Android.App.Application.Context.ApplicationInfo;
            var path = Path.Combine(appInfo!.NativeLibraryDir!, "ffmpeg");
            return path;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FFmpeg extraction failed: {ex.Message}");
            return null;
        }
    }
}