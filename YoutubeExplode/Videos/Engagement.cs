using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Videos;

/// <summary>
/// Engagement statistics.
/// </summary>
public class Engagement(long viewCount, long likeCount, long dislikeCount)
{
    /// <summary>
    /// View count.
    /// </summary>
    public long ViewCount { get; } = viewCount;

    /// <summary>
    /// Like count.
    /// </summary>
    public long LikeCount { get; } = likeCount;

    /// <summary>
    /// Dislike count.
    /// </summary>
    /// <remarks>
    /// YouTube no longer shows dislikes, so this value is always 0.
    /// </remarks>
    public long DislikeCount { get; } = dislikeCount;

    /// <summary>
    /// Average rating.
    /// </summary>
    /// <remarks>
    /// YouTube no longer shows dislikes, so this value is always 5.
    /// </remarks>
    public double AverageRating =>
        LikeCount + DislikeCount != 0 ? 1 + 4.0 * LikeCount / (LikeCount + DislikeCount) : 0; // avoid division by 0

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Rating: {AverageRating:N1}";
}
