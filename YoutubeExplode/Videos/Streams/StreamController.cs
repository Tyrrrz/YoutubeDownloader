using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Videos.Streams;

internal class StreamController(HttpClient http) : VideoController(http)
{
    public async ValueTask<PlayerSource> GetPlayerSourceAsync(
        CancellationToken cancellationToken = default
    )
    {
        var iframe = await Http.GetStringAsync(
            "https://www.youtube.com/iframe_api",
            cancellationToken
        );

        var version = Regex.Match(iframe, @"player\\?/([0-9a-fA-F]{8})\\?/").Groups[1].Value;
        if (string.IsNullOrWhiteSpace(version))
            throw new YoutubeExplodeException("Failed to extract the player version.");

        return PlayerSource.Parse(
            await Http.GetStringAsync(
                $"https://www.youtube.com/s/player/{version}/player_ias.vflset/en_US/base.js",
                cancellationToken
            )
        );
    }

    public async ValueTask<DashManifest> GetDashManifestAsync(
        string url,
        CancellationToken cancellationToken = default
    ) => DashManifest.Parse(await Http.GetStringAsync(url, cancellationToken));
}
