using System;
using System.IO;
using System.Linq;

namespace YoutubeDownloader.Utils
{
    internal static class PathEx
    {
        public static string EscapeFileName(string fileName) =>
            Path.GetInvalidFileNameChars().Aggregate(fileName, (current, invalidChar) => current.Replace(invalidChar, '_'));

        public static string MakeUniqueFilePath(string baseFilePath, int maxAttempts = 100)
        {
            if (!File.Exists(baseFilePath))
                return baseFilePath;

            var baseDirPath = Path.GetDirectoryName(baseFilePath);
            var baseFileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFilePath);
            var baseFileExtension = Path.GetExtension(baseFilePath);

            for (var i = 1; i <= maxAttempts; i++)
            {
                var filePath = $"{baseFileNameWithoutExtension} ({i}){baseFileExtension}";
                if (!string.IsNullOrWhiteSpace(baseDirPath))
                    filePath = Path.Combine(baseDirPath, filePath);

                if (!File.Exists(filePath))
                    return filePath;
            }

            return baseFilePath;
        }

        public static void CreateEmptyFile(string filePath) =>
            File.WriteAllBytes(filePath, Array.Empty<byte>());

        public static void CreateDirectoryForFile(string filePath)
        {
            var dirPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dirPath))
                Directory.CreateDirectory(dirPath);
        }
    }
}