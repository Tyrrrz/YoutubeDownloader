using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Stylet;
using YoutubeDownloader.Models;
using YoutubeExplode.Models;

namespace YoutubeDownloader.ViewModels.Components
{
    public class DownloadViewModel : PropertyChangedBase
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public Video Video { get; set; }

        public string FilePath { get; set; }

        public string FileName => Path.GetFileName(FilePath);

        public string Format { get; set; }

        public DownloadOption DownloadOption { get; set; }

        public double Progress { get; set; }

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public bool IsCompleted { get; private set; }

        public bool IsCanceled { get; private set; }

        public bool IsRunning => !IsCompleted && !IsCanceled;

        public void MarkAsCompleted()
        {
            _cancellationTokenSource.Dispose();
            IsCompleted = true;
        }

        public IProgress<double> GetProgressRouter() => new Progress<double>(p => Progress = p);

        public bool CanCancel => IsRunning;

        public void Cancel()
        {
            if (!CanCancel)
                return;

            // Cancel
            _cancellationTokenSource.Cancel();
            IsCanceled = true;

            // Mark as completed
            MarkAsCompleted();
        }

        public bool CanShowFile => IsCompleted && !IsCanceled;

        public void ShowFile()
        {
            if (!CanShowFile)
                return;

            // This opens explorer, navigates to the output directory and selects the video file
            Process.Start("explorer", $"/select, \"{FilePath}\"");
        }

        public bool CanOpenFile => IsCompleted && !IsCanceled;

        public void OpenFile()
        {
            if (!CanOpenFile)
                return;

            // This opens the video file using the default player
            Process.Start(FilePath);
        }
    }
}