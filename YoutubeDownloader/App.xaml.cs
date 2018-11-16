using System.Net;
using System.Windows;
using System.Windows.Threading;

namespace YoutubeDownloader
{
    public partial class App
    {
        static App()
        {
            // Increase connection limit to allow multiple simultaneous downloads
            ServicePointManager.DefaultConnectionLimit = 10;
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            MessageBox.Show(args.Exception.ToString(), "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
