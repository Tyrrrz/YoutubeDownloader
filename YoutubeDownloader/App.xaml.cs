using System;
using System.Net;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaWebView;
using Material.Styles.Themes;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeDownloader.Views;
using YoutubeDownloader.Views.Framework;

namespace YoutubeDownloader;

public partial class App
{
    private static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();

    public static new string Name { get; } = Assembly.GetName().Name!;

    public static Version Version { get; } = Assembly.GetName().Version!;

    public static string VersionString { get; } = Version.ToString(3);

    public static string ProjectUrl { get; } = "https://github.com/Tyrrrz/YoutubeDownloader";

    public static string LatestReleaseUrl { get; } = ProjectUrl + "/releases/latest";
}

[DoNotNotify]
public partial class App : Application
{
    private readonly IServiceProvider? _serviceProvider;

    private static Theme LightTheme { get; } =
        Theme.Create(Theme.Light, Color.Parse("#343838"), Color.Parse("#F9A825"));

    private static Theme DarkTheme { get; } =
        Theme.Create(Theme.Dark, Color.Parse("#E8E8E8"), Color.Parse("#F9A825"));

    public App()
    {
        _serviceProvider = ConfigureServices();
    }

    public static void SetLightTheme()
    {
        App.Current!.RequestedThemeVariant = ThemeVariant.Light;
        var theme = App.Current!.LocateMaterialTheme<MaterialThemeBase>();
        theme.CurrentTheme = LightTheme;

        Current!.Resources["SuccessBrush"] = new SolidColorBrush(Colors.DarkGreen);
        Current!.Resources["CanceledBrush"] = new SolidColorBrush(Colors.DarkOrange);
        Current!.Resources["FailedBrush"] = new SolidColorBrush(Colors.DarkRed);
    }

    public static void SetDarkTheme()
    {
        App.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        var theme = App.Current!.LocateMaterialTheme<MaterialThemeBase>();
        theme.CurrentTheme = DarkTheme;

        Current!.Resources["SuccessBrush"] = new SolidColorBrush(Colors.LightGreen);
        Current!.Resources["CanceledBrush"] = new SolidColorBrush(Colors.Orange);
        Current!.Resources["FailedBrush"] = new SolidColorBrush(Colors.OrangeRed);
    }

    public override void Initialize()
    {
        // Increase maximum concurrent connections
        ServicePointManager.DefaultConnectionLimit = 20;

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (_serviceProvider is null)
        {
            return; // fix for Designer
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var rootViewModel = ActivatorUtilities.CreateInstance<RootViewModel>(_serviceProvider);

            desktop.MainWindow = new RootView { DataContext = rootViewModel, };
        }

        base.OnFrameworkInitializationCompleted();

        // var settinsService = _serviceProvider.GetService<SettingsService>();
        //
        // settinsService?.Load();
        //
        // if (settinsService?.IsDarkModeEnabled ?? false)
        // {
        //     SetDarkTheme();
        // }
        // else
        // {
        //     SetLightTheme();
        // }
    }

    public override void RegisterServices()
    {
        base.RegisterServices();

        AvaloniaWebViewBuilder.Initialize(config => config.IsInPrivateModeEnabled = true);
    }

    protected ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<SettingsService>();
        services.AddSingleton<UpdateService>();
        services.AddSingleton<IViewManager, ViewManager>();
        services.AddSingleton<DialogManager>();
        services.AddSingleton<IViewModelFactory, ViewModelFactory>();
        services.AddTransient<IClipboard>(sp =>
            sp.GetRequiredService<IViewManager>().GetTopLevel()!.Clipboard!
        );
        services.AddTransient<IApplicationLifetime>(sp => App.Current!.ApplicationLifetime!);
        services.AddTransient<IControlledApplicationLifetime>(sp =>
            (App.Current!.ApplicationLifetime! as IControlledApplicationLifetime)!
        );

        services.AddSingleton(sp => App.Current!.PlatformSettings!);

        return services.BuildServiceProvider(true);
    }
}
