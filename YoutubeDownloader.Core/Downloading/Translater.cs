using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Downloading
{
    public class Translater
    {
        private readonly YoutubeClient _youtube = new(Http.Client);
        private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com";

        // location, also known as region.
        // required if you're using a multi-service or regional (not global) resource. It can be found in the Azure portal on the Keys and Endpoint page.
        private static readonly string location = "eastasia";

        public async Task TranslateAsync(IVideo video, string path, string key, CancellationToken cancellationToken = default)
        {
            string tempPath = Path.ChangeExtension(path, "txt");
            // Input and output languages are defined as parameters.
            string route = "/translate?api-version=3.0&from=en&to=zh";
            var description = (await _youtube.Videos.GetAsync(video.Id, cancellationToken)).Description;
            object[] body = new object[] { new { Text = video.Title }, new { Text = description } };
            var requestBody = JsonSerializer.Serialize(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                // location required if you're using a multi-service or regional (not global) resource.
                request.Headers.Add("Ocp-Apim-Subscription-Region", location);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                // Read response as a string.
                string json = await response.Content.ReadAsStringAsync();
                JsonDocument jsonDocument = JsonDocument.Parse(json);
                var texts = new List<string>();
                foreach (var element in jsonDocument.RootElement.EnumerateArray())
                {
                    var text = element.GetProperty("translations")[0].GetProperty("text").ToString();
                    texts.AddRange(text.Split('\n'));
                }
                await File.WriteAllLinesAsync(tempPath, texts, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
