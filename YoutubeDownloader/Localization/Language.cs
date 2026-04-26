using System.ComponentModel.DataAnnotations;

namespace YoutubeDownloader.Localization;

public enum Language
{
    System,
    English,
    Ukrainian,
    German,
    French,
    Spanish,

    [Display(Name = "Simplified Chinese")]
    ChineseSimplified,
}
