using System;
using System.Diagnostics.CodeAnalysis;

// TODO: breaking change: update the namespace
// ReSharper disable once CheckNamespace
namespace YoutubeExplode.Videos.ClosedCaptions;

/// <summary>
/// Language information.
/// </summary>
public readonly partial struct Language(string code, string name)
{
    /// <summary>
    /// Two-letter or three-letter language code, possibly with a regional identifier
    /// (e.g., 'en' or 'en-US' or 'eng').
    /// </summary>
    public string Code { get; } = code;

    /// <summary>
    /// Full international name of the language.
    /// </summary>
    public string Name { get; } = name;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{Code} ({Name})";
}

public partial struct Language : IEquatable<Language>
{
    /// <inheritdoc />
    public bool Equals(Language other) =>
        string.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Language other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(Language left, Language right) => left.Equals(right);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator !=(Language left, Language right) => !(left == right);
}
