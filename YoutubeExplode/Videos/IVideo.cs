using System;
using System.Collections.Generic;
using YoutubeExplode.Common;

namespace YoutubeExplode.Videos;

/// <summary>
/// Properties shared by video metadata resolved from different sources.
/// </summary>
public interface IVideo
{
    /// <summary>
    /// Video ID.
    /// </summary>
    VideoId Id { get; }

    /// <summary>
    /// Video URL.
    /// </summary>
    string Url { get; }

    /// <summary>
    /// Video title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Video author.
    /// </summary>
    Author Author { get; }

    /// <summary>
    /// Video duration.
    /// </summary>
    /// <remarks>
    /// May be null if the video is a currently ongoing live stream.
    /// </remarks>
    TimeSpan? Duration { get; }

    /// <summary>
    /// Video thumbnails.
    /// </summary>
    IReadOnlyList<Thumbnail> Thumbnails { get; }
}
