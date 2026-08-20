using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PowerKit.Extensions;
using YoutubeExplode.Bridge;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Playlists;

namespace YoutubeExplode.Channels;

/// <summary>
/// Operations related to YouTube channels.
/// </summary>
public class ChannelClient(HttpClient http)
{
    private readonly ChannelController _controller = new(http);

    private Channel Get(ChannelPage channelPage)
    {
        var channelId =
            channelPage.Id
            ?? throw new YoutubeExplodeException("Failed to extract the channel ID.");

        var title =
            channelPage.Title
            ?? throw new YoutubeExplodeException("Failed to extract the channel title.");

        var logoUrl =
            channelPage.LogoUrl
            ?? throw new YoutubeExplodeException("Failed to extract the channel logo URL.");

        var logoSize =
            Regex
                .Matches(logoUrl, @"\bs(\d+)\b")
                .ToArray()
                .LastOrDefault()
                ?.Groups[1]
                .Value.NullIfWhiteSpace()
                ?.Pipe(s => int.ParseOrNull(s, CultureInfo.InvariantCulture))
            ?? 100;

        var thumbnails = new[] { new Thumbnail(logoUrl, new Resolution(logoSize, logoSize)) };

        return new Channel(channelId, title, thumbnails);
    }

    /// <summary>
    /// Gets the metadata associated with the specified channel.
    /// </summary>
    public async ValueTask<Channel> GetAsync(
        ChannelId channelId,
        CancellationToken cancellationToken = default
    )
    {
        // Special case for the "Movies & TV" channel, which has a custom page
        if (channelId == "UCuVPpxrm2VAgpH3Ktln4HXg")
        {
            return new Channel(
                "UCuVPpxrm2VAgpH3Ktln4HXg",
                "Movies & TV",
                [
                    new Thumbnail(
                        "https://www.gstatic.com/youtube/img/tvfilm/clapperboard_profile.png",
                        new Resolution(1024, 1024)
                    ),
                ]
            );
        }

        return Get(await _controller.GetChannelPageAsync(channelId, cancellationToken));
    }

    /// <summary>
    /// Gets the metadata associated with the channel of the specified user.
    /// </summary>
    public async ValueTask<Channel> GetByUserAsync(
        UserName userName,
        CancellationToken cancellationToken = default
    ) => Get(await _controller.GetChannelPageAsync(userName, cancellationToken));

    /// <summary>
    /// Gets the metadata associated with the channel identified by the specified slug or legacy custom URL.
    /// </summary>
    public async ValueTask<Channel> GetBySlugAsync(
        ChannelSlug channelSlug,
        CancellationToken cancellationToken = default
    ) => Get(await _controller.GetChannelPageAsync(channelSlug, cancellationToken));

    /// <summary>
    /// Gets the metadata associated with the channel identified by the specified handle or custom URL.
    /// </summary>
    public async ValueTask<Channel> GetByHandleAsync(
        ChannelHandle channelHandle,
        CancellationToken cancellationToken = default
    ) => Get(await _controller.GetChannelPageAsync(channelHandle, cancellationToken));

    /// <summary>
    /// Enumerates videos uploaded by the specified channel.
    /// </summary>
    // TODO: should return <IVideo> sequence instead (breaking change)
    public IAsyncEnumerable<PlaylistVideo> GetUploadsAsync(
        ChannelId channelId,
        CancellationToken cancellationToken = default
    )
    {
        // Replace 'UC' in the channel ID with 'UU'
        var playlistId = "UU" + channelId.Value[2..];
        return new PlaylistClient(http).GetVideosAsync(playlistId, cancellationToken);
    }
}
