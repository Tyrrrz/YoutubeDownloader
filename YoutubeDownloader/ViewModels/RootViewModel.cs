using System;
using System.Linq;
using System.Reflection;
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

        public string Query { get; set; }

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
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            DisplayName = $"YoutubeDownloader v{version}";
        }

        protected override async void OnViewLoaded()
        {
            base.OnViewLoaded();

            // Load settings
            _settingsService.Load();

            // Check and prepare update
            try
            {
                var updateVersion = await _updateService.CheckPrepareUpdateAsync();
                if (updateVersion != null)
                {
                    // Show notification
                    Notifications.Enqueue(
                        $"Update to YoutubeDownloader v{updateVersion} will be installed when you exit",
                        "INSTALL NOW", () =>
                        {
                            _updateService.FinalizeUpdate(true);
                            RequestClose();
                        });
                }
            }
            catch
            {
                Notifications.Enqueue("Failed to perform application auto-update");
            }
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

        // This is async void on purpose because this is supposed to be always ran in background
        private void EnqueueAndStartDownload(DownloadViewModel download)
        {
            // Cancel existing downloads for this file path to prevent writing to the same file
            foreach (var existingDownload in Downloads.Where(d => d.FilePath == download.FilePath))
                existingDownload.Cancel();

            // Add to list
            Downloads.Add(download);

            // Bind progress manager
            download.ProgressManager = ProgressManager;

            // Start download
            download.Start();
        }

        public bool CanProcessQuery => !IsBusy && !Query.IsNullOrWhiteSpace();

        public async void ProcessQuery()
        {
            try
            {
                // Enter busy state
                IsBusy = true;
                IsProgressIndeterminate = true;

                // Execute query
                var executedQuery = await _queryService.ExecuteQueryAsync(Query);

                // If no videos were found - tell the user
                if (executedQuery.Videos.Count <= 0)
                {
                    // Create dialog
                    var dialog = _viewModelFactory.CreateMessageBoxViewModel("Nothing found",
                        "Couldn't find any videos based on the query or URL you provided");

                    // Show dialog
                    await _dialogManager.ShowDialogAsync(dialog);
                }

                // If only one video was found - show download setup for single video
                else if (executedQuery.Videos.Count == 1)
                {
                    // Get single video
                    var video = executedQuery.Videos.Single();

                    // Get download options
                    var downloadOptions = await _downloadService.GetDownloadOptionsAsync(video.Id);

                    // Create dialog
                    var dialog = _viewModelFactory.CreateDownloadSingleSetupViewModel(executedQuery.Title, video, downloadOptions);

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
                    var dialog = _viewModelFactory.CreateDownloadMultipleSetupViewModel(executedQuery.Title, executedQuery.Videos);

                    // If this is not a search result - preselect all videos
                    if (executedQuery.Query.Type != QueryType.Search)
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
                // Reset busy state
                IsBusy = false;
                IsProgressIndeterminate = false;
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
    }
}