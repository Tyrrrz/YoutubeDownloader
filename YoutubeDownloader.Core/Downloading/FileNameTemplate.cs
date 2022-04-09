using YoutubeDownloader.Core.Utils;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading;

public class FileNameTemplate
{
    public static string Apply(
        string template,
        IVideo video,
        Container container,
        int? number = null) =>
        PathEx.EscapeFileName(
            template
                .Replace("$num", number is not null ? $"[{number}]" : "")
                .Replace("$id", video.Id)
                .Replace("$title", video.Title)
                .Replace("$author", video.Author.Title)
                .Trim() + '.' + container.Name
        );
}