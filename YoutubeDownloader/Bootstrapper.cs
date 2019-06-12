using System.Windows;
using System.Windows.Threading;
using Stylet;
using StyletIoC;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader
{
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);

            // Bind SettingsService as singleton
            builder.Bind<SettingsService>().ToSelf().InSingletonScope();

            // Bind DownloadService as singleton
            builder.Bind<DownloadService>().ToSelf().InSingletonScope();

            // Bind view model factory
            builder.Bind<IViewModelFactory>().ToAbstractFactory();
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(e);

            MessageBox.Show(e.Exception.ToString(), "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}