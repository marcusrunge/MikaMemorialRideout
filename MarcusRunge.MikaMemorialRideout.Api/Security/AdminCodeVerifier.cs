using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MarcusRunge.MikaMemorialRideout.Api.Security;

internal sealed class AdminCodeVerifier : IAdminCodeVerifier
{
    private readonly byte[] _expectedHash;
    private readonly byte[] _salt;

    public AdminCodeVerifier(IConfiguration configuration)
    {
        _salt = ReadBase64Setting(configuration, "RideoutAdminCodeSalt");
        _expectedHash = ReadBase64Setting(configuration, "RideoutAdminCodeHash");
    }

    public bool IsValid(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var codeBytes = Encoding.UTF8.GetBytes(code);
        var input = new byte[_salt.Length + codeBytes.Length];
        Buffer.BlockCopy(_salt, 0, input, 0, _salt.Length);
        Buffer.BlockCopy(codeBytes, 0, input, _salt.Length, codeBytes.Length);

        var actualHash = SHA256.HashData(input);
        return CryptographicOperations.FixedTimeEquals(actualHash, _expectedHash);
    }

    private static byte[] ReadBase64Setting(IConfiguration configuration, string key)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Die Konfiguration {key} fehlt.");

        try
        {
            return Convert.FromBase64String(value);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException($"Die Konfiguration {key} ist kein gültiger Base64-Wert.", exception);
        }
    }
}
