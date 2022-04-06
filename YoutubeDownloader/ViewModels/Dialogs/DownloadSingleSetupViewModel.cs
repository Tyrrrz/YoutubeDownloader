using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class DownloadSingleSetupViewModel : DialogScreen
{
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

   public DownloadSetupViewModel? DownloadSetup { get; set; }

    public DownloadSingleSetupViewModel(DialogManager dialogManager, SettingsService settingsService)
    {
        _dialogManager = dialogManager;
        _settingsService = settingsService;
    }

    public void OnViewFullyLoaded()
    {
        DownloadSetup!.RestoreDefaults();
    }

    public void Confirm()
    {
        var format = DownloadSetup!.SelectedDownloadOption!.Container.Name;

        DownloadSetup.FilePath = _dialogManager.PromptSaveFilePath(
            $"{format} file|*.{format}",
            FileNameTemplate.Apply(
                _settingsService.FileNameTemplate,
                DownloadSetup.Video!,
                DownloadSetup.SelectedDownloadOption!
            )
        );

        if (string.IsNullOrWhiteSpace(DownloadSetup.FilePath))
            return;

        _settingsService.LastFormat = format;

        Close(true);
    }
}

public static class DownloadSingleSetupViewModelExtensions
{
    public static DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel(
        this IViewModelFactory factory,
        DownloadSetupViewModel downloadSetup)
    {
        var viewModel = factory.CreateDownloadSingleSetupViewModel();

        viewModel.DownloadSetup = downloadSetup;

        return viewModel;
    }
}