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
    private HttpClientHandler _innerHandler = new();

    public AuthHandler() => InnerHandler = _innerHandler;

    public void SetCookies(string cookies)
    {

        //Invalidate all cookies without creating a new one 
         foreach (Cookie cookie in _innerHandler.CookieContainer.GetCookies(_baseUri))
             cookie.Expired = true;

         if (string.IsNullOrWhiteSpace(cookies))
             return;
         
         _innerHandler.CookieContainer.SetCookies(_baseUri, cookies);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var papisid = _innerHandler.CookieContainer.GetCookies(_baseUri)["SAPISID"] ?? _innerHandler.CookieContainer.GetCookies(_baseUri)["__Secure-3PAPISID"];

        if (papisid is null)
            return base.SendAsync(request, cancellationToken);

        request.Headers.Remove("Cookie");
        request.Headers.Remove("Authorization");
        request.Headers.Remove("Origin");
        request.Headers.Remove("X-Origin");
        request.Headers.Remove("Referer");

        request.Headers.Add("Authorization", $"SAPISIDHASH {GenerateSidBasedAuth(papisid.Value, Origin)}");
        request.Headers.Add("Origin", Origin);
        request.Headers.Add("X-Origin", Origin);
        request.Headers.Add("Referer", Origin);

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