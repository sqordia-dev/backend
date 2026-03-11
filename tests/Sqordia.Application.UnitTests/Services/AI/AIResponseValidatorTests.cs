using FluentAssertions;
using Sqordia.Application.Services.AI;

namespace Sqordia.Application.UnitTests.Services.AI;

public class AIResponseValidatorTests
{
    #region ValidateSectionContent

    [Fact]
    public void ValidateSectionContent_WithValidContent_ReturnsValid()
    {
        var content = new string('A', 200) + " This is a valid business plan section with enough meaningful content.";

        var result = AIResponseValidator.ValidateSectionContent(content, "MarketAnalysis");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSectionContent_WithEmptyContent_ReturnsInvalid()
    {
        var result = AIResponseValidator.ValidateSectionContent("", "MarketAnalysis");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("empty"));
    }

    [Fact]
    public void ValidateSectionContent_WithShortContent_ReturnsInvalid()
    {
        var result = AIResponseValidator.ValidateSectionContent("Too short", "MarketAnalysis");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("too short"));
    }

    [Theory]
    [InlineData("I cannot generate this content because it requires real data.")]
    [InlineData("As an AI, I am unable to provide accurate financial projections.")]
    [InlineData("Je ne peux pas fournir ces informations sans données réelles.")]
    public void ValidateSectionContent_WithAIRefusal_ReturnsInvalid(string content)
    {
        // Pad to meet minimum length
        var paddedContent = content + new string(' ', 100);

        var result = AIResponseValidator.ValidateSectionContent(paddedContent, "FinancialProjections");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("refusal"));
    }

    [Fact]
    public void ValidateSectionContent_WithJsonArtifacts_ReturnsInvalid()
    {
        var content = """{"prose": [{"id": "1", "content": "Some content"}], "visualElements": []}""";

        var result = AIResponseValidator.ValidateSectionContent(content, "ExecutiveSummary");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("JSON"));
    }

    #endregion

    #region ValidateQualityReport

    [Fact]
    public void ValidateQualityReport_WithValidJson_ReturnsValid()
    {
        var json = """
        {
            "coherenceScore": 85,
            "bankReadinessScore": 72,
            "synthesizedExecutiveSummary": "This is a valid executive summary with enough content."
        }
        """;

        var result = AIResponseValidator.ValidateQualityReport(json);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateQualityReport_WithOutOfRangeScore_ReturnsInvalid()
    {
        var json = """
        {
            "coherenceScore": 150,
            "bankReadinessScore": 72,
            "synthesizedExecutiveSummary": "Summary"
        }
        """;

        var result = AIResponseValidator.ValidateQualityReport(json);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("coherenceScore") && e.Contains("out of range"));
    }

    [Fact]
    public void ValidateQualityReport_WithMissingFields_ReturnsInvalid()
    {
        var json = """{"coherenceScore": 80}""";

        var result = AIResponseValidator.ValidateQualityReport(json);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void ValidateQualityReport_WithInvalidJson_ReturnsInvalid()
    {
        var result = AIResponseValidator.ValidateQualityReport("not json at all");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invalid JSON"));
    }

    #endregion

    #region ValidateGenerationPlan

    [Fact]
    public void ValidateGenerationPlan_WithValidJson_ReturnsValid()
    {
        var json = """
        {
            "overallTheme": "Innovation in HR tech",
            "narrativeArc": "Problem -> Solution -> Market -> Financials",
            "sectionGuidance": {
                "ExecutiveSummary": "Focus on key metrics"
            }
        }
        """;

        var result = AIResponseValidator.ValidateGenerationPlan(json);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateGenerationPlan_WithMissingTheme_ReturnsInvalid()
    {
        var json = """
        {
            "narrativeArc": "Some arc",
            "sectionGuidance": {}
        }
        """;

        var result = AIResponseValidator.ValidateGenerationPlan(json);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("overallTheme"));
    }

    #endregion
}
