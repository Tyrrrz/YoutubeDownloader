using System;
using System.Linq;
using System.Threading.Tasks;
using Gress;
using MaterialDesignThemes.Wpf;
using Stylet;
using Tyrrrz.Extensions;
using YoutubeDownloader.Models;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;

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

        public BindableCollection<DownloadViewModel> Downloads { get; } = new BindableCollection<DownloadViewModel>();

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
                (sender, args) => IsProgressIndeterminate = ProgressManager.IsActive && ProgressManager.Progress.IsEither(0, 1));
            ProgressManager.Bind(o => o.Progress,
                (sender, args) => IsProgressIndeterminate = ProgressManager.IsActive && ProgressManager.Progress.IsEither(0, 1));
        }

        private async Task HandleAutoUpdateAsync()
        {
            try
            {
                // Check for updates
                var updateVersion = await _updateService.CheckForUpdatesAsync();
                if (updateVersion == null)
                    return;

                // Notify user of an update and prepare it
                Notifications.Enqueue($"Downloading update to YoutubeDownloader v{updateVersion}...");
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

            // Load settings
            _settingsService.Load();

            // Check and prepare update
            await HandleAutoUpdateAsync();
        }

        protected override void OnClose()
        {
            base.OnClose();

            // Save settings
            _settingsService.Save();

            // Cancel all downloads
            foreach (var download in Downloads)
                download.Cancel();

            // Finalize updates if necessary
            _updateService.FinalizeUpdate(false);
        }

        public bool CanShowSettings => !IsBusy;

        public async void ShowSettings()
        {
            // Create dialog
            var dialog = _viewModelFactory.CreateSettingsViewModel();

            // Show dialog
            await _dialogManager.ShowDialogAsync(dialog);
        }

        private void EnqueueAndStartDownload(DownloadViewModel download)
        {
            // Cancel and remove downloads with the same file path
            var existingDownloads = Downloads.Where(d => d.FilePath == download.FilePath).ToArray();
            foreach (var existingDownload in existingDownloads)
            {
                existingDownload.Cancel();
                Downloads.Remove(existingDownload);
            }

            // Add to list
            Downloads.Insert(0, download);

            // Bind progress manager
            download.ProgressManager = ProgressManager;

            // Start download
            download.Start();
        }

        public bool CanProcessQuery => !IsBusy && !string.IsNullOrWhiteSpace(Query);

        public async void ProcessQuery()
        {
            // Small operation weight to not offset any existing download operations
            var operation = ProgressManager.CreateOperation(0.01);

            // Lock this method for re-entry
            IsBusy = true;

            try
            {
                // Split query into separate lines and parse them
                var parsedQueries = _queryService.ParseMultilineQuery(Query!);

                // Execute separate queries
                var executedQueries = await _queryService.ExecuteQueriesAsync(parsedQueries, operation);

                // Extract videos and other details
                var videos = executedQueries.SelectMany(q => q.Videos).Distinct(v => v.Id).ToArray();
                var dialogTitle = executedQueries.Count == 1 ? executedQueries.Single().Title : "Multiple queries";

                // If no videos were found - tell the user
                if (videos.Length <= 0)
                {
                    // Create dialog
                    var dialog = _viewModelFactory.CreateMessageBoxViewModel("Nothing found",
                        "Couldn't find any videos based on the query or URL you provided");

                    // Show dialog
                    await _dialogManager.ShowDialogAsync(dialog);
                }

                // If only one video was found - show download setup for single video
                else if (videos.Length == 1)
                {
                    // Get single video
                    var video = videos.Single();

                    // Get download options
                    var downloadOptions = await _downloadService.GetDownloadOptionsAsync(video.Id);

                    // Get subtitle options
                    var subtitleOptions = await _downloadService.GetSubtitleOptionsAsync(video.Id);

                    // Create dialog
                    var dialog = _viewModelFactory.CreateDownloadSingleSetupViewModel(dialogTitle, video, downloadOptions, subtitleOptions);

                    // Show dialog and get download
                    var download = await _dialogManager.ShowDialogAsync(dialog);

                    // If canceled - return
                    if (download == null)
                        return;

                    // Enqueue download
                    EnqueueAndStartDownload(download);
                }

                // If multiple videos were found - show download setup for multiple videos
                else
                {
                    // Create dialog
                    var dialog = _viewModelFactory.CreateDownloadMultipleSetupViewModel(dialogTitle, videos);

                    // Preselect all videos if none of the videos come from a search query
                    if (executedQueries.All(q => q.Query.Type != QueryType.Search))
                        dialog.SelectedVideos = dialog.AvailableVideos;

                    // Show dialog and get downloads
                    var downloads = await _dialogManager.ShowDialogAsync(dialog);

                    // If canceled - return
                    if (downloads == null)
                        return;

                    // Enqueue downloads
                    foreach (var download in downloads)
                        EnqueueAndStartDownload(download);
                }
            }
            catch (Exception ex)
            {
                // Create dialog
                var dialog = _viewModelFactory.CreateMessageBoxViewModel("Error", ex.Message);

                // Show dialog
                await _dialogManager.ShowDialogAsync(dialog);
            }
            finally
            {
                // Dispose progress operation
                operation.Dispose();

                // Unlock this method
                IsBusy = false;
            }
        }

        public void RemoveDownload(DownloadViewModel download)
        {
            download.Cancel();
            Downloads.Remove(download);
        }

        public void RemoveInactiveDownloads()
        {
            var inactiveDownloads = Downloads.Where(d => !d.IsActive).ToArray();
            Downloads.RemoveRange(inactiveDownloads);
        }

        public void RemoveSuccessfulDownloads()
        {
            var successfulDownloads = Downloads.Where(d => d.IsSuccessful).ToArray();
            Downloads.RemoveRange(successfulDownloads);
        }

        public void RestartFailedDownloads()
        {
            var failedDownloads = Downloads.Where(d => d.IsFailed).ToArray();
            foreach (var failedDownload in failedDownloads)
                failedDownload.Restart();
        }
    }
}