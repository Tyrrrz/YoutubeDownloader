using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using PowerKit.Extensions;

namespace YoutubeExplode.Playlists;

/// <summary>
/// Represents a syntactically valid YouTube playlist ID.
/// </summary>
public readonly partial struct PlaylistId(string value)
{
    /// <summary>
    /// Raw ID value.
    /// </summary>
    public string Value { get; } = value;

    /// <inheritdoc />
    public override string ToString() => Value;
}

public partial struct PlaylistId
{
    private static bool IsValid(string playlistId) =>
        // Playlist IDs vary greatly in length, but they are at least 2 characters long
        playlistId.Length >= 2
        && playlistId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    private static string? TryNormalize(string? playlistIdOrUrl)
    {
        static string? TryExtractId(string url, string pattern)
        {
            var id = Regex.Match(url, pattern).Groups[1].Value.Pipe(WebUtility.UrlDecode);
            return !string.IsNullOrWhiteSpace(id) && IsValid(id) ? id : null;
        }

        if (string.IsNullOrWhiteSpace(playlistIdOrUrl))
            return null;

        // Check if already passed an ID
        // PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H
        if (IsValid(playlistIdOrUrl))
            return playlistIdOrUrl;

        // Try to extract the ID from the URL
        return
            // Regular playlist URL
            // https://www.youtube.com/playlist?list=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H
            TryExtractId(playlistIdOrUrl, @"youtube\..+?/playlist.*?list=(.*?)(?:&|/|$)")
            // Video URL with playlist ID
            // https://www.youtube.com/watch?v=b8m9zhNAgKs&list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            ?? TryExtractId(playlistIdOrUrl, @"youtube\..+?/watch.*?list=(.*?)(?:&|/|$)")
            // Short video URL with playlist ID
            // https://youtu.be/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            ?? TryExtractId(playlistIdOrUrl, @"youtu\.be/.*?/.*?list=(.*?)(?:&|/|$)")
            // Embed URL with playlist ID
            // https://www.youtube.com/embed/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            ?? TryExtractId(playlistIdOrUrl, @"youtube\..+?/embed/.*?/.*?list=(.*?)(?:&|/|$)");
    }

    /// <summary>
    /// Attempts to parse the specified string as a YouTube playlist ID or URL.
    /// Returns <see langword="null" /> in case of failure.
    /// </summary>
    public static PlaylistId? TryParse(string? playlistIdOrUrl) =>
        TryNormalize(playlistIdOrUrl)?.Pipe(id => new PlaylistId(id));

    /// <summary>
    /// Parses the specified string as a YouTube playlist ID or URL.
    /// </summary>
    public static PlaylistId Parse(string playlistIdOrUrl) =>
        TryParse(playlistIdOrUrl)
        ?? throw new ArgumentException($"Invalid YouTube playlist ID or URL '{playlistIdOrUrl}'.");

    /// <summary>
    /// Converts string to ID.
    /// </summary>
    public static implicit operator PlaylistId(string playlistIdOrUrl) => Parse(playlistIdOrUrl);

    /// <summary>
    /// Converts ID to string.
    /// </summary>
    public static implicit operator string(PlaylistId playlistId) => playlistId.ToString();
}

public partial struct PlaylistId : IEquatable<PlaylistId>
{
    /// <inheritdoc />
    public bool Equals(PlaylistId other) =>
        string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PlaylistId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(PlaylistId left, PlaylistId right) => left.Equals(right);

    /// <inheritdoc cref="operator ==(PlaylistId, PlaylistId)" />
    public static bool operator !=(PlaylistId left, PlaylistId right) => !(left == right);
}
