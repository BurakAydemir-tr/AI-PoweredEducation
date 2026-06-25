using System.Security.Cryptography;
using System.Text;

namespace AI.PoweredEducation.Core.Security;

public static class SecureToken
{
    private const int DefaultTokenSizeInBytes = 32;

    public static string Generate(int sizeInBytes = DefaultTokenSizeInBytes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(sizeInBytes, 32);

        var bytes = RandomNumberGenerator.GetBytes(sizeInBytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var bytes = Encoding.UTF8.GetBytes(token);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
