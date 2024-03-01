using System;
using CommunityToolkit.Mvvm.Input;

namespace YoutubeDownloader.ViewModels.Framework;

public abstract partial class DialogScreen<T> : ViewModelBase
{
    public T? DialogResult { get; private set; }

    public event EventHandler? Closed;

    [RelayCommand]
    public void Close(T? dialogResult = default)
    {
        DialogResult = dialogResult;
        Closed?.Invoke(this, EventArgs.Empty);
    }
}

public abstract class DialogScreen : DialogScreen<bool?>;
