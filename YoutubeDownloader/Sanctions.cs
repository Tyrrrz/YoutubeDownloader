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

        var isSanctioned = new[]
        {
            CultureInfo.CurrentCulture,
            CultureInfo.CurrentUICulture,
            CultureInfo.InstalledUICulture,
            CultureInfo.DefaultThreadCurrentCulture,
            CultureInfo.DefaultThreadCurrentUICulture
        }.Any(c =>
            c is not null && (
                c.Name.Contains("-ru", StringComparison.OrdinalIgnoreCase) ||
                c.Name.Contains("-by", StringComparison.OrdinalIgnoreCase)
            )
        );

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