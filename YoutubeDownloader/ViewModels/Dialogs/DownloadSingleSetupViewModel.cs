using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadSingleSetupViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    SettingsService settingsService,
    IClipboard clipboard
) : DialogViewModelBase<DownloadViewModel>
{
    [ObservableProperty]
    private IVideo? _video;
    
    [ObservableProperty]
    private IReadOnlyList<VideoDownloadOption>? _availableDownloadOptions;
    
    [ObservableProperty]
    private VideoDownloadOption? _selectedDownloadOption;

    [RelayCommand]
    private void Initialize()
    {
        SelectedDownloadOption = AvailableDownloadOptions?.FirstOrDefault(o =>
            o.Container == settingsService.LastContainer
        );
    }

    [RelayCommand]
    private async Task CopyTitleAsync() => await clipboard.SetTextAsync(Video?.Title);

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (Video is null || SelectedDownloadOption is null)
            return;
        
        var container = SelectedDownloadOption.Container;

        var filePath = await dialogManager.PromptSaveFilePathAsync(
            new[]
            {
                new FilePickerFileType($"{container.Name} file")
                {
                    Patterns = new[] { $"*.{container.Name}" }
                }
            },
            FileNameTemplate.Apply(settingsService.FileNameTemplate, Video, container)
        );

        if (string.IsNullOrWhiteSpace(filePath))
            return;

        // Download does not start immediately, so lock in the file path to avoid conflicts
        DirectoryEx.CreateDirectoryForFile(filePath);
        await File.WriteAllBytesAsync(filePath, []);

        settingsService.LastContainer = container;

        Close(viewModelManager.CreateDownloadViewModel(Video, SelectedDownloadOption, filePath));
    }
}
