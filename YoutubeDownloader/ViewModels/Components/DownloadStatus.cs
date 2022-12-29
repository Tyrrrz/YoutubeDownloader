namespace YoutubeDownloader.ViewModels.Components;

public enum DownloadStatus
{
    Enqueued,
    Started,
    Completed,
    Completed_Already,
    Deleted,
    Failed,
    Canceled,
    Canceled_low_quality,
    Canceled_too_short,
    Canceled_too_long
}

public enum ContentStatus
{
    New,
    MostView,
    Old
}