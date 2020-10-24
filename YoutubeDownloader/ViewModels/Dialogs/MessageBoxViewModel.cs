using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class MessageBoxViewModel : DialogScreen
    {
        public string? Title { get; set; }

        public string? Message { get; set; }
    }

    public static class MessageBoxViewModelExtensions
    {
        public static MessageBoxViewModel CreateMessageBoxViewModel(
            this IViewModelFactory factory,
            string title,
            string message)
        {
            var viewModel = factory.CreateMessageBoxViewModel();

            viewModel.Title = title;
            viewModel.Message = message;

            return viewModel;
        }
    }
}