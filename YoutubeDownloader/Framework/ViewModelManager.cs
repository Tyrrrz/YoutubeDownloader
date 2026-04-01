using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using Microsoft.Extensions.DependencyInjection;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Framework;

public class ViewModelManager(IServiceProvider services)
{
    public MainViewModel CreateMainViewModel() => services.GetRequiredService<MainViewModel>();

    public DashboardViewModel CreateDashboardViewModel() =>
        services.GetRequiredService<DashboardViewModel>();

    public AuthSetupDialogViewModel CreateAuthSetupDialogViewModel() =>
        services.GetRequiredService<AuthSetupDialogViewModel>();

    public DownloadViewModel CreateDownloadViewModel(
        IVideo video,
        VideoDownloadOption downloadOption,
        string filePath
    )
    {
        var viewModel = services.GetRequiredService<DownloadViewModel>();

        viewModel.Video = video;
        viewModel.DownloadOption = downloadOption;
        viewModel.FilePath = filePath;

        return viewModel;
    }

    public DownloadViewModel CreateDownloadViewModel(
        IVideo video,
        VideoDownloadPreference downloadPreference,
        string filePath
    )
    {
        var viewModel = services.GetRequiredService<DownloadViewModel>();

        viewModel.Video = video;
        viewModel.DownloadPreference = downloadPreference;
        viewModel.FilePath = filePath;

        return viewModel;
    }

    public DownloadMultipleSetupDialogViewModel CreateDownloadMultipleSetupDialogViewModel(
        string title,
        IReadOnlyList<IVideo> availableVideos,
        bool preselectVideos = true
    )
    {
        var viewModel = services.GetRequiredService<DownloadMultipleSetupDialogViewModel>();

        viewModel.Title = title;
        viewModel.AvailableVideos = availableVideos;

        if (preselectVideos)
            viewModel.SelectedVideos.AddRange(availableVideos);

        return viewModel;
    }

    public DownloadSingleSetupDialogViewModel CreateDownloadSingleSetupDialogViewModel(
        IVideo video,
        IReadOnlyList<VideoDownloadOption> availableDownloadOptions
    )
    {
        var viewModel = services.GetRequiredService<DownloadSingleSetupDialogViewModel>();

        viewModel.Video = video;
        viewModel.AvailableDownloadOptions = availableDownloadOptions;

        return viewModel;
    }

    public MessageBoxDialogViewModel CreateMessageBoxDialogViewModel(
        string title,
        string message,
        string? okButtonText,
        string? cancelButtonText
    )
    {
        var viewModel = services.GetRequiredService<MessageBoxDialogViewModel>();

        viewModel.Title = title;
        viewModel.Message = message;
        viewModel.DefaultButtonText = okButtonText;
        viewModel.CancelButtonText = cancelButtonText;

        return viewModel;
    }

    public MessageBoxDialogViewModel CreateMessageBoxDialogViewModel(string title, string message)
    {
        var viewModel = services.GetRequiredService<MessageBoxDialogViewModel>();

        viewModel.Title = title;
        viewModel.Message = message;

        return viewModel;
    }

    public ProgressDialogViewModel CreateProgressDialogViewModel(
        string title,
        Func<IProgress<Percentage>, CancellationToken, Task> operation
    )
    {
        var viewModel = services.GetRequiredService<ProgressDialogViewModel>();

        viewModel.Title = title;
        viewModel.Operation = operation;

        return viewModel;
    }

    public SettingsDialogViewModel CreateSettingsDialogViewModel() =>
        services.GetRequiredService<SettingsDialogViewModel>();
}
