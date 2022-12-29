using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class DownloadMultipleSetupViewModel : DialogScreen<IReadOnlyList<DownloadViewModel>>
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    public string? Title { get; set; }

    public IReadOnlyList<IVideo>? AvailableVideos { get; set; }

    public IReadOnlyList<IVideo>? SelectedVideos { get; set; }

    public IReadOnlyList<Container> AvailableContainers { get; } = new[]
    {
        Container.Mp4
    };

    public Container SelectedContainer { get; set; } = Container.Mp4;

    public IReadOnlyList<VideoQualityPreference> AvailableVideoQualityPreferences { get; } =
        Enum.GetValues<VideoQualityPreference>().Reverse().ToArray();

    public VideoQualityPreference SelectedVideoQualityPreference { get; set; } = VideoQualityPreference.Highest;

    public DownloadMultipleSetupViewModel(
        IViewModelFactory viewModelFactory,
        DialogManager dialogManager,
        SettingsService settingsService)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        _settingsService = settingsService;
    }

    public void OnViewLoaded()
    {
        SelectedContainer = _settingsService.LastContainer;
        SelectedVideoQualityPreference = _settingsService.LastVideoQualityPreference;
    }

    public void CopyTitle() => Clipboard.SetText(Title!);

    public bool CanConfirm => SelectedVideos!.Any();

    public void Confirm()
    {
        var dirPath = _dialogManager.PromptDirectoryPath();
        if (string.IsNullOrWhiteSpace(dirPath))
            return;

        var downloads = new List<DownloadViewModel>();
        Database.Load(dirPath);

        for (var i = 0; i < SelectedVideos!.Count; i++)
        {
            var video = SelectedVideos[i];
            VideoInfo? videoInfo = Database.Find(video!.Id);
            int number;
            if(videoInfo == null){
                Database.InsertOrUpdate(new VideoInfo(0, video!.Title, video!.Id, "", ""));
                number = Database.Count();
            }else{
                // already exist, get it
                number = videoInfo.Number;
            }

            var filePath = Path.Combine(
                    dirPath,
                    FileNameTemplate.Apply(
                        _settingsService.FileNameTemplate,
                        video!,
                        SelectedContainer,
                        (number).ToString().PadLeft(YoutubeDownloader.Utils.AppConsts.LenNumber, '0')
                    ));    

            downloads.Add(
                _viewModelFactory.CreateDownloadViewModel(
                    video,
                    new VideoDownloadPreference(SelectedContainer, SelectedVideoQualityPreference),
                    filePath
                )
            );
        }

        _settingsService.LastContainer = SelectedContainer;
        _settingsService.LastVideoQualityPreference = SelectedVideoQualityPreference;

        Close(downloads);
        Database.Save();
    }
}

public static class DownloadMultipleSetupViewModelExtensions
{
    public static DownloadMultipleSetupViewModel CreateDownloadMultipleSetupViewModel(
        this IViewModelFactory factory,
        string title,
        IReadOnlyList<IVideo> availableVideos,
        bool preselectVideos = true)
    {
        var viewModel = factory.CreateDownloadMultipleSetupViewModel();

        viewModel.Title = title;
        viewModel.AvailableVideos = availableVideos;
        viewModel.SelectedVideos = preselectVideos
            ? availableVideos
            : Array.Empty<IVideo>();

        return viewModel;
    }
}