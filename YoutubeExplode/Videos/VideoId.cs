using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using PowerKit.Extensions;

namespace YoutubeExplode.Videos;

/// <summary>
/// Represents a syntactically valid YouTube video ID.
/// </summary>
public readonly partial struct VideoId(string value)
{
    /// <summary>
    /// Raw ID value.
    /// </summary>
    public string Value { get; } = value;

    /// <inheritdoc />
    public override string ToString() => Value;
}

public partial struct VideoId
{
    private static bool IsValid(string videoId) =>
        videoId.Length == 11 && videoId.All(c => char.IsLetterOrDigit(c) || c is '_' or '-');

    private static string? TryNormalize(string? videoIdOrUrl)
    {
        static string? TryExtractId(string url, string pattern)
        {
            var id = Regex.Match(url, pattern).Groups[1].Value.Pipe(WebUtility.UrlDecode);
            return !string.IsNullOrWhiteSpace(id) && IsValid(id) ? id : null;
        }

        if (string.IsNullOrWhiteSpace(videoIdOrUrl))
            return null;

        // Check if already passed an ID
        // yIVRs6YSbOM
        if (IsValid(videoIdOrUrl))
            return videoIdOrUrl;

        // Try to extract the ID from the URL
        return
            // Regular video URL
            // https://www.youtube.com/watch?v=yIVRs6YSbOM
            TryExtractId(videoIdOrUrl, @"youtube\..+?/watch.*?v=(.*?)(?:&|/|$)")
            // Short video URL (type 1)
            // https://youtu.be/watch?v=Fcds0_MrgNU
            ?? TryExtractId(videoIdOrUrl, @"youtu\.be/watch.*?v=(.*?)(?:\?|&|/|$)")
            // Short video URL (type 2)
            // https://youtu.be/yIVRs6YSbOM
            ?? TryExtractId(videoIdOrUrl, @"youtu\.be/(.*?)(?:\?|&|/|$)")
            // Embed URL
            // https://www.youtube.com/embed/yIVRs6YSbOM
            ?? TryExtractId(videoIdOrUrl, @"youtube\..+?/embed/(.*?)(?:\?|&|/|$)")
            // Shorts URL
            // https://www.youtube.com/shorts/sKL1vjP0tIo
            ?? TryExtractId(videoIdOrUrl, @"youtube\..+?/shorts/(.*?)(?:\?|&|/|$)")
            // Live URL
            // https://www.youtube.com/live/jfKfPfyJRdk
            ?? TryExtractId(videoIdOrUrl, @"youtube\..+?/live/(.*?)(?:\?|&|/|$)");
    }

    /// <summary>
    /// Attempts to parse the specified string as a video ID or URL.
    /// Returns <see langword="null" /> in case of failure.
    /// </summary>
    public static VideoId? TryParse(string? videoIdOrUrl) =>
        TryNormalize(videoIdOrUrl)?.Pipe(id => new VideoId(id));

    /// <summary>
    /// Parses the specified string as a YouTube video ID or URL.
    /// Throws an exception in case of failure.
    /// </summary>
    public static VideoId Parse(string videoIdOrUrl) =>
        TryParse(videoIdOrUrl)
        ?? throw new ArgumentException($"Invalid YouTube video ID or URL '{videoIdOrUrl}'.");

    /// <summary>
    /// Converts string to ID.
    /// </summary>
    public static implicit operator VideoId(string videoIdOrUrl) => Parse(videoIdOrUrl);

    /// <summary>
    /// Converts ID to string.
    /// </summary>
    public static implicit operator string(VideoId videoId) => videoId.ToString();
}

public partial struct VideoId : IEquatable<VideoId>
{
    /// <inheritdoc />
    public bool Equals(VideoId other) =>
        string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is VideoId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(VideoId left, VideoId right) => left.Equals(right);

    /// <inheritdoc cref="operator ==(VideoId, VideoId)" />
    public static bool operator !=(VideoId left, VideoId right) => !(left == right);
}
