using System;
using System.Reflection;

namespace YoutubeDownloader.Core.Utils;

public static class ProgramInfo
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    public static string Name { get; } = Assembly.GetName().Name ?? "YoutubeDownloader";

    public static Version Version { get; } = Assembly.GetName().Version ?? new Version(0, 0, 0);

    public static string VersionString { get; } = Version.ToString(3);

    public static bool IsDevelopmentBuild { get; } = Version.Major is <= 0 or >= 999;

    public static string ProjectUrl { get; } = "https://github.com/Tyrrrz/YoutubeDownloader";

    public static string ProjectReleasesUrl { get; } = $"{ProjectUrl}/releases";
}
