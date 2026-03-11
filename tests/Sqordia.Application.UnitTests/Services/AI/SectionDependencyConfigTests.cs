using FluentAssertions;
using Sqordia.Application.Services.AI;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.UnitTests.Services.AI;

public class SectionDependencyConfigTests
{
    [Theory]
    [InlineData(BusinessPlanType.BusinessPlan)]
    [InlineData(BusinessPlanType.StrategicPlan)]
    [InlineData(BusinessPlanType.LeanCanvas)]
    public void GetTiersForPlanType_ReturnsNonEmptyTiers(BusinessPlanType planType)
    {
        var tiers = SectionDependencyConfig.GetTiersForPlanType(planType);

        tiers.Should().NotBeEmpty();
        tiers.Should().AllSatisfy(tier => tier.Should().NotBeEmpty());
    }

    [Fact]
    public void GetTiersForPlanType_BusinessPlan_HasMultipleTiers()
    {
        var tiers = SectionDependencyConfig.GetTiersForPlanType(BusinessPlanType.BusinessPlan);

        tiers.Should().HaveCountGreaterThanOrEqualTo(2,
            because: "sections should be grouped into dependency tiers");
    }

    [Fact]
    public void GetTiersForPlanType_EachSectionAppearsOnce()
    {
        var tiers = SectionDependencyConfig.GetTiersForPlanType(BusinessPlanType.BusinessPlan);
        var allSections = tiers.SelectMany(t => t).ToList();

        allSections.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GetCrossReferences_WithGeneratedSections_ReturnsRelevantDependencies()
    {
        var generatedKeys = new[] { "MarketAnalysis", "CompetitiveAnalysis", "BusinessModel" };

        var refs = SectionDependencyConfig.GetCrossReferences("MarketingStrategy", generatedKeys);

        // MarketingStrategy depends on MarketAnalysis, CompetitiveAnalysis, BusinessModel
        refs.Should().NotBeEmpty();
        refs.Should().Contain("MarketAnalysis");
    }

    [Fact]
    public void GetCrossReferences_ExecutiveSummary_ReturnsAllOtherSections()
    {
        var generatedKeys = new[] { "MarketAnalysis", "CompetitiveAnalysis", "ExecutiveSummary" };

        var refs = SectionDependencyConfig.GetCrossReferences("ExecutiveSummary", generatedKeys);

        refs.Should().NotContain("ExecutiveSummary");
        refs.Should().Contain("MarketAnalysis");
        refs.Should().Contain("CompetitiveAnalysis");
    }

    [Fact]
    public void GetAvailableSections_BusinessPlan_ReturnsNonEmpty()
    {
        var sections = SectionDependencyConfig.GetAvailableSections("BusinessPlan");

        sections.Should().NotBeEmpty();
        sections.Should().Contain("ExecutiveSummary");
        sections.Should().Contain("FinancialProjections");
    }

    [Fact]
    public void GetAvailableSections_StrategicPlan_IncludesObnlSections()
    {
        var sections = SectionDependencyConfig.GetAvailableSections("StrategicPlan");

        sections.Should().Contain("MissionStatement");
        sections.Should().Contain("SocialImpact");
        sections.Should().Contain("GrantStrategy");
    }

    [Fact]
    public void GetAvailableSections_BusinessPlan_IncludesExitStrategy()
    {
        var sections = SectionDependencyConfig.GetAvailableSections("BusinessPlan");

        sections.Should().Contain("ExitStrategy");
    }
}
