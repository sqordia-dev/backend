using AutoFixture;
using FluentAssertions;
using Sqordia.Domain.Entities;
using Sqordia.Domain.ValueObjects;
using Xunit;

namespace Sqordia.Domain.UnitTests.Entities;

public class OBNLBusinessPlanSimpleTests
{
    private readonly Fixture _fixture;

    public OBNLBusinessPlanSimpleTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var plan = new OBNLBusinessPlan();

        // Assert
        plan.Id.Should().NotBeEmpty();
        plan.OrganizationId.Should().Be(Guid.Empty);
        plan.OBNLType.Should().BeEmpty();
        plan.Mission.Should().BeEmpty();
        plan.Vision.Should().BeEmpty();
        plan.Values.Should().BeEmpty();
        plan.FundingRequirements.Should().Be(0);
        plan.FundingPurpose.Should().BeEmpty();
        plan.ComplianceStatus.Should().Be(ComplianceStatus.Pending);
        plan.LegalStructure.Should().BeEmpty();
        plan.RegistrationNumber.Should().BeEmpty();
        plan.RegistrationDate.Should().Be(default(DateTime));
        plan.GoverningBody.Should().BeEmpty();
        plan.BoardComposition.Should().BeEmpty();
        plan.StakeholderEngagement.Should().BeEmpty();
        plan.ImpactMeasurement.Should().BeEmpty();
        plan.SustainabilityStrategy.Should().BeEmpty();
    }

    [Fact]
    public void SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var plan = new OBNLBusinessPlan();
        var organizationId = Guid.NewGuid();
        var obnlType = "Charitable Organization";
        var mission = "To serve the community";
        var vision = "A thriving community";
        var values = "Compassion, Integrity";
        var fundingRequirements = 250000m;
        var fundingPurpose = "Program expansion";
        var legalStructure = "Non-Profit Corporation";
        var registrationNumber = "123456789";
        var registrationDate = DateTime.UtcNow;
        var governingBody = "Board of Directors";
        var boardComposition = "5 members";
        var stakeholderEngagement = "Community engagement plan";
        var impactMeasurement = "Impact measurement framework";
        var sustainabilityStrategy = "Sustainability strategy";

        // Act
        plan.OrganizationId = organizationId;
        plan.OBNLType = obnlType;
        plan.Mission = mission;
        plan.Vision = vision;
        plan.Values = values;
        plan.FundingRequirements = fundingRequirements;
        plan.FundingPurpose = fundingPurpose;
        plan.ComplianceStatus = ComplianceStatus.Pending;
        plan.LegalStructure = legalStructure;
        plan.RegistrationNumber = registrationNumber;
        plan.RegistrationDate = registrationDate;
        plan.GoverningBody = governingBody;
        plan.BoardComposition = boardComposition;
        plan.StakeholderEngagement = stakeholderEngagement;
        plan.ImpactMeasurement = impactMeasurement;
        plan.SustainabilityStrategy = sustainabilityStrategy;

        // Assert
        plan.OrganizationId.Should().Be(organizationId);
        plan.OBNLType.Should().Be(obnlType);
        plan.Mission.Should().Be(mission);
        plan.Vision.Should().Be(vision);
        plan.Values.Should().Be(values);
        plan.FundingRequirements.Should().Be(fundingRequirements);
        plan.FundingPurpose.Should().Be(fundingPurpose);
        plan.ComplianceStatus.Should().Be(ComplianceStatus.Pending);
        plan.LegalStructure.Should().Be(legalStructure);
        plan.RegistrationNumber.Should().Be(registrationNumber);
        plan.RegistrationDate.Should().Be(registrationDate);
        plan.GoverningBody.Should().Be(governingBody);
        plan.BoardComposition.Should().Be(boardComposition);
        plan.StakeholderEngagement.Should().Be(stakeholderEngagement);
        plan.ImpactMeasurement.Should().Be(impactMeasurement);
        plan.SustainabilityStrategy.Should().Be(sustainabilityStrategy);
    }

    [Fact]
    public void FundingRequirements_WithNegativeValue_ShouldAllowNegativeValues()
    {
        // Arrange
        var plan = new OBNLBusinessPlan();
        var negativeAmount = -1000m;

        // Act
        plan.FundingRequirements = negativeAmount;

        // Assert
        plan.FundingRequirements.Should().Be(negativeAmount);
    }

    [Fact]
    public void RegistrationDate_WithFutureDate_ShouldAcceptFutureDate()
    {
        // Arrange
        var plan = new OBNLBusinessPlan();
        var futureDate = DateTime.UtcNow.AddYears(1);

        // Act
        plan.RegistrationDate = futureDate;

        // Assert
        plan.RegistrationDate.Should().Be(futureDate);
    }

    [Fact]
    public void RegistrationDate_WithPastDate_ShouldAcceptPastDate()
    {
        // Arrange
        var plan = new OBNLBusinessPlan();
        var pastDate = DateTime.UtcNow.AddYears(-5);

        // Act
        plan.RegistrationDate = pastDate;

        // Assert
        plan.RegistrationDate.Should().Be(pastDate);
    }

    [Theory]
    [InlineData("Charitable Organization")]
    [InlineData("Non-Profit Corporation")]
    [InlineData("Foundation")]
    [InlineData("Social Enterprise")]
    [InlineData("Religious Organization")]
    public void OBNLType_WithValidTypes_ShouldAcceptAllTypes(string obnlType)
    {
        // Arrange
        var plan = new OBNLBusinessPlan();

        // Act
        plan.OBNLType = obnlType;

        // Assert
        plan.OBNLType.Should().Be(obnlType);
    }

    [Fact]
    public void CreateOBNLBusinessPlan_WithAllProperties_ShouldCreateCompletePlan()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var obnlType = "Charitable Organization";
        var mission = "To serve the community through essential services";
        var vision = "A thriving community where everyone has access to support";
        var values = "Compassion, Integrity, Community, Service";
        var fundingRequirements = 500000m;
        var fundingPurpose = "Program expansion and sustainability";
        var legalStructure = "Non-Profit Corporation";
        var registrationNumber = "123456789";
        var registrationDate = DateTime.UtcNow.AddYears(-2);
        var governingBody = "Board of Directors";
        var boardComposition = "5 members with diverse backgrounds";
        var stakeholderEngagement = "Community engagement plan with regular meetings";
        var impactMeasurement = "Comprehensive impact measurement framework";
        var sustainabilityStrategy = "Long-term sustainability strategy";

        // Act
        var plan = new OBNLBusinessPlan
        {
            OrganizationId = organizationId,
            OBNLType = obnlType,
            Mission = mission,
            Vision = vision,
            Values = values,
            FundingRequirements = fundingRequirements,
            FundingPurpose = fundingPurpose,
            ComplianceStatus = ComplianceStatus.Pending,
            LegalStructure = legalStructure,
            RegistrationNumber = registrationNumber,
            RegistrationDate = registrationDate,
            GoverningBody = governingBody,
            BoardComposition = boardComposition,
            StakeholderEngagement = stakeholderEngagement,
            ImpactMeasurement = impactMeasurement,
            SustainabilityStrategy = sustainabilityStrategy
        };

        // Assert
        plan.OrganizationId.Should().Be(organizationId);
        plan.OBNLType.Should().Be(obnlType);
        plan.Mission.Should().Be(mission);
        plan.Vision.Should().Be(vision);
        plan.Values.Should().Be(values);
        plan.FundingRequirements.Should().Be(fundingRequirements);
        plan.FundingPurpose.Should().Be(fundingPurpose);
        plan.ComplianceStatus.Should().Be(ComplianceStatus.Pending);
        plan.LegalStructure.Should().Be(legalStructure);
        plan.RegistrationNumber.Should().Be(registrationNumber);
        plan.RegistrationDate.Should().Be(registrationDate);
        plan.GoverningBody.Should().Be(governingBody);
        plan.BoardComposition.Should().Be(boardComposition);
        plan.StakeholderEngagement.Should().Be(stakeholderEngagement);
        plan.ImpactMeasurement.Should().Be(impactMeasurement);
        plan.SustainabilityStrategy.Should().Be(sustainabilityStrategy);
    }
}
