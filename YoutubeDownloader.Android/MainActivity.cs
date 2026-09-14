using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Avalonia.Android;
using Avalonia.Controls.ApplicationLifetimes;
using YoutubeDownloader.ViewModels;

namespace YoutubeDownloader.Android;

[Activity(
    Label = "YoutubeDownloader",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation
        | ConfigChanges.ScreenSize
        | ConfigChanges.UiMode
)]
public class MainActivity : AvaloniaMainActivity
{
    private const int PermissionRequestCode = 1001;
    private const int ManageStorageRequestCode = 1002;

    // Held so the subscriptions it owns live as long as the activity.
    private KeepScreenOnWatcher? _keepScreenOnWatcher;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // Avalonia is initialized by the base implementation, so the single-view root and
        // its view model only exist afterwards.
        base.OnCreate(savedInstanceState);

        ApplySystemBarColors(Resources?.Configuration);
        AndroidFFmpegInitializer.Initialize();
        CheckAndRequestPermissions();

        if (TryGetMainViewModel() is { } mainViewModel)
            _keepScreenOnWatcher = KeepScreenOnWatcher.Attach(this, mainViewModel.Dashboard);
    }

    private static MainViewModel? TryGetMainViewModel() =>
        global::Avalonia.Application.Current?.ApplicationLifetime
            is ISingleViewApplicationLifetime { MainView: { } mainView }
            ? mainView.DataContext as MainViewModel
            : null;

    // --- Permissions ---------------------------------------------------------

    private static string[] GetRequiredPermissions()
    {
        var permissions = new List<string> { Manifest.Permission.Internet };

        // Scoped storage replaced the broad storage permissions in Android 10.
        if (!OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            permissions.Add(Manifest.Permission.ReadExternalStorage);
            permissions.Add(Manifest.Permission.WriteExternalStorage);
        }

        permissions.Add(Manifest.Permission.WakeLock);

        return [.. permissions];
    }

    private void CheckAndRequestPermissions()
    {
        // Android 11+ gates all-files access behind a settings screen rather than a prompt.
        if (
            OperatingSystem.IsAndroidVersionAtLeast(30)
            && !global::Android.OS.Environment.IsExternalStorageManager
        )
        {
            ShowManageStorageDialog();
            return;
        }

        var permissionsToRequest = GetRequiredPermissions()
            .Where(p => p != Manifest.Permission.ManageExternalStorage)
            .Where(p => ContextCompat.CheckSelfPermission(this, p) != Permission.Granted)
            .ToList();

        if (permissionsToRequest.Count == 0)
            return;

        var shouldShowRationale = permissionsToRequest.Any(p =>
            ActivityCompat.ShouldShowRequestPermissionRationale(this, p)
        );

        if (shouldShowRationale)
            ShowPermissionRationaleDialog(permissionsToRequest);
        else
            ActivityCompat.RequestPermissions(
                this,
                [.. permissionsToRequest],
                PermissionRequestCode
            );
    }

    public override void OnRequestPermissionsResult(
        int requestCode,
        string[] permissions,
        Permission[] grantResults
    )
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode != PermissionRequestCode)
            return;

        var deniedCritical = permissions
            .Where((_, i) => grantResults[i] != Permission.Granted)
            .Where(IsCriticalPermission)
            .ToList();

        if (deniedCritical.Count > 0)
            ShowPermissionsDeniedDialog(deniedCritical);
    }

    private static bool IsCriticalPermission(string permission) =>
        permission
            is Manifest.Permission.Internet
                or Manifest.Permission.ReadExternalStorage
                or Manifest.Permission.WriteExternalStorage;

    private void RequestManageExternalStoragePermission()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
            RequestAllFilesAccess();
    }

    // Split out and annotated so the platform-compatibility analyzer can see the guard
    // holds inside the catch blocks too, not just on the happy path.
    [SupportedOSPlatform("android30.0")]
    private void RequestAllFilesAccess()
    {
        // The per-app settings screen is unavailable on some OEM builds, so fall back to
        // the global one before giving up.
        try
        {
            var intent = new Intent(Settings.ActionManageAppAllFilesAccessPermission);
            intent.SetData(global::Android.Net.Uri.Parse($"package:{PackageName}"));
            StartActivityForResult(intent, ManageStorageRequestCode);
        }
        catch (Exception)
        {
            try
            {
                var intent = new Intent(Settings.ActionManageAllFilesAccessPermission);
                StartActivityForResult(intent, ManageStorageRequestCode);
            }
            catch (Exception ex)
            {
                ShowFatalDialog($"Unable to open storage settings: {ex.Message}");
            }
        }
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode != ManageStorageRequestCode || !OperatingSystem.IsAndroidVersionAtLeast(30))
            return;

        if (global::Android.OS.Environment.IsExternalStorageManager)
            CheckAndRequestPermissions();
        else
            ShowFatalDialog(
                "Storage access is required for downloading files. The app cannot function without it."
            );
    }

    // --- Dialogs -------------------------------------------------------------

    private void ShowManageStorageDialog() =>
        new AlertDialog.Builder(this)
            .SetTitle("Storage Access Required")!
            .SetMessage(
                "This app needs to access your device storage to download and save YouTube videos. "
                    + "Please grant 'All files access' permission in the next screen."
            )!
            .SetPositiveButton(
                "Grant Permission",
                (_, _) => RequestManageExternalStoragePermission()
            )!
            .SetNegativeButton("Cancel", (_, _) => Finish())!
            .SetCancelable(false)!
            .Show();

    private void ShowPermissionRationaleDialog(List<string> permissions) =>
        new AlertDialog.Builder(this)
            .SetTitle("Permissions Required")!
            .SetMessage(
                "This app requires the following permissions to function properly:"
                    + System.Environment.NewLine
                    + System.Environment.NewLine
                    + FormatPermissionList(permissions)
            )!
            .SetPositiveButton(
                "Grant Permissions",
                (_, _) =>
                    ActivityCompat.RequestPermissions(this, [.. permissions], PermissionRequestCode)
            )!
            .SetNegativeButton("Cancel", (_, _) => Finish())!
            .SetCancelable(false)!
            .Show();

    private void ShowPermissionsDeniedDialog(List<string> deniedPermissions) =>
        ShowFatalDialog(
            "The following permissions are required for the app to function:"
                + System.Environment.NewLine
                + System.Environment.NewLine
                + FormatPermissionList(deniedPermissions)
        );

    private void ShowFatalDialog(string message) =>
        new AlertDialog.Builder(this)
            .SetTitle("Permission Required")!
            .SetMessage(
                message
                    + System.Environment.NewLine
                    + System.Environment.NewLine
                    + "The app will now close."
            )!
            .SetPositiveButton("Exit", (_, _) => Finish())!
            .SetCancelable(false)!
            .Show();

    private static string FormatPermissionList(IEnumerable<string> permissions) =>
        string.Join(System.Environment.NewLine, permissions.Select(FormatPermissionName));

    private static string FormatPermissionName(string permission) =>
        permission switch
        {
            Manifest.Permission.Internet => "• Internet Access",
            Manifest.Permission.ReadExternalStorage => "• Read External Storage",
            Manifest.Permission.WriteExternalStorage => "• Write External Storage",
            Manifest.Permission.WakeLock => "• Wake Lock",
            _ => $"• {permission}",
        };

    // --- System bars ---------------------------------------------------------

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        ApplySystemBarColors(newConfig);
    }

    private void ApplySystemBarColors(Configuration? configuration)
    {
        if (configuration is null || Window is not { } window)
            return;

        var color = (configuration.UiMode & UiMode.NightMask) switch
        {
            UiMode.NightYes => global::Android.Graphics.Color.Argb(0xFF, 0x42, 0x42, 0x42),
            UiMode.NightNo => global::Android.Graphics.Color.Argb(0xFF, 0xFF, 0xFF, 0xFF),
            _ => (global::Android.Graphics.Color?)null,
        };

        if (color is not { } barColor)
            return;

        // Deprecated in API 35 in favour of edge-to-edge insets, but still the only way to
        // colour the bars on the API levels this app supports.
#pragma warning disable CA1422
        window.SetNavigationBarColor(barColor);
        window.SetStatusBarColor(barColor);
#pragma warning restore CA1422
    }
}
