using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;

namespace YoutubeDownloader;

public static class Sanctions
{
    [ModuleInitializer]
    internal static void Verify()
    {
        var isSkipped = string.Equals(
            Environment.GetEnvironmentVariable("RUSNI"),
            "PYZDA",
            StringComparison.OrdinalIgnoreCase
        );

        if (isSkipped)
            return;

        var locale = CultureInfo.CurrentCulture.Name;

        var region = Registry.CurrentUser
            .OpenSubKey(@"Control Panel\International\Geo", false)?
            .GetValue("Name") as string;

        var isSanctioned =
            locale.EndsWith("-ru", StringComparison.OrdinalIgnoreCase) ||
            locale.EndsWith("-by", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(region, "ru", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(region, "by", StringComparison.OrdinalIgnoreCase);

        if (!isSanctioned)
            return;

        MessageBox.Show(
            "You cannot use this software on the territory of a terrorist state. " +
            "Set the environment variable `RUSNI=PYZDA` if you wish to override this check.",
            "Sanctioned region",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );

        Environment.Exit(0xFACC);
    }
}