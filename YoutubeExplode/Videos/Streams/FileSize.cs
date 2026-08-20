using System;

namespace YoutubeExplode.Videos.Streams;

/// <summary>
/// File size.
/// </summary>
// Loosely based on https://github.com/omar/ByteSize (MIT license)
public readonly partial struct FileSize(long bytes)
{
    /// <summary>
    /// Size in bytes.
    /// </summary>
    public long Bytes { get; } = bytes;

    /// <summary>
    /// Size in kilobytes.
    /// </summary>
    public double KiloBytes => Bytes / 1024.0;

    /// <summary>
    /// Size in megabytes.
    /// </summary>
    public double MegaBytes => KiloBytes / 1024.0;

    /// <summary>
    /// Size in gigabytes.
    /// </summary>
    public double GigaBytes => MegaBytes / 1024.0;

    private string GetLargestWholeNumberSymbol()
    {
        if (Math.Abs(GigaBytes) >= 1)
            return "GB";

        if (Math.Abs(MegaBytes) >= 1)
            return "MB";

        if (Math.Abs(KiloBytes) >= 1)
            return "KB";

        return "B";
    }

    private double GetLargestWholeNumberValue()
    {
        if (Math.Abs(GigaBytes) >= 1)
            return GigaBytes;

        if (Math.Abs(MegaBytes) >= 1)
            return MegaBytes;

        if (Math.Abs(KiloBytes) >= 1)
            return KiloBytes;

        return Bytes;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{GetLargestWholeNumberValue():0.##} {GetLargestWholeNumberSymbol()}";
}

public partial struct FileSize : IComparable<FileSize>, IEquatable<FileSize>
{
    /// <inheritdoc />
    public int CompareTo(FileSize other) => Bytes.CompareTo(other.Bytes);

    /// <inheritdoc />
    public bool Equals(FileSize other) => CompareTo(other) == 0;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is FileSize other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Bytes);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(FileSize left, FileSize right) => left.Equals(right);

    /// <inheritdoc cref="operator ==(FileSize, FileSize)" />
    public static bool operator !=(FileSize left, FileSize right) => !(left == right);

    /// <summary>
    /// Comparison.
    /// </summary>
    public static bool operator >(FileSize left, FileSize right) => left.CompareTo(right) > 0;

    /// <inheritdoc cref="operator >(FileSize, FileSize)" />
    public static bool operator <(FileSize left, FileSize right) => left.CompareTo(right) < 0;
}
