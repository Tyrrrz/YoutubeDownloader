using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.ClosedCaptions;

/// <summary>
/// Describes closed caption tracks available for a YouTube video.
/// </summary>
public class ClosedCaptionManifest(IReadOnlyList<ClosedCaptionTrackInfo> tracks)
{
    /// <summary>
    /// Available closed caption tracks.
    /// </summary>
    public IReadOnlyList<ClosedCaptionTrackInfo> Tracks { get; } = tracks;

    /// <summary>
    /// Gets the closed caption track in the specified language (identified by ISO-639-1 code or display name).
    /// Returns <see langword="null" /> if not found.
    /// </summary>
    public ClosedCaptionTrackInfo? TryGetByLanguage(string language) =>
        Tracks.FirstOrDefault(t =>
            string.Equals(t.Language.Code, language, StringComparison.OrdinalIgnoreCase)
            || string.Equals(t.Language.Name, language, StringComparison.OrdinalIgnoreCase)
        );

    /// <summary>
    /// Gets the closed caption track in the specified language (identified by ISO-639-1 code or display name).
    /// </summary>
    public ClosedCaptionTrackInfo GetByLanguage(string language) =>
        TryGetByLanguage(language)
        ?? throw new InvalidOperationException(
            $"No closed caption track available for language '{language}'."
        );
}
