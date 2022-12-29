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

namespace YoutubeDownloader.ViewModels.Dialogs;

public class DownloadSingleSetupViewModel : DialogScreen<DownloadViewModel>
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    public IVideo? Video { get; set; }

    public IReadOnlyList<VideoDownloadOption>? AvailableDownloadOptions { get; set; }

    public VideoDownloadOption? SelectedDownloadOption { get; set; }

    public DownloadSingleSetupViewModel(
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
        SelectedDownloadOption = AvailableDownloadOptions?.FirstOrDefault(o =>
            o.Container == _settingsService.LastContainer
        );
    }

    public void CopyTitle() => Clipboard.SetText(Video!.Title);

    public void Confirm()
    {
        var dirPath = _dialogManager.PromptDirectoryPath();
        if (string.IsNullOrWhiteSpace(dirPath))
            return;
        var container = SelectedDownloadOption!.Container;
        Database.Load(dirPath);
        VideoInfo? videoInfo = Database.Find(Video!.Id);
        int number;
        if(videoInfo == null){
            Database.InsertOrUpdate(new VideoInfo(0, Video!.Title, Video!.Id, "",""));
            number = Database.Count();
        }else{
            // already exist, get it
            number = videoInfo.Number;
        }

        var filePath = Path.Combine(
                dirPath,
                FileNameTemplate.Apply(
                    _settingsService.FileNameTemplate,
                    Video!,
                    container,
                    (number).ToString().PadLeft(YoutubeDownloader.Utils.AppConsts.LenNumber, '0')
                ));   

        _settingsService.LastContainer = container;

        Close(
            _viewModelFactory.CreateDownloadViewModel(Video!, SelectedDownloadOption!, filePath)
        );
        Database.Save();
    }
}

public static class DownloadSingleSetupViewModelExtensions
{
    public static DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel(
        this IViewModelFactory factory,
        IVideo video,
        IReadOnlyList<VideoDownloadOption> availableDownloadOptions)
    {
        var viewModel = factory.CreateDownloadSingleSetupViewModel();

        viewModel.Video = video;
        viewModel.AvailableDownloadOptions = availableDownloadOptions;

        return viewModel;
    }
}