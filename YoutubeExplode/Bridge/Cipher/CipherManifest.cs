using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Bridge.Cipher;

internal class CipherManifest(string signatureTimestamp, IReadOnlyList<ICipherOperation> operations)
{
    public string SignatureTimestamp { get; } = signatureTimestamp;

    public IReadOnlyList<ICipherOperation> Operations { get; } = operations;

    public string Decipher(string input) =>
        Operations.Aggregate(input, (acc, op) => op.Decipher(acc));
}
