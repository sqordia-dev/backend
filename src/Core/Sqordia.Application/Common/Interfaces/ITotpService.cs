namespace Sqordia.Application.Common.Interfaces;

public interface ITotpService
{
    string GenerateSecretKey();
    string GenerateQrCodeUrl(string email, string secretKey, string issuer = "Sqordia");
    string FormatSecretKeyForManualEntry(string secretKey);
    bool VerifyCode(string secretKey, string code);
    List<string> GenerateBackupCodes(int count = 10);

    /// <summary>
    /// Hash a backup code for secure storage using SHA256.
    /// </summary>
    string HashBackupCode(string code);

    /// <summary>
    /// Check if a plaintext code matches any hashed backup code.
    /// Returns the matched hash (for removal) or null.
    /// </summary>
    string? FindMatchingBackupCode(string code, List<string> hashedCodes);
}

