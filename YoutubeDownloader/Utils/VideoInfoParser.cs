using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeDownloader.Utils
{
    public class VideoInfo
    {


        public VideoInfo()
        {
            this.Number = 0;
            this.Id = "";
            this.Title = "";
            this.DownloadStatus = "";
            this.ContentStatus ="";
        }

        public VideoInfo(int num, string title, string id, string downloadStatus, string contentStatus)
        {
            this.Number = num;
            this.Title = title;
            this.Id = id;
            this.DownloadStatus = downloadStatus;
            this.ContentStatus = contentStatus;
        }


        public int Number { get; set; }
        public string Title { get; set; }
        public string Id { get; set; }

        public string DownloadStatus { get; set; }
        public string ContentStatus { get; set; }
        
        public override string ToString()
        {
            return "[" + (Number).ToString().PadLeft(YoutubeDownloader.Utils.AppConsts.LenNumber, '0') + "]-[" + 
                Title + "]-[" + Id + "]-[" + DownloadStatus + "]-[" + ContentStatus + "]";
        }
    }

    internal static class VideoInfoParser
    {
        public static VideoInfo Parse(string line)
        {
            //line = "[2875]-[Celebrity Impressions - Melissa Villasenor - America's Got Talent Audition - Season 6]-[vuQoQMzfG48]-[Deleted]-[New]";
            string[] parts = line.Trim().Split("]-[");

            string[] parsedParts = { "", "", "", "", "", "", "", "", "", "", "" };

            for (int i = 0; i < parts.Length; i++)
            {
                string p = parts[i].Trim();
                if (p.Length > 0)
                {
                    if (p.Substring(0, 1).Equals("["))
                    {
                        p = p.Substring(1);
                    }
                    else if (p.Substring(p.Length - 1).Equals("]"))
                    {
                        p = p.Substring(0, p.Length - 1);
                    }
                }
                parsedParts[i] = p;
                //Console.WriteLine(p);
            }
            int number = 0;
            try
            {
                number = Int32.Parse(parsedParts[0]);
            }
            catch
            {
            }
            return new VideoInfo(number, parsedParts[1], parsedParts[2], parsedParts[3], parsedParts[4]);
        }
    }

}
