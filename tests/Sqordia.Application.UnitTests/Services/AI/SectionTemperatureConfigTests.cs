using FluentAssertions;
using Sqordia.Application.Services.AI;

namespace Sqordia.Application.UnitTests.Services.AI;

public class SectionTemperatureConfigTests
{
    [Theory]
    [InlineData("FinancialProjections", 0.3f)]
    [InlineData("FundingRequirements", 0.3f)]
    [InlineData("RiskAnalysis", 0.4f)]
    [InlineData("MarketAnalysis", 0.5f)]
    [InlineData("ExecutiveSummary", 0.6f)]
    [InlineData("BrandingStrategy", 0.7f)]
    public void GetTemperature_WithKnownSection_ReturnsExpectedTemperature(string sectionName, float expected)
    {
        var result = SectionTemperatureConfig.GetTemperature(sectionName);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetTemperature_WithUnknownSection_ReturnsDefault()
    {
        var result = SectionTemperatureConfig.GetTemperature("UnknownSection");

        result.Should().Be(0.6f);
    }

    [Fact]
    public void GetTemperature_WithOverride_ReturnsOverrideValue()
    {
        var result = SectionTemperatureConfig.GetTemperature("FinancialProjections", 0.9f);

        result.Should().Be(0.9f);
    }

    [Fact]
    public void GetTemperature_AllFinancialSections_HaveLowTemperature()
    {
        var financialSections = new[] { "FinancialProjections", "FundingRequirements" };

        foreach (var section in financialSections)
        {
            SectionTemperatureConfig.GetTemperature(section).Should().BeLessThanOrEqualTo(0.4f,
                because: $"{section} requires precision");
        }
    }

    [Fact]
    public void GetTemperature_CreativeSections_HaveHigherTemperature()
    {
        var creativeSections = new[] { "BrandingStrategy", "MarketingStrategy", "SocialImpact" };

        foreach (var section in creativeSections)
        {
            SectionTemperatureConfig.GetTemperature(section).Should().BeGreaterThanOrEqualTo(0.6f,
                because: $"{section} benefits from creativity");
        }
    }
}
