using System.Windows;

namespace YoutubeDownloader.Views.Dialogs
{
    public partial class DownloadSingleSetupView
    {
        public DownloadSingleSetupView()
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