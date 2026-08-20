using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Search;

/// <summary>
/// Metadata associated with a YouTube video returned by a search query.
/// </summary>
public class VideoSearchResult(
    VideoId id,
    string title,
    Author author,
    TimeSpan? duration,
    IReadOnlyList<Thumbnail> thumbnails
) : ISearchResult, IVideo
{
    /// <inheritdoc />
    public VideoId Id { get; } = id;

    /// <inheritdoc cref="IVideo.Url" />
    public string Url => $"https://www.youtube.com/watch?v={Id}";

    /// <inheritdoc cref="IVideo.Title" />
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
