using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public SnackbarMessageQueue Notifications { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));

        public bool IsBusy { get; private set; }

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

        private void RemoveDownload(DownloadViewModel download)
        {
            // Find an existing download for this file path
            var existingDownload = Downloads.FirstOrDefault(d => d.FilePath == download.FilePath);

            // If it exists - cancel and remove it
            if (existingDownload != null)
            {
                existingDownload.Cancel();
                Downloads.Remove(existingDownload);
            }
        }

        private void EnqueueDownload(DownloadViewModel download, DownloadOption option)
        {
            // Remove an existing download
            RemoveDownload(download);

            // Start download
            _downloadService.DownloadVideoAsync(download.Video.Id, download.FilePath, option,
                    download.GetProgressRouter(), download.CancellationToken)
                .ContinueWith(t => download.MarkAsCompleted());

            // Add to list
            Downloads.Add(download);
        }

        private void EnqueueDownloads(IReadOnlyList<DownloadViewModel> downloads)
        {
            foreach (var download in downloads)
            {
                // Remove an existing download
                RemoveDownload(download);

                // Start download
                _downloadService
                    .DownloadVideoAsync(download.Video.Id, download.FilePath, download.Format,
                        download.GetProgressRouter(), download.CancellationToken)
                    .ContinueWith(t => download.MarkAsCompleted());

                // Add to list
                Downloads.Add(download);
            }
        }

        public bool CanProcessQuery => !IsBusy && Query.IsNotBlank();

        public async void ProcessQuery()
        {
            try
            {
                // Set busy state
                IsBusy = true;

                // Execute query
                var executedQuery = await _queryService.ExecuteQueryAsync(Query);

                // If no videos were found - tell the user
                if (executedQuery.Videos.Count <= 0)
                {
                    // Create dialog
                    var dialog = _viewModelFactory.CreateMessageBoxViewModel();
                    dialog.DisplayName = "Nothing found";
                    dialog.Message = "Couldn't find any videos based on the query or URL you provided";

                    // Show dialog
                    await _dialogManager.ShowDialogAsync(dialog);
                }
                // If only one video was found - show download setup for single video
                else if (executedQuery.Videos.Count == 1)
                {
                    // Get single video
                    var video = executedQuery.Videos.Single();

                    // Get download options
                    var downloadOptions = await _downloadService.GetVideoDownloadOptionsAsync(video.Id);

                    // Create dialog
                    var dialog = _viewModelFactory.CreateDownloadSingleSetupViewModel();
                    dialog.DisplayName = executedQuery.Title;
                    dialog.Video = video;
                    dialog.AvailableDownloadOptions = downloadOptions;

                    // Show dialog and get download
                    var download = await _dialogManager.ShowDialogAsync(dialog);

                    // If canceled - return
                    if (download == null)
                        return;

                    // Enqueue download
                    EnqueueDownload(download, dialog.SelectedDownloadOption);
                }
                // If multiple videos were found - show download setup for multiple videos
                else
                {
                    // Create dialog
                    var dialog = _viewModelFactory.CreateDownloadMultipleSetupViewModel();
                    dialog.DisplayName = executedQuery.Title;
                    dialog.AvailableVideos = executedQuery.Videos;

                    // If this is a playlist - preselect all videos
                    if (executedQuery.Query.Type == QueryType.Playlist)
                        dialog.SelectedVideos = dialog.AvailableVideos;

                    // Show dialog and get downloads
                    var downloads = await _dialogManager.ShowDialogAsync(dialog);

                    // If canceled - return
                    if (downloads == null)
                        return;

                    // Enqueue downloads
                    EnqueueDownloads(downloads);
                }
            }
            finally
            {
                // Reset busy state
                IsBusy = false;
            }
        }
    }
}