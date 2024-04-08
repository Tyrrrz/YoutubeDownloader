using Avalonia;
using Avalonia.Input;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Components;

namespace YoutubeDownloader.Views.Components;

public partial class DashboardView : UserControl<DashboardViewModel>
{
    public DashboardView() => InitializeComponent();

    private void QueryTextBox_OnKeyDown(object? sender, KeyEventArgs args)
    {
        // Disable new lines when pressing enter without shift
        if (args.Key == Key.Enter && args.KeyModifiers != KeyModifiers.Shift)
        {
            args.Handled = true;

            // We handle the event here so we have to directly "press" the default button
            ProcessQueryButton.Command?.Execute(ProcessQueryButton.CommandParameter);
        }
    }

    private void StatusTextBlock_OnPointerReleased(object sender, PointerReleasedEventArgs args)
    {
        if (sender is IDataContextProvider { DataContext: DownloadViewModel dataContext })
            dataContext.CopyErrorMessageCommand.Execute(null);
    }
}
