using System;
using System.Net;
using System.Reflection;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaWebView;
using Material.Styles.Themes;
using PropertyChanged;
using Stylet;
using StyletIoC;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Framework;

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
public partial class App : StyletApplication<RootViewModel>
{
    private static Theme LightTheme { get; } =
        Theme.Create(Theme.Light, Color.Parse("#343838"), Color.Parse("#F9A825"));

    private static Theme DarkTheme { get; } =
        Theme.Create(Theme.Dark, Color.Parse("#E8E8E8"), Color.Parse("#F9A825"));

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

    protected override void OnStart()
    {
        base.OnStart();

        // Set the default theme.
        // Preferred theme will be set later, once the settings are loaded.
        //App.SetLightTheme();

        // Increase maximum concurrent connections
        ServicePointManager.DefaultConnectionLimit = 20;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        base.Initialize();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();
    }

    public override void RegisterServices()
    {
        base.RegisterServices();

        AvaloniaWebViewBuilder.Initialize(default);
    }

    protected override void ConfigureIoC(IStyletIoCBuilder builder)
    {
        base.ConfigureIoC(builder);

        builder.Bind<SettingsService>().ToSelf().InSingletonScope();
        builder.Bind<IViewModelFactory>().ToAbstractFactory();
        builder
            .Bind<IClipboard>()
            .ToFactory(ctx =>
                (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!
                    .MainWindow!
                    .Clipboard
            );
    }

    protected override void OnLaunch()
    {
        base.OnLaunch();
        if (
            ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime classicDesktopStyleApplicationLifetime
        )
        {
            classicDesktopStyleApplicationLifetime.MainWindow = GetActiveWindow();
        }
    }
}
