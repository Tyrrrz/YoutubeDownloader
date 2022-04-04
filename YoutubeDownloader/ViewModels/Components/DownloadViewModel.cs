using System;
using System.IO;
using System.Threading;
using Gress;
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

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public IVideo? Video { get; set; }

    public VideoDownloadOption? DownloadOption { get; set; }

    public string? FilePath { get; set; }

    public string? FileName => Path.GetFileName(FilePath);

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
        if (string.IsNullOrWhiteSpace(FilePath) || !CanShowFile)
            return;

        try
        {
            // Navigate to the file in Windows Explorer
            ProcessEx.Start("explorer", new[] { "/select,", FilePath });
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
        if (string.IsNullOrWhiteSpace(FilePath) || !CanOpenFile)
            return;

        try
        {
            ProcessEx.StartShellExecute(FilePath);
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
}