using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PowerKit;
using PowerKit.Extensions;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;

namespace YoutubeExplode;

internal class YoutubeHttpHandler : ClientDelegatingHandler
{
    private readonly CookieContainer _cookieContainer = new();

    public YoutubeHttpHandler(
        HttpClient http,
        IReadOnlyList<Cookie> initialCookies,
        bool disposeClient = false
    )
        : base(http, disposeClient)
    {
        // Consent to the use of cookies on YouTube.
        // This is required to access some personalized content, such as mix playlists.
        // https://github.com/Tyrrrz/YoutubeExplode/issues/730
        // https://github.com/Tyrrrz/YoutubeExplode/issues/732
        // https://github.com/Tyrrrz/YoutubeExplode/issues/907
        // The cookie is supposed to be invalidated after 13 months, at which point the value
        // becomes invalid and needs to be manually replaced in code with a new one.
        // https://policies.google.com/technologies/cookies/embedded
        _cookieContainer.Add(
            new Cookie("SOCS", "CAISEwgDEgk4MTM4MzYzNTIaAmVuIAEaBgiApPzGBg")
            {
                Domain = "youtube.com",
            }
        );

        // Add user-supplied cookies
        foreach (var cookie in initialCookies)
            _cookieContainer.Add(cookie);
    }

    private string? TryGenerateAuthHeaderValue(Uri uri)
    {
        var cookies = _cookieContainer.GetCookies(uri).Cast<Cookie>().ToArray();

        var sessionId =
            cookies
                .FirstOrDefault(c =>
                    string.Equals(c.Name, "__Secure-3PAPISID", StringComparison.Ordinal)
                )
                ?.Value
            ?? cookies
                .FirstOrDefault(c => string.Equals(c.Name, "SAPISID", StringComparison.Ordinal))
                ?.Value;

        if (string.IsNullOrWhiteSpace(sessionId))
            return null;

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var token = $"{timestamp} {sessionId} {uri.Domain}";
        var tokenHash = Convert.ToHexString(
            HashAlgorithm.ComputeHash(SHA1.Create(), Encoding.UTF8.GetBytes(token))
        );

        return $"SAPISIDHASH {timestamp}_{tokenHash}";
    }

    private HttpRequestMessage HandleRequest(HttpRequestMessage request)
    {
        // Shouldn't happen?
        if (request.RequestUri is null)
            return request;

        // Set internal API key
        if (
            request.RequestUri.AbsolutePath.StartsWith("/youtubei/", StringComparison.Ordinal)
            && !UrlEx.ContainsQueryParameter(request.RequestUri.Query, "key")
        )
        {
            request.RequestUri = new Uri(
                UrlEx.SetQueryParameter(
                    request.RequestUri.OriginalString,
                    "key",
                    // This key doesn't appear to change
                    "AIzaSyA8eiZmM1FaDVjRy-df2KTyQ_vz_yYM39w"
                )
            );
        }

        // Set localization language
        if (!UrlEx.ContainsQueryParameter(request.RequestUri.Query, "hl"))
        {
            request.RequestUri = new Uri(
                UrlEx.SetQueryParameter(request.RequestUri.OriginalString, "hl", "en")
            );
        }

        // Set origin
        if (!request.Headers.Contains("Origin"))
        {
            request.Headers.Add("Origin", request.RequestUri.Domain);
        }

        // Set user agent
        if (!request.Headers.Contains("User-Agent"))
        {
            request.Headers.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36"
            );
        }

        // Set cookies
        if (!request.Headers.Contains("Cookie") && _cookieContainer.Count > 0)
        {
            var cookieHeaderValue = _cookieContainer.GetCookieHeader(request.RequestUri);
            if (!string.IsNullOrWhiteSpace(cookieHeaderValue))
                request.Headers.Add("Cookie", cookieHeaderValue);
        }

        // Set authorization
        if (
            !request.Headers.Contains("Authorization")
            && TryGenerateAuthHeaderValue(request.RequestUri) is { } authHeaderValue
        )
        {
            request.Headers.Add("Authorization", authHeaderValue);
        }

        return request;
    }

    private HttpResponseMessage HandleResponse(HttpResponseMessage response)
    {
        if (response.RequestMessage?.RequestUri is null)
            return response;

        // Custom exception for rate limit errors
        if ((int)response.StatusCode == 429)
        {
            throw new RequestLimitExceededException(
                "Exceeded request rate limit. "
                    + "Please try again in a few hours. "
                    + "Alternatively, inject cookies corresponding to a pre-authenticated user when initializing an instance of `YoutubeClient`."
            );
        }

        // Set cookies
        if (response.Headers.TryGetValues("Set-Cookie", out var cookieHeaderValues))
        {
            foreach (var cookieHeaderValue in cookieHeaderValues)
            {
                try
                {
                    _cookieContainer.SetCookies(
                        response.RequestMessage.RequestUri,
                        cookieHeaderValue
                    );
                }
                catch (CookieException)
                {
                    // YouTube may send cookies for other domains, ignore them
                    // https://github.com/Tyrrrz/YoutubeExplode/issues/762
                }
            }
        }

        return response;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        for (var retriesRemaining = 5; ; retriesRemaining--)
        {
            var response = HandleResponse(
                await base.SendAsync(
                    // Request will be cloned by the base handler
                    HandleRequest(request),
                    cancellationToken
                )
            );

            // Retry on 5XX errors
            if ((int)response.StatusCode >= 500 && retriesRemaining > 0)
            {
                response.Dispose();
                continue;
            }

            return response;
        }
    }
}
