using AutoFixture;
using FluentAssertions;
using Sqordia.Domain.Entities;
using Xunit;

namespace Sqordia.Domain.UnitTests.Entities;

public class OBNLComplianceSimpleTests
{
    private readonly Fixture _fixture;

    public OBNLComplianceSimpleTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var compliance = new OBNLCompliance();

        // Assert
        compliance.Id.Should().NotBeEmpty();
        compliance.OBNLBusinessPlanId.Should().Be(Guid.Empty);
        compliance.RequirementType.Should().BeEmpty();
        compliance.Description.Should().BeEmpty();
        compliance.Jurisdiction.Should().BeEmpty();
        compliance.RegulatoryBody.Should().BeEmpty();
        compliance.ComplianceLevel.Should().BeEmpty();
        compliance.IsRequired.Should().BeFalse();
        compliance.DueDate.Should().Be(default(DateTime));
        compliance.Status.Should().BeEmpty();
        compliance.Documentation.Should().BeEmpty();
        compliance.Notes.Should().BeEmpty();
    }

    [Fact]
    public void SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var compliance = new OBNLCompliance();
        var planId = Guid.NewGuid();
        var requirementType = "Tax Exemption";
        var description = "Annual tax exemption filing";
        var jurisdiction = "Federal";
        var regulatoryBody = "IRS";
        var complianceLevel = "Full";
        var isRequired = true;
        var dueDate = DateTime.UtcNow.AddMonths(3);
        var status = "Pending";
        var documentation = "Form 990";
        var notes = "Annual filing required";

        // Act
        compliance.OBNLBusinessPlanId = planId;
        compliance.RequirementType = requirementType;
        compliance.Description = description;
        compliance.Jurisdiction = jurisdiction;
        compliance.RegulatoryBody = regulatoryBody;
        compliance.ComplianceLevel = complianceLevel;
        compliance.IsRequired = isRequired;
        compliance.DueDate = dueDate;
        compliance.Status = status;
        compliance.Documentation = documentation;
        compliance.Notes = notes;

        // Assert
        compliance.OBNLBusinessPlanId.Should().Be(planId);
        compliance.RequirementType.Should().Be(requirementType);
        compliance.Description.Should().Be(description);
        compliance.Jurisdiction.Should().Be(jurisdiction);
        compliance.RegulatoryBody.Should().Be(regulatoryBody);
        compliance.ComplianceLevel.Should().Be(complianceLevel);
        compliance.IsRequired.Should().Be(isRequired);
        compliance.DueDate.Should().Be(dueDate);
        compliance.Status.Should().Be(status);
        compliance.Documentation.Should().Be(documentation);
        compliance.Notes.Should().Be(notes);
    }

    [Theory]
    [InlineData("Compliant")]
    [InlineData("Non-Compliant")]
    [InlineData("Pending")]
    [InlineData("Under Review")]
    public void Status_WithValidStatuses_ShouldAcceptAllStatuses(string status)
    {
        // Arrange
        var compliance = new OBNLCompliance();

        // Act
        compliance.Status = status;

        // Assert
        compliance.Status.Should().Be(status);
    }

    [Theory]
    [InlineData("Full")]
    [InlineData("Partial")]
    [InlineData("Basic")]
    [InlineData("Advanced")]
    public void ComplianceLevel_WithValidLevels_ShouldAcceptAllLevels(string level)
    {
        // Arrange
        var compliance = new OBNLCompliance();

        // Act
        compliance.ComplianceLevel = level;

        // Assert
        compliance.ComplianceLevel.Should().Be(level);
    }

    [Fact]
    public void IsRequired_WithTrue_ShouldBeTrue()
    {
        // Arrange
        var compliance = new OBNLCompliance();

        // Act
        compliance.IsRequired = true;

        // Assert
        compliance.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void IsRequired_WithFalse_ShouldBeFalse()
    {
        // Arrange
        var compliance = new OBNLCompliance();

        // Act
        compliance.IsRequired = false;

        // Assert
        compliance.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void DueDate_WithFutureDate_ShouldAcceptFutureDate()
    {
        // Arrange
        var compliance = new OBNLCompliance();
        var futureDate = DateTime.UtcNow.AddMonths(6);

        // Act
        compliance.DueDate = futureDate;

        // Assert
        compliance.DueDate.Should().Be(futureDate);
    }

    [Fact]
    public void DueDate_WithPastDate_ShouldAcceptPastDate()
    {
        // Arrange
        var compliance = new OBNLCompliance();
        var pastDate = DateTime.UtcNow.AddMonths(-6);

        // Act
        compliance.DueDate = pastDate;

        // Assert
        compliance.DueDate.Should().Be(pastDate);
    }

    [Fact]
    public void CreateOBNLCompliance_WithAllProperties_ShouldCreateCompleteCompliance()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var requirementType = "Tax Exemption";
        var description = "Annual tax exemption filing";
        var jurisdiction = "Federal";
        var regulatoryBody = "IRS";
        var complianceLevel = "Full";
        var isRequired = true;
        var dueDate = DateTime.UtcNow.AddMonths(3);
        var status = "Pending";
        var documentation = "Form 990";
        var notes = "Annual filing required";

        // Act
        var compliance = new OBNLCompliance
        {
            OBNLBusinessPlanId = planId,
            RequirementType = requirementType,
            Description = description,
            Jurisdiction = jurisdiction,
            RegulatoryBody = regulatoryBody,
            ComplianceLevel = complianceLevel,
            IsRequired = isRequired,
            DueDate = dueDate,
            Status = status,
            Documentation = documentation,
            Notes = notes
        };

        // Assert
        compliance.OBNLBusinessPlanId.Should().Be(planId);
        compliance.RequirementType.Should().Be(requirementType);
        compliance.Description.Should().Be(description);
        compliance.Jurisdiction.Should().Be(jurisdiction);
        compliance.RegulatoryBody.Should().Be(regulatoryBody);
        compliance.ComplianceLevel.Should().Be(complianceLevel);
        compliance.IsRequired.Should().Be(isRequired);
        compliance.DueDate.Should().Be(dueDate);
        compliance.Status.Should().Be(status);
        compliance.Documentation.Should().Be(documentation);
        compliance.Notes.Should().Be(notes);
    }
}
