using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Downloading;

public class Proxy
{
    internal static System.Net.WebProxy? CreateProxy(bool _useproxy, Uri _uri)
    {
        var uri = new UriBuilder(_uri);
        System.Net.WebProxy? proxy = null;
        if (_useproxy)
        {
            var scheme = uri.Scheme.ToLower();
            switch (scheme)
            {
                case "socks4":
                case "socks5":
                    proxy = new System.Net.WebProxy(uri.Host, uri.Port);
                    break;
                case "http":
                case "https":
                default:
                    proxy = new System.Net.WebProxy(uri.ToString());
                    break;
            }
            if (!string.IsNullOrEmpty(uri.UserName) && !string.IsNullOrEmpty(uri.Password))
            {
                proxy.Credentials = new NetworkCredential(uri.UserName, uri.Password);
            }
        }
        return proxy;
    }
    public static HttpClient Apply(bool useProxy, string proxyAddress)
    {
        System.Net.Http.HttpClientHandler handler;
        if (useProxy && Uri.TryCreate(proxyAddress, UriKind.RelativeOrAbsolute, out Uri? result))
        {
            handler = new()
            {
                Proxy = CreateProxy(true, result),
                UseProxy = true
            };
        }
        else
        {
            handler = new()
            {
                UseProxy = false
            };
        }

        Http.Client = new(handler)
        {
            DefaultRequestHeaders =
        {
            // Required by some of the services we're using
            UserAgent =
            {
                new ProductInfoHeaderValue(
                    "YoutubeDownloader",
                    typeof(Http).Assembly.GetName().Version?.ToString(3)
                )
            }
        }
        };
        return Http.Client;
    }
}