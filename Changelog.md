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