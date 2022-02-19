using System.Collections.Generic;
using System.Linq;
using System.Windows;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class DownloadSetupViewModel : DialogScreen<DownloadViewModel>
{
    private readonly SettingsService _settingsService;
    private readonly DialogManager _dialogManager;

    public IReadOnlyList<DownloadSetupItemViewModel>? Items { get; set; }

    public DownloadSetupViewModel(SettingsService settingsService, DialogManager dialogManager)
    {
        _settingsService = settingsService;
        _dialogManager = dialogManager;
    }

    public bool CanConfirm => Items is not null && Items.Any(i => i.IsSelected);

    public void Confirm()
    {


        Close();
    }
}

public static class DownloadSetupViewModelExtensions
{
    public static DownloadSetupViewModel CreateDownloadSetupViewModel(
        this IViewModelFactory factory,
        IReadOnlyList<DownloadSetupItemViewModel> items)
    {
        var viewModel = factory.CreateDownloadSetupViewModel();
        viewModel.Items = items;

        return viewModel;
    }
}