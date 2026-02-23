using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> EnglishLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryWatermark)] = "URL or search query",
            [nameof(QueryTooltip)] =
                "Any valid YouTube URL or ID is accepted. Prepend a question mark (?) to perform search by text.",
            [nameof(ProcessQueryTooltip)] = "Process query (Enter)",
            [nameof(AuthTooltip)] = "Authentication",
            [nameof(SettingsTooltip)] = "Settings",
            [nameof(DashboardPlaceholder)] = """
                Copy-paste a **URL** or enter a **search query** to start downloading
                Press **Shift+Enter** to add multiple items
                """,
            [nameof(DownloadsFileColumnHeader)] = "File",
            [nameof(DownloadsStatusColumnHeader)] = "Status",
            [nameof(ContextMenuRemoveSuccessful)] = "Remove successful downloads",
            [nameof(ContextMenuRemoveInactive)] = "Remove inactive downloads",
            [nameof(ContextMenuRestartFailed)] = "Restart failed downloads",
            [nameof(ContextMenuCancelAll)] = "Cancel all downloads",
            [nameof(DownloadStatusEnqueued)] = "Pending...",
            [nameof(DownloadStatusCompleted)] = "Done",
            [nameof(DownloadStatusCanceled)] = "Canceled",
            [nameof(DownloadStatusFailed)] = "Failed",
            [nameof(ClickToCopyErrorTooltip)] = "Note: Click to copy this error message",
            [nameof(ShowFileTooltip)] = "Show file",
            [nameof(PlayTooltip)] = "Play",
            [nameof(CancelDownloadTooltip)] = "Cancel download",
            [nameof(RestartDownloadTooltip)] = "Restart download",
            // Settings
            [nameof(SettingsTitle)] = "Settings",
            [nameof(ThemeLabel)] = "Theme",
            [nameof(ThemeTooltip)] = "Preferred user interface theme",
            [nameof(LanguageLabel)] = "Language",
            [nameof(AutoUpdateLabel)] = "Auto-update",
            [nameof(AutoUpdateTooltip)] = """
                Perform automatic updates on every launch.
                Warning: it's recommended to leave this option enabled to ensure that the app is compatible with the latest version of YouTube.
                """,
            [nameof(PersistAuthLabel)] = "Persist authentication",
            [nameof(PersistAuthTooltip)] =
                "Save authentication cookies to a file so that they can be persisted between sessions",
            [nameof(InjectAltLanguagesLabel)] = "Inject alternative languages",
            [nameof(InjectAltLanguagesTooltip)] =
                "Inject audio tracks in alternative languages (if available) into downloaded files",
            [nameof(InjectSubtitlesLabel)] = "Inject subtitles",
            [nameof(InjectSubtitlesTooltip)] =
                "Inject subtitles (if available) into downloaded files",
            [nameof(InjectTagsLabel)] = "Inject media tags",
            [nameof(InjectTagsTooltip)] = "Inject media tags (if available) into downloaded files",
            [nameof(SkipExistingFilesLabel)] = "Skip existing files",
            [nameof(SkipExistingFilesTooltip)] =
                "When downloading multiple videos, skip those that already have matching files in the output directory",
            [nameof(FileNameTemplateLabel)] = "File name template",
            [nameof(FileNameTemplateTooltip)] =
                "Template used for generating file names for downloaded videos.",
            [nameof(FileNameTemplateTokenNumDesc)] =
                "— video's position in the list (if applicable)",
            [nameof(FileNameTemplateTokenIdDesc)] = "— video ID",
            [nameof(FileNameTemplateTokenTitleDesc)] = "— video title",
            [nameof(FileNameTemplateTokenAuthorDesc)] = "— video author",
            [nameof(FileNameTemplateAvailableTokensLabel)] = "Available tokens:",
            [nameof(ParallelLimitLabel)] = "Parallel limit",
            [nameof(ParallelLimitTooltip)] = "How many downloads can be active at the same time",
            [nameof(FFmpegPathLabel)] = "FFmpeg path",
            [nameof(FFmpegPathTooltip)] =
                "Path to the FFmpeg executable. Leave empty to use auto-detection.",
            [nameof(FFmpegPathWatermark)] = "Auto-detect",
            [nameof(FFmpegPathResetTooltip)] = "Reset to auto-detection",
            [nameof(FFmpegPathBrowseTooltip)] = "Browse for FFmpeg executable",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "Authentication",
            [nameof(AuthenticatedText)] = "You are currently authenticated",
            [nameof(LogOutButton)] = "Log out",
            [nameof(LoadingText)] = "Loading...",
            // Download Single Setup
            [nameof(CopyMenuItem)] = "Copy",
            [nameof(LiveLabel)] = "Live",
            [nameof(AudioLabel)] = "Audio",
            [nameof(FormatLabel)] = "Format",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "Container",
            [nameof(VideoQualityLabel)] = "Video quality",
            // Common buttons
            [nameof(CloseButton)] = "CLOSE",
            [nameof(DownloadButton)] = "DOWNLOAD",
            [nameof(CancelButton)] = "CANCEL",
            [nameof(SettingsButton)] = "SETTINGS",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "Thank you for supporting Ukraine!",
            [nameof(UkraineSupportMessage)] = """
                As Russia wages a genocidal war against my country, I'm grateful to everyone who continues to stand with Ukraine in our fight for freedom.

                Click LEARN MORE to find ways that you can help.
                """,
            [nameof(LearnMoreButton)] = "LEARN MORE",
            [nameof(UnstableBuildTitle)] = "Unstable build warning",
            [nameof(UnstableBuildMessage)] = """
                You're using a development build of {0}. These builds are not thoroughly tested and may contain bugs.

                Auto-updates are disabled for development builds.

                Click SEE RELEASES if you want to download a stable release instead.
                """,
            [nameof(SeeReleasesButton)] = "SEE RELEASES",
            [nameof(FFmpegMissingTitle)] = "FFmpeg is missing",
            [nameof(FFmpegMissingMessage)] = """
                FFmpeg is required for {0} to work. Please download it and make it available in the application directory or on the system PATH, or configure the location in settings.

                Alternatively, you can also download a version of {0} that has FFmpeg bundled with it. Look for release assets that are NOT marked as *.Bare.

                Click DOWNLOAD to go to the FFmpeg download page.
                """,
            [nameof(FFmpegPathMissingMessage)] = """
                FFmpeg is required for this app to work, but the configured path does not exist:
                {0}

                Please update the FFmpeg path in settings or clear it to use auto-detection.
                """,
            [nameof(FFmpegMissingSearchedLabel)] =
                "Searched for '{0}' in the following directories:",
            [nameof(NothingFoundTitle)] = "Nothing found",
            [nameof(NothingFoundMessage)] =
                "Couldn't find any videos based on the query or URL you provided",
            [nameof(ErrorTitle)] = "Error",
            [nameof(UpdateDownloadingMessage)] = "Downloading update to {0} v{1}...",
            [nameof(UpdateReadyMessage)] =
                "Update has been downloaded and will be installed when you exit",
            [nameof(UpdateInstallNowButton)] = "INSTALL NOW",
            [nameof(UpdateFailedMessage)] = "Failed to perform application update",
        };
}
