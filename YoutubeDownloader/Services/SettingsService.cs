using System;
using Tyrrrz.Extensions;
using Tyrrrz.Settings;

namespace YoutubeDownloader.Services
{
    public class SettingsService : SettingsManager
    {
        public int MaxConcurrentDownloadCount { get; set; } = Environment.ProcessorCount.Clamp(1, 10);

        public string LastFormat { get; set; }

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = "";
            Configuration.FileName = "Settings.dat";
        }
    }
}