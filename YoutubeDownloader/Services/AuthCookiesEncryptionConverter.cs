using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using YoutubeDownloader.Utils.Extensions;

namespace YoutubeDownloader.Services;

internal class AuthCookiesEncryptionConverter : JsonConverter<IReadOnlyList<Cookie>?>
{
    private const string Prefix = "enc:";

    // Exclusive upper bound for the random padding length range [1, MaxPaddingLength)
    private const int MaxPaddingLength = 17;

    private static readonly Lazy<byte[]> Key = new(() =>
        Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(Environment.TryGetMachineId() ?? string.Empty),
            Encoding.UTF8.GetBytes(ThisAssembly.Project.EncryptionSalt),
            iterations: 10_000,
            HashAlgorithmName.SHA256,
            outputLength: 16
        )
    );

    public override IReadOnlyList<Cookie>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            return null;

        var value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(Prefix, StringComparison.Ordinal))
            return null;

        try
        {
            var data = Convert.FromHexString(value[Prefix.Length..]);

            // Layout: nonce (12 bytes) | paddingLength (1 byte) | tag (16 bytes) | cipher
            var nonce = data.AsSpan(0, 12);
            var paddingLength = data[12];
            var tag = data.AsSpan(13, 16);
            var cipher = data.AsSpan(29);

            var decrypted = new byte[cipher.Length];
            using var aes = new AesGcm(Key.Value, 16);
            aes.Decrypt(nonce, cipher, tag, decrypted);

            return JsonSerializer
                .Deserialize<IReadOnlyList<CookieData>>(decrypted.AsSpan(paddingLength))
                ?.Select(c => new Cookie(c.Name, c.Value, c.Path, c.Domain))
                .ToList();
        }
        catch (Exception ex)
            when (ex
                    is FormatException
                        or CryptographicException
                        or ArgumentException
                        or IndexOutOfRangeException
                        or JsonException
            )
        {
            return null;
        }
    }

    public override void Write(
        Utf8JsonWriter writer,
        IReadOnlyList<Cookie>? value,
        JsonSerializerOptions options
    )
    {
        if (value is null || value.Count == 0)
        {
            writer.WriteNullValue();
            return;
        }

        var json = JsonSerializer.Serialize(
            value.Select(c => new CookieData(c.Name, c.Value, c.Path, c.Domain))
        );
        var cookieData = Encoding.UTF8.GetBytes(json);

        var paddingLength = RandomNumberGenerator.GetInt32(1, MaxPaddingLength);

        // Layout: nonce (12 bytes) | paddingLength (1 byte) | tag (16 bytes) | cipher (paddingLength + cookieData.Length)
        var data = new byte[29 + paddingLength + cookieData.Length];
        RandomNumberGenerator.Fill(data.AsSpan(0, 12)); // nonce
        data[12] = (byte)paddingLength;
        var cipherSource = data.AsSpan(29);
        RandomNumberGenerator.Fill(cipherSource[..paddingLength]); // random padding
        cookieData.CopyTo(cipherSource[paddingLength..]); // payload

        using var aes = new AesGcm(Key.Value, 16);
        aes.Encrypt(data.AsSpan(0, 12), cipherSource, cipherSource, data.AsSpan(13, 16));

        writer.WriteStringValue(Prefix + Convert.ToHexStringLower(data));
    }

    private record CookieData(string Name, string Value, string Path, string Domain);
}
