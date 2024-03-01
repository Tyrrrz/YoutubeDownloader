using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadMultipleSetupViewModel(
    IViewModelFactory viewModelFactory,
    DialogManager dialogManager,
    SettingsService settingsService,
    IClipboard clipboard
) : DialogScreen<IReadOnlyList<DownloadViewModel>>
{
    public string? Title { get; set; }

    public IReadOnlyList<IVideo>? AvailableVideos { get; set; }

    public ObservableCollection<IVideo> SelectedVideos { get; set; } =
        new ObservableCollection<IVideo>();

    public IReadOnlyList<Container> AvailableContainers { get; } =
        new[] { Container.Mp4, Container.WebM, Container.Mp3, new Container("ogg") };

    public Container SelectedContainer { get; set; } = Container.Mp4;

    public IReadOnlyList<VideoQualityPreference> AvailableVideoQualityPreferences { get; } =
        Enum.GetValues<VideoQualityPreference>().Reverse().ToArray();

    public VideoQualityPreference SelectedVideoQualityPreference { get; set; } =
        VideoQualityPreference.Highest;

    protected override void OnViewLoaded()
    {
        SelectedContainer = settingsService.LastContainer;
        SelectedVideoQualityPreference = settingsService.LastVideoQualityPreference;
        SelectedVideos.CollectionChanged += (_, _) => ConfirmCommand.NotifyCanExecuteChanged();
    }

    public async Task CopyTitle() => await clipboard.SetTextAsync(Title!);

    public bool CanConfirm => SelectedVideos.Any();

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    public async Task Confirm()
    {
        var dirPath = await dialogManager.PromptDirectoryPath();
        if (string.IsNullOrWhiteSpace(dirPath))
            return;

        var downloads = new List<DownloadViewModel>();
        for (var i = 0; i < SelectedVideos!.Count; i++)
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
            File.WriteAllBytes(filePath, Array.Empty<byte>());

            downloads.Add(
                viewModelFactory.CreateDownloadViewModel(
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

public static class DownloadMultipleSetupViewModelExtensions
{
    public static DownloadMultipleSetupViewModel CreateDownloadMultipleSetupViewModel(
        this IViewModelFactory factory,
        string title,
        IReadOnlyList<IVideo> availableVideos,
        bool preselectVideos = true
    )
    {
        var viewModel = factory.CreateDownloadMultipleSetupViewModel();

        viewModel.Title = title;
        viewModel.AvailableVideos = availableVideos;
        viewModel.SelectedVideos = preselectVideos
            ? new ObservableCollection<IVideo>(availableVideos)
            : [];

        return viewModel;
    }
}
