using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Framework;

public static class AndroidDownloadingFiles
{
    private static readonly Dictionary<string, IStorageFile> _pendingFiles = [];
    private static readonly Lock _lockObject = new();
    private static int _fileCounter = 0;

    public static async Task<string?> PromptSaveFilePathAndroidAsync(
        TopLevel topLevel,
        SettingsService settingsService,
        VideoDownloadOption SelectedDownloadOption,
        IVideo Video
    )
    {
        try
        {
            if (topLevel?.StorageProvider == null)
                return null;

            string suggestedFileName = FileNameTemplate.Apply(
                settingsService.FileNameTemplate,
                Video!,
                SelectedDownloadOption!.Container
            );

            var saveOptions = new FilePickerSaveOptions
            {
                Title = "Save Downloaded File",
                SuggestedFileName = suggestedFileName,
                ShowOverwritePrompt = true,
            };

            try
            {
                var suggestedStartLocation = await GetSuggestedStartLocationAsync(
                    topLevel.StorageProvider
                );
                if (suggestedStartLocation != null)
                {
                    saveOptions.SuggestedStartLocation = suggestedStartLocation;
                }
            }
            catch
            {
                // Continue without suggested location if it fails
            }

            IStorageFile? outputFile = await topLevel.StorageProvider.SaveFilePickerAsync(
                saveOptions
            );

            if (outputFile == null)
                return null;

            // For Android, we need to handle the file path differently
            if (outputFile.Path.AbsoluteUri.StartsWith("content://"))
            {
                // For content URIs, create a unique temp file path and store the IStorageFile reference
                string tempDir = Path.GetTempPath();

                // Generate a unique temporary file name to avoid conflicts
                string uniqueId;
                lock (_lockObject)
                {
                    uniqueId = $"{DateTime.Now:yyyyMMdd_HHmmss}_{++_fileCounter:D4}";
                }

                string fileExtension = Path.GetExtension(suggestedFileName);
                string tempFileName =
                    $"{Path.GetFileNameWithoutExtension(suggestedFileName)}_{uniqueId}_{fileExtension}";
                string tempPath = Path.Combine(tempDir, tempFileName);

                // Ensure the temp path is unique even if somehow there's still a collision
                tempPath = GetUniqueFilePath(tempPath);

                // Store the IStorageFile reference using the temp path as key
                lock (_lockObject)
                {
                    _pendingFiles[tempPath] = outputFile;
                }

                return tempPath;
            }
            else
            {
                // For regular file URIs, convert to local path
                return GetLocalFilePath(outputFile.Path.AbsoluteUri);
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Moves the downloaded file from temporary location to the final Android storage location
    /// </summary>
    /// <param name="tempFilePath">The temporary file path where the download was saved</param>
    /// <returns>True if the move was successful, false otherwise</returns>
    public static async Task<bool> MoveDownloadedFileAsync(string tempFilePath)
    {
        try
        {
            IStorageFile? storageFile;

            lock (_lockObject)
            {
                if (!_pendingFiles.TryGetValue(tempFilePath, out storageFile))
                {
                    // Not an Android content URI file, no need to move
                    return true;
                }
            }

            // Check if the temporary file exists
            if (!File.Exists(tempFilePath))
            {
                lock (_lockObject)
                {
                    _pendingFiles.Remove(tempFilePath);
                }
                return false;
            }

            // Read the temporary file
            byte[] fileData = await File.ReadAllBytesAsync(tempFilePath);

            // Write to the final storage location
            using (var stream = await storageFile.OpenWriteAsync())
            {
                await stream.WriteAsync(fileData);
                await stream.FlushAsync();
            }

            // Clean up: delete temporary file and remove from tracking
            try
            {
                File.Delete(tempFilePath);
            }
            catch
            {
                // Ignore cleanup errors
            }

            lock (_lockObject)
            {
                _pendingFiles.Remove(tempFilePath);
            }

            return true;
        }
        catch (Exception)
        {
            // Clean up on error
            lock (_lockObject)
            {
                _pendingFiles.Remove(tempFilePath);
            }
            return false;
        }
    }

    /// <summary>
    /// Checks if the given file path is a temporary Android file that needs to be moved
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <returns>True if this is a pending Android file</returns>
    public static bool IsPendingAndroidFile(string filePath)
    {
        lock (_lockObject)
        {
            return _pendingFiles.ContainsKey(filePath);
        }
    }

    /// <summary>
    /// Cleans up any pending files (call this when download is cancelled or fails)
    /// </summary>
    /// <param name="tempFilePath">The temporary file path to clean up</param>
    public static void CleanupPendingFile(string tempFilePath)
    {
        bool wasRemoved;
        lock (_lockObject)
        {
            wasRemoved = _pendingFiles.Remove(tempFilePath);
        }

        if (wasRemoved)
        {
            try
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Gets a unique file path by appending a number if the file already exists
    /// </summary>
    /// <param name="basePath">The base file path</param>
    /// <returns>A unique file path</returns>
    private static string GetUniqueFilePath(string basePath)
    {
        if (!File.Exists(basePath))
            return basePath;

        string directory = Path.GetDirectoryName(basePath) ?? "";
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(basePath);
        string extension = Path.GetExtension(basePath);

        int counter = 1;
        string uniquePath;

        do
        {
            uniquePath = Path.Combine(
                directory,
                $"{fileNameWithoutExtension}_{counter}{extension}"
            );
            counter++;
        } while (File.Exists(uniquePath));

        return uniquePath;
    }

    private static async Task<IStorageFolder?> GetSuggestedStartLocationAsync(
        IStorageProvider storageProvider
    )
    {
        try
        {
            var wellKnownFolders = new[]
            {
                WellKnownFolder.Downloads,
                WellKnownFolder.Documents,
                WellKnownFolder.Music,
                WellKnownFolder.Videos,
            };

            foreach (var folder in wellKnownFolders)
            {
                try
                {
                    var storageFolder = await storageProvider.TryGetWellKnownFolderAsync(folder);
                    if (storageFolder != null)
                    {
                        return storageFolder;
                    }
                }
                catch
                {
                    // Continue to next folder if this one fails
                    continue;
                }
            }

            // If no well-known folders work, try to get desktop folder
            try
            {
                var desktopFolder = await storageProvider.TryGetWellKnownFolderAsync(
                    WellKnownFolder.Desktop
                );
                if (desktopFolder != null)
                {
                    return desktopFolder;
                }
            }
            catch
            {
                // Fall through to return null
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a file URI to a local file system path
    /// </summary>
    /// <param name="path">The file path or URI</param>
    /// <returns>Local file system path</returns>
    private static string GetLocalFilePath(string path)
    {
        // If it's already a local path, return as-is
        if (!path.StartsWith("file://"))
        {
            return path;
        }

        try
        {
            // Convert URI to local path
            Uri uri = new(path);
            return uri.LocalPath;
        }
        catch (UriFormatException)
        {
            // If URI parsing fails, return the original path
            return path;
        }
    }
}
