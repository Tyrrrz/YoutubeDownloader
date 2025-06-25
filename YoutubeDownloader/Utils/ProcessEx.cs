using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.Utils;

internal static class ProcessEx
{
    public static void Start(string path, IReadOnlyList<string>? arguments = null)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(path);

        if (arguments is not null)
        {
            foreach (var argument in arguments)
                process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
    }

    public static void StartShellExecute(string path, IReadOnlyList<string>? arguments = null)
    {
        if (OperatingSystem.IsAndroid())
        {
            var topLevel =
                Application.Current?.ApplicationLifetime?.TryGetTopLevel()
                ?? throw new ApplicationException("Could not find the top-level visual element.");

            var launcher = topLevel.Launcher;

            launcher.LaunchUriAsync(new Uri(path, UriKind.RelativeOrAbsolute));
            return;
        }
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(path) { UseShellExecute = true };

        if (arguments is not null)
        {
            foreach (var argument in arguments)
                process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
    }
}
