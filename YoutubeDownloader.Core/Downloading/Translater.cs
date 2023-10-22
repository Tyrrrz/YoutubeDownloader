using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Core.Utils.Extensions;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Downloading
{
    public class Translater
    {
        private readonly YoutubeClient _youtube = new(Http.Client);
        private const string AzureEndpoint = "https://api.cognitive.microsofttranslator.com";
        private const string AzureRoute = "/translate?api-version=3.0&to=zh";
        private static readonly string AzureLocation = "eastasia";
        private const string BaiduEndpoint = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
        private static readonly Random random = new Random();

        public async Task AzureTranslateAsync(
            IVideo video,
            string path,
            string key,
            CancellationToken cancellationToken = default
        )
        {
            string tempPath = Path.ChangeExtension(path, "txt");
            // Input and output languages are defined as parameters.
            var description = (
                await _youtube.Videos.GetAsync(video.Id, cancellationToken)
            ).Description;
            object[] body = new object[] { new { Text = video.Title }, new { Text = description } };
            var requestBody = JsonSerializer.Serialize(body);

            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(AzureEndpoint + AzureRoute);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                // location required if you're using a multi-service or regional (not global) resource.
                request.Headers.Add("Ocp-Apim-Subscription-Region", AzureLocation);

                // Send the request and get response.
                HttpResponseMessage response = await Http.Client
                    .SendAsync(request, cancellationToken)
                    .ConfigureAwait(false);
                // Read response as a string.
                string json = await response.Content.ReadAsStringAsync();
                JsonDocument jsonDocument = JsonDocument.Parse(json);
                var texts = new List<string>();
                foreach (var element in jsonDocument.RootElement.EnumerateArray())
                {
                    var text = element.GetProperty("translations")[0]
                        .GetProperty("text")
                        .ToString();
                    texts.AddRange(text.Split('\n'));
                }
                texts.Add(video.Url);
                await File.WriteAllLinesAsync(tempPath, texts, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task AzureTranslateSrtAsync(string path, string key)
        {
            var textArr = await File.ReadAllLinesAsync(path);
            var texts = textArr.Where(t => t.Length > 0 && char.IsLetter(t[0])).Distinct().ToList();

            var body = texts.Select(text => new { Text = text }).ToList();

            var requestBody = JsonSerializer.Serialize(body);

            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(AzureEndpoint + AzureRoute);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                request.Headers.Add("Ocp-Apim-Subscription-Region", AzureLocation);

                HttpResponseMessage response = await Http.Client
                    .SendAsync(request)
                    .ConfigureAwait(false);
                string json = await response.Content.ReadAsStringAsync();
                JsonDocument jsonDocument = JsonDocument.Parse(json);
                var result = new List<string>();
                foreach (var element in jsonDocument.RootElement.EnumerateArray())
                {
                    result.Add(
                        element.GetProperty("translations")[0].GetProperty("text").ToString()
                    );
                }
                var dict = new Dictionary<string, string>();
                for (int i = 0; i < texts.Count; i++)
                {
                    dict.Add(texts[i], result[i]);
                }
                for (int i = 0; i < textArr.Length; i++)
                {
                    var t = textArr[i];
                    if (t.Length > 0 && char.IsLetter(t[0]))
                    {
                        textArr[i] = $"{t}\n{dict[t]}";
                    }
                }
                string fileName = Path.GetFileNameWithoutExtension(path);
                var dest = $"{Path.GetDirectoryName(path)}\\{fileName}_chinese.srt";
                await File.WriteAllLinesAsync(dest, textArr);
            }
        }

        public async Task BaiduTranslateContentAsync(
            IVideo video,
            string path,
            string key,
            string appId,
            CancellationToken cancellationToken = default
        )
        {
            string tempPath = Path.ChangeExtension(path, "txt");
            var description = (
                await _youtube.Videos.GetAsync(video.Id, cancellationToken)
            ).Description;
            var content = $"{video.Title}\n{description}";

            var dict = await BaiduTranslateAsync(content, key, appId, cancellationToken);
            dict.Add("url", video.Url);
            await File.WriteAllLinesAsync(tempPath, dict.Values, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<Dictionary<string, string>> BaiduTranslateAsync(
            string content,
            string key,
            string appId,
            CancellationToken cancellationToken = default
        )
        {
            var salt = random.Next(10000).ToString();
            var sign = EncryptString(appId + content + salt + key);
            var queryUrl =
                $"{BaiduEndpoint}q={HttpUtility.UrlEncode(content)}&from=auto&to=zh&appid={appId}&salt={salt}&sign={sign}";
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(queryUrl);
                request.Headers.Add("ContentType", "text/html;charset=UTF-8");
                var response = await Http.Client.SendAsync(request);
                string json = Regex.Unescape(await response.Content.ReadAsStringAsync());
                if (json.Contains("error_code"))
                {
                    throw new Exception(json);
                }
                JsonDocument jsonDocument = JsonDocument.Parse(json);
                var transResult = jsonDocument.RootElement.GetProperty("trans_result");
                var dict = new Dictionary<string, string>();
                foreach (var item in transResult.EnumerateArray())
                {
                    dict.Add(
                        item.GetProperty("src").GetString()!,
                        item.GetProperty("dst").GetString()!
                    );
                }
                return dict;
            }
        }

        public async Task BaiduTranslateSrtAsync(string path, string key, string appId)
        {
            var textArr = await File.ReadAllLinesAsync(path);
            var texts = textArr.Where(t => t.Length > 0 && char.IsLetter(t[0])).Distinct().ToList();

            var sb = new StringBuilder();
            var dict = new Dictionary<string, string>();
            foreach (var text in texts)
            {
                if (sb.Length + text.Length >= 6000)
                {
                    dict.AddRange(await BaiduTranslateAsync(sb.ToString(), key, appId));
                    sb.Clear();
                }
                sb.AppendLine(text + "\n");
            }
            dict.AddRange(await BaiduTranslateAsync(sb.ToString(), key, appId));
            for (int i = 0; i < textArr.Length; i++)
            {
                var t = textArr[i];
                if (t.Length > 0 && char.IsLetter(t[0]))
                {
                    textArr[i] = $"{t}\n{dict[t]}";
                }
            }
            string fileName = Path.GetFileNameWithoutExtension(path);
            var dest = $"{Path.GetDirectoryName(path)}\\{fileName}_chinese.srt";
            await File.WriteAllLinesAsync(dest, textArr);
        }

        private static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
    }
}
