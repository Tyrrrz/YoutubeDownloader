using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Search;

internal class SearchController(HttpClient http)
{
    public async ValueTask<SearchResponse> GetSearchResponseAsync(
        string searchQuery,
        SearchFilter searchFilter,
        string? continuationToken,
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/search"
        );

        request.Content = new StringContent(
            // lang=json
            $$"""
            {
              "query": {{Json.Encode(searchQuery)}},
              "params": {{Json.Encode(searchFilter switch
              {
                SearchFilter.Video => "EgIQAQ%3D%3D",
                SearchFilter.Playlist => "EgIQAw%3D%3D",
                SearchFilter.Channel => "EgIQAg%3D%3D",
                _ => null
              })}},
              "continuation": {{Json.Encode(continuationToken)}},
              "context": {
                "client": {
                  "clientName": "WEB",
                  "clientVersion": "2.20210408.08.00",
                  "hl": "en",
                  "gl": "US",
                  "utcOffsetMinutes": 0
                }
              }
            }
            """
        );

        using var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return SearchResponse.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    }
}
