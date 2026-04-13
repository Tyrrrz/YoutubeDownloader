using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gress;

namespace YoutubeDownloader.Core.Utils.Extensions;

public static class HttpExtensions
{
    extension(HttpClient http)
    {
        public async Task DownloadAsync(
            string url,
            string filePath,
            IProgress<Percentage>? progress = null,
            CancellationToken cancellationToken = default
        )
        {
            using var response = await http.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            response.EnsureSuccessStatusCode();

            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var destination = File.Create(filePath);

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            await source.CopyToAsync(destination, totalBytes, progress, cancellationToken);
        }
    }
}
