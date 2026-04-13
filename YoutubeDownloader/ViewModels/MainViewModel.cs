using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Components;

namespace YoutubeDownloader.ViewModels;

public partial class MainViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    SnackbarManager snackbarManager,
    LocalizationManager localizationManager,
    SettingsService settingsService,
    UpdateService updateService
) : ViewModelBase
{
    public string Title { get; } = $"{Program.Name} v{Program.VersionString}";

    public DashboardViewModel Dashboard { get; } = viewModelManager.GetDashboardViewModel();

    private async Task ShowUkraineSupportMessageAsync()
    {
        if (!settingsService.IsUkraineSupportMessageEnabled)
            return;

        var dialog = viewModelManager.GetMessageBoxViewModel(
            localizationManager.UkraineSupportTitle,
            localizationManager.UkraineSupportMessage,
            localizationManager.LearnMoreButton,
            localizationManager.CloseButton
        );

        // Disable this message in the future
        settingsService.IsUkraineSupportMessageEnabled = false;
        settingsService.Save();

        if (await dialogManager.ShowDialogAsync(dialog) == true)
            Process.StartShellExecute("https://tyrrrz.me/ukraine?source=youtubedownloader");
    }

    private async Task ShowDevelopmentBuildMessageAsync()
    {
        if (!Program.IsDevelopmentBuild)
            return;

        // If debugging, the user is likely a developer
        if (Debugger.IsAttached)
            return;

        var dialog = viewModelManager.GetMessageBoxViewModel(
            localizationManager.UnstableBuildTitle,
            string.Format(localizationManager.UnstableBuildMessage, Program.Name),
            localizationManager.SeeReleasesButton,
            localizationManager.CloseButton
        );

        if (await dialogManager.ShowDialogAsync(dialog) == true)
            Process.StartShellExecute(Program.ProjectReleasesUrl);
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateVersion = await updateService.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            snackbarManager.Notify(
                string.Format(
                    localizationManager.UpdateDownloadingMessage,
                    Program.Name,
                    updateVersion
                )
            );
            await updateService.PrepareUpdateAsync(updateVersion);

            snackbarManager.Notify(
                localizationManager.UpdateReadyMessage,
                localizationManager.UpdateInstallNowButton,
                () =>
                {
                    updateService.FinalizeUpdate(true);

                    if (Application.Current?.ApplicationLifetime?.TryShutdown(2) != true)
                        Environment.Exit(2);
                }
            );
        }
        catch
        {
            // Failure to update shouldn't crash the application
            snackbarManager.Notify(localizationManager.UpdateFailedMessage);
        }
    }

    public override async Task InitializeAsync()
    {
        await ShowUkraineSupportMessageAsync();
        await ShowDevelopmentBuildMessageAsync();
        await CheckForUpdatesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Save settings
            settingsService.Save();

            // Finalize pending updates
            updateService.FinalizeUpdate(false);
        }

        base.Dispose(disposing);
    }
}
