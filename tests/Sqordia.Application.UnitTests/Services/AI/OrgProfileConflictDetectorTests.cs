using FluentAssertions;
using Sqordia.Application.Services.AI;

namespace Sqordia.Application.UnitTests.Services.AI;

public class OrgProfileConflictDetectorTests
{
    [Fact]
    public void DetectConflicts_WithNoConflicts_ReturnsEmpty()
    {
        var answers = new Dictionary<int, string>
        {
            [5] = "Technology software solutions",
            [10] = "5-10 employees",
            [13] = "Pre-revenue startup"
        };

        var conflicts = OrgProfileConflictDetector.DetectConflicts(
            orgIndustry: "Technology",
            orgTeamSize: "5-10",
            orgStage: "Startup",
            answers: answers);

        conflicts.Should().BeEmpty();
    }

    [Fact]
    public void DetectConflicts_WithNullOrgData_ReturnsEmpty()
    {
        var answers = new Dictionary<int, string>
        {
            [5] = "Technology"
        };

        var conflicts = OrgProfileConflictDetector.DetectConflicts(
            orgIndustry: null,
            orgTeamSize: null,
            orgStage: null,
            answers: answers);

        conflicts.Should().BeEmpty();
    }

    [Fact]
    public void DetectConflicts_WithEmptyAnswers_ReturnsEmpty()
    {
        var conflicts = OrgProfileConflictDetector.DetectConflicts(
            orgIndustry: "Technology",
            orgTeamSize: "10-50",
            orgStage: "Growth",
            answers: new Dictionary<int, string>());

        conflicts.Should().BeEmpty();
    }

    [Fact]
    public void DetectConflicts_WithSoloVsTeamConflict_ReturnsConflict()
    {
        var answers = new Dictionary<int, string>
        {
            [10] = "We have a team of 5 people including my co-founder and 3 employees"
        };

        var conflicts = OrgProfileConflictDetector.DetectConflicts(
            orgIndustry: null,
            orgTeamSize: "solo",
            orgStage: null,
            answers: answers);

        conflicts.Should().NotBeEmpty();
        conflicts.Should().Contain(c => c.Field == "TeamSize");
    }

    [Fact]
    public void DetectConflicts_IdeaStageWithRevenue_ReturnsConflict()
    {
        var answers = new Dictionary<int, string>
        {
            [13] = "We currently have $50,000 in revenue from our first clients"
        };

        var conflicts = OrgProfileConflictDetector.DetectConflicts(
            orgIndustry: null,
            orgTeamSize: null,
            orgStage: "idea",
            answers: answers);

        conflicts.Should().NotBeEmpty();
        conflicts.Should().Contain(c => c.Field == "BusinessStage");
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("en")]
    public void FormatForPrompt_WithConflicts_ReturnsFormattedBlock(string language)
    {
        var conflicts = new List<OrgProfileConflictDetector.ConflictWarning>
        {
            new("Industry", "Technology", "Healthcare services", "Mismatch detected")
        };

        var result = OrgProfileConflictDetector.FormatForPrompt(conflicts, language);

        result.Should().NotBeNullOrWhiteSpace();
        (result!.Contains("DATA QUALITY") || result.Contains("QUALITÉ")).Should().BeTrue();
    }

    [Fact]
    public void FormatForPrompt_WithNoConflicts_ReturnsNull()
    {
        var result = OrgProfileConflictDetector.FormatForPrompt(
            new List<OrgProfileConflictDetector.ConflictWarning>(), "en");

        result.Should().BeNull();
    }
}
