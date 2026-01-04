using AutoFixture;
using FluentAssertions;
using Sqordia.Domain.Entities;
using Xunit;

namespace Sqordia.Domain.UnitTests.Entities;

public class GrantApplicationTests
{
    private readonly Fixture _fixture;

    public GrantApplicationTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var application = new GrantApplication();

        // Assert
        application.Id.Should().NotBeEmpty();
        application.OBNLBusinessPlanId.Should().Be(Guid.Empty);
        application.GrantName.Should().BeEmpty();
        application.GrantingOrganization.Should().BeEmpty();
        application.GrantType.Should().BeEmpty();
        application.RequestedAmount.Should().Be(0);
        application.MatchingFunds.Should().Be(0);
        application.ProjectDescription.Should().BeEmpty();
        application.Objectives.Should().BeEmpty();
        application.ExpectedOutcomes.Should().BeEmpty();
        application.TargetPopulation.Should().BeEmpty();
        application.GeographicScope.Should().BeEmpty();
        application.Timeline.Should().BeEmpty();
        application.BudgetBreakdown.Should().BeEmpty();
        application.EvaluationPlan.Should().BeEmpty();
        application.SustainabilityPlan.Should().BeEmpty();
        application.ApplicationDeadline.Should().Be(default(DateTime));
        application.SubmissionDate.Should().Be(default(DateTime));
        application.Status.Should().BeEmpty();
        application.Decision.Should().BeEmpty();
        application.Notes.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueId()
    {
        // Act
        var application1 = new GrantApplication();
        var application2 = new GrantApplication();

        // Assert
        application1.Id.Should().NotBeEmpty();
        application2.Id.Should().NotBeEmpty();
        application1.Id.Should().NotBe(application2.Id);
    }

    [Fact]
    public void SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var application = new GrantApplication();
        var planId = Guid.NewGuid();
        var grantName = "Community Development Grant";
        var grantingOrganization = "Community Foundation";
        var grantType = "Program Grant";
        var requestedAmount = 50000m;
        var matchingFunds = 10000m;
        var projectDescription = "Community development project";
        var objectives = "Improve community services";
        var expectedOutcomes = "Increased community engagement";
        var targetPopulation = "Local residents";
        var geographicScope = "Local community";
        var timeline = "12 months";
        var budgetBreakdown = "Detailed budget breakdown";
        var evaluationPlan = "Evaluation methodology";
        var sustainabilityPlan = "Long-term sustainability plan";
        var applicationDeadline = DateTime.UtcNow.AddMonths(1);
        var submissionDate = DateTime.UtcNow;
        var status = "Draft";
        var decision = "Pending";
        var notes = "Grant application notes";

        // Act
        application.OBNLBusinessPlanId = planId;
        application.GrantName = grantName;
        application.GrantingOrganization = grantingOrganization;
        application.GrantType = grantType;
        application.RequestedAmount = requestedAmount;
        application.MatchingFunds = matchingFunds;
        application.ProjectDescription = projectDescription;
        application.Objectives = objectives;
        application.ExpectedOutcomes = expectedOutcomes;
        application.TargetPopulation = targetPopulation;
        application.GeographicScope = geographicScope;
        application.Timeline = timeline;
        application.BudgetBreakdown = budgetBreakdown;
        application.EvaluationPlan = evaluationPlan;
        application.SustainabilityPlan = sustainabilityPlan;
        application.ApplicationDeadline = applicationDeadline;
        application.SubmissionDate = submissionDate;
        application.Status = status;
        application.Decision = decision;
        application.Notes = notes;

