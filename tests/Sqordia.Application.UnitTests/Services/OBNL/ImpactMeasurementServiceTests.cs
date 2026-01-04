using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.OBNL.Commands;
using Sqordia.Application.OBNL.Queries;
using Sqordia.Domain.Entities;
using Sqordia.Domain.ValueObjects;
using Sqordia.Persistence.Contexts;
using Xunit;

namespace Sqordia.Application.UnitTests.Services.OBNL;

public class ImpactMeasurementServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Fixture _fixture;

    public ImpactMeasurementServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateImpactMeasurement_ValidRequest_ShouldCreateMeasurement()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var plan = new OBNLBusinessPlan
        {
            OrganizationId = organizationId,
            OBNLType = "Charitable Organization",
            Mission = "Test Mission",
            Vision = "Test Vision",
            Values = "Test Values",
            FundingRequirements = 100000,
            FundingPurpose = "Test Purpose",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        var measurement = new ImpactMeasurement
        {
            OBNLBusinessPlanId = planId,
            MetricName = "Community Impact",
            Description = "Measure community impact",
            MeasurementType = "Quantitative",
            UnitOfMeasurement = "Count",
            BaselineValue = 0,
            TargetValue = 500,
            CurrentValue = 0,
            DataSource = "Community surveys",
            CollectionMethod = "Survey",
            Frequency = "Monthly",
            ResponsibleParty = "Program Manager",
            LastMeasurement = DateTime.UtcNow.AddDays(-30),
            NextMeasurement = DateTime.UtcNow.AddDays(30),
            Status = "Active",
            Notes = "Impact measurement notes"
        };

        // Act
        _context.ImpactMeasurements.Add(measurement);
        await _context.SaveChangesAsync();

        // Assert
        var savedMeasurement = await _context.ImpactMeasurements.FirstOrDefaultAsync();
        savedMeasurement.Should().NotBeNull();
        savedMeasurement!.MetricName.Should().Be("Community Impact");
        savedMeasurement.Description.Should().Be("Measure community impact");
        savedMeasurement.MeasurementType.Should().Be("Quantitative");
        savedMeasurement.UnitOfMeasurement.Should().Be("Count");
        savedMeasurement.BaselineValue.Should().Be(0);
        savedMeasurement.TargetValue.Should().Be(500);
        savedMeasurement.CurrentValue.Should().Be(0);
        savedMeasurement.DataSource.Should().Be("Community surveys");
        savedMeasurement.CollectionMethod.Should().Be("Survey");
        savedMeasurement.Frequency.Should().Be("Monthly");
        savedMeasurement.ResponsibleParty.Should().Be("Program Manager");
        savedMeasurement.Status.Should().Be("Active");
        savedMeasurement.Notes.Should().Be("Impact measurement notes");
    }

    [Fact]
    public async Task GetImpactMeasurements_ExistingMeasurements_ShouldReturnAllMeasurements()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var plan = new OBNLBusinessPlan
        {
            OrganizationId = organizationId,
            OBNLType = "Charitable Organization",
            Mission = "Test Mission",
            Vision = "Test Vision",
            Values = "Test Values",
            FundingRequirements = 100000,
            FundingPurpose = "Test Purpose",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        var measurement1 = new ImpactMeasurement
        {
            OBNLBusinessPlanId = planId,
            MetricName = "Community Impact",
            Description = "Measure community impact",
            MeasurementType = "Quantitative",
            UnitOfMeasurement = "Count",
            BaselineValue = 0,
            TargetValue = 500,
            CurrentValue = 0,
            DataSource = "Community surveys",
            CollectionMethod = "Survey",
            Frequency = "Monthly",
            ResponsibleParty = "Program Manager",
            LastMeasurement = DateTime.UtcNow.AddDays(-30),
            NextMeasurement = DateTime.UtcNow.AddDays(30),
            Status = "Active",
            Notes = "Impact measurement notes"
        };

        var measurement2 = new ImpactMeasurement
        {
            OBNLBusinessPlanId = planId,
            MetricName = "Volunteer Engagement",
            Description = "Measure volunteer engagement",
            MeasurementType = "Quantitative",
            UnitOfMeasurement = "Hours",
            BaselineValue = 0,
            TargetValue = 1000,
            CurrentValue = 0,
            DataSource = "Volunteer logs",
            CollectionMethod = "Time tracking",
            Frequency = "Weekly",
            ResponsibleParty = "Volunteer Coordinator",
            LastMeasurement = DateTime.UtcNow.AddDays(-7),
            NextMeasurement = DateTime.UtcNow.AddDays(7),
            Status = "Active",
            Notes = "Volunteer engagement measurement"
        };

        _context.ImpactMeasurements.AddRange(measurement1, measurement2);
        await _context.SaveChangesAsync();

        // Act
        var measurements = await _context.ImpactMeasurements
            .Where(m => m.OBNLBusinessPlanId == planId)
            .ToListAsync();

        // Assert
        measurements.Should().HaveCount(2);
        measurements.Should().Contain(m => m.MetricName == "Community Impact");
        measurements.Should().Contain(m => m.MetricName == "Volunteer Engagement");
    }

    [Fact]
    public async Task UpdateImpactMeasurement_ValidRequest_ShouldUpdateMeasurement()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var plan = new OBNLBusinessPlan
        {
            OrganizationId = organizationId,
            OBNLType = "Charitable Organization",
            Mission = "Test Mission",
            Vision = "Test Vision",
            Values = "Test Values",
            FundingRequirements = 100000,
            FundingPurpose = "Test Purpose",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        var measurement = new ImpactMeasurement
        {
            OBNLBusinessPlanId = planId,
            MetricName = "Community Impact",
            Description = "Measure community impact",
            MeasurementType = "Quantitative",
            UnitOfMeasurement = "Count",
            BaselineValue = 0,
            TargetValue = 500,
            CurrentValue = 0,
            DataSource = "Community surveys",
            CollectionMethod = "Survey",
            Frequency = "Monthly",
            ResponsibleParty = "Program Manager",
            LastMeasurement = DateTime.UtcNow.AddDays(-30),
            NextMeasurement = DateTime.UtcNow.AddDays(30),
            Status = "Active",
            Notes = "Impact measurement notes"
        };

        _context.ImpactMeasurements.Add(measurement);
        await _context.SaveChangesAsync();

        // Act
        measurement.CurrentValue = 150;
        measurement.Status = "In Progress";
        measurement.Notes = "Updated measurement with current progress";
        _context.ImpactMeasurements.Update(measurement);
        await _context.SaveChangesAsync();

        // Assert
        var updatedMeasurement = await _context.ImpactMeasurements.FirstOrDefaultAsync();
        updatedMeasurement.Should().NotBeNull();
        updatedMeasurement!.CurrentValue.Should().Be(150);
        updatedMeasurement.Status.Should().Be("In Progress");
        updatedMeasurement.Notes.Should().Be("Updated measurement with current progress");
    }

    [Fact]
    public async Task DeleteImpactMeasurement_ExistingMeasurement_ShouldDeleteMeasurement()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var plan = new OBNLBusinessPlan
        {
            OrganizationId = organizationId,
            OBNLType = "Charitable Organization",
            Mission = "Test Mission",
            Vision = "Test Vision",
            Values = "Test Values",
            FundingRequirements = 100000,
            FundingPurpose = "Test Purpose",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        var measurement = new ImpactMeasurement
        {
            OBNLBusinessPlanId = planId,
            MetricName = "Community Impact",
            Description = "Measure community impact",
            MeasurementType = "Quantitative",
            UnitOfMeasurement = "Count",
            BaselineValue = 0,
            TargetValue = 500,
            CurrentValue = 0,
            DataSource = "Community surveys",
            CollectionMethod = "Survey",
            Frequency = "Monthly",
            ResponsibleParty = "Program Manager",
            LastMeasurement = DateTime.UtcNow.AddDays(-30),
            NextMeasurement = DateTime.UtcNow.AddDays(30),
            Status = "Active",
            Notes = "Impact measurement notes"
        };

        _context.ImpactMeasurements.Add(measurement);
        await _context.SaveChangesAsync();

        // Act
        _context.ImpactMeasurements.Remove(measurement);
        await _context.SaveChangesAsync();

        // Assert
        var deletedMeasurement = await _context.ImpactMeasurements.FirstOrDefaultAsync();
        deletedMeasurement.Should().BeNull();
    }

    [Fact]
    public async Task GetImpactMeasurementsByStatus_ActiveMeasurements_ShouldReturnActiveMeasurements()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();

        var plan = new OBNLBusinessPlan
        {
            OrganizationId = organizationId,
            OBNLType = "Charitable Organization",
            Mission = "Test Mission",
            Vision = "Test Vision",
            Values = "Test Values",
            FundingRequirements = 100000,
            FundingPurpose = "Test Purpose",
            LegalStructure = "Non-Profit Corporation",
            RegistrationNumber = "123456789",
            RegistrationDate = DateTime.UtcNow,
            GoverningBody = "Board of Directors",
            BoardComposition = "5 members",
            StakeholderEngagement = "Test Engagement",
            ImpactMeasurement = "Test Measurement",
            SustainabilityStrategy = "Test Strategy",
            ComplianceStatus = ComplianceStatus.Pending,
            CreatedBy = "test-user"
        };

        _context.OBNLBusinessPlans.Add(plan);
        await _context.SaveChangesAsync();

        var activeMeasurement = new ImpactMeasurement
        {
            OBNLBusinessPlanId = planId,
            MetricName = "Community Impact",
            Description = "Measure community impact",
            MeasurementType = "Quantitative",
            UnitOfMeasurement = "Count",
            BaselineValue = 0,
            TargetValue = 500,
            CurrentValue = 0,
            DataSource = "Community surveys",
            CollectionMethod = "Survey",
            Frequency = "Monthly",
            ResponsibleParty = "Program Manager",
            LastMeasurement = DateTime.UtcNow.AddDays(-30),
            NextMeasurement = DateTime.UtcNow.AddDays(30),
            Status = "Active",
            Notes = "Impact measurement notes"
        };

        var completedMeasurement = new ImpactMeasurement
        {
            OBNLBusinessPlanId = planId,
            MetricName = "Volunteer Engagement",
            Description = "Measure volunteer engagement",
            MeasurementType = "Quantitative",
            UnitOfMeasurement = "Hours",
            BaselineValue = 0,
            TargetValue = 1000,
            CurrentValue = 1000,
            DataSource = "Volunteer logs",
            CollectionMethod = "Time tracking",
            Frequency = "Weekly",
            ResponsibleParty = "Volunteer Coordinator",
            LastMeasurement = DateTime.UtcNow.AddDays(-7),
            NextMeasurement = DateTime.UtcNow.AddDays(7),
            Status = "Completed",
            Notes = "Volunteer engagement measurement completed"
        };

        _context.ImpactMeasurements.AddRange(activeMeasurement, completedMeasurement);
        await _context.SaveChangesAsync();

        // Act
        var activeMeasurements = await _context.ImpactMeasurements
            .Where(m => m.OBNLBusinessPlanId == planId && m.Status == "Active")
            .ToListAsync();

        // Assert
        activeMeasurements.Should().HaveCount(1);
        activeMeasurements.Should().Contain(m => m.MetricName == "Community Impact");
        activeMeasurements.Should().NotContain(m => m.MetricName == "Volunteer Engagement");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
