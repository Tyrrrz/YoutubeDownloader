using System;
using System.Net;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaWebView;
using Material.Styles.Themes;
using Microsoft.Extensions.DependencyInjection;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.Views;

namespace YoutubeDownloader;

public partial class App : Application, IDisposable
{
    private readonly ServiceProvider _services;
    private readonly MainViewModel _mainViewModel;

    public App()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<SettingsService>();
        services.AddSingleton<UpdateService>();

        // View model framework
        services.AddSingleton<DialogManager>();
        services.AddSingleton<SnackbarManager>();
        services.AddSingleton<ViewModelManager>();

        // View models
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DownloadViewModel>();
        services.AddTransient<AuthSetupViewModel>();
        services.AddTransient<DownloadMultipleSetupViewModel>();
        services.AddTransient<DownloadSingleSetupViewModel>();
        services.AddTransient<MessageBoxViewModel>();
        services.AddTransient<SettingsViewModel>();

        // View framework
        services.AddSingleton<ViewManager>();

        _services = services.BuildServiceProvider(true);
        _mainViewModel = _services.GetRequiredService<ViewModelManager>().CreateMainViewModel();
    }

    public override void Initialize()
    {
        // Increase maximum concurrent connections
        ServicePointManager.DefaultConnectionLimit = 20;

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainView { DataContext = _mainViewModel };

        base.OnFrameworkInitializationCompleted();
        
        // Set custom theme colors
        SetDefaultTheme();
    }

    public override void RegisterServices()
    {
        base.RegisterServices();

        AvaloniaWebViewBuilder.Initialize(config => config.IsInPrivateModeEnabled = true);
    }

    public void Dispose() => _services.Dispose();
}

public partial class App
{
    public static void SetLightTheme()
    {
        if (Current is null)
            return;

        Current.RequestedThemeVariant = ThemeVariant.Light;
        Current.LocateMaterialTheme<MaterialThemeBase>().CurrentTheme = Theme.Create(
            Theme.Light,
            Color.Parse("#343838"),
            Color.Parse("#F9A825")
        );

        Current.Resources["SuccessBrush"] = new SolidColorBrush(Colors.DarkGreen);
        Current.Resources["CanceledBrush"] = new SolidColorBrush(Colors.DarkOrange);
        Current.Resources["FailedBrush"] = new SolidColorBrush(Colors.DarkRed);
    }

    public static void SetDarkTheme()
    {
        if (Current is null)
            return;

        Current.RequestedThemeVariant = ThemeVariant.Dark;
        Current.LocateMaterialTheme<MaterialThemeBase>().CurrentTheme = Theme.Create(
            Theme.Dark,
            Color.Parse("#E8E8E8"),
            Color.Parse("#F9A825")
        );

        Current.Resources["SuccessBrush"] = new SolidColorBrush(Colors.LightGreen);
        Current.Resources["CanceledBrush"] = new SolidColorBrush(Colors.Orange);
        Current.Resources["FailedBrush"] = new SolidColorBrush(Colors.OrangeRed);
    }

    public static void SetDefaultTheme()
    {
        if (Current is null)
            return;
        
        if (Current.RequestedThemeVariant == ThemeVariant.Dark)
            SetDarkTheme();
        else
            SetLightTheme();
    }
}
