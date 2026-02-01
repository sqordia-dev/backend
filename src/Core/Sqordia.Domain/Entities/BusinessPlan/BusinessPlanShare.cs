using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities.BusinessPlan;

/// <summary>
/// Represents a share/permission for a business plan
/// Allows sharing business plans with specific users or via public links
/// </summary>
public class BusinessPlanShare : BaseAuditableEntity
{
    public Guid BusinessPlanId { get; private set; }
    public Guid? SharedWithUserId { get; private set; } // Null for public shares
    public string? SharedWithEmail { get; private set; } // For email invitations
    public SharePermission Permission { get; private set; }
    public bool IsPublic { get; private set; }
    public string? PublicToken { get; private set; } // Unique token for public access
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastAccessedAt { get; private set; }
    public int AccessCount { get; private set; }

    // Vault Share Features (V2)
    public bool IsVaultShare { get; private set; }
    public bool EnableWatermark { get; private set; }
    public string? WatermarkText { get; private set; }
    public bool AllowDownload { get; private set; }
    public bool TrackViews { get; private set; }
    public bool RequireEmailVerification { get; private set; }
    public string? PasswordHash { get; private set; }
    public int? MaxViews { get; private set; }
    
    // Navigation properties
    public BusinessPlan BusinessPlan { get; private set; } = null!;
    public Domain.Entities.Identity.User? SharedWithUser { get; private set; }
    
    private BusinessPlanShare() { } // EF Core constructor
    
    public BusinessPlanShare(
        Guid businessPlanId,
        SharePermission permission,
        Guid? sharedWithUserId = null,
        string? sharedWithEmail = null,
        bool isPublic = false,
        DateTime? expiresAt = null)
    {
        BusinessPlanId = businessPlanId;
        Permission = permission;
        SharedWithUserId = sharedWithUserId;
        SharedWithEmail = sharedWithEmail;
        IsPublic = isPublic;
        ExpiresAt = expiresAt;
        IsActive = true;
        AccessCount = 0;
        
        if (isPublic)
        {
            PublicToken = GeneratePublicToken();
        }
        
        if (isPublic && sharedWithUserId.HasValue)
        {
            throw new ArgumentException("Public shares cannot have a specific user");
        }
    }
    
    public void UpdatePermission(SharePermission newPermission)
    {
        Permission = newPermission;
    }
    
    public void Revoke()
    {
        IsActive = false;
    }
    
    public void Activate()
    {
        IsActive = true;
    }
    
    public void RecordAccess()
    {
        AccessCount++;
        LastAccessedAt = DateTime.UtcNow;
    }
    
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }
    
    public bool CanAccess()
    {
        return IsActive && !IsExpired() && !HasReachedMaxViews();
    }

    public bool HasReachedMaxViews()
    {
        return MaxViews.HasValue && AccessCount >= MaxViews.Value;
    }

    // Vault share configuration methods
    public void ConfigureAsVaultShare(
        bool enableWatermark = true,
        string? watermarkText = null,
        bool allowDownload = false,
        bool trackViews = true,
        bool requireEmailVerification = false,
        string? passwordHash = null,
        int? maxViews = null)
    {
        IsVaultShare = true;
        EnableWatermark = enableWatermark;
        WatermarkText = watermarkText;
        AllowDownload = allowDownload;
        TrackViews = trackViews;
        RequireEmailVerification = requireEmailVerification;
        PasswordHash = passwordHash;
        MaxViews = maxViews;
    }

    public bool VerifyPassword(string password, Func<string, string, bool> verifyFunc)
    {
        if (string.IsNullOrEmpty(PasswordHash))
            return true; // No password required
        return verifyFunc(password, PasswordHash);
    }

    private string GeneratePublicToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "")
            .Substring(0, 22); // 22 characters for a secure token
    }
}

