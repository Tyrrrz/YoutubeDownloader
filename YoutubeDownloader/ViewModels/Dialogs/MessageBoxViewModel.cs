using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeDownloader.Framework;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class MessageBoxViewModel : DialogViewModelBase
{
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsExtraButtonVisible))]
    [NotifyPropertyChangedFor(nameof(ButtonsCount))]
    public partial string? ExtraButtonText { get; set; }

    public bool IsDefaultButtonVisible => !string.IsNullOrWhiteSpace(DefaultButtonText);

    public bool IsCancelButtonVisible => !string.IsNullOrWhiteSpace(CancelButtonText);

    public bool IsExtraButtonVisible => !string.IsNullOrWhiteSpace(ExtraButtonText);

    public int ButtonsCount =>
        (IsDefaultButtonVisible ? 1 : 0)
        + (IsExtraButtonVisible ? 1 : 0)
        + (IsCancelButtonVisible ? 1 : 0);
}
