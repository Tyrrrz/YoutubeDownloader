using System.Collections.Generic;
using System.IO;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Downloading;

public record VideoDownloadRequest(
    string FilePath,
    IVideo Video,
    VideoDownloadOption VideoOption,
    IReadOnlyList<SubtitleDownloadOption> SubtitleOptions)
{
    public string FileName => Path.GetFileName(FilePath);
}