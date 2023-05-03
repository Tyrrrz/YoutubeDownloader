using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeDownloader.Core.Downloading
{
    public class ClosedCaptionsDownloader
    {
        private readonly YoutubeClient _youtube = new(Http.Client);
        public async Task<Tuple<bool, string>> DownloadCCAsync(string path, IVideo video, CancellationToken cancellationToken = default)
        {
            var tempPath = Path.ChangeExtension(path, "srt");
            var manifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(video.Id, cancellationToken);
            bool isChineseCC = false;
            if (manifest.Tracks.Count != 0)
            {
                ClosedCaptionTrackInfo trackInfo;
                try
                {
                    trackInfo = manifest.GetByLanguage("zh");
                    isChineseCC = true;
                }
                catch (InvalidOperationException)
                {
                    trackInfo = manifest.GetByLanguage("en");
                }
                await _youtube.Videos.ClosedCaptions.DownloadAsync(trackInfo, tempPath, cancellationToken: cancellationToken);
                return Tuple.Create(isChineseCC, tempPath);
            }
            return Tuple.Create(isChineseCC, "");
        }
    }
}
