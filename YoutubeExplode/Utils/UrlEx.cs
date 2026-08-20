using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using PowerKit.Extensions;

namespace YoutubeExplode.Utils;

internal static class UrlEx
{
    private static IEnumerable<KeyValuePair<string, string>> EnumerateQueryParameters(string url)
    {
        var query = url.Contains('?') ? url.SubstringAfter("?") : url;

        foreach (var parameter in query.Split('&'))
        {
            var key = WebUtility.UrlDecode(parameter.SubstringUntil("="));
            var value = WebUtility.UrlDecode(parameter.SubstringAfter("="));

            if (string.IsNullOrWhiteSpace(key))
                continue;

            yield return new KeyValuePair<string, string>(key, value);
        }
    }

    public static IReadOnlyDictionary<string, string> GetQueryParameters(string url) =>
        EnumerateQueryParameters(url).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    private static KeyValuePair<string, string>? TryGetQueryParameter(string url, string key)
    {
        foreach (var parameter in EnumerateQueryParameters(url))
        {
            if (string.Equals(parameter.Key, key, StringComparison.Ordinal))
                return parameter;
        }

        return null;
    }

    public static string? TryGetQueryParameterValue(string url, string key) =>
        TryGetQueryParameter(url, key)?.Value;

    public static bool ContainsQueryParameter(string url, string key) =>
        TryGetQueryParameterValue(url, key) is not null;

    public static string RemoveQueryParameter(string url, string key)
    {
        if (!ContainsQueryParameter(url, key))
            return url;

        var urlBuilder = new UriBuilder(url);
        var queryBuilder = new StringBuilder();

        foreach (var parameter in EnumerateQueryParameters(url))
        {
            if (string.Equals(parameter.Key, key, StringComparison.Ordinal))
                continue;

            queryBuilder.Append(queryBuilder.Length > 0 ? '&' : '?');

            queryBuilder.Append(Uri.EscapeDataString(parameter.Key));
            queryBuilder.Append('=');
            queryBuilder.Append(Uri.EscapeDataString(parameter.Value));
        }

        urlBuilder.Query = queryBuilder.ToString();

        return urlBuilder.ToString();
    }

    public static string SetQueryParameter(string url, string key, string value)
    {
        var urlWithoutParameter = RemoveQueryParameter(url, key);
        var hasOtherParameters = urlWithoutParameter.Contains('?');

        return urlWithoutParameter
            + (hasOtherParameters ? '&' : '?')
            + Uri.EscapeDataString(key)
            + '='
            + Uri.EscapeDataString(value);
    }
}
