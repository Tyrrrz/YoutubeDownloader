using System;
using System.Reflection;

namespace YoutubeDownloader
{
    public partial class App
    {
        private static readonly Assembly Assembly = typeof(App).Assembly;

        public static string Name => Assembly.GetName().Name!;

        public static Version Version => Assembly.GetName().Version!;

        public static string VersionString => Version.ToString(3);

        public static string GitHubProjectUrl { get; } = "https://github.com/Tyrrrz/YoutubeDownloader";
    }
}