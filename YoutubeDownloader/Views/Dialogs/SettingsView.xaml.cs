namespace YoutubeDownloader.Views.Dialogs
{
    public partial class SettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void DarkThemeToggle_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            setBaseTheme(Theme.Dark);
        }

        private void DarkThemeToggle_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            setBaseTheme(Theme.Light);
        }

        private void setBaseTheme(Theme theme)
        {
            Theme.SetCurrent(theme);
        }
    }
}