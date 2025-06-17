using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Android;

public static class AndroidFFmpegInitializer
{
    public static async void Initialize()
    {
        var extractedPath = await ExtractFFmpegFromAssetsAsync();
        if (!string.IsNullOrEmpty(extractedPath))
        {
            Core.Downloading.FFmpeg.SetCustomPath(extractedPath);
        }
    }

    private static async Task<string?> ExtractFFmpegFromAssetsAsync()
    {
        try
        {
            var appInfo = global::Android.App.Application.Context.ApplicationInfo;
            var path = Path.Combine(appInfo!.NativeLibraryDir!, "ffmpeg");
            return path;
        }
        catch (Exception ex)
        {
            DialogManager dialogManager = new();
            MessageBoxViewModel messageBoxViewModel = new()
            {
                Title = "FFmpeg Extraction Error",
                Message = $"Failed to extract FFmpeg from assets. Please ensure the file exists in the assets directory.\n{ex.Message}",
                DefaultButtonText = "OK"
            };
            await dialogManager.ShowDialogAsync(messageBoxViewModel);
            Debug.WriteLine($"FFmpeg extraction failed: {ex.Message}");
            return null;
        }
    }
}