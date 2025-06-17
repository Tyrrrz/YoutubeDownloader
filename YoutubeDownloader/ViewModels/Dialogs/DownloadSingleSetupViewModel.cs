using System;
using System.Collections.Generic;
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

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadSingleSetupViewModel(
    ViewModelManager viewModelManager,
    SettingsService settingsService
) : DialogViewModelBase<DownloadViewModel>
{
    [ObservableProperty]
    public partial IVideo? Video { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<VideoDownloadOption>? AvailableDownloadOptions { get; set; }

    [ObservableProperty]
    public partial VideoDownloadOption? SelectedDownloadOption { get; set; }

    [RelayCommand]
    private void Initialize()
    {
        SelectedDownloadOption = AvailableDownloadOptions?.FirstOrDefault(o =>
            o.Container == settingsService.LastContainer
        );
    }

    [RelayCommand]
    private async Task CopyTitleAsync()
    {
        if (Application.Current?.ApplicationLifetime?.TryGetTopLevel()?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(Video?.Title);
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (Video is null || SelectedDownloadOption is null)
            return;

        var container = SelectedDownloadOption.Container;

        string? filePath;

        if (OperatingSystem.IsAndroid())
        {
            TopLevel? topLevel = Application.Current?.ApplicationLifetime?.TryGetTopLevel();
            filePath = await AndroidDownloadingFiles.PromptSaveFilePathAndroidAsync(
                topLevel!,
                settingsService,
                SelectedDownloadOption,
                Video
            );
        }
        else
        {
            filePath = await DialogManager.PromptSaveFilePathAsync(
                [
                    new FilePickerFileType($"{container.Name} file")
                    {
                        Patterns = [$"*.{container.Name}"],
                    },
                ],
                FileNameTemplate.Apply(settingsService.FileNameTemplate, Video, container)
            );
        }

        if (string.IsNullOrWhiteSpace(filePath))
            return;

        if (!OperatingSystem.IsAndroid())
        {
            DirectoryEx.CreateDirectoryForFile(filePath);
            await File.WriteAllBytesAsync(filePath, []);
        }

        settingsService.LastContainer = container;
        Close(viewModelManager.CreateDownloadViewModel(Video, SelectedDownloadOption, filePath));
    }
}
