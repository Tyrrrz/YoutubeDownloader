using System;

namespace YoutubeDownloader.ViewModels.Framework;

public abstract class DialogScreen<T> : ViewModelBase
{
    public T? DialogResult { get; private set; }

    public event EventHandler? Closed;

    public void Close(T? dialogResult)
    {
        DialogResult = dialogResult;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    public void Close()
    {
        // ReSharper disable once IntroduceOptionalParameters.Global
        Close(default);
    }
}

public abstract class DialogScreen : DialogScreen<bool?>;
