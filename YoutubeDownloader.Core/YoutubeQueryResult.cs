using System.Collections.Generic;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core;

public record YoutubeQueryResult(string Label, IReadOnlyList<IVideo> Videos);