using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gress;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class ProgressDialogViewModel : DialogViewModelBase
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly DisposableCollector _eventRoot = new();

    public ProgressDialogViewModel(LocalizationManager localizationManager)
    {
        LocalizationManager = localizationManager;

        _eventRoot.Add(
            Progress.WatchProperty(
                o => o.Current,
                () => OnPropertyChanged(nameof(IsProgressIndeterminate))
            )
        );
    }

    public LocalizationManager LocalizationManager { get; }

    [ObservableProperty]
    public partial string? Title { get; set; }

    public ProgressContainer<Percentage> Progress { get; } = new();

    public bool IsProgressIndeterminate => Progress.Current.Fraction is <= 0 or >= 1;

    public Func<IProgress<Percentage>, CancellationToken, Task>? Operation { get; set; }

    [RelayCommand]
    private async Task RunOperationAsync()
    {
        if (Operation is null)
        {
            Close(true);
            return;
        }

        try
        {
            await Operation(Progress, _cancellationTokenSource.Token);
        }
        catch
        {
            // Ignore errors (cancel, download failure, etc.)
        }
        finally
        {
            Close(true);
        }
    }

    [RelayCommand]
    private void Cancel() => _cancellationTokenSource.Cancel();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventRoot.Dispose();
            _cancellationTokenSource.Dispose();
        }

        base.Dispose(disposing);
    }
}
