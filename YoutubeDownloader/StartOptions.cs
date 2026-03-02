using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace YoutubeDownloader;

public partial class StartOptions
{
    public required string SettingsPath { get; init; }
}

public partial class StartOptions
{
    private static readonly string SettingsFileName = "Settings.dat";

    private static readonly string SettingsPathVariable = "YOUTUBEDOWNLOADER_SETTINGS_DIR";

    private static readonly string DefaultSettingsPath = Path.Combine(
        AppContext.BaseDirectory,
        SettingsFileName
    );

    public static StartOptions Current { get; } = Parse(Environment.GetEnvironmentVariables());

    private static StartOptions Parse(IDictionary environmentVars) =>
        new() { SettingsPath = GetSettingsPath(environmentVars) };

    private static string GetSettingsPath(IDictionary environmentVars)
    {
        if (!environmentVars.Contains(SettingsPathVariable))
            return DefaultSettingsPath;

        var pathFromEnv = environmentVars[SettingsPathVariable] as string;
        if (string.IsNullOrWhiteSpace(pathFromEnv))
            return DefaultSettingsPath;

        // Check Environment Variables first.
        if (TryMakeValidPath(pathFromEnv, out var result))
            return result;

        // Fall back to default path.
        return DefaultSettingsPath;
    }

    private static bool TryMakeValidPath(
        string pathToDirectory,
        [NotNullWhen(true)] out string? result
    )
    {
        try
        {
            // Normalize the directory path and ensure it is a valid filesystem path.
            var fullDirectoryPath = Path.GetFullPath(pathToDirectory);

            // Combine with the settings file name and normalize the full file path.
            var pathToFile = Path.Combine(fullDirectoryPath, SettingsFileName);
            var fullFilePath = Path.GetFullPath(pathToFile);
            result = fullFilePath;
            return true;
        }
        catch (Exception)
        {
            // Any exception indicates an invalid path.
            result = null;
            return false;
        }
    }
}
