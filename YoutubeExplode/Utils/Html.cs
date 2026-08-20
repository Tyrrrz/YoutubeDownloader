using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace YoutubeExplode.Utils;

internal static class Html
{
    // A new HtmlParser instance must be created for each call to avoid thread safety issues
    // when multiple YoutubeClient instances are used concurrently. HtmlParser is not thread-safe
    // and sharing a single static instance causes corruption errors (InvalidOperationException)
    // in AngleSharp's internal collections.
    // https://github.com/Tyrrrz/YoutubeExplode/issues/955
    public static IHtmlDocument Parse(string source) => new HtmlParser().ParseDocument(source);
}
