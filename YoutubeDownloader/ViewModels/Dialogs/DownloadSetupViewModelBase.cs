using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Tyrrrz.Extensions;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Models;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public abstract class DownloadSetupViewModelBase<T> : DialogScreen<T>
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly SettingsService _settingsService;
        private readonly DownloadService _downloadService;

        public IReadOnlyList<string> AvailableFormats { get; } = new[] { "mp4", "mp3" };

        public string SelectedFormat { get; set; }

        protected DownloadSetupViewModelBase(IViewModelFactory viewModelFactory, SettingsService settingsService,
            DownloadService downloadService)
        {
            _viewModelFactory = viewModelFactory;
            _settingsService = settingsService;
            _downloadService = downloadService;
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            // Persist preferences
            SelectedFormat = AvailableFormats.Contains(_settingsService.LastFormat)
                ? _settingsService.LastFormat
                : AvailableFormats.FirstOrDefault();
        }

        protected string GetDefaultFileName(Video video, string format)
        {
            var cleanTitle = video.Title.Replace(Path.GetInvalidFileNameChars(), '_');
            return $"{cleanTitle}.{format}";
        }

        protected DownloadViewModel EnqueueDownload(Video video, string filePath, string format)
        {
            // Persist preferences
            _settingsService.LastFormat = format;

            // Prepare the viewmodel
            var download = _viewModelFactory.CreateDownloadViewModel();
            download.Video = video;
            download.FilePath = filePath;
            download.Format = format;
            download.CancellationTokenSource = new CancellationTokenSource();
            download.StartTime = DateTimeOffset.Now;

            // Set up progress router
            var progressRouter = new Progress<double>(p => download.Progress = p);

            // Get cancellation token
            var cancellationToken = download.CancellationTokenSource.Token;

            // Start download
            _downloadService.DownloadVideoAsync(download.Video.Id, download.FilePath, format, progressRouter, cancellationToken)
                .ContinueWith(t =>
                {
                    download.IsFinished = t.IsCompleted && !t.IsCanceled;
                    download.IsCanceled = t.IsCanceled;
                    download.EndTime = DateTimeOffset.Now;
                    download.CancellationTokenSource.Dispose();
                });

            return download;
        }
    }
}