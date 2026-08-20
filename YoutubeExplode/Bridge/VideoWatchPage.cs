using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using JsonExtensions.Reading;
using Lazy;
using PowerKit.Extensions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class VideoWatchPage(IHtmlDocument content)
{
    [Lazy]
    public bool IsAvailable => content.QuerySelector("meta[property=\"og:url\"]") is not null;

    [Lazy]
    public DateTimeOffset? UploadDate =>
        content
            .QuerySelector("meta[itemprop=\"uploadDate\"]")
            ?.GetAttribute("content")
            ?.NullIfWhiteSpace()
            ?.Pipe(s =>
                DateTimeOffset.ParseOrNull(s, CultureInfo.InvariantCulture, DateTimeStyles.None)
            )
        ?? content
            .QuerySelector("meta[itemprop=\"datePublished\"]")
            ?.GetAttribute("content")
            ?.NullIfWhiteSpace()
            ?.Pipe(s =>
                DateTimeOffset.ParseOrNull(s, CultureInfo.InvariantCulture, DateTimeStyles.None)
            );

    [Lazy]
    public long? LikeCount =>
        content
            .Source.Text.Pipe(s =>
                Regex
                    .Match(
                        s,
                        """
                        "\s*:\s*"([\d,\.]+) likes"
                        """
                    )
                    .Groups[1]
                    .Value
            )
            .NullIfWhiteSpace()
            ?.StripNonDigit()
            .Pipe(s => long.ParseOrNull(s, CultureInfo.InvariantCulture))
        ?? content
            .Source.Text.Pipe(s =>
                Regex
                    .Match(
                        s,
                        """
                        along with ([\d,\.]+) other people"
                        """
                    )
                    .Groups[1]
                    .Value
            )
            .NullIfWhiteSpace()
            ?.StripNonDigit()
            .Pipe(s => long.ParseOrNull(s, CultureInfo.InvariantCulture));

    [Lazy]
    public long? DislikeCount =>
        content
            .Source.Text.Pipe(s =>
                Regex
                    .Match(
                        s,
                        """
                        "\s*:\s*"([\d,\.]+) dislikes"
                        """
                    )
                    .Groups[1]
                    .Value
            )
            .NullIfWhiteSpace()
            ?.StripNonDigit()
            .Pipe(s => long.ParseOrNull(s, CultureInfo.InvariantCulture));

    [Lazy]
    private JsonElement? PlayerConfig =>
        content
            .GetElementsByTagName("script")
            .Select(e => e.Text())
            .Select(s => Regex.Match(s, @"ytplayer\.config\s*=\s*(\{.*\})").Groups[1].Value)
            .WhereNotNullOrWhiteSpace()
            .FirstOrDefault()
            ?.Pipe(Json.Extract)
            .Pipe(Json.TryParse);

    [Lazy]
    public PlayerResponse? PlayerResponse =>
        content
            .GetElementsByTagName("script")
            .Select(e => e.Text())
            .Select(s =>
                Regex.Match(s, @"var\s+ytInitialPlayerResponse\s*=\s*(\{.*\})").Groups[1].Value
            )
            .WhereNotNullOrWhiteSpace()
            .FirstOrDefault()
            ?.Pipe(Json.Extract)
            .Pipe(Json.TryParse)
            ?.Pipe(j => new PlayerResponse(j))
        ?? PlayerConfig
            ?.GetPropertyOrNull("args")
            ?.GetPropertyOrNull("player_response")
            ?.GetStringOrNull()
            ?.Pipe(Json.TryParse)
            ?.Pipe(j => new PlayerResponse(j));
}

internal partial class VideoWatchPage
{
    public static VideoWatchPage? TryParse(string raw)
    {
        var content = Html.Parse(raw);

        if (content.Body?.QuerySelector("#player") is null)
            return null;

        return new VideoWatchPage(content);
    }
}
