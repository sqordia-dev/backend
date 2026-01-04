namespace Sqordia.Contracts.Responses.TwoFactor;

public class TwoFactorStatusResponse
{
    public bool IsEnabled { get; set; }
    public DateTime? EnabledAt { get; set; }
    public int RemainingBackupCodes { get; set; }
}

