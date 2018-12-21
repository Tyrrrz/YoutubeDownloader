using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Stylet;
using YoutubeExplode.Models;

namespace YoutubeDownloader.ViewModels.Components
{
    public class DownloadViewModel : PropertyChangedBase
    {
        public Video Video { get; set; }

        public string FilePath { get; set; }

        public string Format { get; set; }

        public string FileName => Path.GetFileName(FilePath);

        public double Progress { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public bool IsFinished { get; set; }

        public bool IsCanceled { get; set; }

        public bool IsRunning => !IsFinished && !IsCanceled;

        public DateTimeOffset StartTime { get; set; }

        public DateTimeOffset EndTime { get; set; }

        public bool CanCancel => IsRunning;

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
        }

        public bool CanShowFile => IsFinished;

        public void ShowFile()
        {
            // This opens explorer, navigates to the output directory and selects the video file
            Process.Start("explorer", $"/select, \"{FilePath}\"");
        }

        public bool CanOpenFile => IsFinished;

        public void OpenFile()
        {
            // This opens the video file using the default player
            Process.Start(FilePath);
        }
    }
}