using System;
using Stylet;

namespace YoutubeDownloader.ViewModels.Framework
{
    public abstract class DialogScreen<T> : PropertyChangedBase
    {
        // ReSharper disable once RedundantDefaultMemberInitializer
        public T DialogResult { get; private set; } = default!;

        public event EventHandler? Closed;

        public void Close(T dialogResult = default)
        {
            DialogResult = dialogResult!;
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }

    public abstract class DialogScreen : DialogScreen<bool?>
    {
    }
}