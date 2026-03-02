using System;
using System.IO;

namespace YoutubeDownloader;

public partial class StartOptions
{
    public required string SettingsPath { get; init; }
}

public partial class StartOptions
{
    private static readonly string SettingsFileName = "Settings.dat";

    private static readonly string SettingsPathVariable = "YOUTUBEDOWNLOADER_SETTINGS_PATH";

    private static readonly string DefaultSettingsPath = Path.Combine(
        AppContext.BaseDirectory,
        SettingsFileName
    );

    public static StartOptions Current { get; } =
        new()
        {
            SettingsPath = Environment.GetEnvironmentVariable(SettingsPathVariable)
                is { } path
                    and not ""
                ? Path.EndsInDirectorySeparator(path) || Directory.Exists(path)
                    ? Path.Combine(path, SettingsFileName)
                    : path
                : DefaultSettingsPath,
        };
}
