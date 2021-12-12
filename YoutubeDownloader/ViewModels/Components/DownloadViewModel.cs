using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Contextual;
using Contextual.Contexts;
using Gress;
using YoutubeDownloader.Core;
using YoutubeDownloader.Core.Contexts;
using YoutubeDownloader.Core.Tagging;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Components;

public class DownloadViewModel : PropertyChangedBase, IDisposable
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public IVideo Video { get; set; } = null!;

    public string FilePath { get; set; } = "";

    public string FileName => Path.GetFileName(FilePath);

    public string Format { get; set; } = "";

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
        SettingsService settingsService)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        _settingsService = settingsService;
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
            var cancellationContext = Context.Provide(new CancellationContext(_cancellationTokenSource.Token));

            var progressOperation = ProgressManager?.CreateOperation();
            var progressContext = Context.Provide(new ProgressContext(ProgressOperation));
            ProgressOperation = progressOperation;

            try
            {
                // If download option is not set - get the best download option
                VideoOption ??= await Video.TryGetPreferredVideoDownloadOptionAsync(Format, QualityPreference);

                // It's possible that video has no streams
                if (VideoOption is null)
                    throw new InvalidOperationException($"Video '{Video.Id}' contains no {Format} streams.");

                // Download video
                await VideoOption.DownloadAsync(FilePath);

                // Download subtitles
                if (SubtitleOption is not null)
                {
                    var subtitleFilePath = Path.ChangeExtension(FilePath, "srt");

                    await SubtitleOption.DownloadAsync(
                        subtitleFilePath,
                        // TODO: create progress operation?
                        null,
                        _cancellationTokenSource.Token
                    );
                }

                // Inject tags
                if (_settingsService.ShouldInjectTags)
                {
                    await new MediaTagInjector().InjectTagsAsync(
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
                cancellationContext.Dispose();
                progressContext.Dispose();
                progressOperation?.Dispose();
                ProgressOperation = null;
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
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "explorer",
                    ArgumentList = { "/select,", FilePath }
                }
            };

            process.Start();
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

    public void Dispose() => _cancellationTokenSource.Dispose();
}

public static class DownloadViewModelExtensions
{
    public static DownloadViewModel CreateDownloadViewModel(
        this IViewModelFactory factory,
        IVideo video,
        string filePath,
        string format,
        VideoDownloadOption option,
        SubtitleDownloadOption? subtitleOption)
    {
        var viewModel = factory.CreateDownloadViewModel();

        viewModel.Video = video;
        viewModel.FilePath = filePath;
        viewModel.Format = format;
        viewModel.VideoOption = option;
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