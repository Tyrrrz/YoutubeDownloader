using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Components;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadMultipleSetupViewModel(
    ViewModelManager viewModelManager,
    SettingsService settingsService
) : DialogViewModelBase<IReadOnlyList<DownloadViewModel>>
{
    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<IVideo>? AvailableVideos { get; set; }

    [ObservableProperty]
    public partial Container SelectedContainer { get; set; } = Container.Mp4;

    [ObservableProperty]
    public partial VideoQualityPreference SelectedVideoQualityPreference { get; set; } =
        VideoQualityPreference.Highest;

    public ObservableCollection<IVideo> SelectedVideos { get; } = [];

    public IReadOnlyList<Container> AvailableContainers { get; } =
        [Container.Mp4, Container.WebM, Container.Mp3, new("ogg")];

    public IReadOnlyList<VideoQualityPreference> AvailableVideoQualityPreferences { get; } =
        // Without .AsEnumerable(), the below line throws a compile-time error starting with .NET SDK v9.0.200
        Enum.GetValues<VideoQualityPreference>().AsEnumerable().Reverse().ToArray();

    [RelayCommand]
    private void Initialize()
    {
        SelectedContainer = settingsService.LastContainer;
        SelectedVideoQualityPreference = settingsService.LastVideoQualityPreference;
        SelectedVideos.CollectionChanged += (_, _) => ConfirmCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task CopyTitleAsync()
    {
        if (Application.Current?.ApplicationLifetime?.TryGetTopLevel()?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(Title);
    }

    private bool CanConfirm() => SelectedVideos.Any();

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmAsync()
    {
        var downloads = new List<DownloadViewModel>();

        if (OperatingSystem.IsAndroid())
        {
            await HandleAndroidMultipleDownloadsAsync(downloads);
        }
        else
        {
            await HandleDesktopMultipleDownloadsAsync(downloads);
        }

        if (downloads.Count != 0)
        {
            settingsService.LastContainer = SelectedContainer;
            settingsService.LastVideoQualityPreference = SelectedVideoQualityPreference;
            Close(downloads);
        }
    }

    private async Task HandleDesktopMultipleDownloadsAsync(List<DownloadViewModel> downloads)
    {
        var dirPath = await DialogManager.PromptDirectoryPathAsync();
        if (string.IsNullOrWhiteSpace(dirPath))
            return;

        for (var i = 0; i < SelectedVideos.Count; i++)
        {
            var video = SelectedVideos[i];

            var baseFilePath = Path.Combine(
                dirPath,
                FileNameTemplate.Apply(
                    settingsService.FileNameTemplate,
                    video,
                    SelectedContainer,
                    (i + 1).ToString().PadLeft(SelectedVideos.Count.ToString().Length, '0')
                )
            );

            if (settingsService.ShouldSkipExistingFiles && File.Exists(baseFilePath))
                continue;

            var filePath = PathEx.EnsureUniquePath(baseFilePath);

            // Download does not start immediately, so lock in the file path to avoid conflicts
            DirectoryEx.CreateDirectoryForFile(filePath);
            await File.WriteAllBytesAsync(filePath, []);

            downloads.Add(
                viewModelManager.CreateDownloadViewModel(
                    video,
                    new VideoDownloadPreference(SelectedContainer, SelectedVideoQualityPreference),
                    filePath
                )
            );
        }
    }

    private async Task HandleAndroidMultipleDownloadsAsync(List<DownloadViewModel> downloads)
    {
        TopLevel? topLevel = Application.Current?.ApplicationLifetime?.TryGetTopLevel();
        if (topLevel?.StorageProvider == null)
            return;

        var folderPickerOptions = new FolderPickerOpenOptions
        {
            Title = "Select Download Folder",
            AllowMultiple = false,
        };

        try
        {
            var suggestedStartLocation =
                await AndroidDownloadingFiles.GetSuggestedStartLocationAsync(
                    topLevel.StorageProvider
                );
            if (suggestedStartLocation != null)
            {
                folderPickerOptions.SuggestedStartLocation = suggestedStartLocation;
            }
        }
        catch
        {
            // Continue without suggested location if it fails
        }

        var selectedFolders = await topLevel.StorageProvider.OpenFolderPickerAsync(
            folderPickerOptions
        );
        if (selectedFolders.Count == 0)
            return;

        var selectedFolder = selectedFolders[0];

        for (var i = 0; i < SelectedVideos.Count; i++)
        {
            var video = SelectedVideos[i];

            // Generate the filename using the same template as desktop
            var fileName = FileNameTemplate.Apply(
                settingsService.FileNameTemplate,
                video,
                SelectedContainer,
                (i + 1).ToString().PadLeft(SelectedVideos.Count.ToString().Length, '0')
            );

            try
            {
                // Create the file in the selected folder
                var outputFile = await selectedFolder.CreateFileAsync(fileName);
                if (outputFile == null)
                    continue; // Skip this file if creation failed

                string? filePath = null;

                // Handle the file path based on URI type
                if (outputFile.Path.AbsoluteUri.StartsWith("content://"))
                {
                    // For content URIs, create a unique temp file path
                    string tempDir = Path.GetTempPath();
                    string uniqueId = $"{DateTime.Now:yyyyMMdd_HHmmss}_{i + 1:D4}";
                    string fileExtension = Path.GetExtension(fileName);
                    string tempFileName =
                        $"{Path.GetFileNameWithoutExtension(fileName)}_{uniqueId}{fileExtension}";
                    string tempPath = Path.Combine(tempDir, tempFileName);

                    // Ensure the temp path is unique
                    tempPath = AndroidDownloadingFiles.GetUniqueFilePath(tempPath);

                    // Use the AndroidDownloadingFiles helper to manage the storage file
                    filePath = AndroidDownloadingFiles.CreateAndroidFileReference(
                        tempPath,
                        outputFile
                    );
                }
                else
                {
                    // For regular file URIs, convert to local path
                    filePath = AndroidDownloadingFiles.GetLocalFilePath(
                        outputFile.Path.AbsoluteUri
                    );
                }

                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    downloads.Add(
                        viewModelManager.CreateDownloadViewModel(
                            video,
                            new VideoDownloadPreference(
                                SelectedContainer,
                                SelectedVideoQualityPreference
                            ),
                            filePath
                        )
                    );
                }
            }
            catch (Exception)
            {
                // Skip this file if there's an error creating it
                continue;
            }
        }
    }
}
