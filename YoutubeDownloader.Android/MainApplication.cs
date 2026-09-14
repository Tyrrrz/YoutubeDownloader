using System;
using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace YoutubeDownloader.Android;

/// <summary>
/// Binds the shared Avalonia application to the Android host.
/// </summary>
/// <remarks>
/// Avalonia 12 moved the app type from the activity (previously
/// <c>AvaloniaMainActivity&lt;TApp&gt;</c>) onto the Android Application object, so the
/// activity is now non-generic and this class names the app instead.
/// </remarks>
[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership)
    : AvaloniaAndroidApplication<global::YoutubeDownloader.App>(handle, ownership)
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).LogToTrace();
}
