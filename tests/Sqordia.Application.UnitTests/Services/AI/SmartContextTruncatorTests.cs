using FluentAssertions;
using Sqordia.Application.Services.AI;

namespace Sqordia.Application.UnitTests.Services.AI;

public class SmartContextTruncatorTests
{
    [Fact]
    public void TruncateWithRelevance_ShortContent_ReturnsFullContent()
    {
        var content = "Short paragraph about market analysis.";

        var result = SmartContextTruncator.TruncateWithRelevance(content, "MarketAnalysis", 1000, "en");

        result.Should().Be(content);
    }

    [Fact]
    public void TruncateWithRelevance_LongContent_TruncatesToLimit()
    {
        var paragraphs = string.Join("\n\n",
            Enumerable.Range(1, 20).Select(i =>
                $"Paragraph {i}: The market analysis reveals significant growth trends with CAGR of {i}% in the technology sector."));

        var result = SmartContextTruncator.TruncateWithRelevance(paragraphs, "MarketAnalysis", 300, "en");

        result.Length.Should().BeLessThanOrEqualTo(300 + 50); // Allow small buffer for paragraph boundaries
    }

    [Fact]
    public void TruncateWithRelevance_PrioritizesRelevantParagraphs()
    {
        var content = """
            This paragraph talks about team building and HR processes.

            The market size is estimated at $2.3B with a CAGR of 11.2%. Customer segmentation reveals three key segments.

            Our office is located in downtown Montreal with great public transit access.
            """;

        var result = SmartContextTruncator.TruncateWithRelevance(content, "MarketAnalysis", 200, "en");

        // The market-related paragraph should be prioritized
        result.Should().Contain("market");
    }

    [Fact]
    public void TruncateWithRelevance_NullContent_ReturnsNull()
    {
        var result = SmartContextTruncator.TruncateWithRelevance(null!, "MarketAnalysis", 500, "en");

        result.Should().BeNull();
    }

    [Fact]
    public void SummarizeForReview_PreservesNumericData()
    {
        var content = """
            Revenue projections show $720K by year 3.
            The team is passionate about innovation.
            Break-even is reached at month 14 with 12 active clients.
            We have a great company culture.
            Market size: $340M addressable market in Quebec.
            """;

        var result = SmartContextTruncator.SummarizeForReview(content, 300);

        // Numeric content should be prioritized
        result.Should().Contain("$");
    }

    [Fact]
    public void SummarizeForReview_ShortContent_ReturnsFullContent()
    {
        var content = "Short content with $100K revenue.";

        var result = SmartContextTruncator.SummarizeForReview(content, 500);

        result.Should().Be(content);
    }
}
