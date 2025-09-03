﻿using System;
using System.Collections.Generic;
using System.Linq;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Avalonia;
using Avalonia.Android;
using YoutubeDownloader.ViewModels.Components;

namespace YoutubeDownloader.Android;

[Activity(
    Label = "YoutubeDownloader",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private const int PERMISSION_REQUEST_CODE = 1001;
    private const int MANAGE_STORAGE_REQUEST_CODE = 1002;
    private PowerManager.WakeLock? _wakeLock;

    protected override async void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        DashboardViewModel.DownloadingStatusChanged += (sender, isDownloading) =>
        {
            if (isDownloading)
                DontSleepDevice();
            else
                AllowSleepDevice();
        };

        InitializeTheme();

        CheckAndRequestPermissions();

        await AndroidFFmpegInitializer.InitializeAsync();
    }

    private void DontSleepDevice()
    {
        PowerManager powerManager = (PowerManager)GetSystemService(PowerService)!;
        _wakeLock = powerManager!.NewWakeLock(WakeLockFlags.Full | WakeLockFlags.AcquireCausesWakeup | WakeLockFlags.OnAfterRelease, "YourApp::NoSleepWakeLock");
        _wakeLock!.Acquire();
    }

    private void AllowSleepDevice()
    {
        if (_wakeLock != null && _wakeLock.IsHeld)
        {
            _wakeLock.Release();
            _wakeLock = null;
        }
    }

    private static string[] GetRequiredPermissions()
    {
        var permissions = new List<string>
        {
            Manifest.Permission.Internet,
        };

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
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            if (!global::Android.OS.Environment.IsExternalStorageManager)
            {
                ShowManageStorageDialog();
                return;
            }
        }

        var requiredPermissions = GetRequiredPermissions();
        var permissionsToRequest = new List<string>();

        foreach (var permission in requiredPermissions)
        {
            if (permission == Manifest.Permission.ManageExternalStorage)
                continue;

            if (ContextCompat.CheckSelfPermission(this, permission) != Permission.Granted)
            {
                permissionsToRequest.Add(permission);
            }
        }

        if (permissionsToRequest.Count > 0)
        {
            bool shouldShowRationale = permissionsToRequest.Any(permission =>
                ActivityCompat.ShouldShowRequestPermissionRationale(this, permission));

            if (shouldShowRationale)
            {
                ShowPermissionRationaleDialog(permissionsToRequest);
            }
            else
            {
                ActivityCompat.RequestPermissions(this, [.. permissionsToRequest], PERMISSION_REQUEST_CODE);
            }
        }
    }

    private void ShowManageStorageDialog()
    {
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Storage Access Required");

        builder.SetMessage($"""
        This app needs to access your device storage to download and save YouTube videos. 
        Please grant 'All files access' permission in the next screen.
        """);

        builder.SetPositiveButton("Grant Permission", (sender, e) =>
        {
            RequestManageExternalStoragePermission();
        });

        builder.SetNegativeButton("Cancel", (sender, e) =>
        {
            Finish();
        });

        builder.SetCancelable(false);
        builder.Show();
    }

    private void ShowPermissionRationaleDialog(List<string> permissions)
    {
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Permissions Required");

        string permissionList = string.Join(System.Environment.NewLine, permissions.Select(FormatPermissionName));

        builder.SetMessage($"""
        This app requires the following permissions to function properly:

        {permissionList}

        Please grant these permissions to continue.
        """);

        builder.SetPositiveButton("Grant Permissions", (sender, e) =>
        {
            ActivityCompat.RequestPermissions(this, [.. permissions], PERMISSION_REQUEST_CODE);
        });

        builder.SetNegativeButton("Cancel", (sender, e) =>
        {
            Finish(); // Close app if user cancels
        });

        builder.SetCancelable(false);
        builder.Show();
    }


    private void RequestManageExternalStoragePermission()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            try
            {
                var intent = new Intent(Settings.ActionManageAppAllFilesAccessPermission);
                intent.SetData(global::Android.Net.Uri.Parse($"package:{PackageName}"));
                StartActivityForResult(intent, MANAGE_STORAGE_REQUEST_CODE);
            }
            catch (Exception)
            {
                if (OperatingSystem.IsAndroidVersionAtLeast(30))
                {
                    try
                    {
                        var intent = new Intent(Settings.ActionManageAllFilesAccessPermission);
                        StartActivityForResult(intent, MANAGE_STORAGE_REQUEST_CODE);
                    }
                    catch (Exception ex)
                    {
                        ShowPermissionDeniedDialog($"Unable to open storage settings: {ex.Message}");
                    }
                }
            }
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == PERMISSION_REQUEST_CODE)
            {
                var deniedPermissions = new List<string>();

                for (int i = 0; i < permissions.Length; i++)
                {
                    if (grantResults[i] != Permission.Granted)
                    {
                        deniedPermissions.Add(permissions[i]);
                    }
                }

                if (deniedPermissions.Count > 0)
                {
                    var criticalPermissions = deniedPermissions.Where(IsCriticalPermission).ToList();

                    if (criticalPermissions.Count != 0)
                    {
                        ShowPermissionsDeniedDialog(criticalPermissions);
                    }
                    else
                    {
                        // Non-critical permissions can be handled gracefully
                        // Continue with app functionality
                    }
                }
            }
        }
    }

    private bool IsCriticalPermission(string permission)
    {
        return permission == Manifest.Permission.Internet ||
               permission == Manifest.Permission.ReadExternalStorage ||
               permission == Manifest.Permission.WriteExternalStorage;
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode == MANAGE_STORAGE_REQUEST_CODE)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                if (global::Android.OS.Environment.IsExternalStorageManager)
                {
                    CheckAndRequestPermissions();
                }
                else
                {
                    ShowPermissionDeniedDialog("Storage access is required for downloading files. The app cannot function without it.");
                }
            }
        }
    }

    private void ShowPermissionsDeniedDialog(List<string> deniedPermissions)
    {
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Permissions Required");

        string permissionList = string.Join(System.Environment.NewLine, deniedPermissions.Select(FormatPermissionName));

        builder.SetMessage($"""
        The following permissions are required for the app to function:

        {permissionList}

        The app cannot continue without these permissions and will now close.
        """);

        builder.SetPositiveButton("Exit", (sender, e) =>
        {
            Finish();
        });

        builder.SetCancelable(false);
        builder.Show();
    }


    private void ShowPermissionDeniedDialog(string message)
    {
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Permission Required");
        builder.SetMessage(message + "\n\nThe app will now close.");
        builder.SetPositiveButton("Exit", (sender, e) =>
        {
            Finish();
        });
        builder.SetCancelable(false);
        builder.Show();
    }

    private string FormatPermissionName(string permission)
    {
        return permission switch
        {
            Manifest.Permission.Internet => "• Internet Access",
            Manifest.Permission.ReadExternalStorage => "• Read External Storage",
            Manifest.Permission.WriteExternalStorage => "• Write External Storage",
            Manifest.Permission.WakeLock => "• Wake Lock",
            _ => $"• {permission}",
        };
    }

    private void InitializeTheme()
    {
        var currentMode = Resources!.Configuration!.UiMode & UiMode.NightMask;

        switch (currentMode)
        {
            case UiMode.NightYes:
                // Dark theme active
#pragma warning disable CA1422 // Validate platform compatibility
                Window!.SetNavigationBarColor(global::Android.Graphics.Color.Argb(0xFF, 0x42, 0x42, 0x42));
                Window!.SetStatusBarColor(global::Android.Graphics.Color.Argb(0xFF, 0x42, 0x42, 0x42));
#pragma warning restore CA1422
                break;
            case UiMode.NightNo:
                // Light theme active
#pragma warning disable CA1422
                Window!.SetNavigationBarColor(global::Android.Graphics.Color.Argb(255, 255, 255, 255));
                Window!.SetStatusBarColor(global::Android.Graphics.Color.Argb(255, 255, 255, 255));
#pragma warning restore CA1422
                break;
        }
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);

        var currentMode = newConfig.UiMode & UiMode.NightMask;

        switch (currentMode)
        {
            case UiMode.NightYes:
                // Dark theme active
#pragma warning disable CA1422 // Validate platform compatibility
                Window!.SetNavigationBarColor(global::Android.Graphics.Color.Argb(0xFF, 0x42, 0x42, 0x42));
                Window!.SetStatusBarColor(global::Android.Graphics.Color.Argb(0xFF, 0x42, 0x42, 0x42));
#pragma warning restore CA1422 // Validate platform compatibility
                break;
            case UiMode.NightNo:
                // Light theme active
#pragma warning disable CA1422 // Validate platform compatibility (total bullshit I ain't spending time on drawing custom background for simple color for future API)
                Window!.SetNavigationBarColor(global::Android.Graphics.Color.Argb(255, 255, 255, 255));
                Window!.SetStatusBarColor(global::Android.Graphics.Color.Argb(255, 255, 255, 255));
#pragma warning restore CA1422 // Validate platform compatibility
                break;
        }
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder);
    }
}