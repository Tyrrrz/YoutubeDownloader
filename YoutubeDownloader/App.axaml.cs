using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using AvaloniaWebView;
using Material.Styles.Themes;
using Microsoft.Extensions.DependencyInjection;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.Views;

namespace YoutubeDownloader;

public class App : Application, IDisposable
{
    private readonly DisposableCollector _eventRoot = new();

    private readonly ServiceProvider _services;
    private readonly SettingsService _settingsService;
    private readonly MainViewModel _mainViewModel;

    public App()
    {
        var services = new ServiceCollection();

        // Framework
        services.AddSingleton<DialogManager>();
        services.AddSingleton<SnackbarManager>();
        services.AddSingleton<ViewManager>();
        services.AddSingleton<ViewModelManager>();

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
        _mainViewModel = _services.GetRequiredService<ViewModelManager>().CreateMainViewModel();

        // Re-initialize the theme when the user changes it
        _eventRoot.Add(
            _settingsService.WatchProperty(
                o => o.Theme,
                () =>
                {
                    RequestedThemeVariant = _settingsService.Theme switch
                    {
                        ThemeVariant.Light => Avalonia.Styling.ThemeVariant.Light,
                        ThemeVariant.Dark => Avalonia.Styling.ThemeVariant.Dark,
                        _ => Avalonia.Styling.ThemeVariant.Default,
                    };

                    InitializeTheme();
                }
            )
        );

        // Apply the selected language when the user changes it
        _eventRoot.Add(_settingsService.WatchProperty(o => o.Language, () => InitializeLanguage()));
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

    private void InitializeLanguage()
    {
        var language = _settingsService.Language;

        if (language == global::YoutubeDownloader.Localization.Language.System)
        {
            // Detect from the system UI culture
            var cultureName = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            language = cultureName switch
            {
                "uk" => global::YoutubeDownloader.Localization.Language.Ukrainian,
                "de" => global::YoutubeDownloader.Localization.Language.German,
                "fr" => global::YoutubeDownloader.Localization.Language.French,
                "es" => global::YoutubeDownloader.Localization.Language.Spanish,
                _ => global::YoutubeDownloader.Localization.Language.English,
            };
        }

        global::YoutubeDownloader.Localization.Localization.Current.Language = language;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainView { DataContext = _mainViewModel };

        base.OnFrameworkInitializationCompleted();

        // Set up custom theme colors
        InitializeTheme();

        // Load settings
        _settingsService.Load();

        // Apply the loaded language
        InitializeLanguage();
    }

    private void Application_OnActualThemeVariantChanged(object? sender, EventArgs args) =>
        // Re-initialize the theme when the system theme changes
        InitializeTheme();

    public void Dispose()
    {
        _eventRoot.Dispose();
        _services.Dispose();
    }
}
