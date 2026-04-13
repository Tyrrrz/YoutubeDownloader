using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using AvaloniaWebView;
using Material.Styles.Themes;
using Microsoft.Extensions.DependencyInjection;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader;

public class App : Application, IDisposable
{
    private readonly ServiceProvider _services;
    private readonly SettingsService _settingsService;

    private readonly DisposableCollector _eventRoot = new();

    private bool _isDisposed;

    public App()
    {
        var services = new ServiceCollection();

        // Framework
        services.AddSingleton<DialogManager>();
        services.AddSingleton<SnackbarManager>();
        services.AddSingleton<ViewManager>();
        services.AddSingleton<ViewModelManager>();

        // Localization
        services.AddSingleton<LocalizationManager>();

        // Services
        services.AddSingleton<SettingsService>();
        services.AddSingleton<UpdateService>();

        // View models
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DownloadViewModel>();
        services.AddTransient<AuthSetupViewModel>();
        services.AddTransient<DownloadMultipleSetupViewModel>();
        services.AddTransient<DownloadSingleSetupViewModel>();
        services.AddTransient<MessageBoxViewModel>();
        services.AddTransient<SettingsViewModel>();

        _services = services.BuildServiceProvider(true);
        _settingsService = _services.GetRequiredService<SettingsService>();

        // Re-initialize the theme when the user changes it
        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.Theme,
                v =>
                {
                    RequestedThemeVariant = v switch
                    {
                        ThemeVariant.Light => Avalonia.Styling.ThemeVariant.Light,
                        ThemeVariant.Dark => Avalonia.Styling.ThemeVariant.Dark,
                        _ => Avalonia.Styling.ThemeVariant.Default,
                    };

                    InitializeTheme();
                }
            )
        );
    }

    private void InitializeTheme()
    {
        var actualTheme = RequestedThemeVariant?.Key switch
        {
            "Light" => PlatformThemeVariant.Light,
            "Dark" => PlatformThemeVariant.Dark,
            _ => PlatformSettings?.GetColorValues().ThemeVariant ?? PlatformThemeVariant.Light,
        };

        this.LocateMaterialTheme<MaterialThemeBase>().CurrentTheme =
            actualTheme == PlatformThemeVariant.Light
                ? Theme.Create(Theme.Light, Color.Parse("#343838"), Color.Parse("#F9A825"))
                : Theme.Create(Theme.Dark, Color.Parse("#E8E8E8"), Color.Parse("#F9A825"));
    }

    public override void Initialize()
    {
        base.Initialize();

        AvaloniaXamlLoader.Load(this);
    }

    public override void RegisterServices()
    {
        base.RegisterServices();

        AvaloniaWebViewBuilder.Initialize(config => config.IsInPrivateModeEnabled = true);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Load settings
        _settingsService.Load();

        // Initialize and configure the main window
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewManager = _services.GetRequiredService<ViewManager>();
            var viewModelManager = _services.GetRequiredService<ViewModelManager>();

            desktop.MainWindow = viewManager.TryBindWindow(viewModelManager.GetMainViewModel());

            // Although `App.Dispose()` is invoked from `Program.Main(...)`, on some platforms
            // it may be called too late in the shutdown lifecycle. Attach an exit
            // handler to ensure timely disposal as a safeguard.
            // https://github.com/Tyrrrz/YoutubeDownloader/issues/795
            desktop.Exit += (_, _) => Dispose();
        }

        // Initialize the theme for the first time; must be done after the main window is created
        InitializeTheme();

        base.OnFrameworkInitializationCompleted();
    }

    private void Application_OnActualThemeVariantChanged(object? sender, EventArgs args) =>
        // Re-initialize the theme when the system theme changes
        InitializeTheme();

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        _eventRoot.Dispose();
        _services.Dispose();
    }
}
