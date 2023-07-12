using System.Text;

namespace YoutubeDownloader.Core.Utils.Extensions;

public static class BinaryExtensions
{
    public static string ToHex(this byte[] data)
    {
        var buffer = new StringBuilder(2 * data.Length);

        foreach (var b in data)
            buffer.Append(b.ToString("x2"));

        return buffer.ToString();
    }
}