using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Gress;

namespace YoutubeDownloader.Core.Downloading;

public static class FFmpeg
{
    private const string Version = "8.0.1"; // Keep in sync with DownloadFFmpeg.csx

    public static string CliFileName { get; } =
        OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";

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
        GetProbeDirectoryPaths()
            .Distinct(StringComparer.Ordinal)
            .Select(dirPath => Path.Combine(dirPath, CliFileName))
            .FirstOrDefault(File.Exists);

    public static bool IsBundled() =>
        File.Exists(Path.Combine(AppContext.BaseDirectory, CliFileName));

    public static async Task DownloadAsync(
        string outputDirPath,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        var arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            var other => throw new PlatformNotSupportedException(
                $"Unsupported architecture: {other}."
            ),
        };

        string platform;
        if (OperatingSystem.IsWindows())
            platform = $"windows-{arch}";
        else if (OperatingSystem.IsLinux())
            platform = $"linux-{arch}";
        else if (OperatingSystem.IsMacOS())
            platform = $"osx-{arch}";
        else
            throw new PlatformNotSupportedException("Unsupported operating system.");

        var outputFilePath = Path.Combine(outputDirPath, CliFileName);
        var archiveFilePath = outputFilePath + ".zip";

        try
        {
            using var http = new HttpClient();
            var url =
                $"https://github.com/Tyrrrz/FFmpegBin/releases/download/{Version}/ffmpeg-{platform}.zip";

            using var response = await http.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
            response.EnsureSuccessStatusCode();

            var totalSize = response.Content.Headers.ContentLength;
            var downloadedSize = 0L;
            var buffer = new byte[81920];

            await using var responseStream = await response.Content.ReadAsStreamAsync(
                cancellationToken
            );
            await using (var archiveFile = File.Create(archiveFilePath))
            {
                int bytesRead;
                while (
                    (
                        bytesRead = await responseStream.ReadAsync(
                            buffer,
                            cancellationToken
                        )
                    ) > 0
                )
                {
                    await archiveFile.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    downloadedSize += bytesRead;

                    if (totalSize > 0)
                    {
                        progress?.Report(
                            Percentage.FromFraction((double)downloadedSize / totalSize.Value)
                        );
                    }
                }
            }

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
                    File.GetUnixFileMode(outputFilePath)
                        | UnixFileMode.UserExecute
                        | UnixFileMode.GroupExecute
                        | UnixFileMode.OtherExecute
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
