namespace Sqordia.Domain.Enums;

/// <summary>
/// Defines the type of organization
/// </summary>
public enum OrganizationType
{
    /// <summary>
    /// Startup or SME (for-profit company in growth)
    /// </summary>
    Startup = 0,
    
    /// <summary>
    /// OBNL (non-profit organization seeking funding/grants)
    /// </summary>
    OBNL = 1,
    
    /// <summary>
    /// Consulting or mentoring firm
    /// </summary>
    ConsultingFirm = 2,
    
    /// <summary>
    /// Company (legacy value, maps to Startup)
    /// </summary>
    Company = 3
}

