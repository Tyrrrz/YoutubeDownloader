using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.ViewModels.Framework;

// Used to instantiate new view models while making use of dependency injection
public interface IViewModelFactory
{
    DownloadViewModel CreateDownloadViewModel();

    DownloadSetupItemViewModel CreateDownloadSetupItemViewModel();

    DownloadSingleSetupViewModel CreateDownloadSetupViewModel();

    MessageBoxViewModel CreateMessageBoxViewModel();

    SettingsViewModel CreateSettingsViewModel();
}