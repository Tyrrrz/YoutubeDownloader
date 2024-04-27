using System;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
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
            "Thank you for supporting Ukraine!",
            """
            As Russia wages a genocidal war against my country, I'm grateful to everyone who continues to stand with Ukraine in our fight for freedom.

            Click LEARN MORE to find ways that you can help.
            """,
            "LEARN MORE",
            "CLOSE"
        );

        // Disable this message in the future
        settingsService.IsUkraineSupportMessageEnabled = false;
        settingsService.Save();

        if (await dialogManager.ShowDialogAsync(dialog) == true)
            ProcessEx.StartShellExecute("https://tyrrrz.me/ukraine?source=youtubedownloader");
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateVersion = await updateService.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            snackbarManager.Notify($"Downloading update to {Program.Name} v{updateVersion}...");
            await updateService.PrepareUpdateAsync(updateVersion);

            snackbarManager.Notify(
                "Update has been downloaded and will be installed when you exit",
                "INSTALL NOW",
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
            snackbarManager.Notify("Failed to perform application update");
        }
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        // Reset settings (needed to resolve the default dark mode setting)
        settingsService.Reset();

        // Load settings
        settingsService.Load();

        // Set the correct theme
        if (settingsService.IsDarkModeEnabled)
            App.SetDarkTheme();
        else
            App.SetLightTheme();

        await ShowUkraineSupportMessageAsync();
        await CheckForUpdatesAsync();

        // App has just been updated, display the changelog
        if (
            settingsService.LastAppVersion is not null
            && settingsService.LastAppVersion != Program.Version
        )
        {
            snackbarManager.Notify(
                $"Successfully updated to {Program.Name} v{Program.VersionString}",
                "WHAT'S NEW",
                () => ProcessEx.StartShellExecute(Program.LatestReleaseUrl)
            );

            settingsService.LastAppVersion = Program.Version;
            settingsService.Save();
        }
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
