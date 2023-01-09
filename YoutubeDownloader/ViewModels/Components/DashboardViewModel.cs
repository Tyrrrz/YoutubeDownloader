using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gress;
using Gress.Completable;
using Stylet;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Core.Resolving;
using YoutubeDownloader.Core.Tagging;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Exceptions;

namespace YoutubeDownloader.ViewModels.Components;

public class DashboardViewModel : PropertyChangedBase, IDisposable
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    private readonly AutoResetProgressMuxer _progressMuxer;
    private readonly ResizableSemaphore _downloadSemaphore = new();

    private readonly QueryResolver _queryResolver = new();
    private readonly VideoDownloader _videoDownloader = new();
    private readonly MediaTagInjector _mediaTagInjector = new();

    public bool IsBusy { get; private set; }

    public ProgressContainer<Percentage> Progress { get; } = new();

    public bool IsProgressIndeterminate => IsBusy && Progress.Current.Fraction is <= 0 or >= 1;

    public string? Query { get; set; }

    public BindableCollection<DownloadViewModel> Downloads { get; } = new();

    public bool IsDownloadsAvailable => Downloads.Any();

    public DashboardViewModel(
        IViewModelFactory viewModelFactory,
        DialogManager dialogManager,
        SettingsService settingsService)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        _settingsService = settingsService;

        _progressMuxer = Progress.CreateMuxer().WithAutoReset();

        _settingsService.BindAndInvoke(o => o.ParallelLimit, (_, e) => _downloadSemaphore.MaxCount = e.NewValue);
        Progress.Bind(o => o.Current, (_, _) => NotifyOfPropertyChange(() => IsProgressIndeterminate));
        Downloads.Bind(o => o.Count, (_, _) => NotifyOfPropertyChange(() => IsDownloadsAvailable));
    }

    public bool CanShowSettings => !IsBusy;

    public async void ShowSettings() => await _dialogManager.ShowDialogAsync(
        _viewModelFactory.CreateSettingsViewModel()
    );

    private void EnqueueDownload(DownloadViewModel download, int position = 0)
    {
        var progress = _progressMuxer.CreateInput();

        Task.Run(async () =>
        {
            try
            {
                using var access = await _downloadSemaphore.AcquireAsync(download.CancellationToken);

                download.Status = DownloadStatus.Started;

                var downloadOption =
                    download.DownloadOption ??
                    await _videoDownloader.GetBestDownloadOptionAsync(
                        download.Video!.Id,
                        download.DownloadPreference!,
                        download.CancellationToken
                    );

                await _videoDownloader.DownloadVideoAsync(
                    download.FilePath!,
                    download.Video!,
                    downloadOption,
                    download.Progress.Merge(progress),
                    download.CancellationToken
                );

                if (_settingsService.ShouldInjectTags)
                {
                    try
                    {
                        await _mediaTagInjector.InjectTagsAsync(
                            download.FilePath!,
                            download.Video!,
                            download.CancellationToken
                        );
                    }
                    catch
                    {
                        // Media tagging is not critical
                    }
                }

                download.Status = DownloadStatus.Completed;
            }
            catch (Exception ex)
            {
                try
                {
                    // Delete incompletely downloaded file
                    File.Delete(download.FilePath!);
                }
                catch
                {
                    // Ignore
                }

                download.Status = ex is OperationCanceledException
                    ? DownloadStatus.Canceled
                    : DownloadStatus.Failed;

                // Short error message for YouTube-related errors, full for others
                download.ErrorMessage = ex is YoutubeExplodeException
                    ? ex.Message
                    : ex.ToString();
            }
            finally
            {
                progress.ReportCompletion();
                download.Dispose();
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
            var result = await _queryResolver.ResolveAsync(
                Query.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                progress
            );

            // Single video
            if (result.Videos.Count == 1)
            {
                var video = result.Videos.Single();
                var downloadOptions = await _videoDownloader.GetDownloadOptionsAsync(video.Id);

                var download = await _dialogManager.ShowDialogAsync(
                    _viewModelFactory.CreateDownloadSingleSetupViewModel(video, downloadOptions)
                );

                if (download is null)
                    return;

                EnqueueDownload(download);
            }
            // Multiple videos
            else if (result.Videos.Count > 1)
            {
                var downloads = await _dialogManager.ShowDialogAsync(
                    _viewModelFactory.CreateDownloadMultipleSetupViewModel(
                        result.Title,
                        result.Videos,
                        // Pre-select videos if they come from a single query and not from search
                        result.Kind is not QueryResultKind.Search and not QueryResultKind.Aggregate
                    )
                );

                if (downloads is null)
                    return;

                foreach (var download in downloads)
                    EnqueueDownload(download);
            }
            // No videos found
            else
            {
                await _dialogManager.ShowDialogAsync(
                    _viewModelFactory.CreateMessageBoxViewModel(
                        "Nothing found",
                        "Couldn't find any videos based on the query or URL you provided"
                    )
                );
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
        Downloads.Remove(download);
        download.Cancel();
        download.Dispose();
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

        var newDownload = download.DownloadOption is not null
            ? _viewModelFactory.CreateDownloadViewModel(
                download.Video!,
                download.DownloadOption,
                download.FilePath!
            )
            : _viewModelFactory.CreateDownloadViewModel(
                download.Video!,
                download.DownloadPreference!,
                download.FilePath!
            );

        EnqueueDownload(newDownload, position);
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

    public void Dispose()
    {
        _downloadSemaphore.Dispose();
    }
}