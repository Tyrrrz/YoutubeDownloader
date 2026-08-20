using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;

namespace YoutubeExplode.Channels;

/// <summary>
/// Metadata associated with a YouTube channel.
/// </summary>
public class Channel(ChannelId id, string title, IReadOnlyList<Thumbnail> thumbnails) : IChannel
{
    /// <inheritdoc />
    public ChannelId Id { get; } = id;

    /// <inheritdoc />
    public string Url => $"https://www.youtube.com/channel/{Id}";

    /// <inheritdoc />
    public string Title { get; } = title;

    /// <inheritdoc />
    public IReadOnlyList<Thumbnail> Thumbnails { get; } = thumbnails;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Channel ({Title})";
}
