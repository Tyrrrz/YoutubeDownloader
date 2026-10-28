using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using PowerKit.Extensions;

namespace YoutubeDownloader.Core.Resolving;

public static class PlaylistXmlWriter
{
    public static string GetFileName(ResolvedPlaylist playlist) =>
        Path.EscapeFileName(playlist.Playlist.Title.Trim()) + ".xml";

    public static async Task WriteAsync(
        string filePath,
        ResolvedPlaylist playlist,
        IReadOnlyDictionary<string, string>? videoFilePaths = null,
        CancellationToken cancellationToken = default
    )
    {
        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement(
                "Playlist",
                new XAttribute("Id", playlist.Playlist.Id.Value),
                new XAttribute("Url", playlist.Playlist.Url),
                new XAttribute("Count", playlist.Videos.Count),
                new XAttribute("SavedAt", DateTimeOffset.Now.ToString("o")),
                new XElement("Title", playlist.Playlist.Title),
                new XElement("Author", playlist.Playlist.Author?.ChannelTitle ?? ""),
                new XElement(
                    "Videos",
                    playlist.Videos.Select(
                        (video, i) =>
                            new XElement(
                                "Video",
                                new XAttribute("Position", i + 1),
                                new XAttribute("Id", video.Id.Value),
                                new XElement("Title", video.Title),
                                new XElement("Author", video.Author.ChannelTitle),
                                new XElement("Duration", video.Duration?.ToString() ?? ""),
                                new XElement("Url", video.Url),
                                videoFilePaths?.TryGetValue(video.Id.Value, out var videoFilePath)
                                == true
                                    ? new XElement("FileName", Path.GetFileName(videoFilePath))
                                    : null
                            )
                    )
                )
            )
        );

        Directory.CreateForFile(filePath);

        await using var stream = File.Create(filePath);
        await document.SaveAsync(stream, SaveOptions.None, cancellationToken);
    }
}
