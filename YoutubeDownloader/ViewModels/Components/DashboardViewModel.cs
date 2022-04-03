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
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Components;

public class DashboardViewModel : PropertyChangedBase
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

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
        _settingsService = settingsService;

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
        var download = _viewModelFactory.CreateDownloadViewModel(video, filePath);
        Downloads.Insert(position, download);

        var progress = _progressMuxer.CreateInput();
        var mergedProgress = progress.Merge(download.Progress);

        Task.Run(async () =>
        {
            download.Status = DownloadStatus.Started;

            try
            {
                await _videoDownloader.DownloadAsync(
                    filePath,
                    video,
                    downloadOption,
                    mergedProgress,
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

            // No videos found
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

                var videoOptions = await _videoDownloader.GetDownloadOptionsAsync(video.Id);

                var dialog = _viewModelFactory.CreateDownloadSingleSetupViewModel(video, videoOptions);
                if (await _dialogManager.ShowDialogAsync(dialog) is null)
                    return;
                
                EnqueueDownload(video, dialog.FilePath!, dialog.SelectedDownloadOption!);
            }

            // Multiple videos
            else
            {
                /*
                var downloads = await _dialogManager.ShowDialogAsync(
                    _viewModelFactory.CreateDownloadMultipleSetupViewModel(
                        queryResult.Label,
                        videos,
                        // Preselect all videos if this is not a search query
                        queryResult.Kind != YoutubeQueryKind.Search
                    )
                );

                if (downloads is null)
                    return;

                foreach (var download in downloads)
                    EnqueueDownload(download);
                    */
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

    public void RemoveInactiveDownloads() =>
        Downloads.RemoveAll(d =>
            d.Status is DownloadStatus.Completed or DownloadStatus.Failed or DownloadStatus.Canceled
        );

    public void RemoveSuccessfulDownloads() =>
        Downloads.RemoveAll(d => d.Status == DownloadStatus.Completed);

    public void RestartFailedDownloads()
    {
        foreach (var download in Downloads)
        {
            if (download.Status == DownloadStatus.Failed)
            {
                throw new NotImplementedException();
                //EnqueueDownload(download.Request!, Downloads.IndexOf(download));
            }
        }
    }
    
    public void CancelAllDownloads()
    {
        foreach (var download in Downloads)
            download.Cancel();
    }
}