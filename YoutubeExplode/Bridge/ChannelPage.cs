using System;
using AngleSharp.Html.Dom;
using Lazy;
using PowerKit.Extensions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Bridge;

internal partial class ChannelPage(IHtmlDocument content)
{
    [Lazy]
    public string? Url =>
        content.QuerySelector("meta[property=\"og:url\"]")?.GetAttribute("content");

    [Lazy]
    public string? Id => Url?.SubstringAfter("channel/", StringComparison.OrdinalIgnoreCase);

    [Lazy]
    public string? Title =>
        content.QuerySelector("meta[property=\"og:title\"]")?.GetAttribute("content");

    [Lazy]
    public string? LogoUrl =>
        content.QuerySelector("meta[property=\"og:image\"]")?.GetAttribute("content");
}

internal partial class ChannelPage
{
    public static ChannelPage? TryParse(string raw)
    {
        var content = Html.Parse(raw);

        if (content.QuerySelector("meta[property=\"og:url\"]") is null)
            return null;

        return new ChannelPage(content);
    }
}
