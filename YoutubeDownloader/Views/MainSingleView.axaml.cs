using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using YoutubeDownloader.Framework;
using YoutubeDownloader.ViewModels;

namespace YoutubeDownloader.Views;

public partial class MainSingleView : UserControl<MainViewModel>
{
    private static readonly Uri UkraineUri = new("https://tyrrrz.me/ukraine");

    public MainSingleView() => InitializeComponent();

    // Launcher rather than a shell execute: Android has no shell to hand a URL to, and this
    // resolves to an intent there and to the default browser everywhere else.
    private async void UkraineButton_OnClick(object? sender, RoutedEventArgs args)
    {
        if (TopLevel.GetTopLevel(this) is { } topLevel)
            await topLevel.Launcher.LaunchUriAsync(UkraineUri);
    }
}
