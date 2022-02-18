using System;
using System.Threading;
using Gress;
using Stylet;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Components;

public class DownloadViewModel : PropertyChangedBase, IDisposable
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public VideoDownloadRequest? Request { get; set; }

    public ProgressContainer<Percentage> Progress { get; } = new();

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public DownloadStatus Status { get; set; }

    public string? ErrorMessage { get; set; }

    public DownloadViewModel(IViewModelFactory viewModelFactory, DialogManager dialogManager)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
    }

    public bool CanCancel => Status == DownloadStatus.Started;

    public void Cancel()
    {
        if (!CanCancel)
            return;

        _cancellationTokenSource.Cancel();
    }

    public bool CanShowFile => Status == DownloadStatus.Completed;

    public async void ShowFile()
    {
        if (Request is null || !CanShowFile)
            return;

        try
        {
            // Navigate to the file in Windows Explorer
            ProcessEx.Start("explorer", new[] { "/select,", Request.FilePath });
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelFactory.CreateMessageBoxViewModel("Error", ex.Message)
            );
        }
    }

    public bool CanOpenFile => Status == DownloadStatus.Completed;

    public async void OpenFile()
    {
        if (Request is null || !CanOpenFile)
            return;

        try
        {
            ProcessEx.StartShellExecute(Request.FilePath);
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelFactory.CreateMessageBoxViewModel("Error", ex.Message)
            );
        }
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}

public static class DownloadViewModelExtensions
{
    public static DownloadViewModel CreateDownloadViewModel(
        this IViewModelFactory factory,
        VideoDownloadRequest request)
    {
        var viewModel = factory.CreateDownloadViewModel();

        viewModel.Request = request;

        return viewModel;
    }
}