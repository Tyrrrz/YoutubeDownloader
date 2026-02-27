using System;
using System.IO;

namespace YoutubeDownloader.Utils.Extensions;

internal static class EnvironmentExtensions
{
    extension(Environment)
    {
        public static string? TryGetMachineId()
        {
            // Windows: stable GUID written during OS installation
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    using var regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                        @"SOFTWARE\Microsoft\Cryptography"
                    );
                    if (
                        regKey?.GetValue("MachineGuid") is string guid
                        && !string.IsNullOrWhiteSpace(guid)
                    )
                        return guid;
                }
                catch { }
            }
            else
            {
                // Unix: /etc/machine-id (set once by systemd at first boot)
                foreach (var path in new[] { "/etc/machine-id", "/var/lib/dbus/machine-id" })
                {
                    try
                    {
                        var id = File.ReadAllText(path).Trim();
                        if (!string.IsNullOrWhiteSpace(id))
                            return id;
                    }
                    catch { }
                }
            }

            // Last-resort fallback
            try
            {
                return Environment.MachineName;
            }
            catch
            {
                return null;
            }
        }
    }
}
