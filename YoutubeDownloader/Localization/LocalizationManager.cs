using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager : ObservableObject, IDisposable
{
    private readonly DisposableCollector _eventRoot = new();

    public LocalizationManager(SettingsService settingsService)
    {
        _eventRoot.Add(
            settingsService.WatchProperty(
                o => o.Language,
                () => Language = settingsService.Language,
                true
            )
        );

        _eventRoot.Add(
            this.WatchProperty(
                o => o.Language,
                () =>
                {
                    foreach (var propertyName in EnglishLocalization.Keys)
                        OnPropertyChanged(propertyName);
                }
            )
        );
    }

    [ObservableProperty]
    public partial Language Language { get; set; } = Language.System;

    private string Get([CallerMemberName] string? key = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        var localization = Language switch
        {
            Language.System =>
                CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName.ToLowerInvariant() switch
                {
                    "ukr" => UkrainianLocalization,
                    "deu" => GermanLocalization,
                    "fra" => FrenchLocalization,
                    "spa" => SpanishLocalization,
                    _ => EnglishLocalization,
                },
            Language.Ukrainian => UkrainianLocalization,
            Language.German => GermanLocalization,
            Language.French => FrenchLocalization,
            Language.Spanish => SpanishLocalization,
            _ => EnglishLocalization,
        };

        if (
            localization.TryGetValue(key, out var value)
            // English is used as a fallback
            || EnglishLocalization.TryGetValue(key, out value)
        )
        {
            return value;
        }

        return $"Missing localization for '{key}'";
    }

    public void Dispose() => _eventRoot.Dispose();
}

public partial class LocalizationManager
{
    // ---- Dashboard ----

    public string QueryWatermark => Get();
    public string QueryTooltip => Get();
    public string ProcessQueryTooltip => Get();
    public string AuthTooltip => Get();
    public string SettingsTooltip => Get();
    public string DashboardPlaceholder => Get();
    public string DownloadsFileColumnHeader => Get();
    public string DownloadsStatusColumnHeader => Get();
    public string ContextMenuRemoveSuccessful => Get();
    public string ContextMenuRemoveInactive => Get();
    public string ContextMenuRestartFailed => Get();
    public string ContextMenuCancelAll => Get();
    public string DownloadStatusEnqueued => Get();
    public string DownloadStatusCompleted => Get();
    public string DownloadStatusCanceled => Get();
    public string DownloadStatusFailed => Get();
    public string ClickToCopyErrorTooltip => Get();
    public string ShowFileTooltip => Get();
    public string PlayTooltip => Get();
    public string CancelDownloadTooltip => Get();
    public string RestartDownloadTooltip => Get();

    // ---- Settings ----

    public string SettingsTitle => Get();
    public string ThemeLabel => Get();
    public string ThemeTooltip => Get();
    public string LanguageLabel => Get();
    public string LanguageTooltip => Get();
    public string AutoUpdateLabel => Get();
    public string AutoUpdateTooltip => Get();
    public string PersistAuthLabel => Get();
    public string PersistAuthTooltip => Get();
    public string InjectAltLanguagesLabel => Get();
    public string InjectAltLanguagesTooltip => Get();
    public string InjectSubtitlesLabel => Get();
    public string InjectSubtitlesTooltip => Get();
    public string InjectTagsLabel => Get();
    public string InjectTagsTooltip => Get();
    public string SkipExistingFilesLabel => Get();
    public string SkipExistingFilesTooltip => Get();
    public string FileNameTemplateLabel => Get();
    public string FileNameTemplateTooltip => Get();
    public string ParallelLimitLabel => Get();
    public string ParallelLimitTooltip => Get();
    public string FFmpegPathLabel => Get();
    public string FFmpegPathTooltip => Get();
    public string FFmpegPathWatermark => Get();
    public string FFmpegPathResetTooltip => Get();
    public string FFmpegPathBrowseTooltip => Get();

    // ---- Auth Setup ----

    public string AuthenticationTitle => Get();
    public string AuthenticatedText => Get();
    public string LogOutButton => Get();
    public string LoadingText => Get();

    // ---- Download Single Setup ----

    public string CopyMenuItem => Get();
    public string LiveLabel => Get();
    public string AudioLabel => Get();
    public string FormatLabel => Get();

    // ---- Download Multiple Setup ----

    public string ContainerLabel => Get();
    public string VideoQualityLabel => Get();

    // ---- Common buttons ----

    public string CloseButton => Get();
    public string DownloadButton => Get();
    public string CancelButton => Get();
    public string SettingsButton => Get();

    // ---- Dialog messages ----

    public string UkraineSupportTitle => Get();
    public string UkraineSupportMessage => Get();
    public string LearnMoreButton => Get();
    public string UnstableBuildTitle => Get();
    public string UnstableBuildMessage => Get();
    public string SeeReleasesButton => Get();
    public string FFmpegMissingTitle => Get();
    public string FFmpegMissingMessage => Get();
    public string FFmpegPathMissingMessage => Get();
    public string FFmpegMissingSearchedLabel => Get();
    public string NothingFoundTitle => Get();
    public string NothingFoundMessage => Get();
    public string ErrorTitle => Get();
    public string UpdateDownloadingMessage => Get();
    public string UpdateReadyMessage => Get();
    public string UpdateInstallNowButton => Get();
    public string UpdateFailedMessage => Get();
}
