using System.Collections.Generic;
using YoutubeDownloader.Models;
using YoutubeDownloader.ViewModels.Components;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Framework
{
    public static class Extensions
    {
        public static DownloadViewModel CreateDownloadViewModel(this IViewModelFactory factory, Video video,
            string filePath, string format, DownloadOption? downloadOption = null, SubtitleOption? subtitleOption = null)
        {
            var viewModel = factory.CreateDownloadViewModel();
            viewModel.Video = video;
            viewModel.FilePath = filePath;
            viewModel.Format = format;
            viewModel.DownloadOption = downloadOption;
            viewModel.SubtitleOption = subtitleOption;

            return viewModel;
        }

        public static DownloadMultipleSetupViewModel CreateDownloadMultipleSetupViewModel(
            this IViewModelFactory factory, string title, IReadOnlyList<Video> availableVideos)
        {
            var viewModel = factory.CreateDownloadMultipleSetupViewModel();
            viewModel.Title = title;
            viewModel.AvailableVideos = availableVideos;

            return viewModel;
        }

        public static DownloadSingleSetupViewModel CreateDownloadSingleSetupViewModel(this IViewModelFactory factory,
            string title, Video video, IReadOnlyList<DownloadOption> availableDownloadOptions, IReadOnlyList<SubtitleOption> availableSubtitleOptions)
        {
            var viewModel = factory.CreateDownloadSingleSetupViewModel();
            viewModel.Title = title;
            viewModel.Video = video;
            viewModel.AvailableDownloadOptions = availableDownloadOptions;
            viewModel.AvailableSubtitleOptions = availableSubtitleOptions;

            return viewModel;
        }

        public static MessageBoxViewModel CreateMessageBoxViewModel(this IViewModelFactory factory, string title,
            string message)
        {
            var viewModel = factory.CreateMessageBoxViewModel();
            viewModel.Title = title;
            viewModel.Message = message;

            return viewModel;
        }
    }
}