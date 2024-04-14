using Avalonia.Interactivity;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class SettingsView : UserControl<SettingsViewModel>
{
    public SettingsView() => InitializeComponent();

    private void DarkModeToggleSwitch_OnIsCheckedChanged(object? sender, RoutedEventArgs args)
    {
        if (DarkModeToggleSwitch.IsChecked is true)
        {
            App.SetDarkTheme();
        }
        else if (DarkModeToggleSwitch.IsChecked is false)
        {
            App.SetLightTheme();
        }
        else
        {
            App.SetDefaultTheme();
        }
    }
}
