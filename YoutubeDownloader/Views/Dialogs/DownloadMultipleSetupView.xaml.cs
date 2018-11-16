using System.Windows;

namespace YoutubeDownloader.Views.Dialogs
{
    public partial class DownloadMultipleSetupView
    {
        public DownloadMultipleSetupView()
        {
            InitializeComponent();

            if (Application.Current.MainWindow != null)
            {
                Width = Application.Current.MainWindow.ActualWidth * 0.85;
                MaxHeight = Application.Current.MainWindow.ActualHeight * 0.85;
            }
        }
    }
}