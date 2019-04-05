using System;
using System.Threading.Tasks;
using Onova;
using Onova.Exceptions;
using Onova.Services;

namespace YoutubeDownloader.Services
{
    public class UpdateService
    {
        private readonly IUpdateManager _updateManager = new UpdateManager(
            new GithubPackageResolver("Tyrrrz", "YoutubeDownloader", "YoutubeDownloader.zip"),
            new ZipPackageExtractor());

        private Version _updateVersion;
        private bool _updaterLaunched;

        public async Task<Version> CheckPrepareUpdateAsync()
        {
            try
            {
                // Check for updates
                var check = await _updateManager.CheckForUpdatesAsync();
                if (!check.CanUpdate)
                    return null;

                // Prepare the update
                await _updateManager.PrepareUpdateAsync(check.LastVersion);

                return _updateVersion = check.LastVersion;
            }
            catch (UpdaterAlreadyLaunchedException)
            {
                return null;
            }
            catch (LockFileNotAcquiredException)
            {
                return null;
            }
        }

        public void FinalizeUpdate(bool needRestart)
        {
            try
            {
                // Check if an update is pending
                if (_updateVersion == null)
                    return;

                // Check if the updater has already been launched
                if (_updaterLaunched)
                    return;

                // Launch the updater
                _updateManager.LaunchUpdater(_updateVersion, needRestart);
                _updaterLaunched = true;
            }
            catch (UpdaterAlreadyLaunchedException)
            {
            }
            catch (LockFileNotAcquiredException)
            {
            }
        }

        public void Dispose() => _updateManager.Dispose();
    }
}