using System;
using System.Linq;
using System.Threading.Tasks;
using Gress;
using Gress.Completable;
using Stylet;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Core.Resolving;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Components;

public class DashboardViewModel : PropertyChangedBase
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;

    private readonly AutoResetProgressMuxer _progressMuxer;
    private readonly ResizableSemaphore _downloadSemaphore = new();

    private readonly QueryResolver _queryResolver = new();
    private readonly VideoDownloader _videoDownloader = new();

    public bool IsBusy { get; private set; }

    public ProgressContainer<Percentage> Progress { get; } = new();

    public bool IsProgressIndeterminate => IsBusy && Progress.Current.Fraction is <= 0 or >= 1;

    public string? Query { get; set; }

    public BindableCollection<DownloadViewModel> Downloads { get; } = new();

    public DashboardViewModel(
        IViewModelFactory viewModelFactory,
        DialogManager dialogManager,
        SettingsService settingsService)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;

        _progressMuxer = Progress.CreateMuxer().WithAutoReset();

        settingsService.BindAndInvoke(o => o.ParallelLimit, (_, e) => _downloadSemaphore.MaxCount = e.NewValue);
        Progress.Bind(o => o.Current, (_, _) => NotifyOfPropertyChange(() => IsProgressIndeterminate));
    }

    public bool CanShowSettings => !IsBusy;

    public async void ShowSettings() => await _dialogManager.ShowDialogAsync(
        _viewModelFactory.CreateSettingsViewModel()
    );

    private void EnqueueDownload(IVideo video, string filePath, VideoDownloadOption downloadOption, int position = 0)
    {
        var download = _viewModelFactory.CreateDownloadViewModel(video, downloadOption, filePath);
        var progress = _progressMuxer.CreateInput();

        Task.Run(async () =>
        {
            try
            {
                download.Status = DownloadStatus.Started;

                await _videoDownloader.DownloadAsync(
                    filePath,
                    video,
                    downloadOption,
                    download.Progress.Merge(progress),
                    download.CancellationToken
                );

                download.Status = DownloadStatus.Completed;
            }
            catch (OperationCanceledException)
            {
                download.Status = DownloadStatus.Canceled;
            }
            catch (Exception ex)
            {
                download.Status = DownloadStatus.Failed;

                // Short error message for YouTube-related errors, full for others
                download.ErrorMessage = ex is YoutubeExplodeException
                    ? ex.Message
                    : ex.ToString();
            }
            finally
            {
                progress.ReportCompletion();
            }
        });

        Downloads.Insert(position, download);
    }

    public bool CanProcessQuery => !IsBusy && !string.IsNullOrWhiteSpace(Query);

    public async void ProcessQuery()
    {
        if (string.IsNullOrWhiteSpace(Query))
            return;

        IsBusy = true;

        // Small weight to not offset any existing download operations
        var progress = _progressMuxer.CreateInput(0.01);

        try
        {
            var queryResult = await _queryResolver.QueryAsync(Query.Split(Environment.NewLine), progress);
            var videos = queryResult.Videos.ToArray();

            // No videos
            if (videos.Length <= 0)
            {
                await _dialogManager.ShowDialogAsync(
                    _viewModelFactory.CreateMessageBoxViewModel(
                        "Nothing found",
                        "Couldn't find any videos based on the query or URL you provided"
                    )
                );
            }

            // Single video
            else if (videos.Length == 1)
            {
                var video = videos.Single();
                var downloadOptions = await _videoDownloader.GetDownloadOptionsAsync(video.Id);

                var dialog = _viewModelFactory.CreateDownloadSingleSetupViewModel(video, downloadOptions);
                if (await _dialogManager.ShowDialogAsync(dialog) is null)
                    return;

                EnqueueDownload(video, dialog.FilePath!, dialog.SelectedDownloadOption!);
            }

            // Multiple videos
            else
            {

            }
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelFactory.CreateMessageBoxViewModel(
                    "Error",
                    // Short error message for YouTube-related errors, full for others
                    ex is YoutubeExplodeException
                        ? ex.Message
                        : ex.ToString()
                )
            );
        }
        finally
        {
            progress.ReportCompletion();
            IsBusy = false;
        }
    }

    public void RemoveDownload(DownloadViewModel download)
    {
        download.Cancel();
        Downloads.Remove(download);
    }

    public void RemoveSuccessfulDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (download.Status == DownloadStatus.Completed)
                RemoveDownload(download);
        }
    }

    public void RemoveInactiveDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (download.Status is DownloadStatus.Completed or DownloadStatus.Failed or DownloadStatus.Canceled)
                RemoveDownload(download);
        }
    }

    public void RestartDownload(DownloadViewModel download)
    {
        var position = Math.Max(0, Downloads.IndexOf(download));
        RemoveDownload(download);
        EnqueueDownload(download.Video!, download.FilePath!, download.DownloadOption!, position);
    }

    public void RestartFailedDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (download.Status == DownloadStatus.Failed)
                RestartDownload(download);
        }
    }

    public void CancelAllDownloads()
    {
        foreach (var download in Downloads)
            download.Cancel();
    }
}