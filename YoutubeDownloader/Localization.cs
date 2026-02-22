using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YoutubeDownloader;

public partial class Localization : ObservableObject
{
    public static Localization Current { get; } = new();

    [ObservableProperty]
    public partial Language Language { get; set; } = Language.English;

    partial void OnLanguageChanged(Language value) =>
        // Notify all string properties so the UI refreshes
        OnPropertyChanged(string.Empty);

    private string Get([CallerMemberName] string? key = null)
    {
        if (key is null)
            return string.Empty;

        if (
            this.Language != Language.English
            && Translations.TryGetValue(this.Language, out var dict)
            && dict.TryGetValue(key, out var translated)
        )
            return translated;

        return Translations[Language.English][key];
    }

    // ---- Dashboard ----

    public string QueryWatermark => Get();
    public string QueryTooltip => Get();
    public string ProcessQueryTooltip => Get();
    public string AuthTooltip => Get();
    public string SettingsTooltip => Get();
    public string DashboardSearchQueryLabel => Get();
    public string DashboardShiftEnterLabel => Get();
    public string DashboardPlaceholderCopyPasteA => Get();
    public string DashboardPlaceholderOrEnterA => Get();
    public string DashboardPlaceholderToStartDownloading => Get();
    public string DashboardPlaceholderPress => Get();
    public string DashboardPlaceholderToAddMultiple => Get();
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
    public string FileNameTemplateTokenNumDesc => Get();
    public string FileNameTemplateTokenIdDesc => Get();
    public string FileNameTemplateTokenTitleDesc => Get();
    public string FileNameTemplateTokenAuthorDesc => Get();
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
    public string NothingFoundTitle => Get();
    public string NothingFoundMessage => Get();
    public string ErrorTitle => Get();
    public string UpdateDownloadingMessage => Get();
    public string UpdateReadyMessage => Get();
    public string UpdateInstallNowButton => Get();
    public string UpdateFailedMessage => Get();

    // ---- Translations ----

