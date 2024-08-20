using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YoutubeDownloader.Core.Downloading;

public static class FFmpeg
{
    public static bool IsBundled() => OperatingSystem.IsWindows()
        ? File.Exists(Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe"))
        : File.Exists(Path.Combine(AppContext.BaseDirectory, "ffmpeg"));

    public static string? TryGetCliFilePath()
    {
        static IEnumerable<string> GetProbeDirectoryPaths()
        {
            yield return AppContext.BaseDirectory;
            yield return Directory.GetCurrentDirectory();

            foreach (
                var path in Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator)
                    ?? Enumerable.Empty<string>()
            )
            {
                yield return path;
            }
        }

        return GetProbeDirectoryPaths()
            .Select(dirPath =>
                Path.Combine(dirPath, OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg")
            )
            .FirstOrDefault(File.Exists);
    }

    public static bool IsAvailable() => !string.IsNullOrWhiteSpace(TryGetCliFilePath());
}
