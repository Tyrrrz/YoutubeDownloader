using System.Collections.Generic;
using System.Diagnostics;

namespace YoutubeDownloader.Utils;

internal static class ProcessEx
{
    public static void Start(string path, IReadOnlyList<string>? arguments = null)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(path)
        };

        if (arguments is not null)
        {
            foreach (var argument in arguments)
                process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
    }

    public static void StartShellExecute(string path, IReadOnlyList<string>? arguments = null)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(path)
            {
                UseShellExecute = true
            }
        };

        if (arguments is not null)
        {
            foreach (var argument in arguments)
                process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();
    }
}