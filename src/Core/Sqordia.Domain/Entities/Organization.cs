using Sqordia.Domain.Common;
using Sqordia.Domain.Enums;

namespace Sqordia.Domain.Entities;

public class Organization : BaseAuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Website { get; private set; }
    public string? LogoUrl { get; private set; }
    public OrganizationType OrganizationType { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }
    
    // Business Context (collected at onboarding, editable via profile)
    public string? Industry { get; private set; }
    public string? Sector { get; private set; }
    public string? TeamSize { get; private set; }
    public string? FundingStatus { get; private set; }
    public string? TargetMarket { get; private set; }
    public string? BusinessStage { get; private set; }
    public string? GoalsJson { get; private set; }
    public string? City { get; private set; }
    public string? Province { get; private set; }
    public string? Country { get; private set; }
    public int ProfileCompletenessScore { get; private set; }

    // Settings
    public int MaxMembers { get; private set; }
    public bool AllowMemberInvites { get; private set; }
    public bool RequireEmailVerification { get; private set; }
    
    // Navigation properties
    public ICollection<OrganizationMember> Members { get; private set; } = new List<OrganizationMember>();
    
    private Organization() { } // EF Core constructor
    
    public Organization(string name, OrganizationType organizationType, string? description = null, string? website = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        OrganizationType = organizationType;
        Description = description;
        Website = website;
        IsActive = true;
        MaxMembers = 10; // Default limit
        AllowMemberInvites = true;
        RequireEmailVerification = true;
        Created = DateTime.UtcNow;
    }
    
    public void UpdateDetails(string name, string? description, string? website)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        Website = website;
    }
    
    public void UpdateLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
    }

    public void UpdateOrganizationType(OrganizationType organizationType)
    {
        OrganizationType = organizationType;
    }
    
    public void UpdateSettings(int maxMembers, bool allowMemberInvites, bool requireEmailVerification)
    {
        if (maxMembers < 1)
            throw new ArgumentException("Max members must be at least 1", nameof(maxMembers));
            
        MaxMembers = maxMembers;
        AllowMemberInvites = allowMemberInvites;
        RequireEmailVerification = requireEmailVerification;
    }
    
    public void Deactivate()
    {
        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
    }
    
    public void Reactivate()
    {
        IsActive = true;
        DeactivatedAt = null;
    }
    
    public bool CanAddMoreMembers()
    {
        return Members.Count(m => m.IsActive) < MaxMembers;
    }

    public void UpdateBusinessContext(
        string? industry,
        string? sector,
        string? teamSize,
        string? fundingStatus,
        string? targetMarket,
        string? businessStage,
        string? goalsJson,
        string? city,
        string? province,
        string? country)
    {
        Industry = industry;
        Sector = sector;
        TeamSize = teamSize;
        FundingStatus = fundingStatus;
        TargetMarket = targetMarket;
        BusinessStage = businessStage;
        GoalsJson = goalsJson;
        City = city;
        Province = province;
        Country = country;
        RecalculateProfileCompleteness();
    }

    public void SetProfileField(string fieldKey, string? value)
    {
        switch (fieldKey)
        {
            case "industry": Industry = value; break;
            case "sector": Sector = value; break;
            case "teamSize": TeamSize = value; break;
            case "fundingStatus": FundingStatus = value; break;
            case "targetMarket": TargetMarket = value; break;
            case "businessStage": BusinessStage = value; break;
            case "goalsJson": GoalsJson = value; break;
            case "city": City = value; break;
            case "province": Province = value; break;
            case "country": Country = value; break;
            case "companyName": Name = value ?? Name; break;
            default: throw new ArgumentException($"Unknown profile field key: {fieldKey}", nameof(fieldKey));
        }
        RecalculateProfileCompleteness();
    }

    public string? GetProfileFieldValue(string fieldKey)
    {
        return fieldKey switch
        {
            "industry" => Industry,
            "sector" => Sector,
            "teamSize" => TeamSize,
            "fundingStatus" => FundingStatus,
            "targetMarket" => TargetMarket,
            "businessStage" => BusinessStage,
            "goalsJson" => GoalsJson,
            "city" => City,
            "province" => Province,
            "country" => Country,
            "companyName" => Name,
            _ => throw new ArgumentException($"Unknown profile field key: {fieldKey}", nameof(fieldKey))
        };
    }

    public void RecalculateProfileCompleteness()
    {
        var fields = new[] { Industry, Sector, TeamSize, FundingStatus, TargetMarket, BusinessStage, GoalsJson, City, Province, Country };
        var filled = fields.Count(f => !string.IsNullOrWhiteSpace(f));
        ProfileCompletenessScore = (int)Math.Round(filled / (double)fields.Length * 100);
    }
}

