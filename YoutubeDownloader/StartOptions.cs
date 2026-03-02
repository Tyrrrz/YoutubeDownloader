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

    private static readonly string SettingsPathVariable = "SETTINGS_PATH";

    private static readonly string DefaultSettingsPath = Path.Combine(
        AppContext.BaseDirectory,
        SettingsFileName
    );

    public static StartOptions Current { get; } = Parse(Environment.GetEnvironmentVariables());

    private static StartOptions Parse(IDictionary environmentVars) =>
        new() { SettingsPath = GetSettingsPath(environmentVars) };

    private static string GetSettingsPath(IDictionary environmentVars)
    {
        if (
            !environmentVars.Contains(SettingsPathVariable)
            || environmentVars[SettingsPathVariable] is not string pathFromEnv
        )
            return DefaultSettingsPath;

        // Check Environment Variables first.
        if (TryMakeValidPath(pathFromEnv, out var result))
            return result;

        // Fall back to default path.
        return DefaultSettingsPath;
    }

    private static bool TryMakeValidPath(
        [NotNullWhen(true)] string? pathToDirectory,
        [NotNullWhen(true)] out string? result
    )
    {
        result = null;

        if (pathToDirectory is null)
            return false;

        var pathToFile = Path.Combine(pathToDirectory, SettingsFileName);

        // Check if the path is a valid URI.
        if (Uri.TryCreate(pathToFile, UriKind.RelativeOrAbsolute, out var uri))
            result = uri.ToString();

        return result is not null;
    }
}
