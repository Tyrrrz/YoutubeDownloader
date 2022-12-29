
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace YoutubeDownloader.Core.Resolving
{
    // This Singleton implementation is called "double check lock". It is safe
    // in multithreaded environment and provides lazy initialization for the
    // Singleton object.
    public class YTChannelPaser
    {
        private YTChannelPaser() { }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private static YTChannelPaser _instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // We now have a lock object that will be used to synchronize threads
        // during first access to the Singleton.
        private static readonly object _lock = new object();

        public static YTChannelPaser GetInstance()
        {
            // This conditional is needed to prevent threads stumbling over the
            // lock once the instance is ready.
            if (_instance == null)
            {
                // Now, imagine that the program has just been launched. Since
                // there's no Singleton instance yet, multiple threads can
                // simultaneously pass the previous conditional and reach this
                // point almost at the same time. The first of them will acquire
                // lock and will proceed further, while the rest will wait here.
                lock (_lock)
                {
                    // The first thread to acquire the lock, reaches this
                    // conditional, goes inside and creates the Singleton
                    // instance. Once it leaves the lock block, a thread that
                    // might have been waiting for the lock release may then
                    // enter this section. But since the Singleton field is
                    // already initialized, the thread won't create a new
                    // object.
                    if (_instance == null)
                    {
                        _instance = new YTChannelPaser();
                        _instance.Channel = null;
                    }
                }
            }
            return _instance;
        }

        public void clearData()
        {
            _instance.Channel = null;
        }
        public YTChannel parseYTChannelInfo(string channelURL)
        {
            _instance.Channel = new YTChannel();
            if (channelURL.Contains("youtube.com/c/") ||
                channelURL.Contains("youtube.com/user/") ||
                channelURL.Contains("youtube.com/@") ||
                channelURL.Contains("youtube.com/channel/")){

                IWebDriver? driver = null;
                try{
                    driver = Utils.Http.GetDriver();

                    System.Drawing.Size windowSize = new System.Drawing.Size(480, 600);
                    driver!.Manage().Window.Size = windowSize;

                    //if URL is not "youtube.com/channel/, then need to construct URL like it by parsing channelID from Youtube page HTML source
                    if (!channelURL.Contains("youtube.com/channel/"))
                    {
                        driver.Navigate().GoToUrl(channelURL);
                        string page = driver.PageSource;
                        //Console.WriteLine(page);
                        string search_word = "<meta itemprop=\"channelId\" content=\"";
                        int found = page.IndexOf(search_word);
                        //Console.WriteLine(found);
                        if (found != -1)
                        {
                            string channelID = page.Substring(found + search_word.Length, 24 /* there are 24 characters in channelID*/);
                            channelURL = "https://www.youtube.com/channel/" + channelID;
                            Console.WriteLine(channelURL);
                        }
                    }

                    driver.Navigate().GoToUrl("https://http5.org/chan");
                    IWebElement elementTxtBoxURL = driver.FindElement(By.ClassName("form-control"));
                    elementTxtBoxURL.SendKeys(channelURL);

                    IWebElement elementBtnSubmit = driver.FindElement(By.ClassName("btn-success"));
                    elementBtnSubmit.Click();
                    string pages = driver.PageSource;
                    //Console.WriteLine(pages);

                    //Unknown link, please let admin know
                    if (pages.Contains("Unknown link, please let admin know"))
                    {

                    }
                    else
                    {
                        string channelID = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[4]/div[1]/div[2]/code")).Text;
                        string channelName = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[4]/div[2]/div[2]/input")).GetAttribute("value");
                        string avatar = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[4]/div[4]/div[2]/a/img")).GetAttribute("src");
                        string banner = driver.FindElement(By.XPath("/html/body/div[2]/div/div/div[4]/div[5]/div[2]/a/img")).GetAttribute("src");

                        _instance.Channel.Id = channelID;
                        _instance.Channel.Name = channelName;
                        _instance.Channel.Avatar = avatar;
                        _instance.Channel.Banner = banner;
                        ReadOnlyCollection<IWebElement> webElements = driver.FindElements(By.XPath("//*[@class='mx-2']"));
                        for (int i = 0; i < webElements.Count; i++)
                        {
                            string videoUrl = webElements[i].GetAttribute("href");
                            _instance.Channel.MostPopularVideoUrls.Add(videoUrl);
                            _instance.Channel.MostPopularVideoUrlsText += videoUrl + "\n";
                            Console.WriteLine(videoUrl);
                        }
                        _instance.Channel.MostPopularVideoUrlsText.Replace("\n\n", "\n");
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (driver != null)
                    {
                        driver.Quit();
                    }
                }
            }
            return _instance.Channel!;
        }

        // We'll use this property to prove that our Singleton really works.
        public YTChannel? Channel { get; set; }
    }

    public class YTChannel
    {

        public YTChannel(){
            this.Id = "";
            this.Name = "";
            this.Avatar = "";
            this.isAvatarDownloaded = false;
            this.Banner = "";
            this.isBannerDownloaded = false;
            countDownloadedVideo = 0;
            MostPopularVideoUrlsText = "";
            MostPopularVideoUrls = new List<string>();
        }
        public YTChannel(string id, string name, string avatar, string banner)
        {
            this.Id = id;
            this.Name = name;
            this.Avatar = avatar;
            this.isAvatarDownloaded = false;
            this.Banner = banner;
            this.isBannerDownloaded = false;
            countDownloadedVideo = 0;
            MostPopularVideoUrlsText = "";
            MostPopularVideoUrls = new List<string>();
        }

        public string Id { get; set;}

        public string Name { get; set;}

        public string Avatar { get; set;}

        public bool isAvatarDownloaded { get; set; }

        public string Banner { get; set;}

        public bool isBannerDownloaded { get; set; }

        public int countDownloadedVideo { get; set; }

        public List<string> MostPopularVideoUrls { get; set;}

        public string MostPopularVideoUrlsText { get; set; }

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Id + "; " + Name + "; " + Avatar + "; " + Banner + "; " + MostPopularVideoUrls.Count;
        }
    }
}