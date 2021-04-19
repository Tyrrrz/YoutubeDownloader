using System;
using System.Linq;
using System.Threading.Tasks;
using Gress;
using MaterialDesignThemes.Wpf;
using Stylet;
using Tyrrrz.Extensions;
using YoutubeDownloader.Models;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Exceptions;

namespace YoutubeDownloader.ViewModels
{
    public class RootViewModel : Screen
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly DialogManager _dialogManager;
        private readonly SettingsService _settingsService;
        private readonly UpdateService _updateService;
        private readonly QueryService _queryService;
        private readonly DownloadService _downloadService;

        public ISnackbarMessageQueue Notifications { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));

        public IProgressManager ProgressManager { get; } = new ProgressManager();

        public bool IsBusy { get; private set; }

        public bool IsProgressIndeterminate { get; private set; }

        public string? Query { get; set; }

        public BindableCollection<DownloadViewModel> Downloads { get; } = new();

        public RootViewModel(IViewModelFactory viewModelFactory, DialogManager dialogManager,
            SettingsService settingsService, UpdateService updateService, QueryService queryService,
            DownloadService downloadService)
        {
            _viewModelFactory = viewModelFactory;
            _dialogManager = dialogManager;
            _settingsService = settingsService;
            _updateService = updateService;
            _queryService = queryService;
            _downloadService = downloadService;

            // Title
            DisplayName = $"{App.Name} v{App.VersionString}";

            // Update busy state when progress manager changes
            ProgressManager.Bind(o => o.IsActive,
                (_, _) => IsProgressIndeterminate =
                    ProgressManager.IsActive && ProgressManager.Progress.IsEither(0, 1)
            );

            ProgressManager.Bind(o => o.Progress,
                (_, _) => IsProgressIndeterminate =
                    ProgressManager.IsActive && ProgressManager.Progress.IsEither(0, 1)
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
                    });
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
            // Small operation weight to not offset any existing download operations
            var operation = ProgressManager.CreateOperation(0.01);

            IsBusy = true;

            try
            {
                var parsedQueries = _queryService.ParseMultilineQuery(Query!);
                var executedQueries = await _queryService.ExecuteQueriesAsync(parsedQueries, operation);

                var videos = executedQueries.SelectMany(q => q.Videos).Distinct(v => v.Id).ToArray();

                var dialogTitle = executedQueries.Count == 1
                    ? executedQueries.Single().Title
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

                    var downloadOptions = await _downloadService.GetVideoDownloadOptionsAsync(video.Id);
                    var subtitleOptions = await _downloadService.GetSubtitleDownloadOptionsAsync(video.Id);

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
                    var dialog = _viewModelFactory.CreateDownloadMultipleSetupViewModel(
                        dialogTitle,
                        videos
                    );

                    // Preselect all videos if none of the videos come from a search query
                    if (executedQueries.All(q => q.Query.Kind != QueryKind.Search))
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
                    // Short error message for expected errors, full for unexpected
                    ex is YoutubeExplodeException
                        ? ex.Message
                        : ex.ToString()
                );

                await _dialogManager.ShowDialogAsync(dialog);
            }
            finally
            {
                operation.Dispose();
                IsBusy = false;
            }
        }

        public void RemoveDownload(DownloadViewModel download)
        {
            download.Cancel();
            Downloads.Remove(download);
        }

        public void RemoveInactiveDownloads() =>
            Downloads.RemoveWhere(d => !d.IsActive);

        public void RemoveSuccessfulDownloads() =>
            Downloads.RemoveWhere(d => d.IsSuccessful);

        public void RestartFailedDownloads()
        {
            var failedDownloads = Downloads.Where(d => d.IsFailed).ToArray();
            foreach (var failedDownload in failedDownloads)
                failedDownload.Restart();
        }
    }
}