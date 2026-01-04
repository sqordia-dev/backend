using AutoFixture;
using FluentAssertions;
using Sqordia.Domain.Entities;
using Xunit;

namespace Sqordia.Domain.UnitTests.Entities;

public class OBNLComplianceTests
{
    private readonly Fixture _fixture;

    public OBNLComplianceTests()
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
    public void Constructor_ShouldGenerateUniqueId()
    {
        // Act
        var compliance1 = new OBNLCompliance();
        var compliance2 = new OBNLCompliance();

        // Assert
        compliance1.Id.Should().NotBeEmpty();
        compliance2.Id.Should().NotBeEmpty();
        compliance1.Id.Should().NotBe(compliance2.Id);
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
    public void Documentation_WithMultipleItems_ShouldStoreAllItems()
    {
        // Arrange
        var compliance = new OBNLCompliance();
        var documentation = "Form 990, Annual Report, Board Minutes";

        // Act
        compliance.Documentation = documentation;

        // Assert
        compliance.Documentation.Should().Be(documentation);
    }

    [Fact]
    public void Notes_WithMultipleItems_ShouldStoreAllItems()
    {
        // Arrange
        var compliance = new OBNLCompliance();
        var notes = "Annual filing required, Board governance structure needed";

        // Act
        compliance.Notes = notes;

        // Assert
        compliance.Notes.Should().Be(notes);
    }

    [Fact]
    public void CreateOBNLCompliance_WithAllProperties_ShouldCreateCompleteCompliance()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var status = "Compliant";
        var level = "Full";
        var requirementType = "Tax Exemption";
        var description = "Annual tax exemption filing";
        var jurisdiction = "Federal";
        var regulatoryBody = "IRS";
        var documentation = "Form 990";
        var notes = "Annual filing required";

        // Act
        var compliance = new OBNLCompliance
        {
            OBNLBusinessPlanId = planId,
            Status = status,
            ComplianceLevel = level,
            RequirementType = requirementType,
            Description = description,
            Jurisdiction = jurisdiction,
            RegulatoryBody = regulatoryBody,
            Documentation = documentation,
            Notes = notes
        };

        // Assert
        compliance.OBNLBusinessPlanId.Should().Be(planId);
        compliance.Status.Should().Be(status);
        compliance.ComplianceLevel.Should().Be(level);
        compliance.RequirementType.Should().Be(requirementType);
        compliance.Description.Should().Be(description);
        compliance.Jurisdiction.Should().Be(jurisdiction);
        compliance.RegulatoryBody.Should().Be(regulatoryBody);
        compliance.Documentation.Should().Be(documentation);
        compliance.Notes.Should().Be(notes);
    }

    [Fact]
    public void Documentation_EmptyString_ShouldBeEmpty()
    {
        // Arrange
        var compliance = new OBNLCompliance();
        var emptyDocumentation = string.Empty;

        // Act
        compliance.Documentation = emptyDocumentation;

        // Assert
        compliance.Documentation.Should().BeEmpty();
    }

    [Fact]
    public void Notes_EmptyString_ShouldBeEmpty()
    {
        // Arrange
        var compliance = new OBNLCompliance();
        var emptyNotes = string.Empty;

        // Act
        compliance.Notes = emptyNotes;

        // Assert
        compliance.Notes.Should().BeEmpty();
    }
}
