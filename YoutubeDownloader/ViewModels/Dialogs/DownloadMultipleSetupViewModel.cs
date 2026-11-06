using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerKit.Extensions;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Components;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadMultipleSetupViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    LocalizationManager localizationManager,
    SettingsService settingsService
) : DialogViewModelBase<IReadOnlyList<DownloadViewModel>>
{
    private IReadOnlyList<IVideo> _initialVideos = [];
    private bool _isSorting;

    public LocalizationManager LocalizationManager { get; } = localizationManager;

    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<IVideo>? AvailableVideos { get; set; }

    [ObservableProperty]
    public partial Container SelectedContainer { get; set; } = Container.Mp4;

    [ObservableProperty]
    public partial VideoQualityPreference SelectedVideoQualityPreference { get; set; } =
        VideoQualityPreference.Highest;

    [ObservableProperty]
    public partial VideoSortOption SelectedSortOption { get; set; } = VideoSortOption.Default;

    public ObservableCollection<IVideo> SelectedVideos { get; } = [];

    public IReadOnlyList<Container> AvailableContainers { get; } =
    [Container.Mp4, Container.WebM, Container.Mp3, new("ogg")];

    public IReadOnlyList<VideoQualityPreference> AvailableVideoQualityPreferences { get; } =
        // Without .AsEnumerable(), the below line throws a compile-time error starting with .NET SDK v9.0.200
        Enum.GetValues<VideoQualityPreference>().AsEnumerable().Reverse().ToArray();

    public IReadOnlyList<VideoSortOption> AvailableVideoSortOptions { get; } =
        Enum.GetValues<VideoSortOption>();

    partial void OnAvailableVideosChanged(
        IReadOnlyList<IVideo>? oldValue,
        IReadOnlyList<IVideo>? newValue
    )
    {
        if (!_isSorting && newValue is not null)
        {
            _initialVideos = newValue;
            if (SelectedSortOption != VideoSortOption.Default)
                ApplySort();
        }
    }

    partial void OnSelectedSortOptionChanged(VideoSortOption value) => ApplySort();

    private void ApplySort()
    {
        if (_initialVideos.Count == 0)
            return;

        var sorted = SelectedSortOption switch
        {
            VideoSortOption.TitleAscending => _initialVideos
                .OrderBy(v => v.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToArray(),
            VideoSortOption.TitleDescending => _initialVideos
                .OrderByDescending(v => v.Title, StringComparer.CurrentCultureIgnoreCase)
                .ToArray(),
            VideoSortOption.DurationAscending => _initialVideos
                .OrderBy(v => v.Duration ?? TimeSpan.Zero)
                .ToArray(),
            VideoSortOption.DurationDescending => _initialVideos
                .OrderByDescending(v => v.Duration ?? TimeSpan.Zero)
                .ToArray(),
            _ => _initialVideos,
        };

        _isSorting = true;
        try
        {
            AvailableVideos = sorted;
        }
        finally
        {
            _isSorting = false;
        }
    }

    public override Task InitializeAsync()
    {
        SelectedContainer = settingsService.LastContainer;
        SelectedVideoQualityPreference = settingsService.LastVideoQualityPreference;
        SelectedVideos.CollectionChanged += (_, _) => ConfirmCommand.NotifyCanExecuteChanged();

        return Task.CompletedTask;
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
        var dirPath = await dialogManager.PromptDirectoryPathAsync();
        if (string.IsNullOrWhiteSpace(dirPath))
            return;

        // Maintain the sorted display order for downloaded files and their numeric index
        var selectedVideos = (AvailableVideos ?? []).Where(SelectedVideos.Contains).ToArray();

        var downloads = new List<DownloadViewModel>();
        foreach (var (i, video) in selectedVideos.Index())
        {
            var baseFilePath = Path.Combine(
                dirPath,
                FileNameTemplate.Apply(
                    settingsService.FileNameTemplate,
                    video,
                    SelectedContainer,
                    (i + 1).ToString().PadLeft(selectedVideos.Length.ToString().Length, '0')
                )
            );

            if (settingsService.ShouldSkipExistingFiles && File.Exists(baseFilePath))
                continue;

            var filePath = Path.EnsureUniqueFilePath(baseFilePath);

            // Download does not start immediately, so lock in the file path to avoid conflicts
            Directory.CreateForFile(filePath);
            await File.WriteAllBytesAsync(filePath, []);

            downloads.Add(
                viewModelManager.GetDownloadViewModel(
                    video,
                    new VideoDownloadPreference(SelectedContainer, SelectedVideoQualityPreference),
                    filePath
                )
            );
        }

        settingsService.LastContainer = SelectedContainer;
        settingsService.LastVideoQualityPreference = SelectedVideoQualityPreference;

        Close(downloads);
    }
}
