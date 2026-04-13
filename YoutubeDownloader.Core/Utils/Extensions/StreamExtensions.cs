using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gress;

namespace YoutubeDownloader.Core.Utils.Extensions;

public static class StreamExtensions
{
    extension(Stream stream)
    {
        public async Task<int> CopyBufferedToAsync(
            Stream destination,
            Memory<byte> buffer,
            CancellationToken cancellationToken = default
        )
        {
            var bytesCopied = await stream.ReadAsync(buffer, cancellationToken);
            await destination.WriteAsync(buffer[..bytesCopied], cancellationToken);

            return bytesCopied;
        }

        public async Task CopyToAsync(
            Stream destination,
            long totalBytes,
            IProgress<Percentage>? progress = null,
            CancellationToken cancellationToken = default
        )
        {
            using var buffer = MemoryPool<byte>.Shared.Rent(81920);

            var totalBytesCopied = 0L;
            int bytesCopied;
            do
            {
                bytesCopied = await stream.CopyBufferedToAsync(
                    destination,
                    buffer.Memory,
                    cancellationToken
                );

                totalBytesCopied += bytesCopied;

                if (totalBytes > 0)
                    progress?.Report(Percentage.FromFraction(1.0 * totalBytesCopied / totalBytes));
            } while (bytesCopied > 0);
        }
    }
}
