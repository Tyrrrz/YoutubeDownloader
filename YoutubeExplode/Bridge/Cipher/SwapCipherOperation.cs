using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace YoutubeExplode.Bridge.Cipher;

internal class SwapCipherOperation(int index) : ICipherOperation
{
    public string Decipher(string input) =>
        new StringBuilder(input) { [0] = input[index], [index] = input[0] }.ToString();

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Swap ({index})";
}
