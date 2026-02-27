using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> UkrainianLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryWatermark)] = "URL або пошуковий запит",
            [nameof(QueryTooltip)] =
                "Приймається будь-який дійсний URL або ID YouTube. Додайте знак питання (?) для пошуку за текстом.",
            [nameof(ProcessQueryTooltip)] = "Виконати запит (Enter)",
            [nameof(AuthTooltip)] = "Автентифікація",
            [nameof(SettingsTooltip)] = "Налаштування",
            [nameof(DashboardPlaceholder)] = """
                Вставте **URL** або введіть **пошуковий запит** для завантаження
                Натисніть **Shift+Enter**, щоб додати декілька елементів
                """,
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
            [nameof(LanguageTooltip)] = "Бажана мова відображення інтерфейсу користувача",
            [nameof(AutoUpdateLabel)] = "Авто-оновлення",
            [nameof(AutoUpdateTooltip)] = """
                Виконувати автоматичні оновлення при кожному запуску.
                **Увага:** рекомендується залишити цю опцію увімкненою для сумісності з останньою версією YouTube.
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
            [nameof(FileNameTemplateTooltip)] = """
                Шаблон для генерації імен файлів завантажених відео.

                Доступні токени:
                **$num** — позиція відео у списку (якщо застосовно)
                **$id** — ідентифікатор відео
                **$title** — назва відео
                **$author** — автор відео
                """,
            [nameof(ParallelLimitLabel)] = "Ліміт паралелізації",
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

                Авто-оновлення вимкнено для збірок розробки.

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
            [nameof(FFmpegMissingSearchedLabel)] = "Шукали '{0}' у таких директоріях:",
            [nameof(NothingFoundTitle)] = "Нічого не знайдено",
            [nameof(NothingFoundMessage)] = "Не вдалося знайти відео за вказаним запитом або URL",
            [nameof(ErrorTitle)] = "Помилка",
            [nameof(UpdateDownloadingMessage)] = "Завантаження оновлення {0} v{1}...",
            [nameof(UpdateReadyMessage)] = "Оновлення завантажено та буде встановлено після виходу",
            [nameof(UpdateInstallNowButton)] = "ВСТАНОВИТИ ЗАРАЗ",
            [nameof(UpdateFailedMessage)] = "Не вдалося виконати оновлення програми",
        };
}
