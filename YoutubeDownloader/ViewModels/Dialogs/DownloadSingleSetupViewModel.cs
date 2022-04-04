using System.Collections.Generic;
using System.Linq;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class DownloadSingleSetupViewModel : DialogScreen
{
    private readonly DialogManager _dialogManager;

    public IVideo? Video { get; set; }

    public IReadOnlyList<VideoDownloadOption>? AvailableDownloadOptions { get; set; }

    public VideoDownloadOption? SelectedDownloadOption { get; set; }

    public string? FilePath { get; set; }

    public DownloadSingleSetupViewModel(DialogManager dialogManager)
    {
        _dialogManager = dialogManager;
    }

    public void OpenVideo()
    {
        var url = Video?.Url;
        if (!string.IsNullOrEmpty(url))
            ProcessEx.StartShellExecute(url);
    }

    public bool CanConfirm => SelectedDownloadOption is not null;

    public void Confirm()
    {
        if (Video is null || SelectedDownloadOption is null)
            return;

        FilePath = _dialogManager.PromptSaveFilePath(
            $"{SelectedDownloadOption.Container.Name} file|*.{SelectedDownloadOption.Container.Name}",
            PathEx.EscapeFileName(Video.Title + '.' + SelectedDownloadOption.Container.Name)
        );

        if (string.IsNullOrWhiteSpace(FilePath))
            return;

        Close(true);
    }
}

public static class DownloadSingleSetupViewModelExtensions
{
    public static DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel(
        this IViewModelFactory factory,
        IVideo video,
        IReadOnlyList<VideoDownloadOption> downloadOptions)
    {
        var viewModel = factory.CreateDownloadSetupViewModel();

        viewModel.Video = video;
        viewModel.AvailableDownloadOptions = downloadOptions;
        viewModel.SelectedDownloadOption = downloadOptions.FirstOrDefault();

        return viewModel;
    }
}