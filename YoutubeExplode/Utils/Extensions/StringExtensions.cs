using System.Linq;
using System.Text;

namespace YoutubeExplode.Utils.Extensions;

internal static class StringExtensions
{
    extension(string str)
    {
        public string StripNonDigit()
        {
            var buffer = new StringBuilder();

            foreach (var c in str.Where(char.IsDigit))
                buffer.Append(c);

            return buffer.ToString();
        }
    }
}
