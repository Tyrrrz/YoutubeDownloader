using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> PortugueseLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryPlaceholderText)] = "URL ou termo de busca",
            [nameof(QueryTooltip)] =
                "Qualquer URL ou ID válido do YouTube é aceito. Adicione um ponto de interrogação (?) no início para realizar uma busca por texto.",
            [nameof(ProcessQueryTooltip)] = "Buscar (Enter)",
            [nameof(AuthTooltip)] = "Autenticação",
            [nameof(SettingsTooltip)] = "Configurações",
            [nameof(DashboardPlaceholder)] = """
                Cole uma **URL** ou digite um **termo de busca** para começar o download.
                Pressione **Shift+Enter** para adicionar múltiplos itens
                """,
            [nameof(DownloadsFileColumnHeader)] = "Arquivo",
            [nameof(DownloadsStatusColumnHeader)] = "Status",
            [nameof(ContextMenuRemoveSuccessful)] = "Remover downloads concluídos",
            [nameof(ContextMenuRemoveInactive)] = "Remover downloads inativos",
            [nameof(ContextMenuRestartFailed)] = "Reiniciar downloads que falharam",
            [nameof(ContextMenuCancelAll)] = "Cancelar todos os downloads",
            [nameof(DownloadStatusEnqueued)] = "Pendente...",
            [nameof(DownloadStatusCompleted)] = "Concluído",
            [nameof(DownloadStatusCanceled)] = "Cancelado",
            [nameof(DownloadStatusFailed)] = "Falhou",
            [nameof(ClickToCopyErrorTooltip)] = "Nota: Clique para copiar essa mensagem de erro",
            [nameof(ShowFileTooltip)] = "Exibir arquivo",
            [nameof(PlayTooltip)] = "Iniciar",
            [nameof(CancelDownloadTooltip)] = "Cancelar download",
            [nameof(RestartDownloadTooltip)] = "Reiniciar download",
            // Configurações
            [nameof(SettingsTitle)] = "Configurações",
            [nameof(ThemeLabel)] = "Tema",
            [nameof(ThemeTooltip)] = "Tema padrão do usuário",
            [nameof(LanguageLabel)] = "Idioma",
            [nameof(LanguageTooltip)] = "Idioma de exibição padrão do usuário",
            [nameof(AutoUpdateLabel)] = "Atualização automática",
            [nameof(AutoUpdateTooltip)] = """
                Realizar atualizações automáticas a cada inicialização.
                **Atenção:** é recomendado manter essa opção ativada para garantir que o aplicativo seja compatível com a versão mais recente do YouTube.
                """,
            [nameof(PersistAuthLabel)] = "Manter autenticado",
            [nameof(PersistAuthTooltip)] = """
                Salve os cookies de autenticação em um arquivo para que possam persistir entre sessões.
                **Atenção:** mesmo que os cookies sejam armazenados com criptografia, eles ainda podem ser recuperados por um invasor que tenha acesso ao seu dispositivo.
                """,
            [nameof(InjectAltLanguagesLabel)] = "Inserir idiomas alternativos",
            [nameof(InjectAltLanguagesTooltip)] =
                "Inserir faixas de áudio em idiomas alternativos (se disponíveis) nos arquivos baixados",
            [nameof(InjectSubtitlesLabel)] = "Inserir legendas",
            [nameof(InjectSubtitlesTooltip)] =
                "Inserir legendas (se disponíveis) nos arquivos baixados",
            [nameof(InjectTagsLabel)] = "Inserir tags de mídia",
            [nameof(InjectTagsTooltip)] =
                "Inserir tags de mídia (se disponíveis) nos arquivos baixados",
            [nameof(SkipExistingFilesLabel)] = "Pular arquivos existentes",
            [nameof(SkipExistingFilesTooltip)] =
                "Ao baixar vários vídeos, pule aqueles que já possuem arquivos correspondentes no diretório de saída",
            [nameof(FileNameTemplateLabel)] = "Padrão de nome de arquivo",
            [nameof(FileNameTemplateTooltip)] = """
                Padrão usado para gerar nomes de arquivo para vídeos baixados.

                Tokens disponíveis:
                **$num** — posição do vídeo na lista (se aplicável)
                **$id** — ID do vídeo
                **$title** — título do vídeo
                **$author** — autor do vídeo
                """,
            [nameof(ParallelLimitLabel)] = "Limite de downloads simultâneos ",
            [nameof(ParallelLimitTooltip)] = "Quantos downloads podem estar ativos ao mesmo tempo",
            [nameof(FFmpegPathLabel)] = "Caminho para o FFmpeg",
            [nameof(FFmpegPathTooltip)] =
                "Caminho para o executável do FFmpeg. Deixe em branco para usar a detecção automática.",
            [nameof(FFmpegPathPlaceholderText)] = "Detectar automaticamente",
            [nameof(FFmpegPathResetTooltip)] = "Redefinir para detecção automática",
            [nameof(FFmpegPathBrowseTooltip)] = "Localizar o executável do FFmpeg",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "Autenticação",
            [nameof(AuthenticatedText)] = "Você está autenticado no momento.",
            [nameof(LogOutButton)] = "Sair",
            [nameof(LoadingText)] = "Carregando...",
            // Download Single Setup
            [nameof(CopyMenuItem)] = "Copiar",
            [nameof(LiveLabel)] = "Live",
            [nameof(AudioLabel)] = "Áudio",
            [nameof(UpscaledLabel)] = "Ampliado",
            [nameof(FormatLabel)] = "Formato",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "Contêiner",
            [nameof(VideoQualityLabel)] = "Qualidade do vídeo",
            // Common buttons
            [nameof(CloseButton)] = "FECHAR",
            [nameof(DownloadButton)] = "BAIXAR",
            [nameof(CancelButton)] = "CANCELAR",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "Obrigado por apoiar a Ucrânia!",
            [nameof(UkraineSupportMessage)] = """
                Enquanto a Rússia trava uma guerra genocida contra o meu país, sou grato a todos que continuam ao lado da Ucrânia em nossa luta pela liberdade.

                Clique em SAIBA MAIS para descobrir maneiras de ajudar.
                """,
            [nameof(LearnMoreButton)] = "SAIBA MAIS",
            [nameof(UnstableBuildTitle)] = "Aviso de versão instável",
            [nameof(UnstableBuildMessage)] = """
                Você está utilizando a build de desenvolvimento {0}. Estas compilações não foram totalmente testadas e podem conter bugs.

                As atualizações automáticas estão desativadas para builds de desenvolvimento.

                Clique em VER VERSÕES para baixar uma versão estável.
                """,
            [nameof(SeeReleasesButton)] = "VER VERSÕES",
            [nameof(FFmpegMissingTitle)] = "FFmpeg não foi encontrado",
            [nameof(FFmpegMissingMessage)] = """
                O FFmpeg é necessário para o funcionamento do {0}. Deseja baixá-lo agora?

                Caso não queira, você pode fechar esta caixa de diálogo e definir manualmente um caminho personalizado para o FFmpeg nas configurações.
                """,
            [nameof(FFmpegDownloadingTitle)] = "Baixando FFmpeg...",
            [nameof(FFmpegDownloadCompletedTitle)] = "FFmpeg baixado",
            [nameof(NothingFoundTitle)] = "Nada encontrado",
            [nameof(NothingFoundMessage)] =
                "Não foi possível encontrar vídeos com o termo ou a URL utilizada",
            [nameof(ErrorTitle)] = "Erro",
            [nameof(UpdateDownloadingMessage)] = "Baixando atualizações para {0} v{1}...",
            [nameof(UpdateReadyMessage)] =
                "A atualização foi baixada e será instalada quando você sair",
            [nameof(UpdateInstallNowButton)] = "INSTALAR AGORA",
            [nameof(UpdateFailedMessage)] = "Falha ao realizar a atualização do aplicativo",
        };
}
