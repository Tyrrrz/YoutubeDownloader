using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Android.App;

namespace YoutubeDownloader.Android;

public static class AndroidFFmpegInitializer
{
    private static readonly string[] AllArchitectures = ["arm", "arm64-v8a", "i686", "x86_64"]; // "armv7-a", "arm-v7n", add if needed.

    public static async Task InitializeAsync()
    {
        var extractedPath = await ExtractFFmpegFromAssetsAsync();
        if (!string.IsNullOrEmpty(extractedPath))
        {
            Core.Downloading.FFmpeg.SetCustomPath(extractedPath);

            CleanupUnusedArchitecturesAsync();
        }
    }

    private static async Task<string?> ExtractFFmpegFromAssetsAsync()
    {
        try
        {
            var context = Application.Context;
            if (context == null) return null;

            var architecture = GetAndroidArchitecture();
            var assetPath = $"{architecture}/ffmpeg";

            var internalDir = context.FilesDir?.AbsolutePath;
            if (string.IsNullOrEmpty(internalDir)) return null;

            var extractedPath = Path.Combine(internalDir, "ffmpeg");

            if (File.Exists(extractedPath) && new FileInfo(extractedPath).Length > 0)
                return extractedPath;

            // Extract from assets
            using var assetStream = context.Assets?.Open(assetPath);
            if (assetStream == null) return null;

            using var fileStream = File.Create(extractedPath);
            await assetStream.CopyToAsync(fileStream);

            // Set execute permissions
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{extractedPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit();
            }
            catch { }

            return extractedPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FFmpeg extraction failed: {ex.Message}");
            return null;
        }
    }

    private static void CleanupUnusedArchitecturesAsync()
    {
        try
        {
            var context = Application.Context;
            if (context == null) return;

            var internalDir = context.FilesDir?.AbsolutePath;
            if (string.IsNullOrEmpty(internalDir)) return;

            var currentArch = GetAndroidArchitecture();

            // Remove any previously extracted FFmpeg binaries from other architectures
            foreach (var arch in AllArchitectures)
            {
                if (arch == currentArch) continue;

                try
                {
                    var oldBinaryPath = Path.Combine(internalDir, $"ffmpeg_{arch}");
                    if (File.Exists(oldBinaryPath))
                    {
                        File.Delete(oldBinaryPath);
                        Debug.WriteLine($"Cleaned up unused FFmpeg binary: {arch}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to cleanup {arch}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Cleanup failed: {ex.Message}");
        }
    }

    private static string GetAndroidArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64-v8a",
            Architecture.X86 => "i686",
            Architecture.X64 => "x86_64",
            _ => "arm64-v8a"
        };
    }
}