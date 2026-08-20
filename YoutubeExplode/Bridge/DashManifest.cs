using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Lazy;
using PowerKit.Extensions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Bridge;

internal partial class DashManifest(XElement content)
{
    [Lazy]
    public IReadOnlyList<IStreamData> Streams =>
        content
            .Descendants("Representation")
            // Skip non-media representations (like "rawcc")
            // https://github.com/Tyrrrz/YoutubeExplode/issues/546
            .Where(x => x.Attribute("id")?.Value.All(char.IsDigit) == true)
            // Skip segmented streams
            // https://github.com/Tyrrrz/YoutubeExplode/issues/159
            .Where(x =>
                x.Descendants("Initialization")
                    .FirstOrDefault()
                    ?.Attribute("sourceURL")
                    ?.Value.Contains("sq/") != true
            )
            // Skip streams without codecs
            .Where(x => !string.IsNullOrWhiteSpace(x.Attribute("codecs")?.Value))
            .Select(x => new StreamData(x))
            .ToArray();
}

internal partial class DashManifest
{
    public class StreamData(XElement content) : IStreamData
    {
        [Lazy]
        public int? Itag => (int?)content.Attribute("id");

        [Lazy]
        public string? Url => (string?)content.Element("BaseURL");

        // DASH streams don't have signatures
        public string? Signature => null;

        // DASH streams don't have signatures
        public string? SignatureParameter => null;

        [Lazy]
        public long? ContentLength =>
            (long?)content.Attribute("contentLength")
            ?? Url?.Pipe(s => Regex.Match(s, @"[/\?]clen[/=](\d+)").Groups[1].Value)
                .NullIfWhiteSpace()
                ?.Pipe(s => long.ParseOrNull(s, CultureInfo.InvariantCulture));

        [Lazy]
        public long? Bitrate => (long?)content.Attribute("bandwidth");

        [Lazy]
        public string? Container =>
            Url
                ?.Pipe(s => Regex.Match(s, @"mime[/=]\w*%2F([\w\d]*)").Groups[1].Value)
                .Pipe(WebUtility.UrlDecode);

        [Lazy]
        private bool IsAudioOnly => content.Element("AudioChannelConfiguration") is not null;

        [Lazy]
        public string? AudioCodec => IsAudioOnly ? (string?)content.Attribute("codecs") : null;

        public string? AudioLanguageCode => null;

        public string? AudioLanguageName => null;

        public bool? IsAudioLanguageDefault => null;

        [Lazy]
        public string? VideoCodec => IsAudioOnly ? null : (string?)content.Attribute("codecs");

        public string? VideoQualityLabel => null;

        [Lazy]
        public int? VideoWidth => (int?)content.Attribute("width");

        [Lazy]
        public int? VideoHeight => (int?)content.Attribute("height");

        public bool IsVideoUpscaled => false;

        [Lazy]
        public int? VideoFramerate => (int?)content.Attribute("frameRate");
    }
}

internal partial class DashManifest
{
    public static DashManifest Parse(string raw) => new(Xml.Parse(raw));
}
