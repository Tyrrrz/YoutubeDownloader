using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;

namespace YoutubeDownloader.Utils.Extensions;

internal static class AvaloniaExtensions
{
    extension(IApplicationLifetime lifetime)
    {
        public Window? TryGetMainWindow() =>
            lifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime
                ? desktopLifetime.MainWindow
                : null;

        public TopLevel? TryGetTopLevel() =>
            lifetime.TryGetMainWindow()
            ?? (lifetime as ISingleViewApplicationLifetime)?.MainView?.GetVisualRoot() as TopLevel;

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
