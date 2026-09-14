using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using Gress.Integrations;
using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.Core.Downloading;

public static class FFmpeg
{
    private const string Version = "8.1";

    public static string CliFileName { get; } =
        OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";

    /// <summary>
    /// Explicit path to the FFmpeg binary, for platforms where probing cannot find it.
    /// Android ships FFmpeg as a native library (libffmpeg.so) inside the APK rather than
    /// as an executable named <see cref="CliFileName" /> on a probe path, so the Android
    /// head resolves it at startup and assigns it here.
    /// </summary>
    public static string? CustomCliFilePath { get; set; }

    public static IEnumerable<string> GetProbeDirectoryPaths()
    {
        yield return AppContext.BaseDirectory;
        yield return Directory.GetCurrentDirectory();

        // Process PATH
        if (
            Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) is
            { } processPaths
        )
        {
            foreach (var path in processPaths)
                if (!string.IsNullOrWhiteSpace(path))
                    yield return path;
        }

        // Registry-based PATH variables
        if (OperatingSystem.IsWindows())
        {
            // User PATH
            if (
                Environment
                    .GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)
                    ?.Split(Path.PathSeparator) is
                { } userPaths
            )
            {
                foreach (var path in userPaths)
                    if (!string.IsNullOrWhiteSpace(path))
                        yield return path;
            }

            // System PATH
            if (
                Environment
                    .GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine)
                    ?.Split(Path.PathSeparator) is
                { } systemPaths
            )
            {
                foreach (var path in systemPaths)
                    if (!string.IsNullOrWhiteSpace(path))
                        yield return path;
            }
        }
    }

    public static string? TryGetCliFilePath() =>
        !string.IsNullOrWhiteSpace(CustomCliFilePath) && File.Exists(CustomCliFilePath)
            ? CustomCliFilePath
            : GetProbeDirectoryPaths()
                .Distinct(StringComparer.Ordinal)
                .Select(dirPath => Path.Combine(dirPath, CliFileName))
                .FirstOrDefault(File.Exists);

    private static string GetDownloadUrl()
    {
        static string GetSystemMoniker()
        {
            if (OperatingSystem.IsWindows())
                return "windows";

            if (OperatingSystem.IsLinux())
                return "linux";

            if (OperatingSystem.IsMacOS())
                return "osx";

            throw new PlatformNotSupportedException("Unsupported operating system.");
        }

        static string GetArchitectureMoniker()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                return "x64";

            if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                return "x86";

            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                return "arm64";

            throw new PlatformNotSupportedException(
                $"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}."
            );
        }

        var sys = GetSystemMoniker();
        var arch = GetArchitectureMoniker();

        return $"https://github.com/Tyrrrz/FFmpegBin/releases/download/{Version}/ffmpeg-{sys}-{arch}.zip";
    }

    public static async Task DownloadAsync(
        string outputFilePath,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        var archiveFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".zip");

        try
        {
            await Http.Client.DownloadAsync(
                GetDownloadUrl(),
                archiveFilePath,
                progress,
                cancellationToken
            );

            using var zip = ZipFile.OpenRead(archiveFilePath);
            var entry =
                zip.GetEntry(CliFileName)
                ?? throw new InvalidOperationException(
                    $"Entry '{CliFileName}' not found in the downloaded archive."
                );

            entry.ExtractToFile(outputFilePath, true);

            // Make executable on Unix
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(
                    outputFilePath,
                    File.GetUnixFileMode(outputFilePath) | UnixFileMode.UserExecute
                );
            }
        }
        finally
        {
            // Clean up the temporary archive
            if (File.Exists(archiveFilePath))
                File.Delete(archiveFilePath);
        }
    }
}
