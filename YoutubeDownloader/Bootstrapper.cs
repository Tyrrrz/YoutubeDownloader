using System;
using System.IO;
using System.Net;
using System.Windows;
using Stylet;
using StyletIoC;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Framework;

#if !DEBUG
using System.Windows;
using System.Windows.Threading;
#endif

namespace YoutubeDownloader;

public class Bootstrapper : Bootstrapper<RootViewModel>
{
    protected override void OnStart()
    {
        base.OnStart();

        // Set the default theme.
        // Preferred theme will be set later, once the settings are loaded.
        App.SetLightTheme();

        // Increase maximum concurrent connections
        ServicePointManager.DefaultConnectionLimit = 20;
    }

    protected override void OnExit(ExitEventArgs args)
    {
        // Remove WebView2 browsing data
        try
        {
            Directory.Delete(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "YoutubeDownloader.exe.WebView2"),
                true
            );
        }
        catch
        {
            // Ignore
        }

        base.OnExit(args);
    }

    protected override void ConfigureIoC(IStyletIoCBuilder builder)
    {
        base.ConfigureIoC(builder);

        builder.Bind<SettingsService>().ToSelf().InSingletonScope();
        builder.Bind<IViewModelFactory>().ToAbstractFactory();
    }

#if !DEBUG
    protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs args)
    {
        base.OnUnhandledException(args);

        MessageBox.Show(
            args.Exception.ToString(),
            "Error occured",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }
#endif
}