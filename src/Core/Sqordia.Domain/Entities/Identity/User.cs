using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;
using Sqordia.Domain.ValueObjects;

namespace Sqordia.Domain.Entities.Identity;

public class User : BaseAuditableEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public EmailAddress Email { get; private set; } = null!;
    public string UserName { get; private set; } = null!;
    public string? PasswordHash { get; private set; }
    public bool IsEmailConfirmed { get; private set; }
    public DateTime? EmailConfirmedAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int AccessFailedCount { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public bool LockoutEnabled { get; private set; } = true;
    public string? PhoneNumber { get; private set; }
    public bool PhoneNumberVerified { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public DateTime? PasswordLastChangedAt { get; private set; }
    public bool RequirePasswordChange { get; private set; }
    public UserType UserType { get; private set; }
    public PersonaType? Persona { get; private set; }
    
    // OAuth fields
    public string? GoogleId { get; private set; }
    public string? MicrosoftId { get; private set; }
    public string Provider { get; private set; } = "local"; // "local", "google", "microsoft"

    // Onboarding fields
    public bool OnboardingCompleted { get; private set; }
    public int? OnboardingStep { get; private set; }
    public string? OnboardingData { get; private set; } // JSON storage for onboarding step data

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<UserConsent> Consents { get; private set; } = new List<UserConsent>();

    private User() { } // EF Core constructor

    public User(string firstName, string lastName, EmailAddress email, string userName, UserType userType = UserType.Entrepreneur)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        UserType = userType;
        IsActive = true;
        IsEmailConfirmed = false;
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        PasswordLastChangedAt = DateTime.UtcNow;
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        EmailConfirmedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        SoftDelete();
    }

    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Reactivate a deactivated account (within recovery window)
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        Restore();
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    // Lockout management
    public bool IsLockedOut => LockoutEnabled && LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;

    public void RecordFailedLogin()
    {
        AccessFailedCount++;
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
    }

    public void LockAccount(DateTime lockoutEnd)
    {
        LockoutEnd = lockoutEnd;
    }

    public void UnlockAccount()
    {
        LockoutEnd = null;
        AccessFailedCount = 0;
    }

    public void SetLockoutEnabled(bool enabled)
    {
        LockoutEnabled = enabled;
    }

    // Profile management
    public void UpdateProfile(string firstName, string lastName, string? userName = null)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        
        if (!string.IsNullOrEmpty(userName))
        {
            UserName = userName;
        }
    }

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
        if (string.IsNullOrEmpty(phoneNumber))
        {
            PhoneNumberVerified = false;
        }
    }

    public void ConfirmPhoneNumber()
    {
        PhoneNumberVerified = true;
    }

    public void UpdateProfilePicture(string? profilePictureUrl)
    {
        ProfilePictureUrl = profilePictureUrl;
    }

    public void UpdateUserType(UserType userType)
    {
        UserType = userType;
    }

    public void SetPersona(PersonaType? persona)
    {
        Persona = persona;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
        PasswordLastChangedAt = DateTime.UtcNow;
        RequirePasswordChange = false;
    }

    public void ForcePasswordChange()
    {
        RequirePasswordChange = true;
    }

    public bool IsPasswordExpired(int passwordExpiryDays)
    {
        if (!PasswordLastChangedAt.HasValue)
        {
            return false; // Never changed, use registration date
        }

        return (DateTime.UtcNow - PasswordLastChangedAt.Value).TotalDays > passwordExpiryDays;
    }

    // OAuth methods
    public static User CreateGoogleUser(string googleId, string firstName, string lastName, EmailAddress email, string? profilePictureUrl = null)
    {
        var user = new User(firstName, lastName, email, email.Value, UserType.Entrepreneur);
        user.GoogleId = googleId;
        user.Provider = "google";
        user.IsEmailConfirmed = true; // Google emails are pre-verified
        user.EmailConfirmedAt = DateTime.UtcNow;
        user.ProfilePictureUrl = profilePictureUrl;
        return user;
    }

    public void LinkGoogleAccount(string googleId, string? profilePictureUrl = null)
    {
        if (Provider != "local")
            throw new InvalidOperationException("Cannot link Google account to non-local user");
            
        GoogleId = googleId;
        Provider = "google";
        if (!string.IsNullOrEmpty(profilePictureUrl))
        {
            ProfilePictureUrl = profilePictureUrl;
        }
    }

    public void UnlinkGoogleAccount()
    {
        if (Provider != "google")
            throw new InvalidOperationException("Cannot unlink Google account from non-Google user");
            
        GoogleId = null;
        Provider = "local";
    }

    public bool IsGoogleUser => Provider == "google" && !string.IsNullOrEmpty(GoogleId);
    public bool IsMicrosoftUser => Provider == "microsoft" && !string.IsNullOrEmpty(MicrosoftId);

    // Microsoft OAuth methods
    public static User CreateMicrosoftUser(string microsoftId, string firstName, string lastName, EmailAddress email, string? profilePictureUrl = null)
    {
        var user = new User(firstName, lastName, email, email.Value, UserType.Entrepreneur);
        user.MicrosoftId = microsoftId;
        user.Provider = "microsoft";
        user.IsEmailConfirmed = true; // Microsoft emails are pre-verified
        user.EmailConfirmedAt = DateTime.UtcNow;
        user.ProfilePictureUrl = profilePictureUrl;
        return user;
    }

    public void LinkMicrosoftAccount(string microsoftId, string? profilePictureUrl = null)
    {
        if (Provider != "local")
            throw new InvalidOperationException("Cannot link Microsoft account to non-local user");

        MicrosoftId = microsoftId;
        Provider = "microsoft";
        if (!string.IsNullOrEmpty(profilePictureUrl))
        {
            ProfilePictureUrl = profilePictureUrl;
        }
    }

    public void UnlinkMicrosoftAccount()
    {
        if (Provider != "microsoft")
            throw new InvalidOperationException("Cannot unlink Microsoft account from non-Microsoft user");

        MicrosoftId = null;
        Provider = "local";
    }

    // Onboarding methods
    public void UpdateOnboardingProgress(int step, string? data = null)
    {
        OnboardingStep = step;
        if (data != null)
        {
            OnboardingData = data;
        }
    }

    public void CompleteOnboarding()
    {
        OnboardingCompleted = true;
    }

    public void ResetOnboarding()
    {
        OnboardingCompleted = false;
        OnboardingStep = null;
        OnboardingData = null;
    }
}
