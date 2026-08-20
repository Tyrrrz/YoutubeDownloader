using System;
using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Videos.ClosedCaptions;

/// <summary>
/// Individual closed caption part contained within a track.
/// </summary>
public class ClosedCaptionPart(string text, TimeSpan offset)
{
    /// <summary>
    /// Text displayed by the caption part.
    /// </summary>
    public string Text { get; } = text;

    /// <summary>
    /// Time at which the caption part starts displaying, relative to the caption's own offset.
    /// </summary>
    public TimeSpan Offset { get; } = offset;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => Text;
}
