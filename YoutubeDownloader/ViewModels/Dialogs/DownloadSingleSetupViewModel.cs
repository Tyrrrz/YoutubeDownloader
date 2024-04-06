using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadSingleSetupViewModel(
    IViewModelFactory viewModelFactory,
    DialogManager dialogManager,
    SettingsService settingsService,
    IClipboard clipboard
) : DialogScreen<DownloadViewModel>
{
    public IVideo? Video { get; set; }

    public IReadOnlyList<VideoDownloadOption>? AvailableDownloadOptions { get; set; }

    public VideoDownloadOption? SelectedDownloadOption { get; set; }

    protected override void OnViewLoaded()
    {
        SelectedDownloadOption = AvailableDownloadOptions?.FirstOrDefault(o =>
            o.Container == settingsService.LastContainer
        );
    }

    [RelayCommand]
    public async Task CopyTitleAsync() => await clipboard.SetTextAsync(Video!.Title);

    [RelayCommand]
    public async Task ConfirmAsync()
    {
        var container = SelectedDownloadOption!.Container;

        var fileType = new FilePickerFileType($"{container.Name} file")
        {
            Patterns = new[] { $"*.{container.Name}" },
        };

        var filePath = await dialogManager.PromptSaveFilePathAsync(
            new[] { fileType },
            FileNameTemplate.Apply(settingsService.FileNameTemplate, Video!, container)
        );

        if (string.IsNullOrWhiteSpace(filePath))
            return;

        // Download does not start immediately, so lock in the file path to avoid conflicts
        DirectoryEx.CreateDirectoryForFile(filePath);
        File.WriteAllBytes(filePath, Array.Empty<byte>());

        settingsService.LastContainer = container;

        Close(viewModelFactory.CreateDownloadViewModel(Video!, SelectedDownloadOption!, filePath));
    }
}
