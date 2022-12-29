using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;
using WindowsInput;
using WindowsInput.Native;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Utils;

public static class Http
{
    public static HttpClient Client { get; } = new()
    {
        DefaultRequestHeaders =
        {
            // Required by some of the services we're using
            UserAgent =
            {
                new ProductInfoHeaderValue(
                    "YoutubeDownloader",
                    typeof(Http).Assembly.GetName().Version?.ToString(3)
                )
            }
        }
    };

    public static bool SignInGJW(string email, string pass, string path, string title, string category){
        bool result = false;
        IWebDriver driver = GetDriver();
        if(driver != null){


            driver.Navigate().GoToUrl("https://www.ganjing.com/signin");
            
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            
            string emailCSSSelector = "input[name='email'][type='email']";

            // wait maximum 10 seconds
            wait.Until(driver=>driver.FindElement(By.CssSelector(emailCSSSelector)));

            IWebElement elementTxtBoxEmail = driver.FindElement(By.CssSelector(emailCSSSelector));
            elementTxtBoxEmail.SendKeys(email);

            IWebElement elementTxtBoxPass = driver.FindElement(By.CssSelector("input[name='password'][type='password']"));
            elementTxtBoxPass.SendKeys(pass);

            IWebElement elementCheckBoxRemember = driver.FindElement(By.Id("comments"));
            elementCheckBoxRemember.Click();

            elementTxtBoxPass.Submit();

            //IWebElement elementBtnSubmit = driver.FindElement(By.ClassName("btn-basic btn-contained w-full mt-5 dark:bg-gjw-gray-600 dark:text-white"));
            //elementBtnSubmit.Click();

            wait.Until(driver=>driver.FindElement(By.Id("headlessui-menu-button-2")));
            IWebElement elementMenuBtn = driver.FindElement(By.Id("headlessui-menu-button-2"));
            //Console.WriteLine(pages);
            if (elementMenuBtn != null)
            {
                driver.Navigate().GoToUrl("https://studio.ganjing.com/");

                //
                string createContentXpath = "//button[normalize-space() = 'Create Content']";

                wait.Until(driver => driver.FindElement(By.XPath(createContentXpath)));
                IWebElement elementBtnCreateContent = driver.FindElement(By.XPath(createContentXpath));
                elementBtnCreateContent.Click();
                //

                string uploadVideoXpath = "/html/body/div[3]/div[3]/ul/li[1]";

                try
                {
                    wait.Until(driver => driver.FindElement(By.XPath(uploadVideoXpath)));
                    IWebElement elementBtnUploadVideo = driver.FindElement(By.XPath(uploadVideoXpath));

                    elementBtnUploadVideo.Click();
                }
                catch (Exception)
                {
                    wait.Until(driver => driver.FindElement(By.XPath(uploadVideoXpath)));
                    IWebElement elementBtnUploadVideo1 = driver.FindElement(By.XPath(uploadVideoXpath));
                    elementBtnUploadVideo1.Click();
                }

                string selectFileBtnXpath = "//button[normalize-space() = 'Select File']";
                wait.Until(driver=>driver.FindElement(By.XPath(selectFileBtnXpath)));
                IWebElement elementBtnSelectFile = driver.FindElement(By.XPath(selectFileBtnXpath));
                elementBtnSelectFile.Click();

                // select video
                Thread.Sleep(2000);
                InputSimulator sim = new InputSimulator();
                //System.Windows.Clipboard.SetText(path);
                sim.Keyboard.TextEntry(path);
                //sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
                sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);


                // select thumbnail
                
                string selectThumnailBtnXpath = "//div[normalize-space() = 'Upload Thumbnail']";
                wait.Until(driver => driver.FindElement(By.XPath(selectThumnailBtnXpath)));
                IWebElement elementBtnSelectThumnail = driver.FindElement(By.XPath(selectThumnailBtnXpath));
                elementBtnSelectThumnail.Click();
                Thread.Sleep(2000);
                //System.Windows.Clipboard.SetText(System.IO.Path.GetFileNameWithoutExtension(path) + ".jpg");
                sim.Keyboard.TextEntry(System.IO.Path.GetFileNameWithoutExtension(path) + ".jpg");
                //sim.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
                sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                
                // title
                Thread.Sleep(2000);
                string titleXpath = "/html/body/div[3]/div[3]/div/div/div/div/div[2]/div/div[1]/div/div/div/input";
                wait.Until(driver => driver.FindElement(By.XPath(titleXpath)));
                IWebElement titleElement = driver.FindElement(By.XPath(titleXpath));
                titleElement.Click();
                Thread.Sleep(1000);
                sim.Keyboard.TextEntry(title.Substring(0, Math.Min(title.Length,100 /*max 100 characters*/)));

                // category
                Thread.Sleep(1000);
                string categoryXpath = "/html/body/div[3]/div[3]/div/div/div/div/div[2]/div/div[3]/div/div[1]/div/div/div/div/input";
                wait.Until(driver => driver.FindElement(By.XPath(categoryXpath)));
                IWebElement categoryElement = driver.FindElement(By.XPath(categoryXpath));
                string selectedCategory = categoryElement.GetAttribute("value");
                if (selectedCategory.Equals("")){
                    categoryElement.SendKeys(category);
                }


                // save button
                Thread.Sleep(1000);
                string selectSaveBtnXpath = "/html/body/div[3]/div[3]/div/div/div/div/div[3]/button";
                wait.Until(driver=>driver.FindElement(By.XPath(selectSaveBtnXpath)));
                IWebElement elementBtnSave = driver.FindElement(By.XPath(selectSaveBtnXpath));
                elementBtnSave.Click();

                result = true;
            }
            else
            {

            }
        }

