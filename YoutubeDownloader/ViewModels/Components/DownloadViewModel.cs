using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gress;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Components;

public partial class DownloadViewModel : ViewModelBase
{
    private readonly ViewModelManager _viewModelManager;
    private readonly DialogManager _dialogManager;
    private readonly IClipboard _clipboard;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [ObservableProperty]
    private IVideo? _video;

    [ObservableProperty]
    private VideoDownloadOption? _downloadOption;

    [ObservableProperty]
    private VideoDownloadPreference? _downloadPreference;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileName))]
    private string? _filePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRunning))]
    [NotifyPropertyChangedFor(nameof(IsCanceledOrFailed))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenFileCommand))]
    private DownloadStatus _status = DownloadStatus.Enqueued;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CopyErrorMessageCommand))]
    private string? _errorMessage;

    public DownloadViewModel(
        ViewModelManager viewModelManager,
        DialogManager dialogManager,
        IClipboard clipboard
    )
    {
        _viewModelManager = viewModelManager;
        _dialogManager = dialogManager;
        _clipboard = clipboard;

        Progress.WatchProperty(
            o => o.Current,
            () => OnPropertyChanged(nameof(IsProgressIndeterminate))
        );
    }

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public string? FileName => Path.GetFileName(FilePath);

    public ProgressContainer<Percentage> Progress { get; } = new();

    public bool IsProgressIndeterminate => Progress.Current.Fraction is <= 0 or >= 1;

    public bool IsRunning => Status is DownloadStatus.Started;

    public bool IsCanceledOrFailed => Status is DownloadStatus.Canceled or DownloadStatus.Failed;

    private bool CanCancel() => Status is DownloadStatus.Enqueued or DownloadStatus.Started;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel() => _cancellationTokenSource.Cancel();

    private bool CanShowFile() => Status == DownloadStatus.Completed;

    [RelayCommand(CanExecute = nameof(CanShowFile))]
    private async Task ShowFileAsync()
    {
        try
        {
            // Navigate to the file in Windows Explorer
            ProcessEx.Start("explorer", ["/select,", FilePath!]);
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelManager.CreateMessageBoxViewModel("Error", ex.Message)
            );
        }
    }

    private bool CanOpenFile() => Status == DownloadStatus.Completed;

    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    private async Task OpenFileAsync()
    {
        try
        {
            ProcessEx.StartShellExecute(FilePath!);
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelManager.CreateMessageBoxViewModel("Error", ex.Message)
            );
        }
    }

    private bool CanCopyErrorMessage() => !string.IsNullOrWhiteSpace(ErrorMessage);

    [RelayCommand(CanExecute = nameof(CanCopyErrorMessage))]
    private async Task CopyErrorMessageAsync()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
            return;

        await _clipboard.SetTextAsync(ErrorMessage);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource.Dispose();
        }

        base.Dispose(disposing);
    }
}
