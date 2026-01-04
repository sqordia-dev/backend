namespace Sqordia.Application.Common.Interfaces;

public interface ITotpService
{
    string GenerateSecretKey();
    string GenerateQrCodeUrl(string email, string secretKey, string issuer = "Sqordia");
    string FormatSecretKeyForManualEntry(string secretKey);
    bool VerifyCode(string secretKey, string code);
    List<string> GenerateBackupCodes(int count = 10);
}