    private static readonly IReadOnlyDictionary<
        Language,
        IReadOnlyDictionary<string, string>
    > Translations = new Dictionary<Language, IReadOnlyDictionary<string, string>>
    {
        [Language.English] = new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryWatermark)] = "URL or search query",
            [nameof(QueryTooltip)] =
                "Any valid YouTube URL or ID is accepted. Prepend a question mark (?) to perform search by text.",
            [nameof(ProcessQueryTooltip)] = "Process query (Enter)",
            [nameof(AuthTooltip)] = "Authentication",
            [nameof(SettingsTooltip)] = "Settings",
            [nameof(DashboardSearchQueryLabel)] = "search query",
            [nameof(DashboardShiftEnterLabel)] = "Shift+Enter",
            [nameof(DashboardPlaceholderCopyPasteA)] = "Copy-paste a ",
            [nameof(DashboardPlaceholderOrEnterA)] = " or enter a ",
            [nameof(DashboardPlaceholderToStartDownloading)] = " to start downloading",
            [nameof(DashboardPlaceholderPress)] = "Press ",
            [nameof(DashboardPlaceholderToAddMultiple)] = " to add multiple items",
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
            [nameof(NothingFoundTitle)] = "Nothing found",
            [nameof(NothingFoundMessage)] =
                "Couldn't find any videos based on the query or URL you provided",
            [nameof(ErrorTitle)] = "Error",
            [nameof(UpdateDownloadingMessage)] = "Downloading update to {0} v{1}...",
            [nameof(UpdateReadyMessage)] =
                "Update has been downloaded and will be installed when you exit",
            [nameof(UpdateInstallNowButton)] = "INSTALL NOW",
            [nameof(UpdateFailedMessage)] = "Failed to perform application update",
        },
        [Language.Ukrainian] = new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryWatermark)] = "URL або пошуковий запит",
            [nameof(QueryTooltip)] =
                "Приймається будь-який дійсний URL або ID YouTube. Додайте знак питання (?) для пошуку за текстом.",
            [nameof(ProcessQueryTooltip)] = "Виконати запит (Enter)",
            [nameof(AuthTooltip)] = "Автентифікація",
            [nameof(SettingsTooltip)] = "Налаштування",
            [nameof(DashboardSearchQueryLabel)] = "пошуковий запит",
            [nameof(DashboardShiftEnterLabel)] = "Shift+Enter",
            [nameof(DashboardPlaceholderCopyPasteA)] = "Вставте ",
            [nameof(DashboardPlaceholderOrEnterA)] = " або введіть ",
            [nameof(DashboardPlaceholderToStartDownloading)] = " для завантаження",
            [nameof(DashboardPlaceholderPress)] = "Натисніть ",
            [nameof(DashboardPlaceholderToAddMultiple)] = ", щоб додати декілька елементів",
            [nameof(DownloadsFileColumnHeader)] = "Файл",
            [nameof(DownloadsStatusColumnHeader)] = "Статус",
            [nameof(ContextMenuRemoveSuccessful)] = "Видалити успішні завантаження",
            [nameof(ContextMenuRemoveInactive)] = "Видалити неактивні завантаження",
            [nameof(ContextMenuRestartFailed)] = "Перезапустити невдалі завантаження",
            [nameof(ContextMenuCancelAll)] = "Скасувати всі завантаження",
            [nameof(DownloadStatusEnqueued)] = "В черзі...",
            [nameof(DownloadStatusCompleted)] = "Готово",
            [nameof(DownloadStatusCanceled)] = "Скасовано",
            [nameof(DownloadStatusFailed)] = "Помилка",
            [nameof(ClickToCopyErrorTooltip)] = "Примітка: натисніть, щоб скопіювати повідомлення",
            [nameof(ShowFileTooltip)] = "Показати файл",
            [nameof(PlayTooltip)] = "Відтворити",
            [nameof(CancelDownloadTooltip)] = "Скасувати завантаження",
            [nameof(RestartDownloadTooltip)] = "Перезапустити завантаження",
            // Settings
            [nameof(SettingsTitle)] = "Налаштування",
            [nameof(ThemeLabel)] = "Тема",
            [nameof(ThemeTooltip)] = "Бажана тема інтерфейсу",
            [nameof(LanguageLabel)] = "Мова",
            [nameof(AutoUpdateLabel)] = "Автооновлення",
            [nameof(AutoUpdateTooltip)] = """
                Виконувати автоматичні оновлення при кожному запуску.
                Увага: рекомендується залишити цю опцію увімкненою для сумісності з останньою версією YouTube.
                """,
            [nameof(PersistAuthLabel)] = "Зберігати автентифікацію",
            [nameof(PersistAuthTooltip)] =
                "Зберігати файли cookie у файлі для збереження між сеансами",
            [nameof(InjectAltLanguagesLabel)] = "Вставляти альтернативні мови",
            [nameof(InjectAltLanguagesTooltip)] =
                "Вставляти аудіодоріжки альтернативними мовами (якщо доступні) у завантажені файли",
            [nameof(InjectSubtitlesLabel)] = "Вставляти субтитри",
            [nameof(InjectSubtitlesTooltip)] =
                "Вставляти субтитри (якщо доступні) у завантажені файли",
            [nameof(InjectTagsLabel)] = "Вставляти медіатеги",
            [nameof(InjectTagsTooltip)] = "Вставляти медіатеги (якщо доступні) у завантажені файли",
            [nameof(SkipExistingFilesLabel)] = "Пропускати наявні файли",
            [nameof(SkipExistingFilesTooltip)] =
                "При завантаженні кількох відео пропускати ті, для яких вже є відповідні файли",
            [nameof(FileNameTemplateLabel)] = "Шаблон імені файлу",
            [nameof(FileNameTemplateTooltip)] =
                "Шаблон для генерації імен файлів завантажених відео.",
            [nameof(FileNameTemplateTokenNumDesc)] = "— позиція відео у списку (якщо застосовно)",
            [nameof(FileNameTemplateTokenIdDesc)] = "— ідентифікатор відео",
            [nameof(FileNameTemplateTokenTitleDesc)] = "— назва відео",
            [nameof(FileNameTemplateTokenAuthorDesc)] = "— автор відео",
            [nameof(ParallelLimitLabel)] = "Паралельний ліміт",
            [nameof(ParallelLimitTooltip)] = "Скільки завантажень може бути активними одночасно",
            [nameof(FFmpegPathLabel)] = "Шлях FFmpeg",
            [nameof(FFmpegPathTooltip)] =
                "Шлях до виконуваного файлу FFmpeg. Залиште порожнім для автоматичного визначення.",
            [nameof(FFmpegPathWatermark)] = "Авто",
            [nameof(FFmpegPathResetTooltip)] = "Скинути до автоматичного визначення",
            [nameof(FFmpegPathBrowseTooltip)] = "Вибрати файл FFmpeg",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "Автентифікація",
            [nameof(AuthenticatedText)] = "Ви автентифіковані",
            [nameof(LogOutButton)] = "Вийти",
            [nameof(LoadingText)] = "Завантаження...",
            // Download Single Setup
            [nameof(CopyMenuItem)] = "Копіювати",
            [nameof(LiveLabel)] = "Живе",
            [nameof(AudioLabel)] = "Аудіо",
            [nameof(FormatLabel)] = "Формат",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "Контейнер",
            [nameof(VideoQualityLabel)] = "Якість відео",
            // Common buttons
            [nameof(CloseButton)] = "ЗАКРИТИ",
            [nameof(DownloadButton)] = "ЗАВАНТАЖИТИ",
            [nameof(CancelButton)] = "СКАСУВАТИ",
            [nameof(SettingsButton)] = "НАЛАШТУВАННЯ",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "Дякуємо за підтримку України!",
            [nameof(UkraineSupportMessage)] = """
                Поки Росія веде геноцидну війну проти моєї країни, я вдячний кожному, хто продовжує підтримувати Україну у нашій боротьбі за свободу.

                Натисніть ДІЗНАТИСЬ БІЛЬШЕ, щоб знайти способи допомогти.
                """,
            [nameof(LearnMoreButton)] = "ДІЗНАТИСЬ БІЛЬШЕ",
            [nameof(UnstableBuildTitle)] = "Попередження про нестабільну збірку",
            [nameof(UnstableBuildMessage)] = """
                Ви використовуєте збірку розробки {0}. Ці збірки не пройшли ретельного тестування та можуть містити помилки.

                Автооновлення вимкнено для збірок розробки.

                Натисніть ПЕРЕГЛЯНУТИ РЕЛІЗИ, щоб завантажити стабільний реліз.
                """,
            [nameof(SeeReleasesButton)] = "ПЕРЕГЛЯНУТИ РЕЛІЗИ",
            [nameof(FFmpegMissingTitle)] = "FFmpeg відсутній",
            [nameof(FFmpegMissingMessage)] = """
                FFmpeg потрібен для роботи {0}. Завантажте його та зробіть доступним у каталозі програми або у системному PATH, або вкажіть розташування у налаштуваннях.

                Альтернативно, ви можете завантажити версію {0} з вбудованим FFmpeg. Шукайте ресурси релізу, які НЕ позначені як *.Bare.

                Натисніть ЗАВАНТАЖИТИ, щоб перейти на сторінку завантаження FFmpeg.
                """,
            [nameof(FFmpegPathMissingMessage)] = """
                FFmpeg потрібен для роботи програми, але вказаний шлях не існує:
                {0}

                Будь ласка, оновіть шлях FFmpeg у налаштуваннях або очистіть його для автовизначення.
                """,
            [nameof(NothingFoundTitle)] = "Нічого не знайдено",
            [nameof(NothingFoundMessage)] = "Не вдалося знайти відео за вказаним запитом або URL",
            [nameof(ErrorTitle)] = "Помилка",
            [nameof(UpdateDownloadingMessage)] = "Завантаження оновлення {0} v{1}...",
            [nameof(UpdateReadyMessage)] = "Оновлення завантажено та буде встановлено після виходу",
            [nameof(UpdateInstallNowButton)] = "ВСТАНОВИТИ ЗАРАЗ",
            [nameof(UpdateFailedMessage)] = "Не вдалося виконати оновлення програми",
        },
        [Language.German] = new Dictionary<string, string>
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
            [nameof(DashboardPlaceholderToStartDownloading)] =
                " eingeben um den Download zu starten",
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
            [nameof(NothingFoundTitle)] = "Nichts gefunden",
            [nameof(NothingFoundMessage)] =
                "Es konnten keine Videos basierend auf der angegebenen Anfrage oder URL gefunden werden",
            [nameof(ErrorTitle)] = "Fehler",
            [nameof(UpdateDownloadingMessage)] = "Update für {0} v{1} wird heruntergeladen...",
            [nameof(UpdateReadyMessage)] =
                "Update wurde heruntergeladen und wird beim Beenden installiert",
            [nameof(UpdateInstallNowButton)] = "JETZT INSTALLIEREN",
            [nameof(UpdateFailedMessage)] = "Anwendungsupdate konnte nicht durchgeführt werden",
        },
        [Language.French] = new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryWatermark)] = "URL ou requête de recherche",
            [nameof(QueryTooltip)] =
                "Toute URL ou ID YouTube valide est acceptée. Ajoutez un point d'interrogation (?) pour rechercher par texte.",
            [nameof(ProcessQueryTooltip)] = "Traiter la requête (Entrée)",
            [nameof(AuthTooltip)] = "Authentification",
            [nameof(SettingsTooltip)] = "Paramètres",
            [nameof(DashboardSearchQueryLabel)] = "requête de recherche",
            [nameof(DashboardShiftEnterLabel)] = "Shift+Entrée",
            [nameof(DashboardPlaceholderCopyPasteA)] = "Collez une ",
            [nameof(DashboardPlaceholderOrEnterA)] = " ou entrez une ",
            [nameof(DashboardPlaceholderToStartDownloading)] = " pour commencer",
            [nameof(DashboardPlaceholderPress)] = "Appuyez sur ",
            [nameof(DashboardPlaceholderToAddMultiple)] = " pour ajouter plusieurs éléments",
            [nameof(DownloadsFileColumnHeader)] = "Fichier",
            [nameof(DownloadsStatusColumnHeader)] = "Statut",
            [nameof(ContextMenuRemoveSuccessful)] = "Supprimer les téléchargements réussis",
            [nameof(ContextMenuRemoveInactive)] = "Supprimer les téléchargements inactifs",
            [nameof(ContextMenuRestartFailed)] = "Relancer les téléchargements échoués",
            [nameof(ContextMenuCancelAll)] = "Annuler tous les téléchargements",
            [nameof(DownloadStatusEnqueued)] = "En attente...",
            [nameof(DownloadStatusCompleted)] = "Terminé",
            [nameof(DownloadStatusCanceled)] = "Annulé",
            [nameof(DownloadStatusFailed)] = "Échec",
            [nameof(ClickToCopyErrorTooltip)] = "Note : Cliquez pour copier ce message d'erreur",
            [nameof(ShowFileTooltip)] = "Afficher le fichier",
            [nameof(PlayTooltip)] = "Lire",
            [nameof(CancelDownloadTooltip)] = "Annuler le téléchargement",
            [nameof(RestartDownloadTooltip)] = "Relancer le téléchargement",
            // Settings
            [nameof(SettingsTitle)] = "Paramètres",
            [nameof(ThemeLabel)] = "Thème",
            [nameof(ThemeTooltip)] = "Thème d'interface préféré",
            [nameof(LanguageLabel)] = "Langue",
            [nameof(AutoUpdateLabel)] = "Mise à jour automatique",
            [nameof(AutoUpdateTooltip)] = """
                Effectuer des mises à jour automatiques à chaque démarrage.
                Avertissement : il est recommandé de laisser cette option activée pour assurer la compatibilité avec la dernière version de YouTube.
                """,
            [nameof(PersistAuthLabel)] = "Conserver l'authentification",
            [nameof(PersistAuthTooltip)] =
                "Enregistrer les cookies d'authentification dans un fichier pour les conserver entre les sessions",
            [nameof(InjectAltLanguagesLabel)] = "Injecter les langues alternatives",
            [nameof(InjectAltLanguagesTooltip)] =
                "Injecter des pistes audio en langues alternatives (si disponibles) dans les fichiers téléchargés",
            [nameof(InjectSubtitlesLabel)] = "Injecter les sous-titres",
            [nameof(InjectSubtitlesTooltip)] =
                "Injecter les sous-titres (si disponibles) dans les fichiers téléchargés",
            [nameof(InjectTagsLabel)] = "Injecter les balises média",
            [nameof(InjectTagsTooltip)] =
                "Injecter les balises média (si disponibles) dans les fichiers téléchargés",
            [nameof(SkipExistingFilesLabel)] = "Ignorer les fichiers existants",
            [nameof(SkipExistingFilesTooltip)] =
                "Lors du téléchargement de plusieurs vidéos, ignorer celles qui ont déjà des fichiers correspondants dans le répertoire de sortie",
            [nameof(FileNameTemplateLabel)] = "Modèle de nom de fichier",
            [nameof(FileNameTemplateTooltip)] =
                "Modèle utilisé pour générer les noms de fichiers des vidéos téléchargées.",
            [nameof(FileNameTemplateTokenNumDesc)] =
                "— position de la vidéo dans la liste (si applicable)",
            [nameof(FileNameTemplateTokenIdDesc)] = "— ID de la vidéo",
            [nameof(FileNameTemplateTokenTitleDesc)] = "— titre de la vidéo",
            [nameof(FileNameTemplateTokenAuthorDesc)] = "— auteur de la vidéo",
            [nameof(ParallelLimitLabel)] = "Limite parallèle",
            [nameof(ParallelLimitTooltip)] =
                "Combien de téléchargements peuvent être actifs en même temps",
            [nameof(FFmpegPathLabel)] = "Chemin FFmpeg",
            [nameof(FFmpegPathTooltip)] =
                "Chemin vers l'exécutable FFmpeg. Laisser vide pour la détection automatique.",
            [nameof(FFmpegPathWatermark)] = "Auto",
            [nameof(FFmpegPathResetTooltip)] = "Réinitialiser la détection automatique",
            [nameof(FFmpegPathBrowseTooltip)] = "Parcourir l'exécutable FFmpeg",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "Authentification",
            [nameof(AuthenticatedText)] = "Vous êtes actuellement authentifié",
            [nameof(LogOutButton)] = "Se déconnecter",
            [nameof(LoadingText)] = "Chargement...",
            // Download Single Setup
            [nameof(CopyMenuItem)] = "Copier",
            [nameof(LiveLabel)] = "En direct",
            [nameof(AudioLabel)] = "Audio",
            [nameof(FormatLabel)] = "Format",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "Conteneur",
            [nameof(VideoQualityLabel)] = "Qualité vidéo",
            // Common buttons
            [nameof(CloseButton)] = "FERMER",
            [nameof(DownloadButton)] = "TÉLÉCHARGER",
            [nameof(CancelButton)] = "ANNULER",
            [nameof(SettingsButton)] = "PARAMÈTRES",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "Merci de soutenir l'Ukraine !",
            [nameof(UkraineSupportMessage)] = """
                Alors que la Russie mène une guerre génocidaire contre mon pays, je suis reconnaissant envers tous ceux qui continuent à soutenir l'Ukraine dans notre combat pour la liberté.

                Cliquez sur EN SAVOIR PLUS pour trouver des moyens d'aider.
                """,
            [nameof(LearnMoreButton)] = "EN SAVOIR PLUS",
            [nameof(UnstableBuildTitle)] = "Avertissement : build instable",
            [nameof(UnstableBuildMessage)] = """
                Vous utilisez une version de développement de {0}. Ces versions ne sont pas rigoureusement testées et peuvent contenir des bugs.

                Les mises à jour automatiques sont désactivées pour les versions de développement.

                Cliquez sur VOIR LES VERSIONS pour télécharger une version stable.
                """,
            [nameof(SeeReleasesButton)] = "VOIR LES VERSIONS",
            [nameof(FFmpegMissingTitle)] = "FFmpeg est manquant",
            [nameof(FFmpegMissingMessage)] = """
                FFmpeg est requis pour que {0} fonctionne. Veuillez le télécharger et le rendre disponible dans le répertoire de l'application ou dans le PATH système, ou configurer son emplacement dans les paramètres.

                Alternativement, vous pouvez télécharger une version de {0} avec FFmpeg intégré. Cherchez les fichiers de version qui ne sont PAS marqués *.Bare.

                Cliquez sur TÉLÉCHARGER pour accéder à la page de téléchargement de FFmpeg.
                """,
            [nameof(FFmpegPathMissingMessage)] = """
                FFmpeg est requis pour cette application, mais le chemin configuré n'existe pas :
                {0}

                Veuillez mettre à jour le chemin FFmpeg dans les paramètres ou le vider pour utiliser la détection automatique.
                """,
            [nameof(NothingFoundTitle)] = "Rien trouvé",
            [nameof(NothingFoundMessage)] =
                "Impossible de trouver des vidéos correspondant à la requête ou l'URL fournie",
            [nameof(ErrorTitle)] = "Erreur",
            [nameof(UpdateDownloadingMessage)] = "Téléchargement de la mise à jour {0} v{1}...",
            [nameof(UpdateReadyMessage)] =
                "La mise à jour a été téléchargée et sera installée à la fermeture",
            [nameof(UpdateInstallNowButton)] = "INSTALLER MAINTENANT",
            [nameof(UpdateFailedMessage)] = "Échec de la mise à jour de l'application",
        },
        [Language.Spanish] = new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryWatermark)] = "URL o consulta de búsqueda",
            [nameof(QueryTooltip)] =
                "Se acepta cualquier URL o ID de YouTube válido. Antepone un signo de interrogación (?) para buscar por texto.",
            [nameof(ProcessQueryTooltip)] = "Procesar consulta (Enter)",
            [nameof(AuthTooltip)] = "Autenticación",
            [nameof(SettingsTooltip)] = "Configuración",
            [nameof(DashboardSearchQueryLabel)] = "consulta de búsqueda",
            [nameof(DashboardShiftEnterLabel)] = "Shift+Enter",
            [nameof(DashboardPlaceholderCopyPasteA)] = "Pega una ",
            [nameof(DashboardPlaceholderOrEnterA)] = " o ingresa una ",
            [nameof(DashboardPlaceholderToStartDownloading)] = " para comenzar",
            [nameof(DashboardPlaceholderPress)] = "Presiona ",
            [nameof(DashboardPlaceholderToAddMultiple)] = " para agregar múltiples elementos",
            [nameof(DownloadsFileColumnHeader)] = "Archivo",
            [nameof(DownloadsStatusColumnHeader)] = "Estado",
            [nameof(ContextMenuRemoveSuccessful)] = "Eliminar descargas exitosas",
            [nameof(ContextMenuRemoveInactive)] = "Eliminar descargas inactivas",
            [nameof(ContextMenuRestartFailed)] = "Reiniciar descargas fallidas",
            [nameof(ContextMenuCancelAll)] = "Cancelar todas las descargas",
            [nameof(DownloadStatusEnqueued)] = "Pendiente...",
            [nameof(DownloadStatusCompleted)] = "Listo",
            [nameof(DownloadStatusCanceled)] = "Cancelado",
            [nameof(DownloadStatusFailed)] = "Fallido",
            [nameof(ClickToCopyErrorTooltip)] = "Nota: Haz clic para copiar este mensaje de error",
            [nameof(ShowFileTooltip)] = "Mostrar archivo",
            [nameof(PlayTooltip)] = "Reproducir",
            [nameof(CancelDownloadTooltip)] = "Cancelar descarga",
            [nameof(RestartDownloadTooltip)] = "Reiniciar descarga",
            // Settings
            [nameof(SettingsTitle)] = "Configuración",
            [nameof(ThemeLabel)] = "Tema",
            [nameof(ThemeTooltip)] = "Tema de interfaz preferido",
            [nameof(LanguageLabel)] = "Idioma",
            [nameof(AutoUpdateLabel)] = "Actualización automática",
            [nameof(AutoUpdateTooltip)] = """
                Realizar actualizaciones automáticas en cada inicio.
                Advertencia: se recomienda dejar esta opción habilitada para asegurar la compatibilidad con la última versión de YouTube.
                """,
            [nameof(PersistAuthLabel)] = "Conservar autenticación",
            [nameof(PersistAuthTooltip)] =
                "Guardar las cookies de autenticación en un archivo para persistirlas entre sesiones",
            [nameof(InjectAltLanguagesLabel)] = "Insertar idiomas alternativos",
            [nameof(InjectAltLanguagesTooltip)] =
                "Insertar pistas de audio en idiomas alternativos (si están disponibles) en los archivos descargados",
            [nameof(InjectSubtitlesLabel)] = "Insertar subtítulos",
            [nameof(InjectSubtitlesTooltip)] =
                "Insertar subtítulos (si están disponibles) en los archivos descargados",
            [nameof(InjectTagsLabel)] = "Insertar etiquetas multimedia",
            [nameof(InjectTagsTooltip)] =
                "Insertar etiquetas multimedia (si están disponibles) en los archivos descargados",
            [nameof(SkipExistingFilesLabel)] = "Omitir archivos existentes",
            [nameof(SkipExistingFilesTooltip)] =
                "Al descargar múltiples videos, omitir los que ya tengan archivos correspondientes en el directorio de salida",
            [nameof(FileNameTemplateLabel)] = "Plantilla de nombre de archivo",
            [nameof(FileNameTemplateTooltip)] =
                "Plantilla para generar nombres de archivo de los videos descargados.",
            [nameof(FileNameTemplateTokenNumDesc)] = "— posición del video en la lista (si aplica)",
            [nameof(FileNameTemplateTokenIdDesc)] = "— ID del video",
            [nameof(FileNameTemplateTokenTitleDesc)] = "— título del video",
            [nameof(FileNameTemplateTokenAuthorDesc)] = "— autor del video",
            [nameof(ParallelLimitLabel)] = "Límite paralelo",
            [nameof(ParallelLimitTooltip)] =
                "Cuántas descargas pueden estar activas al mismo tiempo",
            [nameof(FFmpegPathLabel)] = "Ruta de FFmpeg",
            [nameof(FFmpegPathTooltip)] =
                "Ruta al ejecutable de FFmpeg. Dejar vacío para detección automática.",
            [nameof(FFmpegPathWatermark)] = "Auto",
            [nameof(FFmpegPathResetTooltip)] = "Restablecer detección automática",
            [nameof(FFmpegPathBrowseTooltip)] = "Buscar ejecutable de FFmpeg",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "Autenticación",
            [nameof(AuthenticatedText)] = "Actualmente estás autenticado",
            [nameof(LogOutButton)] = "Cerrar sesión",
            [nameof(LoadingText)] = "Cargando...",
            // Download Single Setup
            [nameof(CopyMenuItem)] = "Copiar",
            [nameof(LiveLabel)] = "En vivo",
            [nameof(AudioLabel)] = "Audio",
            [nameof(FormatLabel)] = "Formato",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "Contenedor",
            [nameof(VideoQualityLabel)] = "Calidad de video",
            // Common buttons
            [nameof(CloseButton)] = "CERRAR",
            [nameof(DownloadButton)] = "DESCARGAR",
            [nameof(CancelButton)] = "CANCELAR",
            [nameof(SettingsButton)] = "AJUSTES",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "¡Gracias por apoyar a Ucrania!",
            [nameof(UkraineSupportMessage)] = """
                Mientras Rusia libra una guerra genocida contra mi país, estoy agradecido con todos los que continúan apoyando a Ucrania en nuestra lucha por la libertad.

                Haz clic en MÁS INFORMACIÓN para encontrar formas en que puedes ayudar.
                """,
            [nameof(LearnMoreButton)] = "MÁS INFORMACIÓN",
            [nameof(UnstableBuildTitle)] = "Advertencia: versión inestable",
            [nameof(UnstableBuildMessage)] = """
                Estás usando una versión de desarrollo de {0}. Estas versiones no han sido probadas exhaustivamente y pueden contener errores.

                Las actualizaciones automáticas están desactivadas para versiones de desarrollo.

                Haz clic en VER LANZAMIENTOS para descargar una versión estable.
                """,
            [nameof(SeeReleasesButton)] = "VER LANZAMIENTOS",
            [nameof(FFmpegMissingTitle)] = "Falta FFmpeg",
            [nameof(FFmpegMissingMessage)] = """
                FFmpeg es necesario para que {0} funcione. Descárgalo y ponlo disponible en el directorio de la aplicación o en el PATH del sistema, o configura la ubicación en los ajustes.

                Alternativamente, puedes descargar una versión de {0} que incluye FFmpeg. Busca los archivos de lanzamiento que NO estén marcados como *.Bare.

                Haz clic en DESCARGAR para ir a la página de descarga de FFmpeg.
                """,
            [nameof(FFmpegPathMissingMessage)] = """
                FFmpeg es necesario para esta aplicación, pero la ruta configurada no existe:
                {0}

                Por favor, actualiza la ruta de FFmpeg en los ajustes o bórrala para usar la detección automática.
                """,
            [nameof(NothingFoundTitle)] = "Nada encontrado",
            [nameof(NothingFoundMessage)] =
                "No se encontraron videos basados en la consulta o URL proporcionada",
            [nameof(ErrorTitle)] = "Error",
            [nameof(UpdateDownloadingMessage)] = "Descargando actualización de {0} v{1}...",
            [nameof(UpdateReadyMessage)] =
                "La actualización se ha descargado y se instalará al salir",
            [nameof(UpdateInstallNowButton)] = "INSTALAR AHORA",
            [nameof(UpdateFailedMessage)] = "Error al realizar la actualización de la aplicación",
        },
    };
}
