using System.IO;

namespace YoutubeDownloader.Utils.Extensions;

internal static class DirectoryExtensions
{
    extension(Directory)
    {
        public static void CreateDirectoryForFile(string filePath)
        {
            var dirPath = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(dirPath))
                return;

            Directory.CreateDirectory(dirPath);
        }
    }
}
