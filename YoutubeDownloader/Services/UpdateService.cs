using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Onova;
using Onova.Exceptions;
using Onova.Services;
using YoutubeDownloader.Core.Downloading;

namespace YoutubeDownloader.Services;

public class UpdateService(SettingsService settingsService) : IDisposable
{
    private readonly IUpdateManager? _updateManager = OperatingSystem.IsWindows()
        ? new UpdateManager(
            new GithubPackageResolver(
                "Tyrrrz",
                "YoutubeDownloader",
                // Examples:
                // YoutubeDownloader.win-arm64.zip
                // YoutubeDownloader.win-x64.zip
                // YoutubeDownloader.linux-x64.zip
                // YoutubeDownloader.Bare.linux-x64.zip
                FFmpeg.IsBundled()
                    ? $"YoutubeDownloader.{RuntimeInformation.RuntimeIdentifier}.zip"
                    : $"YoutubeDownloader.Bare.{RuntimeInformation.RuntimeIdentifier}.zip"
            ),
            new ZipPackageExtractor()
        )
        : null;

    private Version? _updateVersion;
    private bool _updatePrepared;
    private bool _updaterLaunched;

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
            _updatePrepared = true;
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

        if (_updateVersion is null || !_updatePrepared || _updaterLaunched)
            return;

        try
        {
            _updateManager.LaunchUpdater(_updateVersion, needRestart);
            _updaterLaunched = true;
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
