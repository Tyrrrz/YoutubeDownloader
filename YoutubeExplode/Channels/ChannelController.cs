using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Channels;

internal class ChannelController(HttpClient http)
{
    private async ValueTask<ChannelPage> GetChannelPageAsync(
        string channelRoute,
        CancellationToken cancellationToken = default
    )
    {
        for (var retriesRemaining = 5; ; retriesRemaining--)
        {
            var channelPage = ChannelPage.TryParse(
                await http.GetStringAsync(
                    "https://www.youtube.com/" + channelRoute,
                    cancellationToken
                )
            );

            if (channelPage is null)
            {
                if (retriesRemaining > 0)
                    continue;

                throw new YoutubeExplodeException(
                    "Channel page is broken. Please try again in a few minutes."
                );
            }

            return channelPage;
        }
    }

    public async ValueTask<ChannelPage> GetChannelPageAsync(
        ChannelId channelId,
        CancellationToken cancellationToken = default
    ) => await GetChannelPageAsync("channel/" + channelId, cancellationToken);

    public async ValueTask<ChannelPage> GetChannelPageAsync(
        UserName userName,
        CancellationToken cancellationToken = default
    ) => await GetChannelPageAsync("user/" + userName, cancellationToken);

    public async ValueTask<ChannelPage> GetChannelPageAsync(
        ChannelSlug channelSlug,
        CancellationToken cancellationToken = default
    ) => await GetChannelPageAsync("c/" + channelSlug, cancellationToken);

    public async ValueTask<ChannelPage> GetChannelPageAsync(
        ChannelHandle channelHandle,
        CancellationToken cancellationToken = default
    ) => await GetChannelPageAsync("@" + channelHandle, cancellationToken);
}
