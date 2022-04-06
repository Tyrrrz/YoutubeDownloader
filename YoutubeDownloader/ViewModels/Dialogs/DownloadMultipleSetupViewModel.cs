using System.Collections.Generic;
using System.IO;
using System.Linq;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class DownloadMultipleSetupViewModel : DialogScreen
{
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    public IReadOnlyList<DownloadSetupViewModel>? DownloadSetups { get; set; }

    public int SelectedDownloadSetupsCount => DownloadSetups?.Count(s => s.IsSelected) ?? 0;

    public DownloadMultipleSetupViewModel(DialogManager dialogManager, SettingsService settingsService)
    {
        _dialogManager = dialogManager;
        _settingsService = settingsService;
    }

    public void OnViewFullyLoaded()
    {
        foreach (var downloadSetup in DownloadSetups!)
            downloadSetup.RestoreDefaults();
    }

    public bool CanConfirm => SelectedDownloadSetupsCount > 0;

    public void Confirm()
    {
        if (SelectedDownloadSetupsCount <= 0)
            return;

        var dirPath = _dialogManager.PromptDirectoryPath();
        if (string.IsNullOrWhiteSpace(dirPath))
            return;

        foreach (var downloadSetup in DownloadSetups!)
        {
            downloadSetup.FilePath = Path.Combine(
                dirPath,
                FileNameTemplate.Apply(
                    _settingsService.FileNameTemplate,
                    downloadSetup.Video!,
                    downloadSetup.SelectedDownloadOption!
                )
            );
        }

        Close(true);
    }
}

public static class DownloadMultipleSetupViewModelExtensions
{
    public static DownloadMultipleSetupViewModel CreateDownloadMultipleSetupViewModel(
        this IViewModelFactory factory,
        IReadOnlyList<DownloadSetupViewModel> downloadSetups)
    {
        var viewModel = factory.CreateDownloadMultipleSetupViewModel();

        viewModel.DownloadSetups = downloadSetups;

        return viewModel;
    }
}