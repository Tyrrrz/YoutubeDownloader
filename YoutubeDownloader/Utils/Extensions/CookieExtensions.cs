using System;
using System.Net;

namespace YoutubeDownloader.Utils.Extensions;

internal static class CookieExtensions
{
    extension(Cookie cookie)
    {
        // Cookie domains may be scoped to a parent domain (e.g. ".youtube.com"), so check
        // that the URI's host matches the cookie's domain exactly or as a subdomain
        public bool IsApplicableFor(Uri uri)
        {
            var domain = cookie.Domain.TrimStart('.');

            return string.Equals(domain, uri.Host, StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith('.' + domain, StringComparison.OrdinalIgnoreCase);
        }
    }
}
