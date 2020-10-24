using System.Windows;

namespace YoutubeDownloader.Views.Dialogs
{
    public partial class SettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void DarkModeToggleButton_Checked(object sender, RoutedEventArgs args) =>
            App.SetDarkTheme();

        private void DarkModeToggleButton_Unchecked(object sender, RoutedEventArgs args) =>
            App.SetLightTheme();
    }
}