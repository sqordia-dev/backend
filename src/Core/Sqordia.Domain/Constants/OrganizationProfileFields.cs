namespace Sqordia.Domain.Constants;

public static class OrganizationProfileFields
{
    public const string CompanyName = "companyName";
    public const string Industry = "industry";
    public const string Sector = "sector";
    public const string TeamSize = "teamSize";
    public const string FundingStatus = "fundingStatus";
    public const string TargetMarket = "targetMarket";
    public const string BusinessStage = "businessStage";
    public const string GoalsJson = "goalsJson";
    public const string City = "city";
    public const string Province = "province";
    public const string Country = "country";

    public static readonly HashSet<string> AllKeys = new()
    {
        CompanyName, Industry, Sector, TeamSize, FundingStatus,
        TargetMarket, BusinessStage, GoalsJson, City, Province, Country
    };
}
