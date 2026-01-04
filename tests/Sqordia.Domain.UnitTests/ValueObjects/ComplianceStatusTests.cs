using FluentAssertions;
using Sqordia.Domain.ValueObjects;
using Xunit;

namespace Sqordia.Domain.UnitTests.ValueObjects;

public class ComplianceStatusTests
{
    [Fact]
    public void ComplianceStatus_ShouldHaveAllExpectedValues()
    {
        // Act & Assert
        ComplianceStatus.Pending.Should().NotBeNull();
        ComplianceStatus.Compliant.Should().NotBeNull();
        ComplianceStatus.NonCompliant.Should().NotBeNull();
        ComplianceStatus.InProgress.Should().NotBeNull();
    }

    [Fact]
    public void ComplianceStatus_ShouldHaveCorrectProperties()
    {
        // Act & Assert
        ComplianceStatus.Pending.Status.Should().Be("Pending");
        ComplianceStatus.Pending.Level.Should().Be("Basic");
        ComplianceStatus.Compliant.Status.Should().Be("Compliant");
        ComplianceStatus.Compliant.Level.Should().Be("Full");
        ComplianceStatus.NonCompliant.Status.Should().Be("Non-Compliant");
        ComplianceStatus.NonCompliant.Level.Should().Be("None");
        ComplianceStatus.InProgress.Status.Should().Be("In Progress");
        ComplianceStatus.InProgress.Level.Should().Be("Intermediate");
    }

    [Fact]
    public void ComplianceStatus_ShouldHaveCorrectStringRepresentations()
    {
        // Act & Assert
        ComplianceStatus.Pending.Status.Should().Be("Pending");
        ComplianceStatus.Compliant.Status.Should().Be("Compliant");
        ComplianceStatus.NonCompliant.Status.Should().Be("Non-Compliant");
        ComplianceStatus.InProgress.Status.Should().Be("In Progress");
    }

    [Fact]
    public void ComplianceStatus_ShouldBeComparable()
    {
        // Act & Assert
        (ComplianceStatus.Pending == ComplianceStatus.Pending).Should().BeTrue();
        (ComplianceStatus.Compliant == ComplianceStatus.Compliant).Should().BeTrue();
        (ComplianceStatus.Pending != ComplianceStatus.Compliant).Should().BeTrue();
        (ComplianceStatus.NonCompliant != ComplianceStatus.InProgress).Should().BeTrue();
    }

    [Fact]
    public void ComplianceStatus_ShouldBeCreatableFromString()
    {
        // Act & Assert
        var pending = ComplianceStatus.Create("Pending", "Basic");
        pending.Status.Should().Be("Pending");
        pending.Level.Should().Be("Basic");

        var compliant = ComplianceStatus.Create("Compliant", "Full");
        compliant.Status.Should().Be("Compliant");
        compliant.Level.Should().Be("Full");

        var nonCompliant = ComplianceStatus.Create("Non-Compliant", "None");
        nonCompliant.Status.Should().Be("Non-Compliant");
        nonCompliant.Level.Should().Be("None");
    }

    [Fact]
    public void ComplianceStatus_ShouldBeCreatableWithNotes()
    {
        // Act & Assert
        var status = ComplianceStatus.Create("Pending", "Basic", "Custom notes");
        status.Status.Should().Be("Pending");
        status.Level.Should().Be("Basic");
        status.Notes.Should().Be("Custom notes");
    }

    [Fact]
    public void ComplianceStatus_ShouldHaveCorrectStaticProperties()
    {
        // Act & Assert
        ComplianceStatus.Pending.Status.Should().Be("Pending");
        ComplianceStatus.Pending.Level.Should().Be("Basic");
        ComplianceStatus.Pending.Notes.Should().Be("Compliance review pending");
        
        ComplianceStatus.Compliant.Status.Should().Be("Compliant");
        ComplianceStatus.Compliant.Level.Should().Be("Full");
        ComplianceStatus.Compliant.Notes.Should().Be("All requirements met");
    }

    [Fact]
    public void ComplianceStatus_ShouldHaveLastUpdatedTimestamp()
    {
        // Act & Assert
        ComplianceStatus.Pending.LastUpdated.Should().Be(DateTime.MinValue);
        ComplianceStatus.Compliant.LastUpdated.Should().Be(DateTime.MinValue);
        ComplianceStatus.NonCompliant.LastUpdated.Should().Be(DateTime.MinValue);
        ComplianceStatus.InProgress.LastUpdated.Should().Be(DateTime.MinValue);
    }
}
