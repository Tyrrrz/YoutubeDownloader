using System;
using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Common;

/// <summary>
/// Resolution of an image or a video.
/// </summary>
public readonly partial struct Resolution(int width, int height)
{
    /// <summary>
    /// Viewport width, measured in pixels.
    /// </summary>
    public int Width { get; } = width;

    /// <summary>
    /// Viewport height, measured in pixels.
    /// </summary>
    public int Height { get; } = height;

    /// <summary>
    /// Viewport area (i.e., width multiplied by height).
    /// </summary>
    public int Area => Width * Height;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{Width}x{Height}";
}

public partial struct Resolution : IEquatable<Resolution>
{
    /// <inheritdoc />
    public bool Equals(Resolution other) => Width == other.Width && Height == other.Height;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Resolution other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Width, Height);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(Resolution left, Resolution right) => left.Equals(right);

    /// <inheritdoc cref="operator ==(Resolution, Resolution)" />
    public static bool operator !=(Resolution left, Resolution right) => !(left == right);
}
