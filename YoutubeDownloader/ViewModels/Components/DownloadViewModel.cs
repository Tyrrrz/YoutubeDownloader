using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using Gress;
using PropertyChanged;
using Stylet;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Components;

public class DownloadViewModel : PropertyChangedBase, IDisposable
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private readonly IClipboard _clipboard;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public IVideo? Video { get; set; }

    public VideoDownloadOption? DownloadOption { get; set; }

    public VideoDownloadPreference? DownloadPreference { get; set; }

    public string? FilePath { get; set; }

    public string? FileName => Path.GetFileName(FilePath);

    public ProgressContainer<Percentage> Progress { get; } = new();

    public bool IsProgressIndeterminate => Progress.Current.Fraction is <= 0 or >= 1;

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    [AlsoNotifyFor(nameof(IsRunning))]
    public DownloadStatus Status { get; set; } = DownloadStatus.Enqueued;

    public bool IsRunning => Status is DownloadStatus.Started;

    public bool IsCanceledOrFailed => Status is DownloadStatus.Canceled or DownloadStatus.Failed;

    public string? ErrorMessage { get; set; }

    public DownloadViewModel(IViewModelFactory viewModelFactory, DialogManager dialogManager, IClipboard clipboard)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        _clipboard = clipboard;

        Progress.Bind(
            o => o.Current,
            (_, _) => NotifyOfPropertyChange(() => IsProgressIndeterminate)
        );
    }

    public bool CanCancel => Status is DownloadStatus.Enqueued or DownloadStatus.Started;

    public void Cancel()
    {
        if (!CanCancel)
            return;

        _cancellationTokenSource.Cancel();
    }

    public bool CanShowFile => Status == DownloadStatus.Completed;

    public async Task ShowFile()
    {
        if (!CanShowFile)
            return;

        try
        {
            // Navigate to the file in Windows Explorer
            ProcessEx.Start("explorer", new[] { "/select,", FilePath! });
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelFactory.CreateMessageBoxViewModel("Error", ex.Message)
            );
        }
    }

    public bool CanOpenFile => Status == DownloadStatus.Completed;

    public async Task OpenFile()
    {
        if (!CanOpenFile)
            return;

        try
        {
            ProcessEx.StartShellExecute(FilePath!);
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelFactory.CreateMessageBoxViewModel("Error", ex.Message)
            );
        }
    }

    public bool CanCopyErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public async Task CopyErrorMessage()
    {
        if (!CanCopyErrorMessage)
            return;

        await _clipboard.SetTextAsync(ErrorMessage!);
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}

public static class DownloadViewModelExtensions
{
    public static DownloadViewModel CreateDownloadViewModel(
        this IViewModelFactory factory,
        IVideo video,
        VideoDownloadOption downloadOption,
        string filePath)
    {
        var viewModel = factory.CreateDownloadViewModel();

        viewModel.Video = video;
        viewModel.DownloadOption = downloadOption;
        viewModel.FilePath = filePath;

        return viewModel;
    }

    public static DownloadViewModel CreateDownloadViewModel(
        this IViewModelFactory factory,
        IVideo video,
        VideoDownloadPreference downloadPreference,
        string filePath)
    {
        var viewModel = factory.CreateDownloadViewModel();

        viewModel.Video = video;
        viewModel.DownloadPreference = downloadPreference;
        viewModel.FilePath = filePath;

        return viewModel;
    }
}