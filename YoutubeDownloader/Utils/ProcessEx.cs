using System.Diagnostics;

namespace YoutubeDownloader.Utils
{
    internal static class ProcessEx
    {
        public static void StartShellExecute(string path)
        {
            var startInfo = new ProcessStartInfo(path)
            {
                UseShellExecute = true
            };

            using (Process.Start(startInfo)) {}
        }
    }
}