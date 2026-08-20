using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace YoutubeExplode.Utils;

internal static class Json
{
    public static string Extract(string source)
    {
        var buffer = new StringBuilder();

        var depth = 0;
        var isInsideString = false;

        // We trust that the source contains valid json, we just need to extract it.
        // To do it, we will be matching curly braces until we even out.
        foreach (var (i, ch) in source.Index())
        {
            var prev = i > 0 ? source[i - 1] : default;

            buffer.Append(ch);

            // Detect if inside a string
            if (ch == '"' && prev != '\\')
                isInsideString = !isInsideString;
            // Opening brace
            else if (ch == '{' && !isInsideString)
                depth++;
            // Closing brace
            else if (ch == '}' && !isInsideString)
                depth--;

            // Break when evened out
            if (depth == 0)
                break;
        }

        return buffer.ToString();
    }

    public static JsonElement Parse(string source)
    {
        using var document = JsonDocument.Parse(source);
        return document.RootElement.Clone();
    }

    public static JsonElement? TryParse(string source)
    {
        try
        {
            return Parse(source);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static string Escape(string value)
    {
        var buffer = new StringBuilder(value.Length);

        foreach (var ch in value)
        {
            if (ch == '\n')
                buffer.Append("\\n");
            else if (ch == '\r')
                buffer.Append("\\r");
            else if (ch == '\t')
                buffer.Append("\\t");
            else if (ch == '\\')
                buffer.Append("\\\\");
            else if (ch == '"')
                buffer.Append("\\\"");
            else
                buffer.Append(ch);
        }

        return buffer.ToString();
    }

    public static string Encode(string? value) =>
        value is not null ? '"' + Escape(value) + '"' : "null";

    public static string Encode(int? value) =>
        value is not null ? value.Value.ToString(CultureInfo.InvariantCulture) : "null";
}
