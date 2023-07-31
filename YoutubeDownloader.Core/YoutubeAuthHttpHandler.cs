using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils.Extensions;

namespace YoutubeDownloader.Core;

public partial class YoutubeAuthHttpHandler : DelegatingHandler
{
    private readonly CookieContainer _cookieContainer = new();

    public YoutubeAuthHttpHandler(IReadOnlyDictionary<string, string> cookies)
    {
        foreach (var (key, value) in cookies)
            _cookieContainer.Add(YoutubeDomainUri, new Cookie(key, value));

        InnerHandler = new SocketsHttpHandler
        {
            UseCookies = true,
            // Need to use a cookie container because YouTube sets additional cookies
            // even after the initial authentication.
            CookieContainer = _cookieContainer
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var cookies = _cookieContainer
            .GetCookies(YoutubeDomainUri)
            // Most specific cookies first
            .OrderByDescending(c => string.Equals(c.Domain, YoutubeDomain, StringComparison.OrdinalIgnoreCase))
            // Discard less specific cookies
            .DistinctBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(c => c.Name, c => c.Value, StringComparer.OrdinalIgnoreCase);

        var sessionId =
            cookies.GetValueOrDefault("__Secure-3PAPISID") ??
            cookies.GetValueOrDefault("SAPISID");

        if (sessionId is not null)
        {
            // If only __Secure-3PAPISID is present, add SAPISID manually
            if (!cookies.ContainsKey("SAPISID"))
                _cookieContainer.Add(YoutubeDomainUri, new Cookie("SAPISID", sessionId));

            request.Headers.Remove("Authorization");
            request.Headers.Add("Authorization", $"SAPISIDHASH {GenerateAuthHash(sessionId)}");

            request.Headers.Remove("Origin");
            request.Headers.Add("Origin", YoutubeDomain);

            request.Headers.Remove("X-Origin");
            request.Headers.Add("X-Origin", YoutubeDomain);

            // Set to 0 as it is only allowed to be logged in with one account
            request.Headers.Add("X-Goog-AuthUser", "0");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

public partial class YoutubeAuthHttpHandler
{
    private const string YoutubeDomain = "https://www.youtube.com";
    private static readonly Uri YoutubeDomainUri = new(YoutubeDomain);

    private static string GenerateAuthHash(string sessionId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000;

        var token = $"{timestamp} {sessionId} {YoutubeDomain}";
        var tokenHash = SHA1.HashData(Encoding.UTF8.GetBytes(token)).ToHex();

        return timestamp + "_" + tokenHash;
    }
}