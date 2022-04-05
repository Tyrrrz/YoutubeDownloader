using YoutubeDownloader.Core.Utils;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Downloading;

public class FileNameTemplate
{
    public static string Apply(
        string template,
        IVideo video,
        VideoDownloadOption downloadOption,
        int? number = null) =>
        PathEx.EscapeFileName(
            template
                .Replace("$num", number is not null ? $"[{number}]" : "")
                .Replace("$id", video.Id)
                .Replace("$title", video.Title)
                .Replace("$author", video.Author.Title)
                .Trim() + '.' + downloadOption.Container.Name
        );
}