using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using MaterialDesignThemes.Wpf;
using Stylet;
using Tyrrrz.Extensions;
using YoutubeDownloader.Internal;
using YoutubeDownloader.Models;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Models;

namespace YoutubeDownloader.ViewModels
{
    public class RootViewModel : Screen, IDisposable
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly DialogManager _dialogManager;
        private readonly SettingsService _settingsService;
        private readonly UpdateService _updateService;
        private readonly QueryService _queryService;
        private readonly DownloadService _downloadService;

        private readonly CancellationTokenSource _killSwitchCts = new CancellationTokenSource();

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

        private DownloadViewModel CreateDownloadViewModel(Video video, string filePath, string format)
        {
            // Find an existing download for this file path
            var existingDownload = Downloads.FirstOrDefault(d => d.FilePath == filePath);

            // If it exists - cancel and remove it
            if (existingDownload != null)
            {
                if (existingDownload.CanCancel)
                    existingDownload.Cancel();

                Downloads.Remove(existingDownload);
            }

            // Prepare the download viewmodel
            var download = _viewModelFactory.CreateDownloadViewModel();
            download.Video = video;
            download.FilePath = filePath;
            download.Format = format;
            download.CancellationTokenSource = new CancellationTokenSource();
            download.StartTime = DateTimeOffset.Now;

            return download;
        }

        private void EnqueueDownload(Video video, DownloadOption downloadOption, string filePath)
        {
            // Create download
            var download = CreateDownloadViewModel(video, filePath, downloadOption.Format);

            // Set up progress router
            var progressRouter = new Progress<double>(p => download.Progress = p);

            // Create cancellation token based on download's own cancellation token and kill switch
            var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
                download.CancellationTokenSource.Token,
                _killSwitchCts.Token).Token;

            // Start download
            _downloadService.DownloadVideoAsync(video.Id, filePath, downloadOption, progressRouter, cancellationToken)
                .ContinueWith(t =>
                {
                    download.IsFinished = t.IsCompleted && !t.IsCanceled;
                    download.IsCanceled = t.IsCanceled;
                    download.EndTime = DateTimeOffset.Now;
                    download.CancellationTokenSource.Dispose();
                });

            // Add to list
            Downloads.Add(download);
        }

        private void EnqueueDownloads(IReadOnlyList<Video> videos, string dirPath, string format)
        {
            foreach (var video in videos)
            {
                // Generate file path
                var fileName = $"{video.GetFileNameSafeTitle()}.{format}";
                var filePath = Path.Combine(dirPath, fileName);

                // Ensure file paths are unique because users will not be able to confirm overwrites
                filePath = FileEx.MakeUniqueFilePath(filePath);

                // Create download
                var download = CreateDownloadViewModel(video, filePath, format);

                // Set up progress router
                var progressRouter = new Progress<double>(p => download.Progress = p);

                // Create cancellation token based on download's own cancellation token and kill switch
                var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
                    download.CancellationTokenSource.Token,
                    _killSwitchCts.Token).Token;

                // Start download
                _downloadService.DownloadVideoAsync(video.Id, filePath, format, progressRouter, cancellationToken)
                    .ContinueWith(t =>
                    {
                        download.IsFinished = t.IsCompleted && !t.IsCanceled;
                        download.IsCanceled = t.IsCanceled;
                        download.EndTime = DateTimeOffset.Now;
                        download.CancellationTokenSource.Dispose();
                    });

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

                    // Show dialog, if canceled - return
                    if (await _dialogManager.ShowDialogAsync(dialog) != true)
                        return;

                    // Enqueue download
                    EnqueueDownload(dialog.Video, dialog.SelectedDownloadOption, dialog.FilePath);
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

                    // Show dialog, if canceled - return
                    if (await _dialogManager.ShowDialogAsync(dialog) != true)
                        return;

                    // Enqueue downloads
                    EnqueueDownloads(dialog.SelectedVideos, dialog.DirPath, dialog.SelectedFormat);
                }
            }
            finally
            {
                // Reset busy state
                IsBusy = false;
            }
        }

        public void Dispose()
        {
            // Trigger kill switch to cancel all active downloads
            _killSwitchCts.Cancel();
            _killSwitchCts.Dispose();
        }
    }
}