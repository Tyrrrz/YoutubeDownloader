using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;

namespace YoutubeDownloader.Utils.Extensions;

internal static class AvaloniaExtensions
{
    public static Window? TryGetMainWindow(this IApplicationLifetime lifetime) =>
        lifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime
            ? desktopLifetime.MainWindow
            : null;

    public static TopLevel? TryGetTopLevel(this IApplicationLifetime lifetime) =>
        lifetime.TryGetMainWindow()
        ?? (lifetime as ISingleViewApplicationLifetime)?.MainView?.GetVisualRoot() as TopLevel;

    public static void Shutdown(this IApplicationLifetime lifetime, int exitCode = 0)
    {
        if (lifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.TryShutdown(exitCode);
            return;
        }

        if (lifetime is IControlledApplicationLifetime controlledLifetime)
        {
            controlledLifetime.Shutdown(exitCode);
            return;
        }

        Environment.Exit(exitCode);
    }
}
