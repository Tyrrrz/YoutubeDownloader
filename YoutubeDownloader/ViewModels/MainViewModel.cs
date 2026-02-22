using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Components;

namespace YoutubeDownloader.ViewModels;

public partial class MainViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    SnackbarManager snackbarManager,
    SettingsService settingsService,
    UpdateService updateService
) : ViewModelBase
{
    public string Title { get; } = $"{Program.Name} v{Program.VersionString}";

    public DashboardViewModel Dashboard { get; } = viewModelManager.CreateDashboardViewModel();

    private async Task ShowUkraineSupportMessageAsync()
    {
        if (!settingsService.IsUkraineSupportMessageEnabled)
            return;

        var dialog = viewModelManager.CreateMessageBoxViewModel(
            Localization.UkraineSupportTitle,
            Localization.UkraineSupportMessage,
            Localization.LearnMoreButton,
            Localization.CloseButton
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

        var dialog = viewModelManager.CreateMessageBoxViewModel(
            Localization.UnstableBuildTitle,
            string.Format(Localization.UnstableBuildMessage, Program.Name),
            Localization.SeeReleasesButton,
            Localization.CloseButton
        );

        if (await dialogManager.ShowDialogAsync(dialog) == true)
            Process.StartShellExecute(Program.ProjectReleasesUrl);
    }

    private async Task ShowFFmpegMissingMessageAsync()
    {
        if (settingsService.FFmpegFilePath is { } ffmpegFilePath)
        {
            // Explicit path set — only show the dialog if the file is missing
            if (File.Exists(ffmpegFilePath))
                return;

            var dialog = viewModelManager.CreateMessageBoxViewModel(
                Localization.FFmpegMissingTitle,
                string.Format(Localization.FFmpegPathMissingMessage, ffmpegFilePath),
                Localization.SettingsButton,
                Localization.CloseButton
            );

            if (await dialogManager.ShowDialogAsync(dialog) == true)
                await dialogManager.ShowDialogAsync(viewModelManager.CreateSettingsViewModel());
        }
        else
        {
            // No explicit path — fall back to auto-detection check
            if (FFmpeg.TryGetCliFilePath() is not null)
                return;

            var dialog = viewModelManager.CreateMessageBoxViewModel(
                Localization.FFmpegMissingTitle,
                $"""
                {string.Format(Localization.FFmpegMissingMessage, Program.Name)}

                ――――――――――――――――――――――――――――――――――――――――――

                Searched for '{FFmpeg.CliFileName}' in the following directories:
                {string.Join(
                    Environment.NewLine,
                    FFmpeg.GetProbeDirectoryPaths().Distinct(StringComparer.Ordinal).Select(d =>
                        $"- {d}"
                    )
                )}
                """,
                Localization.DownloadButton,
                Localization.CloseButton
            );

            if (await dialogManager.ShowDialogAsync(dialog) == true)
                Process.StartShellExecute("https://ffmpeg.org/download.html");
        }

        if (Application.Current?.ApplicationLifetime?.TryShutdown(3) != true)
            Environment.Exit(3);
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateVersion = await updateService.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            snackbarManager.Notify(
                string.Format(Localization.UpdateDownloadingMessage, Program.Name, updateVersion)
            );
            await updateService.PrepareUpdateAsync(updateVersion);

            snackbarManager.Notify(
                Localization.UpdateReadyMessage,
                Localization.UpdateInstallNowButton,
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
            snackbarManager.Notify(Localization.UpdateFailedMessage);
        }
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await ShowUkraineSupportMessageAsync();
        await ShowDevelopmentBuildMessageAsync();
        await ShowFFmpegMissingMessageAsync();
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
