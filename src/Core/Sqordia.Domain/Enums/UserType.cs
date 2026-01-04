namespace Sqordia.Domain.Enums;

/// <summary>
/// Defines the type of user/professional profile
/// </summary>
public enum UserType
{
    /// <summary>
    /// Entrepreneur (startups, SMEs in growth)
    /// </summary>
    Entrepreneur = 0,
    
    /// <summary>
    /// OBNL representative (non-profit organizations seeking grants/subsidies)
    /// </summary>
    OBNL = 1,
    
    /// <summary>
    /// Consultant/Mentor who accompanies entrepreneurs and OBNLs
    /// </summary>
    Consultant = 2
}

