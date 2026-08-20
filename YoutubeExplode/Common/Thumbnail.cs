using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Common;

/// <summary>
/// Thumbnail image.
/// </summary>
public partial class Thumbnail(string url, Resolution resolution)
{
    /// <summary>
    /// Thumbnail URL.
    /// </summary>
    public string Url { get; } = url;

    /// <summary>
    /// Thumbnail resolution.
    /// </summary>
    public Resolution Resolution { get; } = resolution;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Thumbnail ({Resolution})";
}

public partial class Thumbnail
{
    internal static IReadOnlyList<Thumbnail> GetDefaultSet(VideoId videoId) =>
        [
            new($"https://img.youtube.com/vi/{videoId}/default.jpg", new Resolution(120, 90)),
            new($"https://img.youtube.com/vi/{videoId}/mqdefault.jpg", new Resolution(320, 180)),
            new($"https://img.youtube.com/vi/{videoId}/hqdefault.jpg", new Resolution(480, 360)),
        ];
}

/// <summary>
/// Extensions for <see cref="Thumbnail" />.
/// </summary>
public static class ThumbnailExtensions
{
    /// <inheritdoc cref="ThumbnailExtensions" />
    extension(IEnumerable<Thumbnail> thumbnails)
    {
        /// <summary>
        /// Gets the thumbnail with the highest resolution (by area).
        /// Returns <see langword="null" /> if the sequence is empty.
        /// </summary>
        public Thumbnail? TryGetWithHighestResolution() => thumbnails.MaxBy(t => t.Resolution.Area);

        /// <summary>
        /// Gets the thumbnail with the highest resolution (by area).
        /// </summary>
        public Thumbnail GetWithHighestResolution() =>
            thumbnails.TryGetWithHighestResolution()
            ?? throw new InvalidOperationException("Input thumbnail collection is empty.");
    }
}
