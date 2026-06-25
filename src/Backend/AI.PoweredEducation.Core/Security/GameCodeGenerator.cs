using System.Security.Cryptography;

namespace AI.PoweredEducation.Core.Security;

public static class GameCodeGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Generate(int length = 6)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);

        return string.Create(length, Alphabet, static (buffer, alphabet) =>
        {
            for (var index = 0; index < buffer.Length; index++)
            {
                buffer[index] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            }
        });
    }
}
