using System.IO;
using Tyrrrz.Extensions;

namespace YoutubeDownloader.Internal
{
    internal static class FileEx
    {
        public static string MakeUniqueFilePath(string baseFilePath, int maxAttempts = 100)
        {
            // Check if base file path exists
            if (!File.Exists(baseFilePath))
                return baseFilePath;

            // Get file path components
            var baseDirPath = Path.GetDirectoryName(baseFilePath);
            var baseFileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFilePath);
            var baseFileExtension = Path.GetExtension(baseFilePath);

            // Try with incrementing suffixes
            for (var i = 1; i <= maxAttempts; i++)
            {
                // Assemble file path
                var fileName = $"{baseFileNameWithoutExtension} ({i}){baseFileExtension}";
                var filePath = baseDirPath.IsNotBlank() ? Path.Combine(baseDirPath, fileName) : fileName;

                // Check if file exists
                if (!File.Exists(filePath))
                    return filePath;
            }

            // If number of attempts exceeded, just return original path
            return baseFilePath;
        }

        public static void CreateEmptyFile(string filePath) => File.WriteAllText(filePath, "");
    }
}