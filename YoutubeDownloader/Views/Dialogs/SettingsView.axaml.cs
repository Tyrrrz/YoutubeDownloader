using Avalonia.Interactivity;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class SettingsView : UserControl<SettingsViewModel>
{
    public SettingsView() => InitializeComponent();

    private void DarkModeToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs args)
    {
        if (DarkModeToggleButton.IsChecked is true)
        {
            App.SetDarkTheme();
        }
        else
        {
            App.SetLightTheme();
        }
    }
}
