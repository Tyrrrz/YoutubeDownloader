using System.Collections.Generic;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving;

public record ResolvedPlaylist(Playlist Playlist, IReadOnlyList<IVideo> Videos);
