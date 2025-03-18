using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace YoutubeDownloader.Framework;

public abstract partial class DialogViewModelBase<T> : ViewModelBase
{
    private readonly TaskCompletionSource<T> _closeTcs = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );

    [ObservableProperty]
    public partial T? DialogResult { get; set; }

    [RelayCommand]
    protected void Close(T dialogResult)
    {
        DialogResult = dialogResult;
        _closeTcs.TrySetResult(dialogResult);
    }

    public async Task<T> WaitForCloseAsync() => await _closeTcs.Task;
}

public abstract class DialogViewModelBase : DialogViewModelBase<bool?>;
