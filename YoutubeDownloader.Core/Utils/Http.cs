using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace YoutubeDownloader.Core.Utils;

public static class Http
{
    public static bool ShouldUseProxy { get; set; }
    public static string ProxyUrl { get; set; } = string.Empty;

    private static bool _lastShouldUseProxy;
    private static string? _lastProxyUrl;

    [field: AllowNull, MaybeNull]
    public static HttpClient Client
    {
        get
        {
            if (field is null || _lastShouldUseProxy != ShouldUseProxy || _lastProxyUrl != ProxyUrl)
            {
                field = new HttpClient(CreateHttpMessageHandler())
                {
                    DefaultRequestHeaders =
                    {
                        // Required by some of the services we're using
                        UserAgent =
                        {
                            new ProductInfoHeaderValue(
                                "YoutubeDownloader",
                                Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
                            ),
                        },
                    },
                };

                _lastShouldUseProxy = ShouldUseProxy;
                _lastProxyUrl = ProxyUrl;
            }

            return field;
        }
    }

    private static HttpMessageHandler CreateHttpMessageHandler()
    {
        var handler = new HttpClientHandler();

        if (ShouldUseProxy && Uri.TryCreate(ProxyUrl, UriKind.Absolute, out var proxyUri))
        {
            handler.Proxy = new WebProxy(proxyUri)
            {
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,
            };
            handler.UseProxy = true;
        }

        return handler;
    }
}