        return result;
    }

    public static IWebDriver GetDriver()
    {
        IWebDriver? driver = null;

        try
        {
            bool edgeInstalled = false;
            bool chromeInstalled = false;
            bool internetExplorerInstalled = false;
            try
            {
                foreach (Browser? browser in GetAllInstalledBrowsers.GetBrowsers())
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0}: \n\tPath: {1} \n\tVersion: {2} \n\tIcon: {3}", browser.Name, browser.Path, browser.Version, browser.IconPath));
                    if (browser.Name!.Equals("Microsoft Edge"))
                    {
                        edgeInstalled = true;
                    }
                    else if (browser.Name!.Equals("Google Chrome"))
                    {
                        chromeInstalled = true;
                    }
                    else if (browser.Name!.Equals("Internet Explorer"))
                    {
                        internetExplorerInstalled = true;
                    }
                }
            }
            catch (Exception)
            {
                // default
                edgeInstalled = true;
            }

            EdgeDriverService? edgeDriverService = null;
            ChromeDriverService? chromeDriverService = null;
            InternetExplorerDriverService? internetExplorerDriverService = null;
            if (chromeInstalled)
            {
                // https://www.nuget.org/packages/WebDriverManager/
                new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser);

                // hide black windows
                chromeDriverService = ChromeDriverService.CreateDefaultService();
                chromeDriverService.HideCommandPromptWindow = true;
                //
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--ignore-certificate-errors");
                // Open Chrome
                driver = new ChromeDriver(chromeDriverService, options);
            }
            else if (edgeInstalled)
            {
                // https://www.nuget.org/packages/WebDriverManager/
                new DriverManager().SetUpDriver(new EdgeConfig(), VersionResolveStrategy.MatchingBrowser);

                // hide black windows
                edgeDriverService = EdgeDriverService.CreateDefaultService();
                edgeDriverService.HideCommandPromptWindow = true;
                //
                EdgeOptions options = new EdgeOptions();
                options.AddArgument("--ignore-certificate-errors");

                // Open MS Edge
                driver = new EdgeDriver(edgeDriverService, options);
            }
            else if (internetExplorerInstalled)
            {
                // https://www.nuget.org/packages/WebDriverManager/
                new DriverManager().SetUpDriver(new InternetExplorerConfig(), VersionResolveStrategy.MatchingBrowser);

                // hide black windows
                internetExplorerDriverService = InternetExplorerDriverService.CreateDefaultService();
                internetExplorerDriverService.HideCommandPromptWindow = true;

                // Open InternetExplorer
                driver = new InternetExplorerDriver(internetExplorerDriverService);
            }
        }

        catch (Exception ex)
        {
            throw ex;
        }finally
        {

        }
        return driver!;
    }

    // https://social.msdn.microsoft.com/Forums/sqlserver/en-US/42650aa1-abd8-48d5-97e3-801414e936c8/get-a-list-of-all-browsers-installed-and-their-versions-from-remote-desktop?forum=csharpgeneral
    class GetAllInstalledBrowsers
    {
        /*
        static void Main(string[] args)
        {
            foreach (Browser browser in GetBrowsers())
            {
                Console.WriteLine(string.Format("{0}: \n\tPath: {1} \n\tVersion: {2} \n\tIcon: {3}", browser.Name, browser.Path, browser.Version, browser.IconPath));
            }
            Console.ReadKey();
        }
        */
        internal static String StripQuotes(String s)
        {
            if (s.EndsWith("\"") && s.StartsWith("\""))
            {
                return s.Substring(1, s.Length - 2);
            }
            else
            {
                return s;
            }
        }
        public static List<Browser> GetBrowsers()
        {
            RegistryKey browserKeys;
            //on 64bit the browsers are in a different location
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet");
            if (browserKeys == null)
            {
                browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
            }
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            string[] browserNames = browserKeys.GetSubKeyNames();
            var browsers = new List<Browser>();
            for (int i = 0; i < browserNames.Length; i++)
            {
                RegistryKey browserKey = browserKeys.OpenSubKey(browserNames[i]);
                string name = (string)browserKey.GetValue(null);
                if (name.Equals("Microsoft Edge") ||
                    name.Equals("Google Chrome") ||
                    name.Equals("Internet Explorer"))
                {
                    Browser browser = new Browser();
                    browser.Name = name;
                    RegistryKey browserKeyPath = browserKey.OpenSubKey(@"shell\open\command");
#pragma warning disable CS8604 // Possible null reference argument.
                    browser.Path = StripQuotes(browserKeyPath.GetValue(null).ToString());
                    RegistryKey browserIconPath = browserKey.OpenSubKey(@"DefaultIcon");
                    browser.IconPath = StripQuotes(browserIconPath.GetValue(null).ToString());
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    browsers.Add(browser);
                    if (browser.Path != null)
                        browser.Version = FileVersionInfo.GetVersionInfo(browser.Path).FileVersion;
                    else
                        browser.Version = "unknown";
                }
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            return browsers;
        }
    }

    class Browser
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? IconPath { get; set; }
        public string? Version { get; set; }
    }

}