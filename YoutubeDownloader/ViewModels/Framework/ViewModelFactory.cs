using System;
using Microsoft.Extensions.DependencyInjection;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.ViewModels.Framework;

public class ViewModelFactory(IServiceProvider serviceProvider) : IViewModelFactory
{
    public DashboardViewModel CreateDashboardViewModel() => GetViewModel<DashboardViewModel>();

    public DownloadMultipleSetupViewModel CreateDownloadMultipleSetupViewModel() =>
        GetViewModel<DownloadMultipleSetupViewModel>();

    public AuthSetupViewModel CreateAuthSetupViewModel() => GetViewModel<AuthSetupViewModel>();

    public DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel() =>
        GetViewModel<DownloadSingleSetupViewModel>();

    public DownloadViewModel CreateDownloadViewModel() => GetViewModel<DownloadViewModel>();

    public MessageBoxViewModel CreateMessageBoxViewModel() => GetViewModel<MessageBoxViewModel>();

    public SettingsViewModel CreateSettingsViewModel() => GetViewModel<SettingsViewModel>();

    private T GetViewModel<T>() =>
        ActivatorUtilities.GetServiceOrCreateInstance<T>(serviceProvider);
}
