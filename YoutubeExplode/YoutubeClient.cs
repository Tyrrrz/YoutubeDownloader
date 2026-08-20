using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using YoutubeExplode.Channels;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Utils;
using YoutubeExplode.Videos;

namespace YoutubeExplode;

/// <summary>
/// Client for interacting with YouTube.
/// </summary>
public class YoutubeClient : IDisposable
{
    private readonly HttpClient _youtubeHttp;

    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    public YoutubeClient(HttpClient http, IReadOnlyList<Cookie> initialCookies)
    {
        _youtubeHttp = new HttpClient(new YoutubeHttpHandler(http, initialCookies), true);

        Videos = new VideoClient(_youtubeHttp);
        Playlists = new PlaylistClient(_youtubeHttp);
        Channels = new ChannelClient(_youtubeHttp);
        Search = new SearchClient(_youtubeHttp);
    }

    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    public YoutubeClient(HttpClient http)
        : this(http, []) { }

    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    public YoutubeClient(IReadOnlyList<Cookie> initialCookies)
        : this(Http.Client, initialCookies) { }

    /// <summary>
    /// Initializes an instance of <see cref="YoutubeClient" />.
    /// </summary>
    public YoutubeClient()
        : this(Http.Client) { }

    /// <summary>
    /// Operations related to YouTube videos.
    /// </summary>
    public VideoClient Videos { get; }

    /// <summary>
    /// Operations related to YouTube playlists.
    /// </summary>
    public PlaylistClient Playlists { get; }

    /// <summary>
    /// Operations related to YouTube channels.
    /// </summary>
    public ChannelClient Channels { get; }

    /// <summary>
    /// Operations related to YouTube search.
    /// </summary>
    public SearchClient Search { get; }

    /// <inheritdoc />
    public void Dispose() => _youtubeHttp.Dispose();
}
