namespace Sqordia.Contracts.Responses.Privacy;

/// <summary>
/// Response after account deletion request (Quebec Bill 25 compliance)
/// </summary>
public class AccountDeletionResponse
{
    public bool Success { get; set; }
    public string DeletionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// For deactivation only: deadline by which user can reactivate their account
    /// </summary>
    public DateTime? ReactivationDeadline { get; set; }
}
