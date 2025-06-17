using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
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
    SettingsService settingsService,
    DialogManager dialogManager
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
        [.. Enum.GetValues<VideoQualityPreference>().AsEnumerable().Reverse()];

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
            TopLevel? topLevel = Application.Current?.ApplicationLifetime?.TryGetTopLevel();
            if (topLevel == null)
                return;

            var downloader = new VideoDownloader(settingsService.LastAuthCookies);

            for (var i = 0; i < SelectedVideos.Count; i++)
            {
                var video = SelectedVideos[i];

                try
                {
                    var downloadOptions = await downloader.GetDownloadOptionsAsync(
                        video.Id,
                        settingsService.ShouldInjectLanguageSpecificAudioStreams
                    );

                    var selectedOption = await downloader.GetBestDownloadOptionAsync(
                        video.Id,
                        new VideoDownloadPreference(
                            SelectedContainer,
                            SelectedVideoQualityPreference
                        ),
                        settingsService.ShouldInjectLanguageSpecificAudioStreams
                    );

                    if (selectedOption == null)
                    {
                        continue;
                    }

                    var filePath = await AndroidDownloadingFiles.PromptSaveFilePathAndroidAsync(
                        topLevel,
                        settingsService,
                        selectedOption,
                        video
                    );

                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        continue;
                    }

                    downloads.Add(
                        viewModelManager.CreateDownloadViewModel(video, selectedOption, filePath)
                    );
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
        else
        {
            var dirPath = await DialogManager.PromptDirectoryPathAsync();
            if (string.IsNullOrWhiteSpace(dirPath))
                return;

            for (var i = 0; i < SelectedVideos.Count; i++)
            {
                var video = SelectedVideos[i];
                var videoNumber = (i + 1)
                    .ToString()
                    .PadLeft(SelectedVideos.Count.ToString().Length, '0');

                var baseFilePath = Path.Combine(
                    dirPath,
                    FileNameTemplate.Apply(
                        settingsService.FileNameTemplate,
                        video,
                        SelectedContainer,
                        videoNumber
                    )
                );

                if (settingsService.ShouldSkipExistingFiles && File.Exists(baseFilePath))
                    continue;

                var filePath = PathEx.EnsureUniquePath(baseFilePath);

                DirectoryEx.CreateDirectoryForFile(filePath);
                await File.WriteAllBytesAsync(filePath, []);

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

        if (downloads.Count == 0)
        {
            if (OperatingSystem.IsAndroid())
            {
                await dialogManager.ShowDialogAsync(
                    viewModelManager.CreateMessageBoxViewModel(
                        "No files selected",
                        "No files were selected for download. This could be because you cancelled the file selection or no suitable download options were found for the selected quality and format."
                    )
                );
            }
            return;
        }

        settingsService.LastContainer = SelectedContainer;
        settingsService.LastVideoQualityPreference = SelectedVideoQualityPreference;

        Close(downloads);
    }
}
