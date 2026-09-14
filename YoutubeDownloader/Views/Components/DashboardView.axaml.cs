using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using PowerKit.Extensions;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Components;

namespace YoutubeDownloader.Views.Components;

public partial class DashboardView : UserControl<DashboardViewModel>
{
    public DashboardView()
    {
        InitializeComponent();

        // Bind the event with the tunnel strategy to handle keys that take part in writing text
        QueryTextBox.AddHandler(KeyDownEvent, QueryTextBox_OnKeyDown, RoutingStrategies.Tunnel);
    }

    private void UserControl_OnLoaded(object? sender, RoutedEventArgs args) => QueryTextBox.Focus();

    private void QueryTextBox_OnKeyDown(object? sender, KeyEventArgs args)
    {
        // When pressing Enter without Shift, execute the default button command
        // instead of adding a new line.
        if (args.Key == Key.Enter && args.KeyModifiers != KeyModifiers.Shift)
        {
            args.Handled = true;
            ProcessQueryButton.Command?.ExecuteIfCan(ProcessQueryButton.CommandParameter);
        }
    }

    private void StatusTextBlock_OnPointerReleased(object sender, PointerReleasedEventArgs args)
    {
        if (sender is not IDataContextProvider { DataContext: DownloadViewModel dataContext })
            return;

        // Copying is the useful action with a pointer, where the tooltip has already shown
        // the error and the clipboard is somewhere to put it. On a touch screen there is no
        // hover and so no tooltip, which left a failed download with no way to see why - so
        // show the reason instead of silently copying it.
        if (OperatingSystem.IsAndroid())
            dataContext.ShowErrorMessageCommand.ExecuteIfCan(null);
        else
            dataContext.CopyErrorMessageCommand.ExecuteIfCan(null);
    }
}
