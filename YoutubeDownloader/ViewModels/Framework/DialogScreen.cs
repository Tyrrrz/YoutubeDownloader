using Stylet;

namespace YoutubeDownloader.ViewModels.Framework
{
    public abstract class DialogScreen : Screen
    {
        public void Close(bool? dialogResult = null)
        {
            // If there is a parent - ask them to close this dialog
            if (Parent != null)
                RequestClose(dialogResult);
            // Otherwise close ourselves
            else
                ((IScreenState) this).Close();
        }
    }

    public abstract class DialogScreen<T> : DialogScreen where T : class
    {
        public T DialogResult { get; private set; }

        public void Close(T dialogResult = null)
        {
            // Set the result
            DialogResult = dialogResult;

            // Close
            Close(dialogResult != null);
        }
    }
}