using System.Windows;
using System.Windows.Threading;

namespace YoutubeDownloader
{
    public partial class App
    {
        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            MessageBox.Show(args.Exception.ToString(), "Error occured", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
