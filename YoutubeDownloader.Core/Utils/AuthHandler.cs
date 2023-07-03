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

public class AuthHandler : DelegatingHandler
{
    private readonly CookieContainer _cookieContainer = new();

    public AuthHandler() => InnerHandler = new HttpClientHandler()
    {
        CookieContainer = _cookieContainer
    };
    
    public Dictionary<string,string> Cookies
    {
        set => _cookieContainer.SetCookies(new Uri("https://www.youtube.com"), value.Select(i => $"{i.Key}={i.Value}").Join(","));
        get => _cookieContainer.GetCookies(new Uri("https://www.youtube.com")).DistinctBy(i => i.Name).ToDictionary(i => i.Name, i => i.Value);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) 
    {
        const string origin = "https://www.youtube.com";
        var papisid = Cookies.TryGetValue("SAPISID") ?? Cookies.TryGetValue("__Secure-3PAPISID");
        
        if (papisid is null)
            return await base.SendAsync(request, cancellationToken);

        request.Headers.Remove("Cookie");
        request.Headers.Remove("Authorization");
        request.Headers.Remove("Origin");
        request.Headers.Remove("X-Origin");
        request.Headers.Remove("Referer");
        request.Headers.Remove("X-Goog-AuthUser");
        
        request.Headers.Add("Authorization", $"SAPISIDHASH {GenerateSidBasedAuth(papisid, origin)}");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("X-Origin", origin);
        request.Headers.Add("Referer", origin);
        
        var response = await base.SendAsync(request, cancellationToken);
        return response;
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