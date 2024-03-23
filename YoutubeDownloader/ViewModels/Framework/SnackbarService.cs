using System;
using Avalonia.Threading;
using Material.Styles.Controls;
using Material.Styles.Models;

namespace YoutubeDownloader.ViewModels.Framework;

public class SnackbarService
{
    private readonly TimeSpan _defaultDuration;

    public SnackbarService(TimeSpan defaultDuration)
    {
        _defaultDuration = defaultDuration;
    }

    /// <summary>
    /// Posts to the default SnackBarHost
    /// </summary>
    public void Post(string message, TimeSpan? duration = null)
    {
        SnackbarHost.Post(
            new SnackbarModel(message, duration ?? _defaultDuration),
            null,
            DispatcherPriority.Normal
        );
    }

    /// <summary>
    /// Posts to the default SnackBarHost
    /// </summary>
    public void Post(
        string message,
        string actionText,
        Action actionHandler,
        TimeSpan? duration = null
    )
    {
        SnackbarHost.Post(
            new SnackbarModel(
                message,
                duration ?? _defaultDuration,
                new SnackbarButtonModel { Text = actionText, Action = actionHandler }
            ),
            null,
            DispatcherPriority.Normal
        );
    }
}
