using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PowerKit.Extensions;

namespace YoutubeDownloader.Utils.Extensions;

internal static class AvaloniaExtensions
{
    extension(IApplicationLifetime lifetime)
    {
        public Control? TryGetMainView() =>
            lifetime switch
            {
                IClassicDesktopStyleApplicationLifetime desktopLifetime =>
                    desktopLifetime.MainWindow,

                ISingleViewApplicationLifetime singleViewLifetime => singleViewLifetime.MainView,

                _ => null,
            };

        public TopLevel? TryGetTopLevel() => lifetime.TryGetMainView()?.Pipe(TopLevel.GetTopLevel);

        public bool TryShutdown(int exitCode = 0)
        {
            if (lifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return desktopLifetime.TryShutdown(exitCode);
            }

            if (lifetime is IControlledApplicationLifetime controlledLifetime)
            {
                controlledLifetime.Shutdown(exitCode);
                return true;
            }

            return false;
        }
    }
}
