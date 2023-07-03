using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils.Extensions;

namespace YoutubeDownloader.Core.Utils;

public class AuthHandler : DelegatingHandler
{
    public AuthHandler() => InnerHandler = new HttpClientHandler();
    
    public Dictionary<string,string> Cookies { set; get; } = new();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) 
    {
        var papisid = Cookies.TryGetValue("SAPISID") ?? Cookies.TryGetValue("__Secure-3PAPISID");
        
        if (papisid is null)
            return base.SendAsync(request, cancellationToken);
        
        const string origin = "https://www.youtube.com";
        
        request.Headers.Remove("Cookie");
        request.Headers.Remove("Authorization");
        request.Headers.Remove("Origin");
        request.Headers.Remove("X-Origin");
        request.Headers.Remove("Referer");
        
        request.Headers.Add("Cookie", Cookies.Select(i => $"{i.Key}={i.Value}").Join("; "));
        request.Headers.Add("Authorization", $"SAPISIDHASH {GenerateSidBasedAuth(papisid, origin)}");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("X-Origin", origin);
        request.Headers.Add("Referer", origin);
        
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