        // Assert
        application.OBNLBusinessPlanId.Should().Be(planId);
        application.GrantName.Should().Be(grantName);
        application.GrantingOrganization.Should().Be(grantingOrganization);
        application.GrantType.Should().Be(grantType);
        application.RequestedAmount.Should().Be(requestedAmount);
        application.MatchingFunds.Should().Be(matchingFunds);
        application.ProjectDescription.Should().Be(projectDescription);
        application.Objectives.Should().Be(objectives);
        application.ExpectedOutcomes.Should().Be(expectedOutcomes);
        application.TargetPopulation.Should().Be(targetPopulation);
        application.GeographicScope.Should().Be(geographicScope);
        application.Timeline.Should().Be(timeline);
        application.BudgetBreakdown.Should().Be(budgetBreakdown);
        application.EvaluationPlan.Should().Be(evaluationPlan);
        application.SustainabilityPlan.Should().Be(sustainabilityPlan);
        application.ApplicationDeadline.Should().Be(applicationDeadline);
        application.SubmissionDate.Should().Be(submissionDate);
        application.Status.Should().Be(status);
        application.Decision.Should().Be(decision);
        application.Notes.Should().Be(notes);
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("Submitted")]
    [InlineData("Under Review")]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    [InlineData("Withdrawn")]
    public void Status_WithValidStatuses_ShouldAcceptAllStatuses(string status)
    {
        // Arrange
        var application = new GrantApplication();

        // Act
        application.Status = status;

        // Assert
        application.Status.Should().Be(status);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    [InlineData("Under Review")]
    [InlineData("Withdrawn")]
    public void Decision_WithValidDecisions_ShouldAcceptAllDecisions(string decision)
    {
        // Arrange
        var application = new GrantApplication();

        // Act
        application.Decision = decision;

        // Assert
        application.Decision.Should().Be(decision);
    }

    [Theory]
    [InlineData("Program Grant")]
    [InlineData("Capital Grant")]
    [InlineData("Operating Grant")]
    [InlineData("Research Grant")]
    [InlineData("Capacity Building Grant")]
    public void GrantType_WithValidTypes_ShouldAcceptAllTypes(string grantType)
    {
        // Arrange
        var application = new GrantApplication();

        // Act
        application.GrantType = grantType;

        // Assert
        application.GrantType.Should().Be(grantType);
    }

    [Fact]
    public void RequestedAmount_WithNegativeValue_ShouldAllowNegativeValues()
    {
        // Arrange
        var application = new GrantApplication();
        var negativeAmount = -1000m;

        // Act
        application.RequestedAmount = negativeAmount;

        // Assert
        application.RequestedAmount.Should().Be(negativeAmount);
    }

    [Fact]
    public void MatchingFunds_WithNegativeValue_ShouldAllowNegativeValues()
    {
        // Arrange
        var application = new GrantApplication();
        var negativeAmount = -500m;

        // Act
        application.MatchingFunds = negativeAmount;

        // Assert
        application.MatchingFunds.Should().Be(negativeAmount);
    }

    [Fact]
    public void ApplicationDeadline_WithFutureDate_ShouldAcceptFutureDate()
    {
        // Arrange
        var application = new GrantApplication();
        var futureDate = DateTime.UtcNow.AddMonths(6);

        // Act
        application.ApplicationDeadline = futureDate;

        // Assert
        application.ApplicationDeadline.Should().Be(futureDate);
    }

    [Fact]
    public void SubmissionDate_WithPastDate_ShouldAcceptPastDate()
    {
        // Arrange
        var application = new GrantApplication();
        var pastDate = DateTime.UtcNow.AddDays(-30);

        // Act
        application.SubmissionDate = pastDate;

        // Assert
        application.SubmissionDate.Should().Be(pastDate);
    }

    [Fact]
    public void CreateGrantApplication_WithAllProperties_ShouldCreateCompleteApplication()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var grantName = "Community Development Grant";
        var grantingOrganization = "Community Foundation";
        var grantType = "Program Grant";
        var requestedAmount = 75000m;
        var matchingFunds = 15000m;
        var projectDescription = "Comprehensive community development project";
        var objectives = "Improve community services and engagement";
        var expectedOutcomes = "Increased community engagement and service delivery";
        var targetPopulation = "Local residents and community members";
        var geographicScope = "Local community and surrounding areas";
        var timeline = "18 months";
        var budgetBreakdown = "Detailed budget breakdown with line items";
        var evaluationPlan = "Comprehensive evaluation methodology";
        var sustainabilityPlan = "Long-term sustainability and impact plan";
        var applicationDeadline = DateTime.UtcNow.AddMonths(2);
        var submissionDate = DateTime.UtcNow.AddDays(-7);
        var status = "Under Review";
        var decision = "Pending";
        var notes = "Comprehensive grant application with detailed project plan";

        // Act
        var application = new GrantApplication
        {
            OBNLBusinessPlanId = planId,
            GrantName = grantName,
            GrantingOrganization = grantingOrganization,
            GrantType = grantType,
            RequestedAmount = requestedAmount,
            MatchingFunds = matchingFunds,
            ProjectDescription = projectDescription,
            Objectives = objectives,
            ExpectedOutcomes = expectedOutcomes,
            TargetPopulation = targetPopulation,
            GeographicScope = geographicScope,
            Timeline = timeline,
            BudgetBreakdown = budgetBreakdown,
            EvaluationPlan = evaluationPlan,
            SustainabilityPlan = sustainabilityPlan,
            ApplicationDeadline = applicationDeadline,
            SubmissionDate = submissionDate,
            Status = status,
            Decision = decision,
            Notes = notes
        };

        // Assert
        application.OBNLBusinessPlanId.Should().Be(planId);
        application.GrantName.Should().Be(grantName);
        application.GrantingOrganization.Should().Be(grantingOrganization);
        application.GrantType.Should().Be(grantType);
        application.RequestedAmount.Should().Be(requestedAmount);
        application.MatchingFunds.Should().Be(matchingFunds);
        application.ProjectDescription.Should().Be(projectDescription);
        application.Objectives.Should().Be(objectives);
        application.ExpectedOutcomes.Should().Be(expectedOutcomes);
        application.TargetPopulation.Should().Be(targetPopulation);
        application.GeographicScope.Should().Be(geographicScope);
        application.Timeline.Should().Be(timeline);
        application.BudgetBreakdown.Should().Be(budgetBreakdown);
        application.EvaluationPlan.Should().Be(evaluationPlan);
        application.SustainabilityPlan.Should().Be(sustainabilityPlan);
        application.ApplicationDeadline.Should().Be(applicationDeadline);
        application.SubmissionDate.Should().Be(submissionDate);
        application.Status.Should().Be(status);
        application.Decision.Should().Be(decision);
        application.Notes.Should().Be(notes);
    }
}
