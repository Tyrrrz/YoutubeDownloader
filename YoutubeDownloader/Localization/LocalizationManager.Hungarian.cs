using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> HungarianLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryPlaceholderText)] = "URL vagy keresés",
            [nameof(QueryTooltip)] =
                "Bármilyen érvényes YouTube URL vagy videóazonosító megadható. Szöveges kereséshez írj egy kérdőjelet (?) a keresőkifejezés elé.",
            [nameof(ProcessQueryTooltip)] = "Keresés indítása (Enter)",
            [nameof(AuthTooltip)] = "Bejelentkezés",
            [nameof(SettingsTooltip)] = "Beállítások",
            [nameof(DashboardPlaceholder)] = """
                Másold be egy videó **URL**-jét vagy írj be egy **kifejezést** a kereséshez
                Több elem hozzáadásához nyomj **Shift+Enter**-t
                """,
            [nameof(DownloadsFileColumnHeader)] = "Fájl",
            [nameof(DownloadsStatusColumnHeader)] = "Státusz",
            [nameof(ContextMenuRemoveSuccessful)] = "Sikeres letöltések eltávolítása",
            [nameof(ContextMenuRemoveInactive)] = "Inaktív letöltések eltávolítása",
            [nameof(ContextMenuRestartFailed)] = "Sikertelen letöltések újraindítása",
            [nameof(ContextMenuCancelAll)] = "Összes letöltés megszakítása",
            [nameof(DownloadStatusEnqueued)] = "Függőben...",
            [nameof(DownloadStatusCompleted)] = "Kész",
            [nameof(DownloadStatusCanceled)] = "Megszakítva",
            [nameof(DownloadStatusFailed)] = "Sikertelen",
            [nameof(ClickToCopyErrorTooltip)] = "Megjegyzés: kattints a hibaüzenet másolásához",
            [nameof(ShowFileTooltip)] = "Fájlhoz ugrás",
            [nameof(PlayTooltip)] = "Lejátszás",
            [nameof(CancelDownloadTooltip)] = "Letöltés megszakítása",
            [nameof(RestartDownloadTooltip)] = "Letöltés újraindítása",
            // Settings
            [nameof(SettingsTitle)] = "Beállítások",
            [nameof(ThemeLabel)] = "Téma",
            [nameof(ThemeTooltip)] = "Felhasználói felület témája",
            [nameof(LanguageLabel)] = "Nyelv",
            [nameof(LanguageTooltip)] = "Felhasználói felület nyelve",
            [nameof(AutoUpdateLabel)] = "Automatikus frissítés",
            [nameof(AutoUpdateTooltip)] = """
                Frissítések keresése minden indításkor.
                **Figyelem:** javasolt ennek a beállításnak a bekapcsolása annak érdekében, hogy az alkalmazás mindig naprakész és a YouTube aktuális szolgáltatásaival kompatibilis legyen.
                """,
            [nameof(PersistAuthLabel)] = "Bejelentkezve maradok",
            [nameof(PersistAuthTooltip)] = """
                Sütik elmentése fájlba, hogy a későbbi munkamenetek során is belépve tudj maradni.
                **Figyelem**: bár a sütik titkosítva tárolódnak, hozzáértő támadók a rendszeredhez hozzáférve vissza tudják fejteni.
                """,
            [nameof(InjectAltLanguagesLabel)] = "Alternatív nyelvek beszúrása",
            [nameof(InjectAltLanguagesTooltip)] =
                "Más nyelvű audiosávok beszúrása (amennyiben léteznek) a letöltött fájlba",
            [nameof(InjectSubtitlesLabel)] = "Feliratok beszúrása",
            [nameof(InjectSubtitlesTooltip)] = "Feliratok (ha vannak) beszúrása a letöltött fájlba",
            [nameof(InjectTagsLabel)] = "Médiacímkék beszúrása",
            [nameof(InjectTagsTooltip)] = "Médiacímkék (ha vannak) beszúrása a letöltött fájlokba",
            [nameof(SkipExistingFilesLabel)] = "Létező fájlok kihagyása",
            [nameof(SkipExistingFilesTooltip)] =
                "Több videó letöltése esetén azok kihagyása, amik már léteznek a célmappában",
            [nameof(FileNameTemplateLabel)] = "Fájlnévminta",
            [nameof(FileNameTemplateTooltip)] = """
                A letöltött videók fájlnevének létrehozásához használt minta.

                Elérhető változók:
                **$num** — videó pozíciója/sorszáma a listában (ha van)
                **$id** — videó azonosítója
                **$title** — videó címe
                **$author** — videó szerzője
                """,
            [nameof(ParallelLimitLabel)] = "Egyidejű letöltések",
            [nameof(ParallelLimitTooltip)] = "Hány letöltés futhat egyidejűleg",
            [nameof(FFmpegPathLabel)] = "FFmpeg elérési útja",
            [nameof(FFmpegPathTooltip)] = "Az FFmpeg futtatható fájljának elérési útja. Az automatikus felismeréshez hagyd üresen",
            [nameof(FFmpegPathPlaceholderText)] = "Automatikus felismerés",
            [nameof(FFmpegPathResetTooltip)] = "Visszaállítás automatikus felismerésre",
            [nameof(FFmpegPathBrowseTooltip)] = "FFmpeg tallózása",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "Bejelentkezés",
            [nameof(AuthenticatedText)] = "Be vagy jelentkezve",
            [nameof(LogOutButton)] = "Kijelentkezés",
            [nameof(LoadingText)] = "Betöltés...",
            // Download Single Setup
            [nameof(CopyMenuItem)] = "Másolás",
            [nameof(LiveLabel)] = "Élő",
            [nameof(AudioLabel)] = "Audió",
            [nameof(UpscaledLabel)] = "Felskálázott",
            [nameof(FormatLabel)] = "Formátum",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "Konténer",
            [nameof(VideoQualityLabel)] = "Videó minőség",
            // Common buttons
            [nameof(CloseButton)] = "BEZÁRÁS",
            [nameof(DownloadButton)] = "LETÖLTÉS",
            [nameof(CancelButton)] = "MÉGSE",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "Köszönet Ukrajna támogatásáért!",
            [nameof(UkraineSupportMessage)] = """
                Mialatt Oroszország népirtó háborút vív hazám ellen, hálás vagyok mindenkinek, aki továbbra is Ukrajna mellett áll a szabadságért folytatott harcunkban.

                A TUDJ MEG TÖBBET gombra kattintva megtudhatod, hogyan segíthetsz.
                """,
            [nameof(LearnMoreButton)] = "Tudj meg többet",
            [nameof(UnstableBuildTitle)] = "Figyelmeztetés nem stabil kiadásra",
            [nameof(UnstableBuildMessage)] = """
                A(z) {0} fejlesztői verzióját használod. Ezek a kiadások még nincsenek alaposan tesztelve és hibákat tartalmazhatnak.

                Automatikus frissítések ki vannak kapcsolva a fejlesztői verziók esetén.

                Válaszd inkább a KIADÁSOK MEGTEKINTÉSE opciót a stabil kiadás letöltéséhez.
                """,
            [nameof(SeeReleasesButton)] = "KIADÁSOK MEGTEKINTÉSE",
            [nameof(FFmpegMissingTitle)] = "Az FFmpeg hiányzik",
            [nameof(FFmpegMissingMessage)] = """
                Az FFmpeg nem található a rendszereden. A(z) {0} működéséhez szükség van rá. Szeretnéd most letölteni?

                Másik lehetőségként bezárhatod ezt az ablakot, és saját kezűleg beállíthatod az FFmpeg elérési útját a beállításokban.
                """,
            [nameof(FFmpegDownloadingTitle)] = "FFmpeg letöltése...",
            [nameof(FFmpegDownloadCompletedTitle)] = "FFmpeg letöltve",
            [nameof(NothingFoundTitle)] = "Nem található",
            [nameof(NothingFoundMessage)] =
                "Nem találhatók videók a megadott keresés vagy URL alapján.",
            [nameof(ErrorTitle)] = "Hiba",
            [nameof(UpdateDownloadingMessage)] = "Frissítés letöltése a(z) {0} v{1} verzióra...",
            [nameof(UpdateReadyMessage)] =
                "A frissítés letöltődött és telepítve lesz, miután bezártad az alkalmazást",
            [nameof(UpdateInstallNowButton)] = "TELEPÍTÉS MOST",
            [nameof(UpdateFailedMessage)] = "Az alkalmazás frissítése sikertelen",
        };
}
