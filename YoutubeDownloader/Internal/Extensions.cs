using System.IO;
using Tyrrrz.Extensions;
using YoutubeExplode.Models;

namespace YoutubeDownloader.Internal
{
    internal static class Extensions
    {
        public static string GetFileNameSafeTitle(this Video video)
            => video.Title.Replace(Path.GetInvalidFileNameChars(), '_');
    }
}