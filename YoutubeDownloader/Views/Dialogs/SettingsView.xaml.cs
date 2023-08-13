using Avalonia.Interactivity;
using YoutubeDownloader.Views.Components;

namespace YoutubeDownloader.Views.Dialogs;

public partial class SettingsView : UserControlBase
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void DarkModeToggleButton_OnChecked(object sender, RoutedEventArgs args) =>
    App.SetDarkTheme();

    private void DarkModeToggleButton_OnUnchecked(object sender, RoutedEventArgs args) =>
        App.SetLightTheme();
}
