using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class DownloadMultipleSetupViewModel : DialogScreen
{
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    public IReadOnlyList<IVideo>? AvailableVideos { get; set; }

    public IReadOnlyList<IVideo>? SelectedVideos { get; set; }

    public IReadOnlyList<Container> AvailableContainers { get; } = new[]
    {
        Container.Mp4,
        Container.WebM,
        Container.Mp3,
        new Container("ogg"),
        new Container("m4a")
    };

    public Container SelectedContainer { get; set; } = Container.Mp4;

    public IReadOnlyDictionary<IVideo, string>? FilePaths { get; set; }

    public DownloadMultipleSetupViewModel(DialogManager dialogManager, SettingsService settingsService)
    {
        _dialogManager = dialogManager;
        _settingsService = settingsService;
    }

    public void OnViewLoaded()
    {
        SelectedContainer = _settingsService.LastContainer;
    }

    public void OpenVideoPage(IVideo video) => ProcessEx.StartShellExecute(video.Url);

    public bool CanConfirm => SelectedVideos!.Any();

    public void Confirm()
    {
        var dirPath = _dialogManager.PromptDirectoryPath();
        if (string.IsNullOrWhiteSpace(dirPath))
            return;

        var filePaths = new Dictionary<IVideo, string>();

        foreach (var video in SelectedVideos!)
        {
            var baseFilePath = Path.Combine(
                dirPath,
                FileNameTemplate.Apply(
                    _settingsService.FileNameTemplate,
                    video,
                    SelectedContainer
                )
            );

            var filePath = PathEx.GenerateUniquePath(baseFilePath);

            // Download does not start immediately, so lock in the file path to avoid conflicts
            DirectoryEx.CreateDirectoryForFile(filePath);
            File.WriteAllBytes(filePath, Array.Empty<byte>());

            filePaths[video] = filePath;
        }

        FilePaths = filePaths;

        _settingsService.LastContainer = SelectedContainer;

        Close(true);
    }
}

public static class DownloadMultipleSetupViewModelExtensions
{
    public static DownloadMultipleSetupViewModel CreateDownloadMultipleSetupViewModel(
        this IViewModelFactory factory,
        IReadOnlyList<IVideo> availableVideos,
        bool preselectVideos = true)
    {
        var viewModel = factory.CreateDownloadMultipleSetupViewModel();

        viewModel.AvailableVideos = availableVideos;
        viewModel.SelectedVideos = preselectVideos
            ? availableVideos
            : Array.Empty<IVideo>();

        return viewModel;
    }
}