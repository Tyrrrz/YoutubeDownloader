using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using PowerKit.Extensions;

namespace YoutubeExplode.Channels;

/// <summary>
/// Represents a syntactically valid YouTube channel ID.
/// </summary>
public readonly partial struct ChannelId(string value)
{
    /// <summary>
    /// Raw ID value.
    /// </summary>
    public string Value { get; } = value;

    /// <inheritdoc />
    public override string ToString() => Value;
}

public partial struct ChannelId
{
    private static bool IsValid(string channelId) =>
        channelId.StartsWith("UC", StringComparison.Ordinal)
        && channelId.Length == 24
        && channelId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    private static string? TryNormalize(string? channelIdOrUrl)
    {
        if (string.IsNullOrWhiteSpace(channelIdOrUrl))
            return null;

        // Check if already passed an ID
        // UC3xnGqlcL3y-GXz5N3wiTJQ
        if (IsValid(channelIdOrUrl))
            return channelIdOrUrl;

        // Try to extract the ID from the URL
        // https://www.youtube.com/channel/UC3xnGqlcL3y-GXz5N3wiTJQ
        var id = Regex
            .Match(channelIdOrUrl, @"youtube\..+?/channel/(.*?)(?:\?|&|/|$)")
            .Groups[1]
            .Value.Pipe(WebUtility.UrlDecode);

        if (!string.IsNullOrWhiteSpace(id) && IsValid(id))
            return id;

        // Invalid input
        return null;
    }

    /// <summary>
    /// Attempts to parse the specified string as a YouTube channel ID or URL.
    /// Returns <see langword="null" /> in case of failure.
    /// </summary>
    public static ChannelId? TryParse(string? channelIdOrUrl) =>
        TryNormalize(channelIdOrUrl)?.Pipe(id => new ChannelId(id));

    /// <summary>
    /// Parses the specified string as a YouTube channel ID or URL.
    /// </summary>
    public static ChannelId Parse(string channelIdOrUrl) =>
        TryParse(channelIdOrUrl)
        ?? throw new ArgumentException($"Invalid YouTube channel ID or URL '{channelIdOrUrl}'.");

    /// <summary>
    /// Converts string to ID.
    /// </summary>
    public static implicit operator ChannelId(string channelIdOrUrl) => Parse(channelIdOrUrl);

    /// <summary>
    /// Converts ID to string.
    /// </summary>
    public static implicit operator string(ChannelId channelId) => channelId.ToString();
}

public partial struct ChannelId : IEquatable<ChannelId>
{
    /// <inheritdoc />
    public bool Equals(ChannelId other) =>
        string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ChannelId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(ChannelId left, ChannelId right) => left.Equals(right);

    /// <inheritdoc cref="operator ==(ChannelId, ChannelId)" />
    public static bool operator !=(ChannelId left, ChannelId right) => !(left == right);
}
