using System.Collections.Generic;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core;

public record YoutubeQueryResult(YoutubeQueryKind Kind, string Label, IReadOnlyList<IVideo> Videos);