using System.Diagnostics.CodeAnalysis;
using PowerKit.Extensions;

namespace YoutubeExplode.Bridge.Cipher;

internal class ReverseCipherOperation : ICipherOperation
{
    public string Decipher(string input) => input.Reverse();

    [ExcludeFromCodeCoverage]
    public override string ToString() => "Reverse";
}
