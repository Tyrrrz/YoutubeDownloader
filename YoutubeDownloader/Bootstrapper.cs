using System.Net;
using Stylet;
using StyletIoC;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Framework;

#if !DEBUG
using System.Windows;
using System.Windows.Threading;
#endif

namespace YoutubeDownloader
{
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        protected override void OnStart()
        {
            base.OnStart();

            // Set default theme
            // (preferred theme will be chosen later, once the settings are loaded)
            App.SetLightTheme();

            // Increase maximum concurrent connections
            ServicePointManager.DefaultConnectionLimit = 20;
        }

        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);

            // Bind singleton services singleton
            builder.Bind<DownloadService>().ToSelf().InSingletonScope();
            builder.Bind<SettingsService>().ToSelf().InSingletonScope();
            builder.Bind<TaggingService>().ToSelf().InSingletonScope();

            // Bind view model factory
            builder.Bind<IViewModelFactory>().ToAbstractFactory();
        }

#if !DEBUG
        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(e);

            MessageBox.Show(e.Exception.ToString(), "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
#endif
    }
}