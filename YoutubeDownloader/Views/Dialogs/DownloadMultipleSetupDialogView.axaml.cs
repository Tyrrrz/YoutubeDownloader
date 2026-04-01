using Avalonia.Interactivity;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels.Dialogs;

namespace YoutubeDownloader.Views.Dialogs;

public partial class DownloadMultipleSetupDialogView : UserControl<DownloadMultipleSetupDialogViewModel>
{
    public DownloadMultipleSetupDialogView() => InitializeComponent();

    private void UserControl_OnLoaded(object? sender, RoutedEventArgs args) =>
        DataContext.InitializeCommand.Execute(null);
}
