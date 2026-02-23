using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class MessageBoxViewModel : DialogViewModelBase
{
    public MessageBoxViewModel(LocalizationManager localizationManager)
    {
        LocalizationManager = localizationManager;

        DefaultButtonText = LocalizationManager.CloseButton;
        CancelButtonText = LocalizationManager.CancelButton;
    }

    public LocalizationManager LocalizationManager { get; }

    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial string? Message { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDefaultButtonVisible))]
    [NotifyPropertyChangedFor(nameof(ButtonsCount))]
    public partial string? DefaultButtonText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCancelButtonVisible))]
    [NotifyPropertyChangedFor(nameof(ButtonsCount))]
    public partial string? CancelButtonText { get; set; }

    public bool IsDefaultButtonVisible => !string.IsNullOrWhiteSpace(DefaultButtonText);

    public bool IsCancelButtonVisible => !string.IsNullOrWhiteSpace(CancelButtonText);

    public int ButtonsCount => (IsDefaultButtonVisible ? 1 : 0) + (IsCancelButtonVisible ? 1 : 0);
}
