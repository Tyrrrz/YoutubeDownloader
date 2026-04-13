using System.Windows.Input;

namespace YoutubeDownloader.Utils.Extensions;

internal static class CommandExtensions
{
    extension(ICommand command)
    {
        public void ExecuteIfCan(object? parameter = null)
        {
            if (command.CanExecute(parameter))
                command.Execute(parameter);
        }
    }
}
