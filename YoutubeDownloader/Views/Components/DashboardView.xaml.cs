using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PropertyChanged;

namespace YoutubeDownloader.Views.Components;

[DoNotNotify]
public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        QueryTextBox.AddHandler(InputElement.KeyDownEvent, OnQueryTextBoxKeyDown, RoutingStrategies.Tunnel);
    }

    private void OnQueryTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        // Disable new lines when pressing enter without shift
        if (e.Key == Key.Enter && e.KeyModifiers != KeyModifiers.Shift)
        {
            e.Handled = true;

            // We handle the event here so we have to directly "press" the default button
            ProcessQueryButton.Command?.Execute(ProcessQueryButton.CommandParameter);
        }
    }
}