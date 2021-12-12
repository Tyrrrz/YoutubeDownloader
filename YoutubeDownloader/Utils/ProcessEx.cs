using System.Diagnostics;

namespace YoutubeDownloader.Utils;

internal static class ProcessEx
{
    public static void StartShellExecute(string path)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(path)
            {
                UseShellExecute = true
            }
        };

        process.Start();
    }
}