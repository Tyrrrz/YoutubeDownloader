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

namespace YoutubeDownloader.Core.Utils;

public partial class AuthHandler : DelegatingHandler
{
    private const string Origin = "https://www.youtube.com";
    private readonly Uri _baseUri = new(Origin);

    private readonly HttpClientHandler _innerHandler = new()
    {
        UseCookies = true,
        CookieContainer = new CookieContainer()
    };

    public AuthHandler() => InnerHandler = _innerHandler;

    public void SetCookies(string cookies)
    {
        foreach (Cookie cookie in _innerHandler.CookieContainer.GetCookies(_baseUri))
            cookie.Expired = true;

        if (!string.IsNullOrWhiteSpace(cookies))
            _innerHandler.CookieContainer.SetCookies(_baseUri, cookies);
    }

    public void SetCookies(IEnumerable<KeyValuePair<string, string>> cookies) => SetCookies(
        string.Join(',', cookies.Select(i => $"{i.Key}={i.Value}"))
    );

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await base.SendAsync(request, cancellationToken);

        var sessionId =
            _innerHandler.CookieContainer.GetCookies(_baseUri)["__Secure-3PAPISID"] ??
            _innerHandler.CookieContainer.GetCookies(_baseUri)["SAPISID"];

        if (sessionId is null)
            return await base.SendAsync(request, cancellationToken);

        if (_innerHandler.CookieContainer.GetCookies(_baseUri)["SAPISID"] is null)
            _innerHandler.CookieContainer.Add(_baseUri, new Cookie("SAPISID", sessionId.Value));

        request.Headers.Remove("Authorization");
        request.Headers.Remove("Origin");
        request.Headers.Remove("X-Origin");

        request.Headers.Add("Authorization", $"SAPISIDHASH {GenerateAuthHash(sessionId.Value)}");
        request.Headers.Add("Origin", Origin);
        request.Headers.Add("X-Origin", Origin);

        // Set to 0 as it is only allowed to be logged in with one account
        request.Headers.Add("X-Goog-AuthUser", "0");

        return await base.SendAsync(request, cancellationToken);
    }
}

public partial class AuthHandler
{
    private static string GenerateAuthHash(string sessionId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000;

        var token = $"{timestamp} {sessionId} {Origin}";
        var tokenHash = SHA1.HashData(Encoding.UTF8.GetBytes(token)).ToHex();

        return timestamp + '_' + tokenHash;
    }
}