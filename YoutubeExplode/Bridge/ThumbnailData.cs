using System.Text.Json;
using JsonExtensions.Reading;
using Lazy;

namespace YoutubeExplode.Bridge;

internal class ThumbnailData(JsonElement content)
{
    [Lazy]
    public string? Url => content.GetPropertyOrNull("url")?.GetStringOrNull();

    [Lazy]
    public int? Width => content.GetPropertyOrNull("width")?.GetInt32OrNull();

    [Lazy]
    public int? Height => content.GetPropertyOrNull("height")?.GetInt32OrNull();
}
