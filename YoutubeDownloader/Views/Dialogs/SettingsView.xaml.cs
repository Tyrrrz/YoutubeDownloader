using Avalonia.Interactivity;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.Views.Framework;

namespace YoutubeDownloader.Views.Dialogs;

public partial class SettingsView : ViewModelAwareUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
    }

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
