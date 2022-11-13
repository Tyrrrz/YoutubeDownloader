using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Onova;
using Onova.Exceptions;
using Onova.Services;
using YoutubeDownloader.Core.Downloading;
using static System.Net.WebRequestMethods;

namespace YoutubeDownloader.Services;

public class UpdateService : IDisposable
{
    private IUpdateManager _updateManager = new UpdateManager(
new GithubPackageResolver("Tyrrrz", "YoutubeDownloader", "YoutubeDownloader.zip"),
new ZipPackageExtractor()
);

    private readonly SettingsService _settingsService;

    private Version? _updateVersion;
    private bool _updatePrepared;
    private bool _updaterLaunched;

    public UpdateService(SettingsService settingsService)
    {
        _settingsService = settingsService;

    }
    public void Reset()
    {
        var client = Proxy.Apply(_settingsService.UseProxy, _settingsService.ProxyAddress);
        _updateManager = new UpdateManager(
                                new GithubPackageResolver(client, "Tyrrrz", "YoutubeDownloader", "YoutubeDownloader.zip"),
                                new ZipPackageExtractor()
                            );
    }
    public async Task<Version?> CheckForUpdatesAsync()
    {
        if (!_settingsService.IsAutoUpdateEnabled)
            return null;
        var check = await _updateManager.CheckForUpdatesAsync();
        return check.CanUpdate ? check.LastVersion : null;
    }

    public async Task PrepareUpdateAsync(Version version)
    {
        if (!_settingsService.IsAutoUpdateEnabled)
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
        if (!_settingsService.IsAutoUpdateEnabled)
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

    public void Dispose() => _updateManager.Dispose();
}