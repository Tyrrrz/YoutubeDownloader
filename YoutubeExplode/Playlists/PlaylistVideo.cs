using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Playlists;

/// <summary>
/// Metadata associated with a YouTube video included in a playlist.
/// </summary>
public class PlaylistVideo(
    PlaylistId playlistId,
    VideoId id,
    string title,
    Author author,
    TimeSpan? duration,
    IReadOnlyList<Thumbnail> thumbnails
) : IVideo, IBatchItem
{
    /// <summary>
    /// Initializes an instance of <see cref="PlaylistVideo" />.
    /// </summary>
    // Binary backwards compatibility (PlaylistId was added)
    [Obsolete("Use the other constructor instead."), ExcludeFromCodeCoverage]
    public PlaylistVideo(
        VideoId id,
        string title,
        Author author,
        TimeSpan? duration,
        IReadOnlyList<Thumbnail> thumbnails
    )
        : this(default, id, title, author, duration, thumbnails) { }

    /// <summary>
    /// ID of the playlist that contains this video.
    /// </summary>
    public PlaylistId PlaylistId { get; } = playlistId;

    /// <inheritdoc />
    public VideoId Id { get; } = id;

    /// <inheritdoc />
    public string Url => $"https://www.youtube.com/watch?v={Id}&list={PlaylistId}";

    /// <inheritdoc />
    public string Title { get; } = title;

    /// <inheritdoc />
    public Author Author { get; } = author;

    /// <inheritdoc />
    public TimeSpan? Duration { get; } = duration;

    /// <inheritdoc />
    public IReadOnlyList<Thumbnail> Thumbnails { get; } = thumbnails;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Video ({Title})";
}
