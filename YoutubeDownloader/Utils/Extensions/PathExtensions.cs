using System;
using System.Collections.Generic;
using System.IO;

namespace YoutubeDownloader.Utils.Extensions;

internal static class PathExtensions
{
    extension(Path)
    {
        public static string EnsureUniqueFilePath(
            string baseFilePath,
            ISet<string>? reservedFilePaths = null,
            int maxRetries = 100
        )
        {
            if (!File.Exists(baseFilePath) && reservedFilePaths?.Contains(baseFilePath) != true)
                return baseFilePath;

            var baseDirPath = Path.GetDirectoryName(baseFilePath);
            var baseFileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFilePath);
            var baseFileExtension = Path.GetExtension(baseFilePath);

            for (var i = 1; i <= maxRetries; i++)
            {
                var fileName = $"{baseFileNameWithoutExtension} ({i}){baseFileExtension}";
                var filePath = !string.IsNullOrWhiteSpace(baseDirPath)
                    ? Path.Combine(baseDirPath, fileName)
                    : fileName;

                if (!File.Exists(filePath) && reservedFilePaths?.Contains(filePath) != true)
                    return filePath;
            }

            throw new InvalidOperationException(
                $"Could not find a unique file path for '{baseFilePath}'."
            );
        }
    }
}
