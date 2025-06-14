using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Avalonia;
using Avalonia.Android;

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

    protected override async void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        await Task.Run(AndroidFFmpegInitializer.InitializeAsync);

        CheckAndRequestPermissionsAsync();
    }

    private string[] GetRequiredPermissions()
    {
        var permissions = new List<string>
        {
            Manifest.Permission.Internet
        };

        // Handle storage permissions based on Android version
        if (OperatingSystem.IsAndroidVersionAtLeast(33)) // Android 13+ (API 33+)
        {
            // For Android 13+, we need specific media permissions
            permissions.Add(Manifest.Permission.ReadMediaAudio);
            permissions.Add(Manifest.Permission.ReadMediaImages);
            permissions.Add(Manifest.Permission.ReadMediaVideo);
        }
        else if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            permissions.Add(Manifest.Permission.ManageMedia);
        }
        else if (OperatingSystem.IsAndroidVersionAtLeast(29)) // Android 10+ (API 29+)
        {
            // For Android 10-12, READ_EXTERNAL_STORAGE still works for reading
            // but WRITE_EXTERNAL_STORAGE is deprecated
            permissions.Add(Manifest.Permission.ReadExternalStorage);
        }
        else
        {
            // For Android 9 and below, use traditional permissions
            permissions.Add(Manifest.Permission.ReadExternalStorage);
            permissions.Add(Manifest.Permission.WriteExternalStorage);
        }

        return [.. permissions];
    }

    private void CheckAndRequestPermissionsAsync()
    {
        // Step 1: Handle MANAGE_EXTERNAL_STORAGE for Android 11+ first
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            if (!global::Android.OS.Environment.IsExternalStorageManager)
            {
                ShowManageStorageDialog();
                return; // Wait for user action
            }
        }

        // Step 2: Check regular permissions
        var requiredPermissions = GetRequiredPermissions();
        var permissionsToRequest = new List<string>();

        foreach (var permission in requiredPermissions)
        {
            if (ContextCompat.CheckSelfPermission(this, permission) != Permission.Granted)
            {
                permissionsToRequest.Add(permission);
            }
        }

        if (permissionsToRequest.Count > 0)
        {
            // Check if we should show rationale for any permission
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
        builder.SetMessage("This app needs to access your device storage to download and save YouTube videos. " +
                          "Please grant 'All files access' permission in the next screen.");
        builder.SetPositiveButton("Grant Permission", (sender, e) =>
        {
            RequestManageExternalStoragePermission();
        });
        builder.SetNegativeButton("Cancel", (sender, e) =>
        {
            Finish(); // Close app if user cancels
        });
        builder.SetCancelable(false);
        builder.Show();
    }

    private void ShowPermissionRationaleDialog(List<string> permissions)
    {
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Permissions Required");
        builder.SetMessage("This app requires the following permissions to function properly:\n\n" +
                          string.Join("\n", permissions.Select(FormatPermissionName)) +
                          "\n\nPlease grant these permissions to continue.");
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
                    // Fallback to general manage all files settings
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
                    // Any denied permission results in app closure
                    ShowPermissionsDeniedDialog(deniedPermissions);
                }
            }
        }
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
                    // Storage permission granted, now check other permissions
                    CheckAndRequestPermissionsAsync();
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
        builder.SetMessage("The following permissions are required for the app to function:\n\n" +
                          string.Join("\n", deniedPermissions.Select(FormatPermissionName)) +
                          "\n\nThe app cannot continue without these permissions and will now close.");
        builder.SetPositiveButton("Exit", (sender, e) =>
        {
            Finish();
        });
        builder.SetCancelable(false);
        builder.Show();
    }

    private void ShowCriticalPermissionDeniedDialog(List<string> deniedPermissions)
    {
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Critical Permissions Required");
        builder.SetMessage("The following critical permissions are required for the app to function:\n\n" +
                          string.Join("\n", deniedPermissions.Select(FormatPermissionName)) +
                          "\n\nThe app cannot continue without these permissions.");
        builder.SetPositiveButton("Open Settings", (sender, e) =>
        {
            OpenAppSettings();
            // Close app after opening settings
            Finish();
        });
        builder.SetNegativeButton("Exit", (sender, e) =>
        {
            Finish();
        });
        builder.SetCancelable(false);
        builder.Show();
    }

    private void ShowNonCriticalPermissionDialog(List<string> deniedPermissions)
    {
        var builder = new AlertDialog.Builder(this);
        builder.SetTitle("Permissions Denied");
        builder.SetMessage("The following permissions were denied:\n\n" +
                          string.Join("\n", deniedPermissions.Select(FormatPermissionName)) +
                          "\n\nThe app cannot function properly without these permissions and will now close.");
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

    private void OpenAppSettings()
    {
        try
        {
            var intent = new Intent(Settings.ActionApplicationDetailsSettings);
            intent.SetData(global::Android.Net.Uri.Parse($"package:{PackageName}"));
            StartActivity(intent);
        }
        catch (Exception)
        {
            try
            {
                var intent = new Intent(Settings.ActionSettings);
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to open settings: {ex.Message}");
            }
        }
    }

    private string FormatPermissionName(string permission)
    {
        if (permission == Manifest.Permission.Internet)
            return "• Internet Access";
        if (permission == Manifest.Permission.ReadExternalStorage)
            return "• Read External Storage";
        if (permission == Manifest.Permission.WriteExternalStorage)
            return "• Write External Storage";
        if (permission == "MANAGE_EXTERNAL_STORAGE")
            return "• Manage External Storage";
        
        // Handle version-specific permissions
        if (OperatingSystem.IsAndroidVersionAtLeast(33))
        {
            if (permission == Manifest.Permission.ReadMediaAudio)
                return "• Read Media Audio";
            if (permission == Manifest.Permission.ReadMediaImages)
                return "• Read Media Images";
            if (permission == Manifest.Permission.ReadMediaVideo)
                return "• Read Media Video";
        }
        else if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            if (permission == Manifest.Permission.ManageMedia)
                return "• Manage Media";
        }

        return $"• {permission}";
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder);
    }
}