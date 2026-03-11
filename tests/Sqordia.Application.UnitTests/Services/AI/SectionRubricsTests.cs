using FluentAssertions;
using Sqordia.Application.Services.AI;

namespace Sqordia.Application.UnitTests.Services.AI;

public class SectionRubricsTests
{
    [Theory]
    [InlineData("ExecutiveSummary", "fr")]
    [InlineData("ExecutiveSummary", "en")]
    [InlineData("MarketAnalysis", "fr")]
    [InlineData("MarketAnalysis", "en")]
    [InlineData("FinancialProjections", "fr")]
    [InlineData("CompetitiveAnalysis", "en")]
    [InlineData("BusinessModel", "fr")]
    public void GetRubric_WithKnownSection_ReturnsRubric(string sectionName, string language)
    {
        var rubric = SectionRubrics.GetRubric(sectionName, language);

        rubric.Should().NotBeNull();
        rubric!.RequiredElements.Should().NotBeEmpty();
        rubric.QualityCriteria.Should().NotBeEmpty();
        rubric.ToneGuidance.Should().NotBeNullOrWhiteSpace();
        rubric.MinWordCount.Should().BePositive();
        rubric.MaxWordCount.Should().BeGreaterThan(rubric.MinWordCount);
        rubric.AntiPatterns.Should().NotBeEmpty();
    }

    [Fact]
    public void GetRubric_WithUnknownSection_ReturnsNull()
    {
        var rubric = SectionRubrics.GetRubric("NonExistentSection", "fr");

        rubric.Should().BeNull();
    }

    [Theory]
    [InlineData("ExecutiveSummary", "fr")]
    [InlineData("MarketAnalysis", "en")]
    public void FormatForPrompt_WithKnownSection_ReturnsFormattedBlock(string sectionName, string language)
    {
        var result = SectionRubrics.FormatForPrompt(sectionName, language);

        result.Should().NotBeNullOrWhiteSpace();
        // FR uses "CRITÈRES DE QUALITÉ", EN uses "QUALITY CRITERIA"
        (result!.Contains("QUALITY CRITERIA") || result.Contains("CRITÈRES DE QUALITÉ")).Should().BeTrue();
        (result.Contains("Required elements") || result.Contains("Éléments requis")).Should().BeTrue();
    }

    [Fact]
    public void FormatForPrompt_WithUnknownSection_ReturnsEmpty()
    {
        var result = SectionRubrics.FormatForPrompt("FakeSection", "en");

        result.Should().BeNullOrEmpty();
    }

    [Fact]
    public void GetRubric_FrenchAndEnglish_HaveDifferentContent()
    {
        var fr = SectionRubrics.GetRubric("ExecutiveSummary", "fr");
        var en = SectionRubrics.GetRubric("ExecutiveSummary", "en");

        fr.Should().NotBeNull();
        en.Should().NotBeNull();
        fr!.ToneGuidance.Should().NotBe(en!.ToneGuidance);
    }
}
