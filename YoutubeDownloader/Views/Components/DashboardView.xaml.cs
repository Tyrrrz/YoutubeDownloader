using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.Views.Framework;

namespace YoutubeDownloader.Views.Components;

public partial class DashboardView : ViewModelAwareUserControl<DashboardViewModel>
{
    public DashboardView()
    {
        InitializeComponent();
        QueryTextBox.AddHandler(
            InputElement.KeyDownEvent,
            (sender, args) =>
            {
                // Disable new lines when pressing enter without shift
                if (args.Key == Key.Enter && args.KeyModifiers != KeyModifiers.Shift)
                {
                    args.Handled = true;

                    // We handle the event here so we have to directly "press" the default button
                    ProcessQueryButton.Command?.Execute(ProcessQueryButton.CommandParameter);
                }
            },
            RoutingStrategies.Tunnel
        );
    }

    public async void OnStatusPointerReleased(object sender, PointerReleasedEventArgs args)
    {
        if (
            args.Pointer.IsPrimary
            && sender is TextBlock { DataContext: DownloadViewModel downloadViewModel }
        )
        {
            await downloadViewModel.CopyErrorMessage();
        }
    }
}
