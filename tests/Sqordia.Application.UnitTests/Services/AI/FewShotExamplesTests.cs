using FluentAssertions;
using Sqordia.Application.Services.AI;

namespace Sqordia.Application.UnitTests.Services.AI;

public class FewShotExamplesTests
{
    [Theory]
    [InlineData("MarketAnalysis", "fr")]
    [InlineData("MarketAnalysis", "en")]
    [InlineData("FinancialProjections", "fr")]
    [InlineData("ExecutiveSummary", "en")]
    [InlineData("CompetitiveAnalysis", "fr")]
    [InlineData("BusinessModel", "en")]
    public void GetExamples_WithKnownSection_ReturnsPair(string sectionName, string language)
    {
        var examples = FewShotExamples.GetExamples(sectionName, language);

        examples.Should().NotBeNull();
        examples!.GoodExample.Should().NotBeNullOrWhiteSpace();
        examples.BadExample.Should().NotBeNullOrWhiteSpace();
        examples.GoodExample.Length.Should().BeGreaterThan(examples.BadExample.Length,
            because: "good examples should be more detailed than bad ones");
    }

    [Fact]
    public void GetExamples_WithUnknownSection_ReturnsNull()
    {
        var examples = FewShotExamples.GetExamples("UnknownSection", "fr");

        examples.Should().BeNull();
    }

    [Theory]
    [InlineData("MarketAnalysis", "fr")]
    [InlineData("ExecutiveSummary", "en")]
    public void FormatForPrompt_WithKnownSection_ReturnsFormattedBlock(string sectionName, string language)
    {
        var result = FewShotExamples.FormatForPrompt(sectionName, language);

        result.Should().NotBeNullOrWhiteSpace();
        // FR uses "BON EXEMPLE"/"MAUVAIS EXEMPLE", EN uses "GOOD EXAMPLE"/"BAD EXAMPLE"
        (result!.Contains("GOOD EXAMPLE") || result.Contains("BON EXEMPLE")).Should().BeTrue();
        (result.Contains("BAD EXAMPLE") || result.Contains("MAUVAIS EXEMPLE")).Should().BeTrue();
    }

    [Fact]
    public void FormatForPrompt_WithUnknownSection_ReturnsEmpty()
    {
        var result = FewShotExamples.FormatForPrompt("FakeSection", "en");

        result.Should().BeNullOrEmpty();
    }

    [Fact]
    public void GetExamples_FrenchAndEnglish_HaveDifferentContent()
    {
        var fr = FewShotExamples.GetExamples("MarketAnalysis", "fr");
        var en = FewShotExamples.GetExamples("MarketAnalysis", "en");

        fr.Should().NotBeNull();
        en.Should().NotBeNull();
        fr!.GoodExample.Should().NotBe(en!.GoodExample);
    }
}
