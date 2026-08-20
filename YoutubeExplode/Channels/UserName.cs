using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using PowerKit.Extensions;

namespace YoutubeExplode.Channels;

/// <summary>
/// Represents a syntactically valid YouTube user name.
/// </summary>
public readonly partial struct UserName(string value)
{
    /// <summary>
    /// Raw user name value.
    /// </summary>
    public string Value { get; } = value;

    /// <inheritdoc />
    public override string ToString() => Value;
}

public partial struct UserName
{
    private static bool IsValid(string userName) =>
        userName.Length <= 20 && userName.All(char.IsLetterOrDigit);

    private static string? TryNormalize(string? userNameOrUrl)
    {
        if (string.IsNullOrWhiteSpace(userNameOrUrl))
            return null;

        // Check if already passed a user name
        // TheTyrrr
        if (IsValid(userNameOrUrl))
            return userNameOrUrl;

        // Try to extract the user name from the URL
        // https://www.youtube.com/user/TheTyrrr
        var userName = Regex
            .Match(userNameOrUrl, @"youtube\..+?/user/(.*?)(?:\?|&|/|$)")
            .Groups[1]
            .Value.Pipe(WebUtility.UrlDecode);

        if (!string.IsNullOrWhiteSpace(userName) && IsValid(userName))
            return userName;

        // Invalid input
        return null;
    }

    /// <summary>
    /// Attempts to parse the specified string as a YouTube user name or profile URL.
    /// Returns <see langword="null" /> in case of failure.
    /// </summary>
    public static UserName? TryParse(string? userNameOrUrl) =>
        TryNormalize(userNameOrUrl)?.Pipe(name => new UserName(name));

    /// <summary>
    /// Parses the specified string as a YouTube user name or profile URL.
    /// </summary>
    public static UserName Parse(string userNameOrUrl) =>
        TryParse(userNameOrUrl)
        ?? throw new ArgumentException(
            $"Invalid YouTube user name or profile URL '{userNameOrUrl}'."
        );

    /// <summary>
    /// Converts string to user name.
    /// </summary>
    public static implicit operator UserName(string userNameOrUrl) => Parse(userNameOrUrl);

    /// <summary>
    /// Converts user name to string.
    /// </summary>
    public static implicit operator string(UserName userName) => userName.ToString();
}

public partial struct UserName : IEquatable<UserName>
{
    /// <inheritdoc />
    public bool Equals(UserName other) =>
        string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is UserName other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Equality check.
    /// </summary>
    public static bool operator ==(UserName left, UserName right) => left.Equals(right);

    /// <inheritdoc cref="operator ==(UserName, UserName)" />
    public static bool operator !=(UserName left, UserName right) => !(left == right);
}
