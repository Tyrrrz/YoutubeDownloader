using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Views;
using YoutubeDownloader.ViewModels.Components;
// Android.App declares a DownloadStatus of its own, so disambiguate against the app's.
using DownloadStatus = YoutubeDownloader.ViewModels.Components.DownloadStatus;

namespace YoutubeDownloader.Android;

/// <summary>
/// Keeps the screen awake while any download is in flight.
/// </summary>
/// <remarks>
/// Uses the KeepScreenOn window flag rather than a PowerManager wake lock: the flag is tied
/// to the window lifetime, so it cannot outlive the activity or leak if the process is
/// killed mid-download. Everything here reads the shared view models through their public
/// surface, so the cross-platform projects need no Android-specific hooks.
/// </remarks>
internal sealed class KeepScreenOnWatcher
{
    private readonly Activity _activity;
    private readonly DashboardViewModel _dashboard;

    private KeepScreenOnWatcher(Activity activity, DashboardViewModel dashboard)
    {
        _activity = activity;
        _dashboard = dashboard;
    }

    public static KeepScreenOnWatcher Attach(Activity activity, DashboardViewModel dashboard)
    {
        var watcher = new KeepScreenOnWatcher(activity, dashboard);

        dashboard.Downloads.CollectionChanged += watcher.OnDownloadsChanged;
        foreach (var download in dashboard.Downloads)
            download.PropertyChanged += watcher.OnDownloadPropertyChanged;

        watcher.Refresh();
        return watcher;
    }

    private void OnDownloadsChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        foreach (var download in args.OldItems?.OfType<DownloadViewModel>() ?? [])
            download.PropertyChanged -= OnDownloadPropertyChanged;

        foreach (var download in args.NewItems?.OfType<DownloadViewModel>() ?? [])
            download.PropertyChanged += OnDownloadPropertyChanged;

        Refresh();
    }

    private void OnDownloadPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is null or nameof(DownloadViewModel.Status))
            Refresh();
    }

    private void Refresh()
    {
        var isActive = _dashboard.Downloads.Any(d =>
            d.Status is DownloadStatus.Enqueued or DownloadStatus.Started
        );

        // Window flags must be touched on the UI thread.
        _activity.RunOnUiThread(() =>
        {
            if (_activity.Window is not { } window)
                return;

            if (isActive)
                window.AddFlags(WindowManagerFlags.KeepScreenOn);
            else
                window.ClearFlags(WindowManagerFlags.KeepScreenOn);
        });
    }
}
