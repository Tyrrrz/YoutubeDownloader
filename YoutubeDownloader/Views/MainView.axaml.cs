using Avalonia.Interactivity;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels;

namespace YoutubeDownloader.Views;

public partial class MainView : Window<MainViewModel>
{
    public MainView() => InitializeComponent();

    private void DialogHost_OnLoaded(object? sender, RoutedEventArgs args) =>
        DataContext.InitializeCommand.Execute(null);
}
