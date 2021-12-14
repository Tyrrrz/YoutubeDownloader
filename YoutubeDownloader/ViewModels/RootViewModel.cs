using System;
using System.Linq;
using System.Threading.Tasks;
using Gress;
using MaterialDesignThemes.Wpf;
using Stylet;
using Tyrrrz.Extensions;
using YoutubeDownloader.Core;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Exceptions;

namespace YoutubeDownloader.ViewModels;

public class RootViewModel : Screen
{
    private readonly ResizableSemaphore _downloadSemaphore = new();

    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;
    private readonly UpdateService _updateService;

    public ISnackbarMessageQueue Notifications { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));

    public IProgressManager ProgressManager { get; } = new ProgressManager();

    public bool IsBusy { get; private set; }

    public bool IsProgressIndeterminate => ProgressManager.IsActive && ProgressManager.Progress is <= 0 or >= 1;

    public string? Query { get; set; }

    public BindableCollection<DownloadViewModel> Downloads { get; } = new();

    public RootViewModel(
        IViewModelFactory viewModelFactory,
        DialogManager dialogManager,
        SettingsService settingsService,
        UpdateService updateService)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        _settingsService = settingsService;
        _updateService = updateService;

        // Title
        DisplayName = $"{App.Name} v{App.VersionString}";

        // Update semaphore capacity when settings change
        settingsService.BindAndInvoke(
            o => o.MaxConcurrentDownloadCount,
            (_, e) => _downloadSemaphore.MaxCount = e.NewValue
        );

        // Update busy state when progress manager changes
        ProgressManager.Bind(
            o => o.IsActive,
            (_, _) => NotifyOfPropertyChange(() => IsProgressIndeterminate)
        );

        ProgressManager.Bind(
            o => o.Progress,
            (_, _) => NotifyOfPropertyChange(() => IsProgressIndeterminate)
        );
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            // Check for updates
            var updateVersion = await _updateService.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            // Notify user of an update and prepare it
            Notifications.Enqueue($"Downloading update to {App.Name} v{updateVersion}...");
            await _updateService.PrepareUpdateAsync(updateVersion);

            // Prompt user to install update (otherwise install it when application exits)
            Notifications.Enqueue(
                "Update has been downloaded and will be installed when you exit",
                "INSTALL NOW", () =>
                {
                    _updateService.FinalizeUpdate(true);
                    RequestClose();
                }
            );
        }
        catch
        {
            // Failure to update shouldn't crash the application
            Notifications.Enqueue("Failed to perform application update");
        }
    }

    protected override async void OnViewLoaded()
    {
        base.OnViewLoaded();

        _settingsService.Load();

        if (_settingsService.IsDarkModeEnabled)
        {
            App.SetDarkTheme();
        }
        else
        {
            App.SetLightTheme();
        }

        await CheckForUpdatesAsync();
    }

    protected override void OnClose()
    {
        base.OnClose();

        _settingsService.Save();

        // Cancel all downloads
        foreach (var download in Downloads)
            download.Cancel();

        _updateService.FinalizeUpdate(false);
    }

    public bool CanShowSettings => !IsBusy;

    public async void ShowSettings()
    {
        var dialog = _viewModelFactory.CreateSettingsViewModel();
        await _dialogManager.ShowDialogAsync(dialog);
    }

    private void EnqueueDownload(DownloadViewModel download)
    {
        // Cancel and remove downloads with the same file path
        var existingDownloads = Downloads.Where(d => d.FilePath == download.FilePath).ToArray();
        foreach (var existingDownload in existingDownloads)
        {
            existingDownload.Cancel();
            Downloads.Remove(existingDownload);
        }

        // Bind progress manager
        download.ProgressManager = ProgressManager;
        download.Start();

        Downloads.Insert(0, download);
    }

    public bool CanProcessQuery => !IsBusy && !string.IsNullOrWhiteSpace(Query);

    public async void ProcessQuery()
    {
        if (string.IsNullOrWhiteSpace(Query))
            return;

        // Small operation weight to not offset any existing download operations
        using var operation = ProgressManager.CreateOperation(0.01);

        IsBusy = true;

        try
        {
            var results = await YoutubeQuery.ExecuteAsync(Query.Split(Environment.NewLine), operation);

            var videos = results.SelectMany(q => q.Videos).Distinct(v => v.Id).ToArray();

            var dialogTitle = results.Count == 1
                ? results.Single().Label
                : "Multiple queries";

            // No videos found
            if (videos.Length <= 0)
            {
                var dialog = _viewModelFactory.CreateMessageBoxViewModel(
                    "Nothing found",
                    "Couldn't find any videos based on the query or URL you provided"
                );

                await _dialogManager.ShowDialogAsync(dialog);
            }

            // Single video
            else if (videos.Length == 1)
            {
                var video = videos.Single();

                var downloadOptions = await video.GetDownloadOptionsAsync();
                var subtitleOptions = await video.GetSubtitleDownloadOptionsAsync();

                var dialog = _viewModelFactory.CreateDownloadSingleSetupViewModel(
                    dialogTitle,
                    video,
                    downloadOptions,
                    subtitleOptions
                );

                var download = await _dialogManager.ShowDialogAsync(dialog);
                if (download is null)
                    return;

                EnqueueDownload(download);
            }

            // Multiple videos
            else
            {
                var dialog = _viewModelFactory.CreateDownloadMultipleSetupViewModel(dialogTitle, videos);

                // Preselect all videos (unless it's a search query, then don't)
                if (results.All(q => q.Kind != YoutubeQueryKind.Search))
                    dialog.SelectedVideos = dialog.AvailableVideos;

                var downloads = await _dialogManager.ShowDialogAsync(dialog);
                if (downloads is null)
                    return;

                foreach (var download in downloads)
                    EnqueueDownload(download);
            }
        }
        catch (Exception ex)
        {
            var dialog = _viewModelFactory.CreateMessageBoxViewModel(
                "Error",
                // Short error message for YouTube-related errors, full for others
                ex is YoutubeExplodeException
                    ? ex.Message
                    : ex.ToString()
            );

            await _dialogManager.ShowDialogAsync(dialog);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void RemoveDownload(DownloadViewModel download)
    {
        download.Cancel();
        Downloads.Remove(download);
    }

    public void RemoveInactiveDownloads() =>
        Downloads.RemoveAll(d => !d.IsActive);

    public void RemoveSuccessfulDownloads() =>
        Downloads.RemoveAll(d => d.IsSuccessful);

    public void RestartFailedDownloads()
    {
        foreach (var download in Downloads)
        {
            if (download.IsFailed)
                download.Restart();
        }
    }
}