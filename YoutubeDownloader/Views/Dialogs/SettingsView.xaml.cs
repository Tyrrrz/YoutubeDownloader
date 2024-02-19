using Avalonia.Interactivity;
using YoutubeDownloader.Views.Components;

namespace YoutubeDownloader.Views.Dialogs;

public partial class SettingsView : UserControlBase
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
