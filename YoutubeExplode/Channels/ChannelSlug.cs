using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using PowerKit.Extensions;

namespace YoutubeExplode.Channels;

/// <summary>
/// Represents a syntactically valid YouTube channel slug.
/// </summary>
public readonly partial struct ChannelSlug(string value)
{
    /// <summary>
    /// Raw slug value.
    /// </summary>
    public string Value { get; } = value;

    /// <inheritdoc />
    public override string ToString() => Value;
}

public partial struct ChannelSlug
{
    private static bool IsValid(string channelSlug) => channelSlug.All(char.IsLetterOrDigit);

    private static string? TryNormalize(string? channelSlugOrUrl)
    {
        if (string.IsNullOrWhiteSpace(channelSlugOrUrl))
            return null;

        // Check if already passed a slug
        // Tyrrrz
        if (IsValid(channelSlugOrUrl))
            return channelSlugOrUrl;

        // Try to extract the slug from the URL
        // https://www.youtube.com/c/Tyrrrz
        var slug = Regex
            .Match(channelSlugOrUrl, @"youtube\..+?/c/(.*?)(?:\?|&|/|$)")
            .Groups[1]
            .Value.Pipe(WebUtility.UrlDecode);

        if (!string.IsNullOrWhiteSpace(slug) && IsValid(slug))
            return slug;

        // Invalid input
        return null;
    }

    /// <summary>
    /// Attempts to parse the specified string as a YouTube channel slug or legacy custom URL.
    /// Returns <see langword="null" /> in case of failure.
    /// </summary>
    public static ChannelSlug? TryParse(string? channelSlugOrUrl) =>
        TryNormalize(channelSlugOrUrl)?.Pipe(slug => new ChannelSlug(slug));

    /// <summary>
    /// Parses the specified string as a YouTube channel slug or legacy custom url.
    /// </summary>
    public static ChannelSlug Parse(string channelSlugOrUrl) =>
        TryParse(channelSlugOrUrl)
        ?? throw new ArgumentException(
            $"Invalid YouTube channel slug or legacy custom URL '{channelSlugOrUrl}'."
        );

    /// <summary>
    /// Converts string to channel slug.
    /// </summary>
    public static implicit operator ChannelSlug(string channelSlugOrUrl) => Parse(channelSlugOrUrl);

    /// <summary>
    /// Converts channel slug to string.
    /// </summary>
    public static implicit operator string(ChannelSlug channelSlug) => channelSlug.ToString();
}

public partial struct ChannelSlug : IEquatable<ChannelSlug>
{
    /// <inheritdoc />
    public bool Equals(ChannelSlug other) =>
        string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ChannelSlug other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(ChannelSlug left, ChannelSlug right) => left.Equals(right);

    /// <inheritdoc cref="operator ==(ChannelSlug, ChannelSlug)" />
    public static bool operator !=(ChannelSlug left, ChannelSlug right) => !(left == right);
}
