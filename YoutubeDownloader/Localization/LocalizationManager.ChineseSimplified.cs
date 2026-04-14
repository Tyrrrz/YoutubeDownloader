using System.Collections.Generic;

namespace YoutubeDownloader.Localization;

public partial class LocalizationManager
{
    private static readonly IReadOnlyDictionary<string, string> ChineseSimplifiedLocalization =
        new Dictionary<string, string>
        {
            // Dashboard
            [nameof(QueryWatermark)] = "粘贴链接或搜索",
            [nameof(QueryTooltip)] =
                "接受任何有效的 YouTube 网页链接或视频 ID。在开头添加问号 (?) 可进行文本搜索。",
            [nameof(ProcessQueryTooltip)] = "开始查询 (Enter)",
            [nameof(AuthTooltip)] = "身份验证",
            [nameof(SettingsTooltip)] = "设置",
            [nameof(DashboardPlaceholder)] = """
                复制并粘贴 **网页链接** 或 **搜索** 以开始下载
                使用 **Shift+Enter** 可添加多个项目
                """,
            [nameof(DownloadsFileColumnHeader)] = "文件",
            [nameof(DownloadsStatusColumnHeader)] = "状态",
            [nameof(ContextMenuRemoveSuccessful)] = "移除已完成的下载",
            [nameof(ContextMenuRemoveInactive)] = "移除未激活的下载",
            [nameof(ContextMenuRestartFailed)] = "重下失败的下载",
            [nameof(ContextMenuCancelAll)] = "取消所有下载",
            [nameof(DownloadStatusEnqueued)] = "正在排队...",
            [nameof(DownloadStatusCompleted)] = "已完成",
            [nameof(DownloadStatusCanceled)] = "已取消",
            [nameof(DownloadStatusFailed)] = "失败",
            [nameof(ClickToCopyErrorTooltip)] = "提示：点击可复制此错误信息",
            [nameof(ShowFileTooltip)] = "显示文件",
            [nameof(PlayTooltip)] = "播放",
            [nameof(CancelDownloadTooltip)] = "取消下载",
            [nameof(RestartDownloadTooltip)] = "重新下载",
            // Settings
            [nameof(SettingsTitle)] = "设置",
            [nameof(ThemeLabel)] = "主题",
            [nameof(ThemeTooltip)] = "首选的用户界面主题",
            [nameof(LanguageLabel)] = "语言",
            [nameof(LanguageTooltip)] = "首选的用户界面显示语言",
            [nameof(AutoUpdateLabel)] = "自动更新",
            [nameof(AutoUpdateTooltip)] = """
                每次启动时执行自动更新。
                **警告：** 建议保持此选项启用，以确保应用程序与最新版本的 YouTube 兼容。
                """,
            [nameof(PersistAuthLabel)] = "持久化身份验证",
            [nameof(PersistAuthTooltip)] = """
                将身份验证 Cookie 保存到文件中，以便在不同会话之间持久化。
                **警告：** 尽管 Cookie 是加密存储的，但拥有系统访问权限的攻击者仍可能恢复它们。
                """,
            [nameof(InjectAltLanguagesLabel)] = "注入多音轨",
            [nameof(InjectAltLanguagesTooltip)] =
                "将替代语言的音频轨道（如果可用）注入到下载的文件中",
            [nameof(InjectSubtitlesLabel)] = "注入字幕",
            [nameof(InjectSubtitlesTooltip)] = "将字幕（如果可用）注入到下载的文件中",
            [nameof(InjectTagsLabel)] = "注入媒体标签",
            [nameof(InjectTagsTooltip)] = "将媒体标签（如果可用）注入到下载的文件中",
            [nameof(SkipExistingFilesLabel)] = "跳过已存在的文件",
            [nameof(SkipExistingFilesTooltip)] =
                "下载多个视频时，跳过输出目录中已存在匹配文件的视频",
            [nameof(FileNameTemplateLabel)] = "文件名模板",
            [nameof(FileNameTemplateTooltip)] = """
                用于生成下载视频文件名的模板。

                可用标记：
                **$num** — 视频在列表中的位置（如果适用）
                **$id** — 视频 ID
                **$title** — 视频标题
                **$author** — 视频作者
                """,
            [nameof(ParallelLimitLabel)] = "并行下载限制",
            [nameof(ParallelLimitTooltip)] = "允许同时进行的下载任务数量",
            [nameof(FFmpegPathLabel)] = "FFmpeg 路径",
            [nameof(FFmpegPathTooltip)] = "FFmpeg 可执行文件的路径。留空则使用自动检测。",
            [nameof(FFmpegPathWatermark)] = "自动检测",
            [nameof(FFmpegPathResetTooltip)] = "重置为自动检测",
            [nameof(FFmpegPathBrowseTooltip)] = "浏览 FFmpeg 可执行文件",
            // Auth Setup
            [nameof(AuthenticationTitle)] = "身份验证",
            [nameof(AuthenticatedText)] = "你当前已通过身份验证",
            [nameof(LogOutButton)] = "退出登录",
            [nameof(LoadingText)] = "正在加载...",
            // Download Single Setup
            [nameof(CopyMenuItem)] = "复制",
            [nameof(LiveLabel)] = "直播",
            [nameof(AudioLabel)] = "音频",
            [nameof(FormatLabel)] = "格式",
            // Download Multiple Setup
            [nameof(ContainerLabel)] = "封装格式",
            [nameof(VideoQualityLabel)] = "视频质量",
            // Common buttons
            [nameof(CloseButton)] = "关闭",
            [nameof(DownloadButton)] = "下载",
            [nameof(CancelButton)] = "取消",
            // Dialog messages
            [nameof(UkraineSupportTitle)] = "感谢你支持乌克兰！",
            [nameof(UkraineSupportMessage)] = """
                由于俄罗斯正对我的国家发动一场种族灭绝战争，我向所有在我们的自由之战中继续支持乌克兰的人表示感谢。

                点击“了解更多”以寻找你可以提供帮助的途径。
                """,
            [nameof(LearnMoreButton)] = "了解更多",
            [nameof(UnstableBuildTitle)] = "不稳定版本警告",
            [nameof(UnstableBuildMessage)] = """
                你正在使用 {0} 的开发版本。这些版本未经严格测试，可能包含漏洞。

                开发版本已禁用自动更新。

                如果你想下载稳定版本，请点击“查看发布版本”。
                """,
            [nameof(SeeReleasesButton)] = "查看发布版本",
            [nameof(FFmpegMissingTitle)] = "缺少 FFmpeg",
            [nameof(FFmpegMissingMessage)] =
                "系统中未找到 FFmpeg。{0} 需要它才能正常工作。是否立即下载？",
            [nameof(FFmpegDownloadingTitle)] = "正在下载 FFmpeg...",
            [nameof(FFmpegDownloadCompletedTitle)] = "FFmpeg 下载完成",
            [nameof(NothingFoundTitle)] = "未找到内容",
            [nameof(NothingFoundMessage)] = "无法根据你提供的查询或 URL 找到任何视频",
            [nameof(ErrorTitle)] = "错误",
            [nameof(UpdateDownloadingMessage)] = "正在下载更新至 {0} v{1}...",
            [nameof(UpdateReadyMessage)] = "更新已下载，将在你退出时安装",
            [nameof(UpdateInstallNowButton)] = "立即安装",
            [nameof(UpdateFailedMessage)] = "应用程序更新失败",
        };
}
