using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using Gress.Completable;
using OpenQA.Selenium;
using Stylet;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Core.Resolving;
using YoutubeDownloader.Core.Tagging;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.ViewModels.Dialogs;
using YoutubeDownloader.ViewModels.Framework;
using YoutubeExplode.Exceptions;
using System.Reflection;

namespace YoutubeDownloader.ViewModels.Components;

public class DashboardViewModel : PropertyChangedBase, IDisposable
{
    private readonly IViewModelFactory _viewModelFactory;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;

    private readonly AutoResetProgressMuxer _progressMuxer;
    private readonly ResizableSemaphore _downloadSemaphore = new();

    private readonly QueryResolver _queryResolver = new();
    private readonly VideoDownloader _videoDownloader = new();
    private readonly MediaTagInjector _mediaTagInjector = new();

    private static Mutex mut = new Mutex();
    public bool IsBusy { get; private set; }
    public ProgressContainer<Percentage> Progress { get; } = new();

    public bool IsProgressIndeterminate => IsBusy && Progress.Current.Fraction is <= 0 or >= 1;

    public string? Query { get; set; }

    private QueryResult firstChannelQueryResult;
    private string channelTitle;

    public BindableCollection<DownloadViewModel> Downloads { get; } = new();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public DashboardViewModel(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        IViewModelFactory viewModelFactory,
        DialogManager dialogManager,
        SettingsService settingsService)
    {
        _viewModelFactory = viewModelFactory;
        _dialogManager = dialogManager;
        _settingsService = settingsService;

        _progressMuxer = Progress.CreateMuxer().WithAutoReset();

        _settingsService.BindAndInvoke(o => o.ParallelLimit, (_, e) => _downloadSemaphore.MaxCount = e.NewValue);
        Progress.Bind(o => o.Current, (_, _) => NotifyOfPropertyChange(() => IsProgressIndeterminate));
    }

