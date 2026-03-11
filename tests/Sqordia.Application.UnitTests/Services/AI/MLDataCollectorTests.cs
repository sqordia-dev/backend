using FluentAssertions;
using Sqordia.Infrastructure.Services;

namespace Sqordia.Application.UnitTests.Services.AI;

public class MLDataCollectorTests
{
    [Fact]
    public void ComputeWordLevelDiff_IdenticalTexts_ReturnsZero()
    {
        var (distance, ratio) = MLDataCollector.ComputeWordLevelDiff(
            "The market is growing rapidly",
            "The market is growing rapidly");

        distance.Should().Be(0);
        ratio.Should().Be(0.0);
    }

    [Fact]
    public void ComputeWordLevelDiff_CompletelyDifferent_ReturnsHighRatio()
    {
        var (distance, ratio) = MLDataCollector.ComputeWordLevelDiff(
            "The market is growing rapidly",
            "Le marché est en croissance rapide");

        ratio.Should().BeGreaterThan(0.8);
    }

    [Fact]
    public void ComputeWordLevelDiff_PartialEdit_ReturnsModerateRatio()
    {
        var (distance, ratio) = MLDataCollector.ComputeWordLevelDiff(
            "The market is growing rapidly with strong potential",
            "The market is growing steadily with moderate potential");

        ratio.Should().BeGreaterThan(0.1);
        ratio.Should().BeLessThan(0.6);
    }

    [Fact]
    public void ComputeWordLevelDiff_EmptyOriginal_ReturnsFullRatio()
    {
        var (distance, ratio) = MLDataCollector.ComputeWordLevelDiff(
            "",
            "New content added by user");

        ratio.Should().Be(1.0);
    }

    [Fact]
    public void ComputeWordLevelDiff_EmptyEdited_ReturnsFullRatio()
    {
        var (distance, ratio) = MLDataCollector.ComputeWordLevelDiff(
            "Original content that was deleted",
            "");

        ratio.Should().Be(1.0);
    }

    [Fact]
    public void ComputeWordLevelDiff_BothEmpty_ReturnsZero()
    {
        var (distance, ratio) = MLDataCollector.ComputeWordLevelDiff("", "");

        distance.Should().Be(0);
        ratio.Should().Be(0.0);
    }

    [Fact]
    public void ComputeWordLevelDiff_SingleWordChange_ReturnsSmallRatio()
    {
        var (distance, ratio) = MLDataCollector.ComputeWordLevelDiff(
            "The market analysis shows strong growth in the technology sector this year",
            "The market analysis shows moderate growth in the technology sector this year");

        distance.Should().Be(1);
        ratio.Should().BeLessThan(0.15);
    }

    [Fact]
    public void ComputeWordLevelDiff_RatioNeverExceedsOne()
    {
        var (_, ratio) = MLDataCollector.ComputeWordLevelDiff(
            "a b c",
            "x y z w v u t s r q p o n m l k j i h g f e d");

        ratio.Should().BeLessThanOrEqualTo(1.0);
    }
}
