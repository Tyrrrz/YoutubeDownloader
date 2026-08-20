using System;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Channels;

namespace YoutubeExplode.Common;

/// <summary>
/// Reference to a channel that owns a specific YouTube video or playlist.
/// </summary>
public class Author(ChannelId channelId, string channelTitle)
{
    /// <summary>
    /// Channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// Channel URL.
    /// </summary>
    public string ChannelUrl => $"https://www.youtube.com/channel/{ChannelId}";

    /// <summary>
    /// Channel title.
    /// </summary>
    public string ChannelTitle { get; } = channelTitle;

    /// <inheritdoc cref="ChannelTitle" />
    [Obsolete("Use ChannelTitle instead."), ExcludeFromCodeCoverage]
    public string Title => ChannelTitle;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => ChannelTitle;
}
