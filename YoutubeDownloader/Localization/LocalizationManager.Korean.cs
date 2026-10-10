using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> EnglishLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryPlaceholderText)] = "URL 또는 검색",
            [nameof(QueryTooltip)] =
                "유효한 YouTube URL 또는 ID는 모두 허용됩니다. 텍스트 검색을 하려면 물음표 (?)를 앞에 붙이세요.",
            [nameof(ProcessQueryTooltip)] = "쿼리 처리 (입력)",
            [nameof(AuthTooltip)] = "인증",
            [nameof(SettingsTooltip)] = "설정",
            [nameof(DashboardPlaceholder)] = """
                **URL**을 복사해 붙여넣거나 **검색**를 입력하여 다운로드를 시작하세요
                여러 항목을 추가하려면 **Shift+Enter**를 누르세요
                """,
            [nameof(DownloadsFileColumnHeader)] = "파일",
            [nameof(DownloadsStatusColumnHeader)] = "상태",
            [nameof(ContextMenuRemoveSuccessful)] = "성공적으로 다운로드한 항목 제거",
            [nameof(ContextMenuRemoveInactive)] = "비활성 다운로드 제거",
            [nameof(ContextMenuRestartFailed)] = "실패한 다운로드 다시 시작",
            [nameof(ContextMenuCancelAll)] = "모든 다운로드 취소",
            [nameof(DownloadStatusEnqueued)] = "보류 중...",
            [nameof(DownloadStatusCompleted)] = "환료",
            [nameof(DownloadStatusCanceled)] = "취소됨",
            [nameof(DownloadStatusFailed)] = "실패",
            [nameof(ClickToCopyErrorTooltip)] = "참고: 이 오류 메시지를 복사하려면 클릭하세요",
            [nameof(ShowFileTooltip)] = "파일 표시",
            [nameof(PlayTooltip)] = "재생",
            [nameof(CancelDownloadTooltip)] = "다운로드 취소",
            [nameof(RestartDownloadTooltip)] = "다운로드 다시 시작",
            // Settings
            [nameof(SettingsTitle)] = "설정",
            [nameof(ThemeLabel)] = "테마",
            [nameof(ThemeTooltip)] = "사용자 인터페이스 테마 선호",
            [nameof(LanguageLabel)] = "언어",
            [nameof(LanguageTooltip)] = "사용자 인터페이스의 기본 표시 언어",
            [nameof(AutoUpdateLabel)] = "자동 업데이트",
            [nameof(AutoUpdateTooltip)] = """
                매번 실행할 때마다 자동 업데이트를 수행합니다.
                **경고:** 앱이 YouTube 최신 버전과 호환되도록 이 옵션을 활성화해 두는 것이 권장됩니다.
                """,
            [nameof(PersistAuthLabel)] = "지속 인증",
            [nameof(PersistAuthTooltip)] = """
                인증 쿠키를 파일에 저장하여 세션 사이에 유지할 수 있도록 합니다.
                **경고**: 쿠키는 암호화된 상태로 저장되지만, 귀하의 시스템에 접근할 수 있는 공격자에 의해 여전히 복구될 수 있습니다.
                """,
            [nameof(InjectAltLanguagesLabel)] = "대체 언어 삽입",
            [nameof(InjectAltLanguagesTooltip)] =
                "대체 언어의 오디오 트랙 (사용 가능하면)을 다운로드한 파일에 삽입",
            [nameof(InjectSubtitlesLabel)] = "자막 삽입",
            [nameof(InjectSubtitlesTooltip)] =
                "다운로드한 파일에 자막 (사용 가능하면) 삽입",
            [nameof(InjectTagsLabel)] = "미디어 태그 삽입",
            [nameof(InjectTagsTooltip)] = "다운로드한 파일에 미디어 태그 (사용 가능하면) 삽입",
            [nameof(SkipExistingFilesLabel)] = "기존 파일 건너뛰기",
            [nameof(SkipExistingFilesTooltip)] =
                "여러 동영상을 다운로드할 때 출력 디렉터리에 이미 일치하는 파일이 있는 동영상은 건너뛰세요",
            [nameof(FileNameTemplateLabel)] = "파일 이름 템플릿",
            [nameof(FileNameTemplateTooltip)] = """
                다운로드한 비디오의 파일 이름을 생성하는 데 사용되는 템플릿입니다.

                사용 가능한 토큰:
                **$num** — 목록에서의 동영상의 위치 (해당하는 경우)
                **$id** — 동열상 ID
                **$title** — 동영상 제목
                **$author** — 동영상 저자
                """,
            [nameof(ParallelLimitLabel)] = "병렬 한계",
            [nameof(ParallelLimitTooltip)] = "동시에 몇 개의 다운로드가 활성화될 수 있습니까",
            [nameof(FFmpegPathLabel)] = "FFmpeg 경로",
            [nameof(FFmpegPathTooltip)] =
                "FFmpeg 실행 파일의 경로입니다. 자동 감지를 사용하려면 비워 두세요.",
            [nameof(FFmpegPathPlaceholderText)] = "자동 감지",
            [nameof(FFmpegPathResetTooltip)] = "자동 감지로 재설정",
            [nameof(FFmpegPathBrowseTooltip)] = "FFmpeg 실행 파일 찾아보기",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "인증",
            [nameof(AuthenticatedText)] = "현재 인증되었습니다",
            [nameof(LogOutButton)] = "로그 오프",
            [nameof(AuthenticationPlaceholderText)] = """
                로드 중...

                브라우저가 표시되지 않는 경우:
                - Windows: Microsoft Edge WebView2 Runtime을 설치하세요.
                - macOS: 최신 시스템 업데이트를 설치하세요.
                - GTK 3, WebKitGTK 4.1, libsoup 3를 설치하세요.
                """,
            // Download Single Setup
            [nameof(CopyMenuItem)] = "복사",
            [nameof(LiveLabel)] = "라이브",
            [nameof(AudioLabel)] = "오디오",
            [nameof(UpscaledLabel)] = "업스케일",
            [nameof(FormatLabel)] = "형식",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "컨테이너",
            [nameof(VideoQualityLabel)] = "동영상 품질",
            // Common buttons
            [nameof(CloseButton)] = "닫기",
            [nameof(DownloadButton)] = "다운로드",
            [nameof(CancelButton)] = "취소",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "우크라이나를 지원해 주셔서 감사합니다!",
            [nameof(UkraineSupportMessage)] = """
                러시아가 우리나라를 상대로 집단학살 전쟁을 벌이고 있는 지금, 자유를 위한 우리의 싸움에서 우크라이나와 계속 함께해 주시는 모든 분들께 감사드립니다.

                도움이 될 수 있는 방법을 알아보려면 '자세히 알아보기'를 클릭하세요.
                """,
            [nameof(LearnMoreButton)] = "자세히 알아보기",
            [nameof(UnstableBuildTitle)] = "불안정한 빌드 경고",
            [nameof(UnstableBuildMessage)] = """
                {0}의 개발 빌드를 사용하고 있습니다. 이 빌드들은 충분히 테스트되지 않았으며, 버그가 포함되어 있을 수 있습니다.

                개발 빌드에서는 자동 업데이트가 비활성화되어 있습니다.

                대신 안정적인 릴리스를 다운로드하려면 '릴리스 보기'를 클릭하세요.
                """,
            [nameof(SeeReleasesButton)] = "릴리스 보기",
            [nameof(FFmpegMissingTitle)] = "FFmpeg가 없습니다",
            [nameof(FFmpegMissingMessage)] = """
                시스템에서 FFmpeg를 찾을 수 없습니다. {0}이 작동하려면 필요합니다. 지금 다운로드하시겠습니까?

                또는 이 대화상자를 닫고 설정에서 사용자 지정 FFmpeg 경로를 수동으로 지정하실 수도 있습니다.
                """,
            [nameof(FFmpegDownloadingTitle)] = "FFmpeg 다운로드 중...",
            [nameof(FFmpegDownloadCompletedTitle)] = "FFmpeg 다운로드 됨",
            [nameof(NothingFoundTitle)] = "찾을 수 없습니다",
            [nameof(NothingFoundMessage)] =
                "입력하신 쿼리 또는 URL을 기준으로 동영상을 찾을 수 없습니다",
            [nameof(ErrorTitle)] = "오류",
            [nameof(UpdateDownloadingMessage)] = "{0} v{1}에 대한 업데이트 다운로드 중...",
            [nameof(UpdateReadyMessage)] =
                "업데이트가 다운로드되었습니다. 종료하시면 설치됩니다",
            [nameof(UpdateInstallNowButton)] = "지금 설치",
            [nameof(UpdateFailedMessage)] = "응용 프로그램 업데이트를 수행하지 못했습니다",
        };
}
