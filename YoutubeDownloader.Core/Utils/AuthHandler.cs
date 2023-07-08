using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core.Utils;

public class AuthHandler : DelegatingHandler
{
    private const string Origin = "https://www.youtube.com";
    private readonly Uri _baseUri = new(Origin);
    private readonly HttpClientHandler _innerHandler = new()
    {
        UseCookies = true,
        CookieContainer = new CookieContainer()
    };

    public AuthHandler() => InnerHandler = _innerHandler;
    
    public string? PageId { get; set; }

    public void SetCookies(string cookies)
    {
        foreach (Cookie cookie in _innerHandler.CookieContainer.GetCookies(_baseUri))
             cookie.Expired = true;

        if (!string.IsNullOrWhiteSpace(cookies))
            _innerHandler.CookieContainer.SetCookies(_baseUri, cookies);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sapisid = _innerHandler.CookieContainer.GetCookies(_baseUri)["__Secure-3PAPISID"] ?? _innerHandler.CookieContainer.GetCookies(_baseUri)["SAPISID"];
        
        if (sapisid is null)
            return base.SendAsync(request, cancellationToken);
        
        if(_innerHandler.CookieContainer.GetCookies(_baseUri)["SAPISID"] is null)
            _innerHandler.CookieContainer.Add(_baseUri, new Cookie("SAPISID", sapisid.Value));

        request.Headers.Remove("Authorization");
        request.Headers.Remove("Origin");
        request.Headers.Remove("X-Origin");

        request.Headers.Add("Authorization", $"SAPISIDHASH {GenerateSidBasedAuth(sapisid.Value, Origin)}");
        request.Headers.Add("Origin", Origin);
        request.Headers.Add("X-Origin", Origin);
        //Set to 0 as it is only allowed to be logged in with one account
        request.Headers.Add("X-Goog-AuthUser", "0");

        //Needed if there are brand accounts (Secondary channels)
        if (PageId is not null)
            request.Headers.Add("X-Goog-PageId", PageId);

        return base.SendAsync(request, cancellationToken);
    }

    private static string GenerateSidBasedAuth(string sid, string origin)
    {
        var date = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var timestamp = date / 1000;
        var sidHash = Hash($"{timestamp} {sid} {origin}");
        return $"{timestamp}_{sidHash}";
    }

    private static string Hash(string input)
    {
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        return string.Concat(hash.Select(b => b.ToString("x2")));
    }
}