using System.Collections.Generic;

namespace YoutubeDownloader;

public partial class Localization
{
    private static readonly IReadOnlyDictionary<string, string> FrenchTranslations = new Dictionary<
        string,
        string
    >
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
        [nameof(FileNameTemplateAvailableTokensLabel)] = "Jetons disponibles :",
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
        [nameof(FFmpegMissingSearchedLabel)] = "'{0}' recherché dans les répertoires suivants :",
        [nameof(NothingFoundTitle)] = "Rien trouvé",
        [nameof(NothingFoundMessage)] =
            "Impossible de trouver des vidéos correspondant à la requête ou l'URL fournie",
        [nameof(ErrorTitle)] = "Erreur",
        [nameof(UpdateDownloadingMessage)] = "Téléchargement de la mise à jour {0} v{1}...",
        [nameof(UpdateReadyMessage)] =
            "La mise à jour a été téléchargée et sera installée à la fermeture",
        [nameof(UpdateInstallNowButton)] = "INSTALLER MAINTENANT",
        [nameof(UpdateFailedMessage)] = "Échec de la mise à jour de l'application",
    };
}
