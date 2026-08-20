namespace YoutubeExplode.Converter;

/// <summary>
/// Encoder preset.
/// </summary>
public enum ConversionPreset
{
    /// <summary>
    /// Much slower conversion speed and smaller output file size.
    /// </summary>
    VerySlow = -2,

    /// <summary>
    /// Slightly slower conversion speed and smaller output file size.
    /// </summary>
    Slow = -1,

    /// <summary>
    /// Default preset.
    /// Balanced conversion speed and output file size.
    /// </summary>
    Medium = 0,

    /// <summary>
    /// Slightly faster conversion speed and bigger output file size.
    /// </summary>
    Fast = 1,

    /// <summary>
    /// Much faster conversion speed and bigger output file size.
    /// </summary>
    VeryFast = 2,

    /// <summary>
    /// Fastest conversion speed and biggest output file size.
    /// </summary>
    UltraFast = 3,
}
