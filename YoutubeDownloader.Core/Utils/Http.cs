using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core.Utils;

public static class Http
{
    public static AuthHandler AuthHandler { get; } = new();
    public static HttpClient Client { get; } = new(AuthHandler, true)
    {
        DefaultRequestHeaders =
        {
            // Required by some of the services we're using
            UserAgent =
            {
                new ProductInfoHeaderValue(
                    "YoutubeDownloader",
                    Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
                )
            }
        }
    };
}

public class AuthHandler : DelegatingHandler
{
    public AuthHandler() => InnerHandler = new HttpClientHandler();
    
    public string? Papisid { get; set; }
    public string? Psid { get; set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) 
    {
        if(Papisid is null || Psid is null)
            return base.SendAsync(request, cancellationToken);
        
        const string origin = "https://www.youtube.com";
        
        request.Headers.Remove("Cookie");
        request.Headers.Remove("Authorization");
        request.Headers.Remove("Origin");
        request.Headers.Remove("X-Origin");
        request.Headers.Remove("Referer");
        
        request.Headers.Add("Cookie", $"CONSENT=YES+cb; YSC=DwKYllHNwuw; __Secure-3PAPISID={Papisid}; __Secure-3PSID={Psid}");
        request.Headers.Add("Authorization", $"SAPISIDHASH {GenerateSidBasedAuth(Papisid, origin)}");
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