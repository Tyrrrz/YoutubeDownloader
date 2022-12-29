using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace YoutubeDownloader.Utils
{
    public class Database
    {
        //private static Mutex mut = new Mutex();
        private static Dictionary<string, string>  MostViewedVideo { get; set; } = new Dictionary<string, string>();
        private static string? DirPath;
        private static bool changed = false;

        //Insert statement
        public static bool InsertOrUpdate(VideoInfo videoInfo)
        {
            bool result = false;
            try
            {
                //mut.WaitOne();
                if(videoInfo != null)
                {
                    // insert
                    if(!MostViewedVideo.ContainsKey(videoInfo!.Id)){
                        videoInfo.Number = MostViewedVideo.Count + 1;
                        MostViewedVideo.Add(videoInfo!.Id, videoInfo.ToString());
                        changed = true;
                    }
                    else{ // update
                        VideoInfo storedVideoInfo = VideoInfoParser.Parse(MostViewedVideo[videoInfo!.Id]);
                        if(! storedVideoInfo.DownloadStatus.Equals(videoInfo.DownloadStatus)){
                            MostViewedVideo[videoInfo!.Id] = videoInfo.ToString();
                            changed = true;
                        }else{
                            changed = false;
                        }
                    }
                }
                result = true;
            }
            catch(System.Exception){
            }finally{
                //mut.ReleaseMutex();
            }
            return result;
        }

        public static int Count(){
            return MostViewedVideo.Count;
        }
        public static bool Save()
        {
            bool result = true;
            //mut.WaitOne();
            if(changed){
                try
                {
                    string videoTitleList = "";
                    foreach (var item in MostViewedVideo) {
                        videoTitleList += item.Value + "\n";
                    }
                    using StreamWriter file = new(DirPath + "/" + YoutubeDownloader.Utils.AppConsts.DatabaseFileName, append: false);
                    file.WriteLine(videoTitleList);
                }catch(System.Exception){
                    result = false;
                }finally{
                    //mut.ReleaseMutex();
                }
                changed = false;
            }
            return result;
        }
        //Select statement
        public static void Load(string dirPath)
        {
            DirPath = dirPath;
            //mut.WaitOne();
            try
            {
                if(MostViewedVideo.Count == 0){
                    if (!string.IsNullOrWhiteSpace(dirPath))
                        Directory.CreateDirectory(dirPath);
                    List<string> lines = File.ReadAllLines(DirPath + "/" + YoutubeDownloader.Utils.AppConsts.DatabaseFileName).Where(arg => !string.IsNullOrWhiteSpace(arg)).ToList();
                    for (int i = 0; i < lines.Count; i++)
                    {
                        VideoInfo videoInfo = VideoInfoParser.Parse(lines[i]);
                        MostViewedVideo.Add(videoInfo.Id, lines[i]);
                    }
                    MostViewedVideo = MostViewedVideo;
                }
            }
            catch (System.Exception)
            {
            }finally{
                //mut.ReleaseMutex();
            }
        }
        public static VideoInfo? Find(string? videoID){
            VideoInfo? videoInfo = null;
            //mut.WaitOne();
            try{
                string? value;
                if (videoID != null && MostViewedVideo.TryGetValue(videoID, out value)){
                    videoInfo = VideoInfoParser.Parse(value);
                }
            }catch(System.Exception){
            }finally{
               // mut.ReleaseMutex();
            }
            return videoInfo;
        }
    }
}
