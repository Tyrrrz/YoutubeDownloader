using YoutubeDownloader.ViewModels.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

public class MessageBoxViewModel : DialogScreen
{
    public string? Title { get; set; }

    public string? Message { get; set; }

    public bool IsOkButtonVisible { get; set; } = true;

    public string? OkButtonText { get; set; }

    public bool IsCancelButtonVisible { get; set; }

    public string? CancelButtonText { get; set; }

    public int ButtonsCount =>
        (IsOkButtonVisible ? 1 : 0) +
        (IsCancelButtonVisible ? 1 : 0);
}

public static class MessageBoxViewModelExtensions
{
    public static MessageBoxViewModel CreateMessageBoxViewModel(
        this IViewModelFactory factory,
        string title,
        string message,
        string? okButtonText,
        string? cancelButtonText)
    {
        var viewModel = factory.CreateMessageBoxViewModel();
        viewModel.Title = title;
        viewModel.Message = message;

        viewModel.IsOkButtonVisible = !string.IsNullOrWhiteSpace(okButtonText);
        viewModel.OkButtonText = okButtonText;
        viewModel.IsCancelButtonVisible = !string.IsNullOrWhiteSpace(cancelButtonText);
        viewModel.CancelButtonText = cancelButtonText;

        return viewModel;
    }

    public static MessageBoxViewModel CreateMessageBoxViewModel(
        this IViewModelFactory factory,
        string title,
        string message) =>
        factory.CreateMessageBoxViewModel(title, message, "CLOSE", null);
}