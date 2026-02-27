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

public partial class SettingsService
{
    private class AuthCookiesEncryptionConverter : JsonConverter<IReadOnlyList<Cookie>?>
    {
        private static readonly Lazy<byte[]> Key = new(() =>
            Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(Environment.TryGetMachineId() ?? string.Empty),
                Encoding.UTF8.GetBytes(ThisAssembly.Project.EncryptionSalt),
                600_000,
                HashAlgorithmName.SHA256,
                16
            )
        );

        public override IReadOnlyList<Cookie>? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            if (reader.TokenType != JsonTokenType.String)
                return null;

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            try
            {
                var encryptedData = Convert.FromHexString(value);
                var cookieData = new byte[encryptedData.AsSpan(28).Length];

                // Layout: nonce (12 bytes) | tag (16 bytes) | cipher
                using var aes = new AesGcm(Key.Value, 16);
                aes.Decrypt(
                    encryptedData.AsSpan(0, 12),
                    encryptedData.AsSpan(28),
                    encryptedData.AsSpan(12, 16),
                    cookieData
                );

                return JsonSerializer
                    .Deserialize<IReadOnlyList<CookieData>>(cookieData)
                    ?.Select(c => new Cookie(c.Name, c.Value, c.Path, c.Domain))
                    .ToArray();
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
            var encryptedData = new byte[28 + cookieData.Length];

            // Nonce
            RandomNumberGenerator.Fill(encryptedData.AsSpan(0, 12));

            // Layout: nonce (12 bytes) | tag (16 bytes) | cipher
            using var aes = new AesGcm(Key.Value, 16);
            aes.Encrypt(
                encryptedData.AsSpan(0, 12),
                cookieData,
                encryptedData.AsSpan(28),
                encryptedData.AsSpan(12, 16)
            );

            writer.WriteStringValue(Convert.ToHexStringLower(encryptedData));
        }

        private record CookieData(string Name, string Value, string Path, string Domain);
    }
}
