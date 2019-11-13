using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class MessageBoxViewModel : DialogScreen
    {
        public string Title { get; set; }

        public string Message { get; set; }
    }
}