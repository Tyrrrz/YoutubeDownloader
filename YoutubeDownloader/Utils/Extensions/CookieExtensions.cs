using System;
using System.Net;

namespace YoutubeDownloader.Utils.Extensions;

internal static class CookieExtensions
{
    extension(Cookie cookie)
    {
        // Cookie domains may be scoped to a parent domain (e.g. ".youtube.com"), so check
        // that the URI's host matches the cookie's domain exactly or as a subdomain.
        // The leading dot is re-added (rather than using cookie.Domain as-is) so that a
        // domain without one (e.g. "youtube.com") still enforces a "." boundary and
        // doesn't match unrelated hosts that merely share a suffix (e.g. "evilyoutube.com").
        public bool IsApplicableFor(Uri uri) =>
            string.Equals(
                cookie.Domain.TrimStart('.'),
                uri.Host,
                StringComparison.OrdinalIgnoreCase
            )
            || uri.Host.EndsWith(
                '.' + cookie.Domain.TrimStart('.'),
                StringComparison.OrdinalIgnoreCase
            );
    }
}