    public bool CanShowSettings => !IsBusy;
    public async void ShowSettings() => await _dialogManager.ShowDialogAsync(
        _viewModelFactory.CreateSettingsViewModel()
    );
    public void Help()
    {
        string url = "https://www.ganjing.com/channel/1fckkvjkqh72FQ5r2Fvprfj6q1lg0c/playlist/1ff41rph6likfNRaA6hssga15e0p";
        IWebDriver driver = Http.GetDriver();
        driver.Navigate().GoToUrl(url);
    }
    public void UploadVideo()
    {
        string url = "https://studio.ganjing.com/";
        IWebDriver driver = Http.GetDriver();
        driver.Navigate().GoToUrl(url);
    }
    private void EnqueueDownload(DownloadViewModel download, int position = 0)
    {
        var progress = _progressMuxer.CreateInput();

        Task.Run(async () =>
        {
            bool shouldDownload = false;
            VideoInfo? videoInfo = null;
            try
            {
                using var access = await _downloadSemaphore.AcquireAsync(download.CancellationToken);
                // get status of video from cache
                videoInfo = Database.Find(download.Video!.Id);
                if (!download.ForceDownload)
                {
                    if (videoInfo!= null && videoInfo.DownloadStatus != "")
                    {
                        string status = videoInfo.DownloadStatus;
                        if (status.Equals(DownloadStatus.Completed.ToString()) || 
                            (status.Equals(DownloadStatus.Completed_Already.ToString())))
                        {
                            if (!File.Exists(download.FilePath))
                            {
                                string? path = Path.GetDirectoryName(download.FilePath!);
                                string[] files = System.IO.Directory.GetFiles(path!, "*" + download.Video!.Id + "*.mp4", System.IO.SearchOption.TopDirectoryOnly);
                                if (files.Length == 0)
                                {
                                    // video deleted from FileExplorer
                                    download.Status = DownloadStatus.Deleted;
                                }
                                else
                                {  // renamed
                                    download.Status = DownloadStatus.Completed_Already;
                                }
                            }
                            else
                            {
                                download.Status = DownloadStatus.Completed_Already;
                            }
                            //throw new Exception(status);
                        }
                        else if (status.Equals(DownloadStatus.Deleted.ToString()))
                        {
                            //throw new Exception(status);
                            download.Status = DownloadStatus.Deleted;
                        }
                        else if (status.Equals(DownloadStatus.Failed.ToString()))
                        {
                            //throw new Exception(status);
                            shouldDownload = true;
                        }
                        else if (status.Equals(DownloadStatus.Canceled.ToString()))
                        {
                            //throw new Exception(status);
                            download.Status = DownloadStatus.Canceled;
                        }
                        else if (status.Equals(DownloadStatus.Canceled_low_quality.ToString()))
                        {
                            //throw new Exception(status);
                            download.Status = DownloadStatus.Canceled_low_quality;
                        }
                        else if (status.Equals(DownloadStatus.Canceled_too_short.ToString()))
                        {
                            //throw new Exception(status);
                            download.Status = DownloadStatus.Canceled_too_short;
                        }
                        else if (status.Equals(DownloadStatus.Canceled_too_long.ToString()))
                        {
                            //throw new Exception(status);
                            download.Status = DownloadStatus.Canceled_too_long;
                        }
                    }else{
                        shouldDownload = true;
                    }
                }else{
                    shouldDownload = true;
                }
                
                if(shouldDownload){
                    // continue checking
                    shouldDownload = false;
                    download.Status = DownloadStatus.Started;
                    var downloadOption = download.DownloadOption is not null ?
                        download.DownloadOption :
                        await _videoDownloader.GetBestDownloadOptionAsync(
                            download.Video!.Id,
                            download.DownloadPreference!,
                            download.CancellationToken
                        );
                    int? quality = downloadOption.VideoQuality?.MaxHeight;
                    bool hightQualityVideoDownloaded = false;
                    bool tooShort = false;
                    bool tooLong = false;
                    if (download.DownloadPreference is null || ((downloadOption.Container == YoutubeExplode.Videos.Streams.Container.WebM ||
                        downloadOption.Container == YoutubeExplode.Videos.Streams.Container.Mp4) &&
                        (download.DownloadPreference!.PreferredVideoQuality == VideoQualityPreference.Highest ||
                        download.DownloadPreference!.PreferredVideoQuality == VideoQualityPreference.UpTo1080p) &&
                        quality >= 720))
                    {
                        hightQualityVideoDownloaded = true;
                    }

                    bool lowQualityVideoDownloaded = false;
                    if (download.DownloadPreference is null || ((downloadOption.Container == YoutubeExplode.Videos.Streams.Container.WebM ||
                        downloadOption.Container == YoutubeExplode.Videos.Streams.Container.Mp4) &&
                        (download.DownloadPreference!.PreferredVideoQuality == VideoQualityPreference.UpTo720p ||
                        download.DownloadPreference!.PreferredVideoQuality == VideoQualityPreference.UpTo480p)))
                    {
                        lowQualityVideoDownloaded = true;
                    }

                    bool audioDownloaded = false;
                    if (downloadOption.Container.Name == YoutubeExplode.Videos.Streams.Container.Mp3.Name ||
                        downloadOption.Container.Name == "ogg" ||
                        downloadOption.Container.Name == "m4a")
                    {
                        audioDownloaded = true;
                    }
                    int SECONDS_IN_MINUTE = 60;

                    if (download.Video!.Duration?.TotalSeconds < SECONDS_IN_MINUTE)
                    {
                        //shorter than 1 minutes
                        tooShort = true;
                    }
                    else if (download.Video!.Duration?.TotalSeconds > 60 * SECONDS_IN_MINUTE)
                    {
                        // longer than 60 minutes
                        tooLong = true;
                    }

                    bool okLength = !tooShort && !tooLong;

                    shouldDownload = (hightQualityVideoDownloaded && okLength) || (lowQualityVideoDownloaded && okLength) || (audioDownloaded && okLength) || download.ForceDownload;

                    if (shouldDownload)
                    {
                        Console.WriteLine("Accepted!");
                        //
                        DirectoryEx.CreateDirectoryForFile(download.FilePath!);
                        File.WriteAllBytes(download.FilePath!, Array.Empty<byte>());
                        await _videoDownloader.DownloadVideoAsync(
                            download.FilePath!,
                            download.Video!,
                            downloadOption,
                            download.Progress.Merge(progress),
                            download.CancellationToken
                        );

                        if (_settingsService.ShouldInjectTags)
                        {
                            try
                            {
                                await _mediaTagInjector.InjectTagsAsync(
                                    download.FilePath!,
                                    download.Video!,
                                    download.CancellationToken
                                );
                            }
                            catch
                            {
                                // Media tagging is not critical
                            }
                        }

                        // download banner & avatar channel if need
                        var dirPath = Path.GetDirectoryName(download.FilePath!);
                        DownloadAvatar(dirPath);
                        DownloadBanner(dirPath);
                        download.Status = DownloadStatus.Completed;
                    }
                    else
                    {
                        if (okLength)
                        {
                            Console.WriteLine("Canceled_low_quality");
                            //throw new Exception("Canceled_low_quality");
                            download.Status = DownloadStatus.Canceled_low_quality;
                        }
                        else if (tooShort)
                        {
                            Console.WriteLine("Canceled_too_short");
                            //throw new Exception("Canceled_too_short");
                            download.Status = DownloadStatus.Canceled_too_short;
                        }
                        else if (tooLong)
                        {
                            Console.WriteLine("Canceled_too_long");
                            //throw new Exception("Canceled_too_long");
                            download.Status = DownloadStatus.Canceled_too_long;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                download.Status = ex is OperationCanceledException
                ? DownloadStatus.Canceled
                : DownloadStatus.Failed;

                // Short error message for YouTube-related errors, full for others
                download.ErrorMessage = ex is YoutubeExplodeException
                    ? ex.Message
                    : ex.ToString();
            }
            finally
            {
                if (videoInfo!= null)
                {
                    string currentStatus = download.Status.ToString();
                    if (!currentStatus.Equals(videoInfo.DownloadStatus))
                    {
                        // status changed, then Database need updated
                        videoInfo.DownloadStatus = download.Status.ToString();
                        // make sure only 1 thread can access Database at the same time
                        mut.WaitOne();
                        Database.InsertOrUpdate(videoInfo);
                        // just work-around to optimize: if too much videos are deleted, then GUI is very slow
                        if(Database.Count() < 200 || !videoInfo.DownloadStatus.Equals(DownloadStatus.Deleted.ToString())){
                            Database.Save();
                        }
                        mut.ReleaseMutex();
                    }
                }
                progress.ReportCompletion();
                download.Dispose();
            }
        });
        Downloads.Insert(position, download);
    }

    private static void DownloadAvatar(string? dirPath)
    {
        if(YTChannelPaser.GetInstance().Channel != null){
            string dst = dirPath + "/[0000]-[Avatar-Kenh]-[" +
            YoutubeDownloader.Core.Utils.PathEx.EscapeFileName(YTChannelPaser.GetInstance().Channel!.Name) + "].jpg";
            if (!YTChannelPaser.GetInstance().Channel!.isAvatarDownloaded &&
                !File.Exists(dst))
            {
                if(YTChannelPaser.GetInstance().Channel!.Avatar == ""){
                    if (!YTChannelPaser.GetInstance().Channel!.isAvatarDownloaded)
                    {
                        try{
                            string src = Directory.GetCurrentDirectory() + "/avatar_trang.jpg";
                            File.Copy(src, dst);
                            YTChannelPaser.GetInstance().Channel!.isAvatarDownloaded = true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }else{
                    using (WebClient webClient = new WebClient())
                    {
                        bool downloadSuccess = true;
                        byte[] dataArr = new byte[1];
                        try
                        {
                            dataArr = webClient.DownloadData(YTChannelPaser.GetInstance().Channel!.Avatar);
                        }
                        catch (Exception)
                        {
                        }
                        mut.WaitOne();
                        try
                        {
                            if (downloadSuccess)
                            {
                                if (!YTChannelPaser.GetInstance().Channel!.isAvatarDownloaded)
                                {
                                    File.WriteAllBytes(dst, dataArr);
                                    YTChannelPaser.GetInstance().Channel!.isAvatarDownloaded = true;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }finally{
                            mut.ReleaseMutex();
                        }
                    }
                }
            }
        }
    }
    private static void DownloadBanner(string? dirPath)
    {
        if(YTChannelPaser.GetInstance().Channel != null){
            string dst = dirPath + "/[0000]-[Banner-Kenh]-[" + 
                YoutubeDownloader.Core.Utils.PathEx.EscapeFileName(YTChannelPaser.GetInstance().Channel!.Name) +"].jpg";
            if (YTChannelPaser.GetInstance().Channel != null &&
                !YTChannelPaser.GetInstance().Channel!.isBannerDownloaded && 
                !File.Exists(dst))
            {
                if(YTChannelPaser.GetInstance().Channel!.Banner == ""){
                    if (!YTChannelPaser.GetInstance().Channel!.isBannerDownloaded)
                    {
                        try
                        {
                            string src = Directory.GetCurrentDirectory() + "/banner_trang.jpg";
                            File.Copy(src, dst);
                            YTChannelPaser.GetInstance().Channel!.isAvatarDownloaded = true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }else{
                    using (WebClient webClient = new WebClient())
                    {
                        bool downloadSuccess = true;
                        byte[] dataArr = new byte[1];
                        try
                        {
                            dataArr = webClient.DownloadData(YTChannelPaser.GetInstance().Channel!.Banner);
                        }
                        catch (Exception)
                        {
                            downloadSuccess = false;
                        }
                        mut.WaitOne();
                        try
                        {
                            if (downloadSuccess)
                            {
                                if (!YTChannelPaser.GetInstance().Channel!.isBannerDownloaded)
                                {
                                    File.WriteAllBytes(dst, dataArr);
                                    YTChannelPaser.GetInstance().Channel!.isBannerDownloaded = true;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }finally{
                            mut.ReleaseMutex();
                        }
                    }
                }
            }
        }
    }

    public void DeleteQuery(){
        if(!IsBusy && !string.IsNullOrWhiteSpace(Query)){
            Query="";
        }
    }
    public bool CanProcessQuery => !IsBusy && !string.IsNullOrWhiteSpace(Query);
    public async void ProcessQuery()
    {
        if (string.IsNullOrWhiteSpace(Query))
            return;

        IsBusy = true;
        // For channel: 1st search for TOP 50 videos, 2nd search for all videos
        int countSearch = 1;
        string originalQuery = Query;
        
        

        // Small weight to not offset any existing download operations
        var progress = _progressMuxer.CreateInput(0.01);

        try
        {
            Console.WriteLine("=============> " + Query);

            if (Query.Contains("youtube.com/c/") ||
                Query.Contains("youtube.com/user/") ||
                Query.Contains("youtube.com/@") ||
                Query.Contains("youtube.com/channel/"))
            {
                //if (_settingsService.DownloadMostViewedVideoOnly)
                {
                    YTChannel channel = YTChannelPaser.GetInstance().parseYTChannelInfo(Query);
                    channelTitle = channel.Name;
                    if (channel.MostPopularVideoUrlsText != "")
                    {
                        originalQuery = Query;
                        Query = channel.MostPopularVideoUrlsText;
                        // enable 2nd search for all videos
                        countSearch = 2;
                    }else{
                        // cannot get most viewed video
                        await _dialogManager.ShowDialogAsync(
                            _viewModelFactory.CreateMessageBoxViewModel(
                                "Không tìm thấy",
                                "Không tìm thấy video nào từ đường link hoặc từ khóa bạn cung cấp. Có thể do mạng yếu hoặc đường link/từ khóa bị sai, xin hãy kiểm tra và thử lại!"
                            )
                        );
                        return;
                    }
                }
            }
            else
            {
                YTChannelPaser.GetInstance().clearData();
            }

            for(int i = 0; i < countSearch; i++){
                var result = await _queryResolver.ResolveAsync(
                    Query.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                    progress
                );

                // Single video
                if (result.Videos.Count == 1)
                {
                    var video = result.Videos.Single();
                    var downloadOptions = await _videoDownloader.GetDownloadOptionsAsync(video.Id);

                    var download = await _dialogManager.ShowDialogAsync(
                        _viewModelFactory.CreateDownloadSingleSetupViewModel(video, downloadOptions)
                    );

                    if (download is null)
                        return;

                    EnqueueDownload(download);
                    // skip the 2nd search
                    break;
                }
                // Multiple videos
                else if (result.Videos.Count > 1)
                {
                    bool showChoseVideoDialog = false;
                    if(countSearch == 2){
                        
                        if(i == 0){
                            firstChannelQueryResult = result;
                        }else{ // 2nd channel query
                            List<YoutubeExplode.Videos.IVideo> videos = firstChannelQueryResult.Videos.Union(result.Videos).ToList();

                            // remove duplicate video
                            var unique = new List<YoutubeExplode.Videos.IVideo>();
                            var hs = new HashSet<string>();
                            foreach (YoutubeExplode.Videos.IVideo t in videos)
                                if (hs.Add(t.Id))
                                    unique.Add(t);

                            QueryResult lastQueryResult = new QueryResult(QueryResultKind.Channel,$"Kênh: {channelTitle}", unique);
                            result = lastQueryResult;
                            showChoseVideoDialog = true;
                        }
                        
                    }else{
                        showChoseVideoDialog = true;
                    }

                    if(showChoseVideoDialog){
                        var downloads = await _dialogManager.ShowDialogAsync(
                            _viewModelFactory.CreateDownloadMultipleSetupViewModel(
                                result.Title,
                                result.Videos,
                                // Pre-select videos if they come from a single query and not from search (e.g: playlist URL)
                                // or URL is a Channel
                                (result.Kind is not QueryResultKind.Search and not QueryResultKind.Aggregate) || true
                            )
                        );

                        if (downloads is null)
                            return;

                        foreach (var download in downloads)
                            EnqueueDownload(download);
                    }
                    
                }
                // No videos found
                else
                {
                    await _dialogManager.ShowDialogAsync(
                        _viewModelFactory.CreateMessageBoxViewModel(
                            "Không tìm thấy",
                            "Không tìm thấy video nào từ đường link hoặc từ khóa bạn cung cấp"
                        )
                    );

                    // skip the 2nd search
                    break;
                }
                // using the 1st query for searching all videos of a channel
                Query = originalQuery;
            }
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelFactory.CreateMessageBoxViewModel(
                    "Lỗi",
                    // Short error message for YouTube-related errors, full for others
                    ex is YoutubeExplodeException
                        ? ex.Message
                        : ex.ToString()
                )
            );
        }
        finally
        {
            progress.ReportCompletion();
            IsBusy = false;
        }
    }

    public void RemoveDownload(DownloadViewModel download)
    {
        Downloads.Remove(download);
        download.Cancel();
        download.Dispose();
    }

    public void RemoveSuccessfulDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (download.Status == DownloadStatus.Completed || download.Status == DownloadStatus.Completed_Already)
                RemoveDownload(download);
        }
    }

    public void RemoveInactiveDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (download.Status is DownloadStatus.Completed or
             DownloadStatus.Completed_Already or
             DownloadStatus.Failed or
             DownloadStatus.Canceled or
             DownloadStatus.Canceled_low_quality or
             DownloadStatus.Canceled_too_short or
             DownloadStatus.Canceled_too_long or
             DownloadStatus.Deleted)
                RemoveDownload(download);
        }
    }

    public void RestartDownload(DownloadViewModel download)
    {
        var position = Math.Max(0, Downloads.IndexOf(download));
        RemoveDownload(download);

        var newDownload = download.DownloadOption is not null
            ? _viewModelFactory.CreateDownloadViewModel(
                download.Video!,
                download.DownloadOption,
                download.FilePath!
            )
            : _viewModelFactory.CreateDownloadViewModel(
                download.Video!,
                download.DownloadPreference!,
                download.FilePath!
            );
        newDownload.ForceDownload = true;
        EnqueueDownload(newDownload, position);
    }

    public void RestartFailedDownloads()
    {
        foreach (var download in Downloads.ToArray().Reverse())
        {
            if (download.Status == DownloadStatus.Failed)
                RestartDownload(download);
        }
    }

    public void RestartCancelDownloads()
    {
        
        foreach (var download in Downloads.ToArray().Reverse())
        {
            if (download.Status == DownloadStatus.Canceled)
                RestartDownload(download);
        }
    }
    

    public void CancelAllDownloads()
    {
        foreach (var download in Downloads.Reverse())
        {
            download.Cancel();
        }
        IsBusy = false;
    }

    public void Dispose()
    {
        _downloadSemaphore.Dispose();
    }
}