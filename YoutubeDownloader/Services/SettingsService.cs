using System;
using Tyrrrz.Settings;

namespace YoutubeDownloader.Services
{
    public class SettingsService : SettingsManager
    {
        public int MaxConcurrentDownloadCount { get; set; } = Environment.ProcessorCount;

        public SettingsService()
        {
            Configuration.StorageSpace = StorageSpace.Instance;
            Configuration.SubDirectoryPath = "";
            Configuration.FileName = "Settings.dat";
        }
    }
}