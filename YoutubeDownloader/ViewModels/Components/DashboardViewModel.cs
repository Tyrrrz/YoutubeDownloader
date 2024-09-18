﻿using System;
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
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    private readonly DisposableCollector _eventRoot = new();
    private readonly ResizableSemaphore _downloadSemaphore = new();
    private readonly AutoResetProgressMuxer _progressMuxer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProgressIndeterminate))]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowAuthSetupCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowSettingsCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    private string? _query;

    public DashboardViewModel(
        ViewModelManager viewModelManager,
        DialogManager dialogManager,
        SettingsService settingsService
    )
    {
        _viewModelManager = viewModelManager;
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

    public ProgressContainer<Percentage> Progress { get; } = new();

    public ObservableCollection<DownloadViewModel> Downloads { get; } = [];

    public bool IsProgressIndeterminate => IsBusy && Progress.Current.Fraction is <= 0 or >= 1;

    private bool CanShowAuthSetup() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanShowAuthSetup))]
    private async Task ShowAuthSetupAsync() =>
        await _dialogManager.ShowDialogAsync(_viewModelManager.CreateAuthSetupViewModel());

    private bool CanShowSettings() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanShowSettings))]
    private async Task ShowSettingsAsync() =>
        await _dialogManager.ShowDialogAsync(_viewModelManager.CreateSettingsViewModel());

    private async void EnqueueDownload(DownloadViewModel download, int position = 0)
    {
        Downloads.Insert(position, download);
        var progress = _progressMuxer.CreateInput();

        try
        {
            var downloader = new VideoDownloader(_settingsService.LastAuthCookies);
            var tagInjector = new MediaTagInjector();

            using var access = await _downloadSemaphore.AcquireAsync(download.CancellationToken);

            download.Status = DownloadStatus.Started;

            var downloadOption =
                download.DownloadOption
                ?? await downloader.GetBestDownloadOptionAsync(
                    download.Video!.Id,
                    download.DownloadPreference!,
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

                await tagInjector.InjectTagsAsync(
                    download.FilePath!,
                    download.Video!,
                    download.CancellationToken
                );
            }

            download.Status = DownloadStatus.Completed;
        }
        catch (Exception ex)
        {
            try
            {
                // Delete the incompletely downloaded file
                if (!string.IsNullOrWhiteSpace(download.FilePath))
                    File.Delete(download.FilePath);
            }
            catch
            {
                // Ignore
            }

            download.Status =
                ex is OperationCanceledException ? DownloadStatus.Canceled : DownloadStatus.Failed;

            // Short error message for YouTube-related errors, full for others
            download.ErrorMessage = ex is YoutubeExplodeException ? ex.Message : ex.ToString();
        }
        finally
        {
            progress.ReportCompletion();
            download.Dispose();
        }
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
            var resolver = new QueryResolver(_settingsService.LastAuthCookies);
            var downloader = new VideoDownloader(_settingsService.LastAuthCookies);

            var result = await resolver.ResolveAsync(
                Query.Split(
                    "\n",
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                ),
                progress
            );

            // Single video
            if (result.Videos.Count == 1)
            {
                var video = result.Videos.Single();
                var downloadOptions = await downloader.GetDownloadOptionsAsync(video.Id);

                var download = await _dialogManager.ShowDialogAsync(
                    _viewModelManager.CreateDownloadSingleSetupViewModel(video, downloadOptions)
                );

                if (download is null)
                    return;

                EnqueueDownload(download);
            }
            // Multiple videos
            else if (result.Videos.Count > 1)
            {
                var downloads = await _dialogManager.ShowDialogAsync(
                    _viewModelManager.CreateDownloadMultipleSetupViewModel(
                        result.Title,
                        result.Videos,
                        // Pre-select videos if they come from a single query and not from search
                        result.Kind
                            is not QueryResultKind.Search
                                and not QueryResultKind.Aggregate
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
                    _viewModelManager.CreateMessageBoxViewModel(
                        "Nothing found",
                        "Couldn't find any videos based on the query or URL you provided"
                    )
                );
            }
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
