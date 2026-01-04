using OtpNet;
using System.Security.Cryptography;
using Sqordia.Application.Common.Interfaces;

namespace Sqordia.Infrastructure.Services;

public class TotpService : ITotpService
{
    private const int SecretKeyLength = 20; // 160 bits

    public string GenerateSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(SecretKeyLength);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrCodeUrl(string email, string secretKey, string issuer = "Sqordia")
    {
        // Format: otpauth://totp/Issuer:email?secret=SECRET&issuer=Issuer
        var formattedEmail = Uri.EscapeDataString(email);
        var formattedIssuer = Uri.EscapeDataString(issuer);
        return $"otpauth://totp/{formattedIssuer}:{formattedEmail}?secret={secretKey}&issuer={formattedIssuer}";
    }

    public string FormatSecretKeyForManualEntry(string secretKey)
    {
        // Format secret key in groups of 4 for easier manual entry
        // Example: ABCD EFGH IJKL MNOP
        var formatted = string.Join(" ", Enumerable.Range(0, secretKey.Length / 4)
            .Select(i => secretKey.Substring(i * 4, 4)));
        
        return formatted;
    }

    public bool VerifyCode(string secretKey, string code)
    {
        try
        {
            var keyBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(keyBytes);
            
            // Verify with a window of ±1 time step (30 seconds each) to account for clock drift
            return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        }
        catch
        {
            return false;
        }
    }

    public List<string> GenerateBackupCodes(int count = 10)
    {
        var backupCodes = new List<string>();
        
        for (int i = 0; i < count; i++)
        {
            // Generate 8-character alphanumeric code
            var code = GenerateRandomCode(8);
            backupCodes.Add(code);
        }

        return backupCodes;
    }

    private static string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var code = new char[length];
        
        for (int i = 0; i < length; i++)
        {
            code[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }

        return new string(code);
    }
}

