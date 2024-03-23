using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.Views.Framework;

namespace YoutubeDownloader.Views.Dialogs;

public partial class MessageBoxView : ViewModelAwareUserControl<MessageBoxViewModel>
{
    public MessageBoxView()
    {
        InitializeComponent();
    }
}
