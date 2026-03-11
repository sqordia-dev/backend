using FluentAssertions;
using Sqordia.Application.Common.Constants;

namespace Sqordia.Application.UnitTests.Services.AI;

public class SectionNamesTests
{
    [Theory]
    [InlineData("executive-summary", "ExecutiveSummary")]
    [InlineData("market-analysis", "MarketAnalysis")]
    [InlineData("swot-analysis", "SwotAnalysis")]
    [InlineData("operations-plan", "OperationsPlan")]
    [InlineData("sustainability-plan", "SustainabilityPlan")]
    public void ToPascalCase_ConvertsKebabCaseCorrectly(string input, string expected)
    {
        SectionNames.ToPascalCase(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("Solution")]
    [InlineData("solution")]
    public void ToPascalCase_SingleWord_ReturnsAsIs(string input)
    {
        SectionNames.ToPascalCase(input).Should().Be(input);
    }

    [Fact]
    public void ToPascalCase_EmptyString_ReturnsEmpty()
    {
        SectionNames.ToPascalCase("").Should().BeEmpty();
    }

    [Theory]
    [InlineData("executive-summary")]
    [InlineData("market-analysis")]
    [InlineData("mission-statement")]
    [InlineData("solution")]
    public void IsValid_RecognizesValidSections(string sectionName)
    {
        SectionNames.IsValid(sectionName).Should().BeTrue();
    }

    [Theory]
    [InlineData("not-a-section")]
    [InlineData("foobar")]
    [InlineData("")]
    public void IsValid_RejectsInvalidSections(string sectionName)
    {
        SectionNames.IsValid(sectionName).Should().BeFalse();
    }

    [Fact]
    public void AllSections_ContainsBothStandardAndObnl()
    {
        SectionNames.AllSections.Should().Contain(SectionNames.ExecutiveSummary);
        SectionNames.AllSections.Should().Contain(SectionNames.MissionStatement);
    }

    [Fact]
    public void BusinessPlanSections_DoesNotContainObnl()
    {
        SectionNames.BusinessPlanSections.Should().NotContain(SectionNames.MissionStatement);
        SectionNames.BusinessPlanSections.Should().NotContain(SectionNames.SocialImpact);
    }

    [Fact]
    public void ObnlSections_DoesNotContainStandard()
    {
        SectionNames.ObnlSections.Should().NotContain(SectionNames.ExecutiveSummary);
        SectionNames.ObnlSections.Should().NotContain(SectionNames.MarketAnalysis);
    }
}
