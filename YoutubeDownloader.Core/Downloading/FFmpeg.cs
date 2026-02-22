using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YoutubeDownloader.Core.Downloading;

public static class FFmpeg
{
    private static string CliFileName { get; } =
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

    public static bool IsAvailable() => !string.IsNullOrWhiteSpace(TryGetCliFilePath());

    public static string GetDiagnosticsReport()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Expected file name: {CliFileName}");
        sb.AppendLine($"Application directory: {AppContext.BaseDirectory}");
        sb.AppendLine($"Working directory: {Directory.GetCurrentDirectory()}");

        var processPathEntries =
            Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
        sb.AppendLine(
            processPathEntries.Length > 0
                ? $"Process PATH ({processPathEntries.Length} entries):"
                : "Process PATH: (not set)"
        );
        foreach (var entry in processPathEntries)
            sb.AppendLine($"  {entry}");

        sb.AppendLine("Probe directories:");
        foreach (var dir in GetProbeDirectoryPaths().Distinct(StringComparer.Ordinal))
        {
            var filePath = Path.Combine(dir, CliFileName);
            var found = File.Exists(filePath);
            sb.AppendLine($"  {filePath}: {(found ? "found" : "not found")}");
        }

        return sb.ToString().TrimEnd();
    }
}
