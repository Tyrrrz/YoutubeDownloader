### v1.8 (18-Apr-2021)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v6.0.
- Removed video upload date from UI and from file name templates.

### v1.7.16 (29-Nov-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.1.9.
- Fixed an issue where excluded formats were ignored when downloading multiple videos. (Thanks [@derech1e](https://github.com/derech1e))

### v1.7.15 (25-Oct-2020)

- Added subtitle download option when downloading single videos. (Thanks [@beawolf](https://github.com/beawolf))
- Added format exclusion list. You can configure in settings a list of containers which you would like to not see, and they will be filtered out in the format selection dropdown. (Thanks [@beawolf](https://github.com/beawolf))
- Added dark mode. You can enable it in settings. (Thanks [@Andrew Kolos](https://github.com/andrewkolos))
- Added video quality preference selection when downloading multiple videos. (Thanks [@Bartłomiej Rogowski](https://github.com/brogowski))
- Added circular progress bars for each individual active download.
- Added meta tag injection for mp4 files. This adds channel and upload date information, as well as thumbnail. (Thanks [@beawolf](https://github.com/beawolf))
- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.1.8.

### v1.7.14 (29-Sep-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.1.6.
- Changed the order in which new downloads appear in the list so that newest downloads are at the top. (Thanks [@Max](https://github.com/badijm))

### v1.7.13 (12-Sep-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.1.5.

### v1.7.12 (29-Jul-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.1.3.

### v1.7.11 (21-Jul-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.1.2.

### v1.7.10 (06-Jul-2020)

- Fixed an issue where mp4 download options took much longer to download due to unnecessary transcoding.

### v1.7.9 (02-Jul-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.1.1.

### v1.7.8 (10-May-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.0.4.

### v1.7.7 (07-May-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.0.3.
- Fixed an issue where conversion progress was not correctly reported. Updated to YoutubeExplode.Converter v1.5.1.

### v1.7.6 (13-Apr-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v5.0.1.
- Improved media tagging. Now it's less reliant on MusicBrainz and should attempt to tag files more often.
- Fixed some issues related to auto-update functionality.

### v1.7.5 (16-Mar-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v4.7.16.

### v1.7.4 (11-Mar-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v4.7.14.

### v1.7.3 (10-Feb-2020)

- Fixed various YouTube-related issues. Updated to YoutubeExplode v4.7.13.

### v1.7.2 (15-Dec-2019)

- Fixed an issue where some videos didn't have any download options. Updated to YoutubeExplode v4.7.11.

### v1.7.1 (15-Nov-2019)

- Fixed an issue where trying to download a single video resulted in an error.

### v1.7 (14-Nov-2019)

- Migrated to .NET Core 3. You will need to install .NET Core runtime in order to run this application starting from this version. You can download it [here](https://dotnet.microsoft.com/download/dotnet-core/current/runtime).
- Added setting "Skip downloads for files that already exist" which, when enabled, skips downloading videos that already have a matching file in the destination directory. Thanks [@mostafa901](https://github.com/mostafa901).
- Changed default file name template to `$title`. You can change it in settings.
- Fixed an issue where the number token in file name template didn't get replaced properly for single-video downloads.

### v1.6.1 (22-Sep-2019)

- Fixed an issue where starting new downloads was not possible if there were already active downloads.

### v1.6 (14-Sep-2019)

- Added support for processing multiple queries in one go. Separate multiple URLs/IDs/searches with new lines (Shift+Enter) to specify multiple queries.
- Added file name template which is used when generating file names for downloaded videos. You can configure it in settings. Refer to the tooltip text for information on what each variable does.
- Added automatic media tagging for downloaded videos (currently only audio files). Tags are resolved from MusicBrainz based on video title. This feature can be disabled in settings.
- Added a context menu button to remove all successfully finished downloads.
- Added a context menu button to restart all failed downloads.
- Added a context menu button to copy title in download setup dialog.
- Starting a new download that overwrites an existing download will now remove the latter from the list.

### v1.5.7 (15-Aug-2019)

- Fixed an issue where some videos failed to download. Updated to YoutubeExplode v4.7.9.

### v1.5.6 (30-Jul-2019)

- Fixed an issue where all videos failed to download. Updated to YoutubeExplode v4.7.7.

### v1.5.5 (27-Jul-2019)

- Fixed an issue where some videos failed to download.

### v1.5.4 (10-Jul-2019)

- Fixed an issue where an attempt to download any video resulted in an error. Updated to YoutubeExplode v4.7.6.

### v1.5.3 (04-Jul-2019)

- Fixed an issue where an attempt to download from channel always resulted in an error. Updated to YoutubeExplode v4.7.5

### v1.5.2 (29-Jun-2019)

- Fixed an issue where some videos were missing from playlists. Updated to YoutubeExplode v4.7.4.
- Fixed an issue where the application crashed when pressing the "play" button if there is no program associated with that file type. An error message is now shown instead.
- Added a context menu button to remove specific download from the list.

### v1.5.1 (21-Jun-2019)

- Fixed an issue where most videos failed to download due to recent YouTube changes. Updated to YoutubeExplode v4.7.3.
- Popups can now be closed by clicking away.
- Default max concurrent download count is now 2 instead of being devised from processor count. You can still tweak it as you want in settings.

### v1.5 (15-Jun-2019)

- Changed the presentation of active downloads to use a data grid.
- Added a context menu button to clear all finished downloads from the list.
- Improved UI by making the general style more consistent.
- Fixed an issue where a download sometimes failed due to a race condition in progress reporting. Updated to Gress v1.1.1.

### v1.4 (13-Jun-2019)

- Fixed an issue where the application crashed when an active download failed. Failure will now be reported in the UI with the option to restart download.
- Fixed an issue where the application crashed when trying to download an unavailable video. Popup with the error message will now be shown instead.
- Fixed an issue where the application crashed due to unknown encoding in some videos. Updated to YoutubeExplode v4.7.2.

### v1.3.2 (12-May-2019)

- Fixed an issue where the application crashed when trying to download videos. Updated to YoutubeExplode v4.7.

### v1.3.1 (03-Mar-2019)

- Fixed an issue where channel URLs were not recognized in some cases. The underlying issue was fixed in YoutubeExplode v4.6.5.
- Fixed an issue where the application would crash sometimes because the progress reported was too high. The underlying issue was fixed in YoutubeExplode.Converter v1.4.1.

### v1.3 (14-Feb-2019)

- Added ability to download videos by channel ID or URL.
- Added ability to download videos by user URL.
- Aggregated progress of all downloads is now shown in the main progress bar and in the taskbar.
- Downloads that have been queued up but not yet started now show "Pending..." instead of "0.00%".
- Selection list for multiple videos now uses Ctrl/Shift to select multiple items.

### v1.2 (19-Jan-2019)

- Added video quality selection when dowloading a single video. For playlists and search results, the highest video quality available for selected format is used.
- Added support for `ogg` format.
- Added support for `webm` format when dowloading a single video. May not always be available.
- Updated the app icon to make it more distinct from YoutubeExplode.
- Fixed an issue where child FFmpeg processes would not exit after the user closes the app while there are active downloads.
- Fixed an issue where the app could sometimes crash when checking for updates.
- Fixed an issue where it was possible to start multiple downloads to the same file path.

### v1.1.1 (22-Dec-2018)

- The list of downloads is now always sorted chronologically.
- When adding multiple downloads, the application will try to ensure unique file names by appending suffixes in order to avoid accidental overwrites.
- Fixed an issue where adding multiple downloads would sometimes cause the application to crash.

### v1.1 (21-Dec-2018)

- Improved UI in all screens.
- Limited the number of concurrent downloads and added an option in settings to configure it.
- Last used download format is now persisted so you don't have to select it all the time.
- Fixed an issue where temporary files were not deleted after the download was canceled. The underlying issue was fixed in YoutubeExplode.Converter v1.0.3 and CliWrap v2.2.
- Fixed an issue where starting multiple downloads would cause them to be added in the wrong order to the list of downloads.