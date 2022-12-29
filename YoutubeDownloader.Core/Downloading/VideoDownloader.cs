using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Common;
using System.Net;
using System.Linq;
using YoutubeDownloader.Core.Resolving;

namespace YoutubeDownloader.Core.Downloading;

public class VideoDownloader
{
    private readonly YoutubeClient _youtube = new(Http.Client);

    public async Task<IReadOnlyList<VideoDownloadOption>> GetDownloadOptionsAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
        return VideoDownloadOption.ResolveAll(manifest);
    }

    public async Task<VideoDownloadOption> GetBestDownloadOptionAsync(
        VideoId videoId,
        VideoDownloadPreference preference,
        CancellationToken cancellationToken = default)
    {
        var options = await GetDownloadOptionsAsync(videoId, cancellationToken);

        return
            preference.TryGetBestOption(options) ??
            throw new InvalidOperationException("No suitable download option found.");
    }

    public async Task DownloadVideoAsync(
        string filePath,
        IVideo video,
        VideoDownloadOption downloadOption,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default)
    {
        int? quality = downloadOption.VideoQuality?.MaxHeight;
        /*
        Console.WriteLine("---------------------");
        Console.WriteLine("["+ quality + "]" + video.Title );
        string filePathWithQuality="";
        string[] tokens = filePath.Split("]-");
        if (tokens.Length == 2){
            filePathWithQuality += tokens[0] +"]-["+ quality+"]- " + tokens[1];
        }else{
            filePathWithQuality = filePath;
        }
        Console.WriteLine(filePathWithQuality);
        */
        File.Delete(filePath);
        // If the target container supports subtitles, embed them in the video too
        var trackInfos = !downloadOption.Container.IsAudioOnly
            ? (await _youtube.Videos.ClosedCaptions.GetManifestAsync(video.Id, cancellationToken)).Tracks
            : Array.Empty<ClosedCaptionTrackInfo>();

        var dirPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dirPath))
            Directory.CreateDirectory(dirPath);

        String[] qualityThumbnails = new String[] {"maxresdefault.jpg", "sddefault.jpg", "hqdefault.jpg", "mqdefault.jpg", "default.jpg"}; 
        using (WebClient webClient = new WebClient()){
            int i = 0;
            String bestQualityThumbnail = qualityThumbnails[i];

            String thumbnailURL;
            while(i < qualityThumbnails.Length){
                bool downloadSuccess = true;
                bestQualityThumbnail = qualityThumbnails[i];
                thumbnailURL ="https://img.youtube.com/vi/"+ video.Id + "/" + bestQualityThumbnail;
                //Console.WriteLine(thumbnailURL);
                byte[] dataArr = new byte[1];

                try {
                    dataArr = webClient.DownloadData(thumbnailURL);
                }
                catch (WebException ex){
                // (HttpWebResponse)ex.Response).StatusCode
                    //Console.WriteLine("DownloadImage", ex.Message + " " + ex.InnerException + "URL: " + thumbnailURL + "Response: " + ((HttpWebResponse)ex.Response).StatusCode.ToString(), "Image");
                    downloadSuccess =false;
                    i++;
                    Console.WriteLine(ex.Message);
                }finally {
                    //Console.WriteLine("DONE");
                }

                if(downloadSuccess){
                    //save file to local
                    String thumbnailPath = System.IO.Path.GetFileNameWithoutExtension(filePath)+".jpg";
                    File.WriteAllBytes(dirPath + "/" + thumbnailPath, dataArr);
                    //Console.WriteLine(thumbnailURL);
                    break;
                }
            }
        }

        await _youtube.Videos.DownloadAsync(
            downloadOption.StreamInfos,
            trackInfos,
            new ConversionRequestBuilder(filePath)
                .SetContainer(downloadOption.Container)
                .SetPreset(ConversionPreset.Medium)
                .Build(),
            progress?.ToDoubleBased(),
            cancellationToken
        );
    }
}