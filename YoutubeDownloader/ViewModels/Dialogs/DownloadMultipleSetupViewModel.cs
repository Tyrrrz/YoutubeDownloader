using System;
using System.Collections.Generic;
using System.IO;
using Tyrrrz.Extensions;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Models;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class DownloadMultipleSetupViewModel : DownloadSetupViewModelBase<IReadOnlyList<DownloadViewModel>>
    {
        private readonly DialogManager _dialogManager;

        public IReadOnlyList<Video> AvailableVideos { get; set; } = Array.Empty<Video>();

        public IReadOnlyList<Video> SelectedVideos { get; set; } = Array.Empty<Video>();

        public DownloadMultipleSetupViewModel(IViewModelFactory viewModelFactory, DownloadService downloadService,
            DialogManager dialogManager)
            : base(viewModelFactory, downloadService)
        {
            _dialogManager = dialogManager;
        }

        public bool CanConfirm => SelectedVideos != null && SelectedVideos.Count > 0;

        public void Confirm()
        {
            // Prompt user for output directory path
            var dirPath = _dialogManager.PromptDirectoryPath();

            // If canceled - return
            if (dirPath.IsBlank())
                return;

            // Get downloads
            var downloads = new List<DownloadViewModel>();
            foreach (var video in SelectedVideos)
            {
                // Generate file path
                var fileName = GetDefaultFileName(video, SelectedFormat);
                var filePath = Path.Combine(dirPath, fileName);

                // Enqueue download
                downloads.Add(EnqueueDownload(video, filePath));
            }

            // Close dialog with result
            Close(downloads);
        }
    }
}