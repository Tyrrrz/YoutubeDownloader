﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YoutubeDownloader.Core.Downloading;

public static class FFmpeg
{
    private static string CliFileName { get; } =
        OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";

    private static string? _customFFmpegPath;

    /// <summary>
    /// Set a custom FFmpeg path (useful for Android apps that extract from assets)
    /// </summary>
    public static void SetCustomPath(string? ffmpegPath)
    {
        _customFFmpegPath = ffmpegPath;
    }

    public static string? TryGetCliFilePath()
    {
        // First check if a custom path was set (e.g., from Android app)
        if (!string.IsNullOrEmpty(_customFFmpegPath) && File.Exists(_customFFmpegPath))
            return _customFFmpegPath;

        // Fallback to original logic
        static IEnumerable<string> GetProbeDirectoryPaths()
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

        return GetProbeDirectoryPaths()
            .Distinct(StringComparer.Ordinal)
            .Select(dirPath => Path.Combine(dirPath, CliFileName))
            .FirstOrDefault(File.Exists);
    }

    public static bool IsBundled() =>
        (!string.IsNullOrEmpty(_customFFmpegPath) && File.Exists(_customFFmpegPath))
        || File.Exists(Path.Combine(AppContext.BaseDirectory, CliFileName));

    public static bool IsAvailable() => !string.IsNullOrWhiteSpace(TryGetCliFilePath());
}
