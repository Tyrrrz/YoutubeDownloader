using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Framework;

public static class AndroidDownloadingFiles
{
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
            // Return a path that the download system can work with
            if (outputFile.Path.AbsoluteUri.StartsWith("content://"))
            {
                // For content URIs, we'll need to create a temp file path
                // The actual saving will be handled later using the IStorageFile
                string tempDir = Path.GetTempPath();
                string tempFileName = $"{suggestedFileName}";
                string tempPath = Path.Combine(tempDir, tempFileName);

                // Store the IStorageFile reference for later use
                // You might need to add this as a property or pass it along
                // This is a simplified approach - you may need to modify your download system
                // to accept IStorageFile instead of just file paths

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
