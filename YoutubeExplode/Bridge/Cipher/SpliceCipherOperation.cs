using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Bridge.Cipher;

internal class SpliceCipherOperation(int index) : ICipherOperation
{
    public string Decipher(string input) => input[index..];

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Splice ({index})";
}
