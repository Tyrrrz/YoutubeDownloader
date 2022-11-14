using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

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

        var isSanctioned =
            CultureInfo.CurrentCulture.Name.EndsWith("-ru", StringComparison.OrdinalIgnoreCase) ||
            CultureInfo.CurrentCulture.Name.EndsWith("-by", StringComparison.OrdinalIgnoreCase);

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