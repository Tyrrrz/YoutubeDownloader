using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gress;
using Gress.Completable;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Core.Resolving;
using YoutubeDownloader.Core.Tagging;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeExplode.Exceptions;

namespace YoutubeDownloader.ViewModels.Components;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly ViewModelManager _viewModelManager;
    private readonly SnackbarManager _snackbarManager;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    private readonly DisposableCollector _eventRoot = new();
    private readonly ResizableSemaphore _downloadSemaphore = new();
    private readonly AutoResetProgressMuxer _progressMuxer;

    public static event EventHandler<bool>? DownloadingStatusChanged;

    public DashboardViewModel(
        ViewModelManager viewModelManager,
        SnackbarManager snackbarManager,
        DialogManager dialogManager,
        SettingsService settingsService
    )
    {
        _viewModelManager = viewModelManager;
        _snackbarManager = snackbarManager;
        _dialogManager = dialogManager;
        _settingsService = settingsService;

        _progressMuxer = Progress.CreateMuxer().WithAutoReset();

        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.ParallelLimit,
                () => _downloadSemaphore.MaxCount = _settingsService.ParallelLimit,
                true
            )
        );

        _eventRoot.Add(
            Progress.WatchProperty(
                o => o.Current,
                () => OnPropertyChanged(nameof(IsProgressIndeterminate))
            )
        );
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProgressIndeterminate))]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowAuthSetupCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowSettingsCommand))]
    public partial bool IsBusy { get; set; }

    public ProgressContainer<Percentage> Progress { get; } = new();

    public bool IsProgressIndeterminate => IsBusy && Progress.Current.Fraction is <= 0 or >= 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    public partial string? Query { get; set; }

    public ObservableCollection<DownloadViewModel> Downloads { get; } = [];

    public static bool IsDownloading { get; private set; }

    private static void UpdateDownloadingStatus(bool isDownloading)
    {
        if (IsDownloading != isDownloading)
        {
            IsDownloading = isDownloading;
            DownloadingStatusChanged?.Invoke(null, isDownloading);
        }
    }

    private void CheckAndUpdateDownloadingStatus()
    {
        var hasActiveDownloads = Downloads.Any(d =>
            d.Status == DownloadStatus.Enqueued || d.Status == DownloadStatus.Started
        );

        UpdateDownloadingStatus(hasActiveDownloads);
    }

    private bool CanShowAuthSetup() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanShowAuthSetup))]
    private async Task ShowAuthSetupAsync() =>
        await _dialogManager.ShowDialogAsync(_viewModelManager.CreateAuthSetupViewModel());

    private bool CanShowSettings() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanShowSettings))]
    private async Task ShowSettingsAsync() =>
        await _dialogManager.ShowDialogAsync(_viewModelManager.CreateSettingsViewModel());

    private void EnqueueDownload(DownloadViewModel download, int position = 0)
    {
        Downloads.Insert(position, download);
        CheckAndUpdateDownloadingStatus(); // Update status when download is enqueued

        var progress = _progressMuxer.CreateInput();

        _ = Task.Run(async () =>
        {
            try
            {
                var downloader = new VideoDownloader(_settingsService.LastAuthCookies);
                var tagInjector = new MediaTagInjector();

                using var access = await _downloadSemaphore.AcquireAsync(
                    download.CancellationToken
                );

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    download.Status = DownloadStatus.Started;
                    CheckAndUpdateDownloadingStatus();
                });

                var downloadOption =
                    download.DownloadOption
                    ?? await downloader.GetBestDownloadOptionAsync(
                        download.Video!.Id,
                        download.DownloadPreference!,
                        _settingsService.ShouldInjectLanguageSpecificAudioStreams,
                        download.CancellationToken
                    );

                await downloader.DownloadVideoAsync(
                    download.FilePath!,
                    download.Video!,
                    downloadOption,
                    _settingsService.ShouldInjectSubtitles,
                    download.Progress.Merge(progress),
                    download.CancellationToken
                );

                if (_settingsService.ShouldInjectTags)
                {
                    try
                    {
                        await tagInjector.InjectTagsAsync(
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

                if (OperatingSystem.IsAndroid())
                {
                    bool moveSuccessful = await AndroidDownloadingFiles.MoveDownloadedFileAsync(
                        download.FilePath!
                    );
                    if (!moveSuccessful)
                    {
                        throw new IOException("Failed to move downloaded file to final location");
                    }
                }

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    download.Status = DownloadStatus.Completed;
                    CheckAndUpdateDownloadingStatus(); // Update when completed
                });
            }
            catch (Exception ex)
            {
                try
                {
                    // Clean up the incompletely downloaded file
                    if (!string.IsNullOrWhiteSpace(download.FilePath))
                    {
                        if (OperatingSystem.IsAndroid())
                        {
                            AndroidDownloadingFiles.CleanupPendingFile(download.FilePath);
                        }
                        else
                        {
                            File.Delete(download.FilePath);
                        }
                    }
                }
                catch
                {
                    // Ignore
                }

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    download.Status =
                        ex is OperationCanceledException
                            ? DownloadStatus.Canceled
                            : DownloadStatus.Failed;

                    download.ErrorMessage =
                        ex is YoutubeExplodeException ? ex.Message : ex.ToString();

                    CheckAndUpdateDownloadingStatus();
                });
            }
            finally
            {
                progress.ReportCompletion();

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    download.Dispose();
                });
            }
        });
    }

    private bool CanProcessQuery() => !IsBusy && !string.IsNullOrWhiteSpace(Query);

    [RelayCommand(CanExecute = nameof(CanProcessQuery))]
    private async Task ProcessQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
            return;

        IsBusy = true;

        // Small weight so as to not offset any existing download operations
        var progress = _progressMuxer.CreateInput(0.01);

        try
        {
            // Move all network operations to background thread
            await Task.Run(async () =>
            {
                var resolver = new QueryResolver(_settingsService.LastAuthCookies);
                var downloader = new VideoDownloader(_settingsService.LastAuthCookies);

                // Split queries by newlines
                var queries = Query.Split(
                    '\n',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                );

                // Process individual queries
                var queryResults = new List<QueryResult>();
                foreach (var (i, query) in queries.Index())
                {
                    try
                    {
                        queryResults.Add(await resolver.ResolveAsync(query));
                    }
                    // If it's not the only query in the list, don't interrupt the process
                    // and report the error via an async notification instead of a sync dialog.
                    // https://github.com/Tyrrrz/YoutubeDownloader/issues/563
                    catch (YoutubeExplodeException ex)
                        when (ex is VideoUnavailableException or PlaylistUnavailableException
                            && queries.Length > 1
                        )
                    {
                        // Schedule snackbar notification on UI thread
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            _snackbarManager.Notify(ex.Message)
                        );
                    }

                    progress.Report(Percentage.FromFraction((i + 1.0) / queries.Length));
                }

                // Aggregate results
                var queryResult = QueryResult.Aggregate(queryResults);

                // Switch back to UI thread for UI operations
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Single video result
                    if (queryResult.Videos.Count == 1)
                    {
                        var video = queryResult.Videos.Single();

                        // Get download options on background thread
                        var downloadOptions = await Task.Run(async () =>
                            await downloader.GetDownloadOptionsAsync(
                                video.Id,
                                _settingsService.ShouldInjectLanguageSpecificAudioStreams
                            )
                        );

                        var download = await _dialogManager.ShowDialogAsync(
                            _viewModelManager.CreateDownloadSingleSetupViewModel(
                                video,
                                downloadOptions
                            )
                        );

                        if (download is null)
                            return;

                        EnqueueDownload(download);

                        Query = "";
                    }
                    // Multiple videos
                    else if (queryResult.Videos.Count > 1)
                    {
                        var downloads = await _dialogManager.ShowDialogAsync(
                            _viewModelManager.CreateDownloadMultipleSetupViewModel(
                                queryResult.Title,
                                queryResult.Videos,
                                // Pre-select videos if they come from a single query and not from search
                                queryResult.Kind
                                    is not QueryResultKind.Search
                                        and not QueryResultKind.Aggregate
                            )
                        );

                        if (downloads is null)
                            return;

                        foreach (var download in downloads)
                            EnqueueDownload(download);

                        Query = "";
                    }
                    // No videos found
                    else
                    {
                        await _dialogManager.ShowDialogAsync(
                            _viewModelManager.CreateMessageBoxViewModel(
                                "Nothing found",
                                "Couldn't find any videos based on the query or URL you provided"
                            )
                        );
                    }
                });
            });
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelManager.CreateMessageBoxViewModel(
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

    private void RemoveDownload(DownloadViewModel download)
    {
        Downloads.Remove(download);
        download.CancelCommand.Execute(null);
        download.Dispose();
        CheckAndUpdateDownloadingStatus();
    }

    [RelayCommand]
    private void RemoveSuccessfulDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (download.Status == DownloadStatus.Completed)
                RemoveDownload(download);
        }
    }

    [RelayCommand]
    private void RemoveInactiveDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (
                download.Status
                is DownloadStatus.Completed
                    or DownloadStatus.Failed
                    or DownloadStatus.Canceled
            )
                RemoveDownload(download);
        }
    }

    [RelayCommand]
    private void RestartDownload(DownloadViewModel download)
    {
        var position = Math.Max(0, Downloads.IndexOf(download));
        RemoveDownload(download);

        var newDownload = download.DownloadOption is not null
            ? _viewModelManager.CreateDownloadViewModel(
                download.Video!,
                download.DownloadOption,
                download.FilePath!
            )
            : _viewModelManager.CreateDownloadViewModel(
                download.Video!,
                download.DownloadPreference!,
                download.FilePath!
            );

        EnqueueDownload(newDownload, position);
    }

    [RelayCommand]
    private void RestartFailedDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (download.Status == DownloadStatus.Failed)
                RestartDownload(download);
        }
    }

    [RelayCommand]
    private void CancelAllDownloads()
    {
        foreach (var download in Downloads)
            download.CancelCommand.Execute(null);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CancelAllDownloads();

            _eventRoot.Dispose();
            _downloadSemaphore.Dispose();
        }

        base.Dispose(disposing);
    }
}
