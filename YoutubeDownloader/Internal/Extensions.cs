using System.IO;
using YoutubeExplode.Models;

namespace YoutubeDownloader.Internal
{
    internal static class Extensions
    {
        public static string GetFileNameSafeTitle(this Video video)
        {
            var result = video.Title;

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                result = result.Replace(invalidChar, '_');

            return result;
        }
    }
}