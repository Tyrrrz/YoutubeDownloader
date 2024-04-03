using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input.Platform;
using CommunityToolkit.Mvvm.Input;
using Gress;
using PropertyChanged;
using ReactiveUI;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Components;

public partial class DownloadViewModel : ViewModelBase, IDisposable
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

    public DownloadViewModel(
        IViewModelFactory viewModelFactory,
        DialogManager dialogManager,
        IClipboard clipboard
    )
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        _clipboard = clipboard;

        Progress
            .WhenAnyValue(o => o.Current)
            .Subscribe(_ => OnPropertyChanged(nameof(IsProgressIndeterminate)));
        this.WhenAnyValue(o => o.Status)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                OnPropertyChanged(nameof(IsRunning));
                CancelCommand.NotifyCanExecuteChanged();
                ShowFileCommand.NotifyCanExecuteChanged();
                OpenFileCommand.NotifyCanExecuteChanged();
            });
        this.WhenAnyValue(o => o.ErrorMessage)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => CopyErrorMessageCommand.NotifyCanExecuteChanged());
    }

    public bool CanCancel => Status is DownloadStatus.Enqueued or DownloadStatus.Started;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    public void Cancel()
    {
        if (!CanCancel)
            return;

        _cancellationTokenSource.Cancel();
    }

    public bool CanShowFile => Status == DownloadStatus.Completed;

    [RelayCommand(CanExecute = nameof(CanShowFile))]
    public async Task ShowFileAsync()
    {
        if (!CanShowFile)
            return;

        try
        {
            // Navigate to the file in Windows Explorer
            ProcessEx.Start("explorer", ["/select,", FilePath!]);
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelFactory.CreateMessageBoxViewModel("Error", ex.Message)
            );
        }
    }

    public bool CanOpenFile => Status == DownloadStatus.Completed;

    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    public async Task OpenFileAsync()
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

    [RelayCommand(CanExecute = nameof(CanCopyErrorMessage))]
    public async Task CopyErrorMessageAsync()
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
        string filePath
    )
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
        string filePath
    )
    {
        var viewModel = factory.CreateDownloadViewModel();

        viewModel.Video = video;
        viewModel.DownloadPreference = downloadPreference;
        viewModel.FilePath = filePath;

        return viewModel;
    }
}
