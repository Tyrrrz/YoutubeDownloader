using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Onova;
using Onova.Exceptions;
using Onova.Services;

namespace YoutubeDownloader.Services;

public class UpdateService(SettingsService settingsService) : IDisposable
{
    private readonly IUpdateManager? _updateManager =
        OperatingSystem.IsWindows() && StartOptions.Current.IsAutoUpdateAllowed
            ? new UpdateManager(
                new GithubPackageResolver(
                    "Tyrrrz",
                    "YoutubeDownloader",
                    // Examples:
                    // YoutubeDownloader.win-arm64.zip
                    // YoutubeDownloader.win-x64.zip
                    // YoutubeDownloader.linux-x64.zip
                    $"YoutubeDownloader.{RuntimeInformation.RuntimeIdentifier}.zip"
                ),
                new ZipPackageExtractor()
            )
            : null;

    private Version? _updateVersion;
    private bool _isUpdatePrepared;
    private bool _isUpdaterLaunched;

    public async Task<Version?> CheckForUpdatesAsync()
    {
        if (_updateManager is null)
            return null;

        if (!settingsService.IsAutoUpdateEnabled)
            return null;

        var check = await _updateManager.CheckForUpdatesAsync();
        return check.CanUpdate ? check.LastVersion : null;
    }

    public async Task PrepareUpdateAsync(Version version)
    {
        if (_updateManager is null)
            return;

        if (!settingsService.IsAutoUpdateEnabled)
            return;

        try
        {
            await _updateManager.PrepareUpdateAsync(_updateVersion = version);
            _isUpdatePrepared = true;
        }
        catch (UpdaterAlreadyLaunchedException)
        {
            // Ignore race conditions
        }
        catch (LockFileNotAcquiredException)
        {
            // Ignore race conditions
        }
    }

    public void FinalizeUpdate(bool needRestart)
    {
        if (_updateManager is null)
            return;

        if (!settingsService.IsAutoUpdateEnabled)
            return;

        if (_updateVersion is null || !_isUpdatePrepared || _isUpdaterLaunched)
            return;

        try
        {
            _updateManager.LaunchUpdater(_updateVersion, needRestart);
            _isUpdaterLaunched = true;
        }
        catch (UpdaterAlreadyLaunchedException)
        {
            // Ignore race conditions
        }
        catch (LockFileNotAcquiredException)
        {
            // Ignore race conditions
        }
    }

    public void Dispose() => _updateManager?.Dispose();
}
