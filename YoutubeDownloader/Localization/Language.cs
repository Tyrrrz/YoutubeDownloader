using System.ComponentModel;

namespace YoutubeDownloader.Localization;

public enum Language
{
    System,
    English,
    Ukrainian,
    German,
    French,
    Spanish,

    [Description("Simplified Chinese")]
    ChineseSimplified,
}
