using System.Collections.Generic;

namespace YoutubeDownloader;

public partial class Localization
{
    private static readonly IReadOnlyDictionary<string, string> German = new Dictionary<
        string,
        string
    >
    {
        // Dashboard
        [nameof(QueryWatermark)] = "URL oder Suchanfrage",
        [nameof(QueryTooltip)] =
            "Jede gültige YouTube-URL oder -ID wird akzeptiert. Stellen Sie ein Fragezeichen (?) voran, um nach Text zu suchen.",
        [nameof(ProcessQueryTooltip)] = "Anfrage verarbeiten (Enter)",
        [nameof(AuthTooltip)] = "Authentifizierung",
        [nameof(SettingsTooltip)] = "Einstellungen",
        [nameof(DashboardSearchQueryLabel)] = "Suchanfrage",
        [nameof(DashboardShiftEnterLabel)] = "Shift+Enter",
        [nameof(DashboardPlaceholderCopyPasteA)] = "",
        [nameof(DashboardPlaceholderOrEnterA)] = " einfügen oder ",
        [nameof(DashboardPlaceholderToStartDownloading)] = " eingeben um den Download zu starten",
        [nameof(DashboardPlaceholderPress)] = "Drücken Sie ",
        [nameof(DashboardPlaceholderToAddMultiple)] = " um mehrere Einträge hinzuzufügen",
        [nameof(DownloadsFileColumnHeader)] = "Datei",
        [nameof(DownloadsStatusColumnHeader)] = "Status",
        [nameof(ContextMenuRemoveSuccessful)] = "Erfolgreiche Downloads entfernen",
        [nameof(ContextMenuRemoveInactive)] = "Inaktive Downloads entfernen",
        [nameof(ContextMenuRestartFailed)] = "Fehlgeschlagene Downloads neu starten",
        [nameof(ContextMenuCancelAll)] = "Alle Downloads abbrechen",
        [nameof(DownloadStatusEnqueued)] = "Ausstehend...",
        [nameof(DownloadStatusCompleted)] = "Fertig",
        [nameof(DownloadStatusCanceled)] = "Abgebrochen",
        [nameof(DownloadStatusFailed)] = "Fehlgeschlagen",
        [nameof(ClickToCopyErrorTooltip)] = "Hinweis: Klicken zum Kopieren der Fehlermeldung",
        [nameof(ShowFileTooltip)] = "Datei anzeigen",
        [nameof(PlayTooltip)] = "Abspielen",
        [nameof(CancelDownloadTooltip)] = "Download abbrechen",
        [nameof(RestartDownloadTooltip)] = "Download neu starten",
        // Settings
        [nameof(SettingsTitle)] = "Einstellungen",
        [nameof(ThemeLabel)] = "Design",
        [nameof(ThemeTooltip)] = "Bevorzugtes Oberflächendesign",
        [nameof(LanguageLabel)] = "Sprache",
        [nameof(AutoUpdateLabel)] = "Automatische Updates",
        [nameof(AutoUpdateTooltip)] = """
            Automatische Updates bei jedem Start durchführen.
            Hinweis: Es wird empfohlen, diese Option aktiviert zu lassen, um die Kompatibilität mit der neuesten YouTube-Version zu gewährleisten.
            """,
        [nameof(PersistAuthLabel)] = "Authentifizierung speichern",
        [nameof(PersistAuthTooltip)] =
            "Authentifizierungs-Cookies in einer Datei speichern für sitzungsübergreifende Persistenz",
        [nameof(InjectAltLanguagesLabel)] = "Alternative Sprachen einbetten",
        [nameof(InjectAltLanguagesTooltip)] =
            "Audiotracks in alternativen Sprachen (falls verfügbar) in heruntergeladene Dateien einbetten",
        [nameof(InjectSubtitlesLabel)] = "Untertitel einbetten",
        [nameof(InjectSubtitlesTooltip)] =
            "Untertitel (falls verfügbar) in heruntergeladene Dateien einbetten",
        [nameof(InjectTagsLabel)] = "Medien-Tags einbetten",
        [nameof(InjectTagsTooltip)] =
            "Medien-Tags (falls verfügbar) in heruntergeladene Dateien einbetten",
        [nameof(SkipExistingFilesLabel)] = "Vorhandene Dateien überspringen",
        [nameof(SkipExistingFilesTooltip)] =
            "Beim Herunterladen mehrerer Videos solche überspringen, für die bereits passende Dateien im Ausgabeverzeichnis vorhanden sind",
        [nameof(FileNameTemplateLabel)] = "Dateinamen-Vorlage",
        [nameof(FileNameTemplateTooltip)] =
            "Vorlage für die Generierung von Dateinamen heruntergeladener Videos.",
        [nameof(FileNameTemplateTokenNumDesc)] =
            "— Position des Videos in der Liste (falls zutreffend)",
        [nameof(FileNameTemplateTokenIdDesc)] = "— Video-ID",
        [nameof(FileNameTemplateTokenTitleDesc)] = "— Videotitel",
        [nameof(FileNameTemplateTokenAuthorDesc)] = "— Videoautor",
        [nameof(FileNameTemplateAvailableTokensLabel)] = "Verfügbare Token:",
        [nameof(ParallelLimitLabel)] = "Paralleles Limit",
        [nameof(ParallelLimitTooltip)] = "Wie viele Downloads gleichzeitig aktiv sein können",
        [nameof(FFmpegPathLabel)] = "FFmpeg-Pfad",
        [nameof(FFmpegPathTooltip)] =
            "Pfad zur FFmpeg-Programmdatei. Leer lassen für automatische Erkennung.",
        [nameof(FFmpegPathWatermark)] = "Auto",
        [nameof(FFmpegPathResetTooltip)] = "Zurücksetzen auf automatische Erkennung",
        [nameof(FFmpegPathBrowseTooltip)] = "FFmpeg-Programmdatei suchen",
        // Auth Setup
        [nameof(AuthenticationTitle)] = "Authentifizierung",
        [nameof(AuthenticatedText)] = "Sie sind derzeit authentifiziert",
        [nameof(LogOutButton)] = "Abmelden",
        [nameof(LoadingText)] = "Laden...",
        // Download Single Setup
        [nameof(CopyMenuItem)] = "Kopieren",
        [nameof(LiveLabel)] = "Live",
        [nameof(AudioLabel)] = "Audio",
        [nameof(FormatLabel)] = "Format",
        // Download Multiple Setup
        [nameof(ContainerLabel)] = "Container",
        [nameof(VideoQualityLabel)] = "Videoqualität",
        // Common buttons
        [nameof(CloseButton)] = "SCHLIESSEN",
        [nameof(DownloadButton)] = "HERUNTERLADEN",
        [nameof(CancelButton)] = "ABBRECHEN",
        [nameof(SettingsButton)] = "EINSTELLUNGEN",
        // Dialog messages
        [nameof(UkraineSupportTitle)] = "Danke für Ihre Unterstützung der Ukraine!",
        [nameof(UkraineSupportMessage)] = """
            Während Russland einen Vernichtungskrieg gegen mein Land führt, bin ich jedem dankbar, der weiterhin zur Ukraine in unserem Kampf für die Freiheit steht.

            Klicken Sie auf MEHR ERFAHREN um Wege zu finden, wie Sie helfen können.
            """,
        [nameof(LearnMoreButton)] = "MEHR ERFAHREN",
        [nameof(UnstableBuildTitle)] = "Warnung: Instabiler Build",
        [nameof(UnstableBuildMessage)] = """
            Sie verwenden einen Entwicklungs-Build von {0}. Diese Builds wurden nicht gründlich getestet und können Fehler enthalten.

            Automatische Updates sind für Entwicklungs-Builds deaktiviert.

            Klicken Sie auf RELEASES ANZEIGEN um stattdessen einen stabilen Release herunterzuladen.
            """,
        [nameof(SeeReleasesButton)] = "RELEASES ANZEIGEN",
        [nameof(FFmpegMissingTitle)] = "FFmpeg fehlt",
        [nameof(FFmpegMissingMessage)] = """
            FFmpeg wird benötigt damit {0} funktioniert. Bitte laden Sie es herunter und machen Sie es im Anwendungsverzeichnis oder im System-PATH verfügbar, oder konfigurieren Sie den Speicherort in den Einstellungen.

            Alternativ können Sie auch eine Version von {0} herunterladen, die FFmpeg enthält. Suchen Sie nach Release-Dateien, die NICHT mit *.Bare markiert sind.

            Klicken Sie auf HERUNTERLADEN um zur FFmpeg-Downloadseite zu gelangen.
            """,
        [nameof(FFmpegPathMissingMessage)] = """
            FFmpeg wird für diese App benötigt, aber der konfigurierte Pfad existiert nicht:
            {0}

            Bitte aktualisieren Sie den FFmpeg-Pfad in den Einstellungen oder löschen Sie ihn zur automatischen Erkennung.
            """,
        [nameof(FFmpegMissingSearchedLabel)] = "'{0}' wurde in folgenden Verzeichnissen gesucht:",
        [nameof(NothingFoundTitle)] = "Nichts gefunden",
        [nameof(NothingFoundMessage)] =
            "Es konnten keine Videos basierend auf der angegebenen Anfrage oder URL gefunden werden",
        [nameof(ErrorTitle)] = "Fehler",
        [nameof(UpdateDownloadingMessage)] = "Update für {0} v{1} wird heruntergeladen...",
        [nameof(UpdateReadyMessage)] =
            "Update wurde heruntergeladen und wird beim Beenden installiert",
        [nameof(UpdateInstallNowButton)] = "JETZT INSTALLIEREN",
        [nameof(UpdateFailedMessage)] = "Anwendungsupdate konnte nicht durchgeführt werden",
    };
}
