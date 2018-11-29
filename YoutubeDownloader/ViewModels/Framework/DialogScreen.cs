using Stylet;

namespace YoutubeDownloader.ViewModels.Framework
{
    public abstract class DialogScreen<T> : Screen
    {
        public T DialogResult { get; private set; }

        public void Close(T dialogResult = default(T))
        {
            // Set the result
            DialogResult = dialogResult;

            // If there is a parent - ask them to close this dialog
            if (Parent != null)
                RequestClose(Equals(dialogResult, default(T)));
            // Otherwise close ourselves
            else
                ((IScreenState) this).Close();
        }
    }

    public abstract class DialogScreen : DialogScreen<bool?>
    {
    }
}