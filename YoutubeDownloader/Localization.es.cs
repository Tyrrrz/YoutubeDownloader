using System.Collections.Generic;

namespace YoutubeDownloader;

public partial class Localization
{
    private static readonly IReadOnlyDictionary<string, string> SpanishTranslations =
        new Dictionary<string, string>
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
            [nameof(FileNameTemplateAvailableTokensLabel)] = "Tokens disponibles:",
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
            [nameof(FFmpegMissingSearchedLabel)] = "Se buscó '{0}' en los siguientes directorios:",
            [nameof(NothingFoundTitle)] = "Nada encontrado",
            [nameof(NothingFoundMessage)] =
                "No se encontraron videos basados en la consulta o URL proporcionada",
            [nameof(ErrorTitle)] = "Error",
            [nameof(UpdateDownloadingMessage)] = "Descargando actualización de {0} v{1}...",
            [nameof(UpdateReadyMessage)] =
                "La actualización se ha descargado y se instalará al salir",
            [nameof(UpdateInstallNowButton)] = "INSTALAR AHORA",
            [nameof(UpdateFailedMessage)] = "Error al realizar la actualización de la aplicación",
        };
}
