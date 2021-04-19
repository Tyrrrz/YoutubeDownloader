using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using YoutubeDownloader.Models;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Components
{
    public class DownloadViewModel : PropertyChangedBase
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly DialogManager _dialogManager;
        private readonly SettingsService _settingsService;
        private readonly DownloadService _downloadService;
        private readonly TaggingService _taggingService;

        private CancellationTokenSource? _cancellationTokenSource;

        public IVideo Video { get; set; } = default!;

        public string FilePath { get; set; } = default!;

        public string FileName => Path.GetFileName(FilePath);

        public string Format { get; set; } = default!;

        public VideoQualityPreference QualityPreference { get; set; } = VideoQualityPreference.Maximum;

        public VideoDownloadOption? VideoOption { get; set; }

        public SubtitleDownloadOption? SubtitleOption { get; set; }

        public IProgressManager? ProgressManager { get; set; }

        public IProgressOperation? ProgressOperation { get; private set; }

        public bool IsActive { get; private set; }

        public bool IsSuccessful { get; private set; }

        public bool IsCanceled { get; private set; }

        public bool IsFailed { get; private set; }

        public string? FailReason { get; private set; }

        public DownloadViewModel(
            IViewModelFactory viewModelFactory,
            DialogManager dialogManager,
            SettingsService settingsService,
            DownloadService downloadService,
            TaggingService taggingService)
        {
            _viewModelFactory = viewModelFactory;
            _dialogManager = dialogManager;
            _settingsService = settingsService;
            _downloadService = downloadService;
            _taggingService = taggingService;
        }

        public bool CanStart => !IsActive;

        public void Start()
        {
            if (!CanStart)
                return;

            IsActive = true;
            IsSuccessful = false;
            IsCanceled = false;
            IsFailed = false;

            Task.Run(async () =>
            {
                _cancellationTokenSource = new CancellationTokenSource();
                ProgressOperation = ProgressManager?.CreateOperation();

                try
                {
                    // If download option is not set - get the best download option
                    VideoOption ??= await _downloadService.TryGetBestVideoDownloadOptionAsync(
                        Video.Id,
                        Format,
                        QualityPreference
                    );

                    // It's possible that video has no streams
                    if (VideoOption is null)
                        throw new InvalidOperationException($"Video '{Video.Id}' contains no streams.");

                    await _downloadService.DownloadAsync(
                        VideoOption,
                        SubtitleOption,
                        FilePath,
                        ProgressOperation,
                        _cancellationTokenSource.Token
                    );

                    if (_settingsService.ShouldInjectTags)
                    {
                        await _taggingService.InjectTagsAsync(
                            Video,
                            Format,
                            FilePath,
                            _cancellationTokenSource.Token
                        );
                    }

                    IsSuccessful = true;
                }
                catch (OperationCanceledException)
                {
                    IsCanceled = true;
                }
                catch (Exception ex)
                {
                    IsFailed = true;

                    // Short error message for expected errors, full for unexpected
                    FailReason = ex is YoutubeExplodeException
                        ? ex.Message
                        : ex.ToString();
                }
                finally
                {
                    IsActive = false;
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                    ProgressOperation?.Dispose();
                }
            });
        }

        public bool CanCancel => IsActive && !IsCanceled;

        public void Cancel()
        {
            if (!CanCancel)
                return;

            _cancellationTokenSource?.Cancel();
        }

        public bool CanShowFile => IsSuccessful;

        public async void ShowFile()
        {
            if (!CanShowFile)
                return;

            try
            {
                // Open explorer, navigate to the output directory and select the video file
                Process.Start("explorer", $"/select, \"{FilePath}\"");
            }
            catch (Exception ex)
            {
                var dialog = _viewModelFactory.CreateMessageBoxViewModel("Error", ex.Message);
                await _dialogManager.ShowDialogAsync(dialog);
            }
        }

        public bool CanOpenFile => IsSuccessful;

        public async void OpenFile()
        {
            if (!CanOpenFile)
                return;

            try
            {
                ProcessEx.StartShellExecute(FilePath);
            }
            catch (Exception ex)
            {
                var dialog = _viewModelFactory.CreateMessageBoxViewModel("Error", ex.Message);
                await _dialogManager.ShowDialogAsync(dialog);
            }
        }

        public bool CanRestart => CanStart && !IsSuccessful;

        public void Restart() => Start();
    }

    public static class DownloadViewModelExtensions
    {
        public static DownloadViewModel CreateDownloadViewModel(
            this IViewModelFactory factory,
            IVideo video,
            string filePath,
            string format,
            VideoDownloadOption videoOption,
            SubtitleDownloadOption? subtitleOption)
        {
            var viewModel = factory.CreateDownloadViewModel();

            viewModel.Video = video;
            viewModel.FilePath = filePath;
            viewModel.Format = format;
            viewModel.VideoOption = videoOption;
            viewModel.SubtitleOption = subtitleOption;

            return viewModel;
        }

        public static DownloadViewModel CreateDownloadViewModel(
            this IViewModelFactory factory,
            IVideo video,
            string filePath,
            string format,
            VideoQualityPreference qualityPreference)
        {
            var viewModel = factory.CreateDownloadViewModel();

            viewModel.Video = video;
            viewModel.FilePath = filePath;
            viewModel.Format = format;
            viewModel.QualityPreference = qualityPreference;

            return viewModel;
        }
    }
}
