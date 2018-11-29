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
        private readonly DownloadService _downloadService;

        public IReadOnlyList<string> AvailableFormats { get; } = new[] { "mp4", "mp3" };

        public string SelectedFormat { get; set; }

        protected DownloadSetupViewModelBase(IViewModelFactory viewModelFactory, DownloadService downloadService)
        {
            _viewModelFactory = viewModelFactory;
            _downloadService = downloadService;

            // Default format
            SelectedFormat = AvailableFormats.FirstOrDefault();
        }

        protected string GetDefaultFileName(Video video, string format)
        {
            var cleanTitle = video.Title.Replace(Path.GetInvalidFileNameChars(), '_');
            return $"{cleanTitle}.{format}";
        }

        protected DownloadViewModel EnqueueDownload(Video video, string filePath)
        {
            // Prepare the viewmodel
            var download = _viewModelFactory.CreateDownloadViewModel();
            download.Video = video;
            download.FilePath = filePath;
            download.CancellationTokenSource = new CancellationTokenSource();

            // Set up progress router
            var progressRouter = new Progress<double>(p => download.Progress = p);

            // Get cancellation token
            var cancellationToken = download.CancellationTokenSource.Token;

            // Start download
            _downloadService.DownloadVideoAsync(download.Video.Id, download.FilePath, progressRouter, cancellationToken)
                .ContinueWith(t =>
                {
                    download.IsFinished = t.IsCompleted && !t.IsCanceled;
                    download.IsCanceled = t.IsCanceled;
                    download.CancellationTokenSource.Dispose();
                });

            return download;
        }
    }
}