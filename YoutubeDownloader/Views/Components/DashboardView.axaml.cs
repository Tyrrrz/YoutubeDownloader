using System;
using System.Linq;
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

    private void UserControl_OnDragOver(object? sender, DragEventArgs args)
    {
        if (
            args.DataTransfer.Contains(DataFormat.Text)
            || args.DataTransfer.Contains(DataFormat.File)
        )
            args.DragEffects = DragDropEffects.Copy | DragDropEffects.Link;
        else
            args.DragEffects = DragDropEffects.None;
    }

    private void UserControl_OnDrop(object? sender, DragEventArgs args)
    {
        var text = args.DataTransfer.TryGetText();
        if (string.IsNullOrWhiteSpace(text))
        {
            var files = args.DataTransfer.TryGetFiles();
            if (files != null && files.Length > 0)
            {
                var paths = files
                    .Select(f => f.Path.ToString())
                    .Where(p => !string.IsNullOrWhiteSpace(p));
                text = string.Join(Environment.NewLine, paths);
            }
        }

        if (string.IsNullOrWhiteSpace(text))
            return;

        if (DataContext is DashboardViewModel viewModel)
        {
            var currentQuery = viewModel.Query ?? string.Empty;
            var trimmedText = text.Trim();

            if (string.IsNullOrWhiteSpace(currentQuery))
            {
                viewModel.Query = trimmedText;
            }
            else
            {
                viewModel.Query = $"{currentQuery.TrimEnd()}{Environment.NewLine}{trimmedText}";
            }
        }
    }

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
        if (sender is IDataContextProvider { DataContext: DownloadViewModel dataContext })
            dataContext.CopyErrorMessageCommand.ExecuteIfCan(null);
    }
}
