using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using YoutubeDownloader.Internal;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs
{
    public class DownloadMultipleSetupViewModel : DialogScreen<IReadOnlyList<DownloadViewModel>>
    {
        private static readonly Dictionary<string, DownloadQuality> AvailableQualities = new Dictionary<string, DownloadQuality>()
        {
            { "Maximum", DownloadQuality.Maximum },
            { "High (up to 1080p)", DownloadQuality.High },
            { "Medium (up to 720p)", DownloadQuality.Medium },
            { "Low (up to 480p)", DownloadQuality.Low },
            { "Minimum", DownloadQuality.Minimum }
        };

        private readonly IViewModelFactory _viewModelFactory;
        private readonly SettingsService _settingsService;
        private readonly DialogManager _dialogManager;

        public string Title { get; set; }

        public IReadOnlyList<Video> AvailableVideos { get; set; }

        public IReadOnlyList<Video> SelectedVideos { get; set; }

        public IReadOnlyList<string> AvailableFormats { get; private set; }

        public string SelectedFormat { get; set; }

        public DownloadMultipleSetupViewModel(IViewModelFactory viewModelFactory, SettingsService settingsService,
            DialogManager dialogManager)
        {
            _viewModelFactory = viewModelFactory;
            _settingsService = settingsService;
            _dialogManager = dialogManager;
        }

        public void OnViewLoaded()
        {
            AvailableFormats = GetAvailableFormats();

            // Select last used format
            SelectedFormat = !string.IsNullOrWhiteSpace(_settingsService.LastFormat) && AvailableFormats.Contains(_settingsService.LastFormat)
                ? _settingsService.LastFormat
                : AvailableFormats.FirstOrDefault();
        }

        public bool CanConfirm => SelectedVideos != null && SelectedVideos.Any();

        public void Confirm()
        {
            // Prompt user for output directory path
            var dirPath = _dialogManager.PromptDirectoryPath();

            // If canceled - return
            if (string.IsNullOrWhiteSpace(dirPath))
                return;

            // Save last used format
            _settingsService.LastFormat = SelectedFormat;

            // Split quality and format
            var (quality, format) = ParseToQualityAndFormat(SelectedFormat);

            // Make sure selected videos are ordered in the same way as available videos
            var orderedSelectedVideos = AvailableVideos.Where(v => SelectedVideos.Contains(v)).ToArray();

            // Create download view models
            var downloads = new List<DownloadViewModel>();
            for (var i = 0; i < orderedSelectedVideos.Length; i++)
            {
                var video = orderedSelectedVideos[i];

                // Generate file path
                var number = (i + 1).ToString().PadLeft(orderedSelectedVideos.Length.ToString().Length, '0');
                var fileName = FileNameGenerator.GenerateFileName(_settingsService.FileNameTemplate, video, format, number);
                var filePath = Path.Combine(dirPath, fileName);

                // If file exists - either skip it or generate a unique file path, depending on user settings
                if (File.Exists(filePath))
                {
                    if (_settingsService.ShouldSkipExistingFiles)
                        continue;

                    filePath = PathEx.MakeUniqueFilePath(filePath);
                }

                // Create empty file to "lock in" the file path
                PathEx.CreateDirectoryForFile(filePath);
                PathEx.CreateEmptyFile(filePath);

                // Create download view model
                var download = _viewModelFactory.CreateDownloadViewModel(video, filePath, format, quality);

                // Add to list
                downloads.Add(download);
            }

            // Close dialog
            Close(downloads);
        }

        private List<string> GetAvailableFormats()
        {
            var formats = new List<string>();

            if (!_settingsService.ExcludedContainerFormats.Contains("mp4"))
                formats.AddRange(AvailableQualities.Select(q => $@"{q.Key} / mp4"));

            if (!_settingsService.ExcludedContainerFormats.Contains("mp3"))
                formats.Add("mp3");

            if (!_settingsService.ExcludedContainerFormats.Contains("ogg"))
                formats.Add("ogg");

            return formats;
        }

        private (DownloadQuality, string) ParseToQualityAndFormat(string selectedFormat)
        {
            var parts = selectedFormat.Split("/");
            var quality = parts.Length > 1 ? AvailableQualities[parts[0].Trim()] : DownloadQuality.Maximum;
            var format = parts.Length > 1 ? parts[1].Trim() : parts[0].Trim();
            return (quality, format);
        }

        public void CopyTitle() => Clipboard.SetText(Title);
    }
}