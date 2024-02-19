using Avalonia.Controls;
using PropertyChanged;
using YoutubeDownloader.Views.Framework;

namespace YoutubeDownloader.Views;

[DoNotNotify]
public partial class RootView : ViewModelAwareWindow
{
    public RootView()
    {
        InitializeComponent();
    }
}
