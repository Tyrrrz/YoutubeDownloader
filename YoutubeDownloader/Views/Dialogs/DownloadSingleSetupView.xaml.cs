using System.Windows;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs
{
    public partial class DownloadSingleSetupView
    {
        public DownloadSingleSetupView()
        {
            InitializeComponent();
        }

        private void SubtitlesCheckBox_OnUnchecked(object sender, RoutedEventArgs args)
        {
            if (DataContext is DownloadSingleSetupViewModel vm)
                vm.SelectedSubtitleOption = null;
        }
    }
}