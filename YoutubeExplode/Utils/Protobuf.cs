using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeExplode.Utils;

internal static class Protobuf
{
    private static bool IsLenField(ulong tag) => (tag & 0x7) == 2;

    private static ulong? TryReadVarint(byte[] data, ref int i)
    {
        var value = 0UL;
        var shift = 0;
        while (i < data.Length)
        {
            var b = data[i++];
            value |= (ulong)(b & 0x7F) << shift;

            if ((b & 0x80) == 0)
                return value;

            shift += 7;
            if (shift >= 64)
                break;
        }

        return null;
    }

    private static string? TryReadString(byte[] data, ref int i)
    {
        var length = TryReadVarint(data, ref i);
        if (length is null)
            return null;

        if (length.Value > int.MaxValue)
            return null;

        var result = Encoding.UTF8.GetString(data, i, (int)length);
        i += (int)length;

        return result;
    }

    // Deserializes a protobuf-encoded map<string, string> payload into a dictionary.
    // Each top-level LEN field (wire type 2) is treated as a map entry submessage where
    // field 1 is the string key and field 2 is the string value.
    // Returns null if the data cannot be parsed.
    public static IReadOnlyDictionary<string, string?>? TryDeserializeMap(byte[] data)
    {
        var result = new Dictionary<string, string?>(StringComparer.Ordinal);

        var i = 0;
        while (i < data.Length)
        {
            var outerTag = TryReadVarint(data, ref i);
            if (outerTag is null)
                return null;

            // Only process LEN-encoded fields (wire type 2) as map entries
            if (!IsLenField(outerTag.Value))
                return null;

            var entryLen = TryReadVarint(data, ref i);
            if (entryLen is null)
                return null;

            var entryEnd = i + (int)entryLen.Value;
            if (entryEnd > data.Length)
                return null;

            // Parse the map entry submessage: field 1 = key (string), field 2 = value (string)
            var key = default(string);
            var value = default(string);
            var j = i;
            while (j < entryEnd)
            {
                var fieldTag = TryReadVarint(data, ref j);
                if (fieldTag is null)
                    break;

                // Only handle LEN-encoded (string) fields
                if (!IsLenField(fieldTag.Value))
                    break;

                var fieldNum = (int)(fieldTag.Value >> 3);

                var str = TryReadString(data, ref j);
                if (str is null)
                    break;

                if (fieldNum == 1)
                    key = str;
                else if (fieldNum == 2)
                    value = str;
            }

            if (key is not null)
                result[key] = value;

            i = entryEnd;
        }

        return result;
    }

    // Decodes a base64-encoded protobuf map<string, string> payload into a dictionary.
    // Returns null if the string is not valid base64 or cannot be parsed.
    public static IReadOnlyDictionary<string, string?>? TryDeserializeMap(string base64)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64);
            return TryDeserializeMap(bytes);
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
