namespace YoutubeExplode.Exceptions;

/// <summary>
/// Exception thrown when the requested playlist is unavailable.
/// </summary>
public class PlaylistUnavailableException(string message) : YoutubeExplodeException(message);
