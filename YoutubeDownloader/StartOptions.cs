using System;
using System.IO;

namespace YoutubeDownloader;

public partial class StartOptions
{
    public required string SettingsPath { get; init; }

    public required bool IsAutoUpdateAllowed { get; init; }
}

public partial class StartOptions
{
    public static StartOptions Current { get; } =
        new()
        {
            SettingsPath =
                Environment.GetEnvironmentVariable("YOUTUBEDOWNLOADER_SETTINGS_PATH") is { } path
                && !string.IsNullOrWhiteSpace(path)
                    ? Path.EndsInDirectorySeparator(path) || Directory.Exists(path)
                        ? Path.Combine(path, "Settings.dat")
                        : path
                    : Path.Combine(AppContext.BaseDirectory, "Settings.dat"),
            IsAutoUpdateAllowed = !(
                Environment.GetEnvironmentVariable("YOUTUBEDOWNLOADER_ALLOW_AUTO_UPDATE") is { } env
                && env.Equals("false", StringComparison.OrdinalIgnoreCase)
            ),
        };
}
