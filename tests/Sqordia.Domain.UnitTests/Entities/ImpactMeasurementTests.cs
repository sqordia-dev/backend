using AutoFixture;
using FluentAssertions;
using Sqordia.Domain.Entities;
using Xunit;

namespace Sqordia.Domain.UnitTests.Entities;

public class ImpactMeasurementTests
{
    private readonly Fixture _fixture;

    public ImpactMeasurementTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var measurement = new ImpactMeasurement();

        // Assert
        measurement.Id.Should().NotBeEmpty();
        measurement.OBNLBusinessPlanId.Should().Be(Guid.Empty);
        measurement.MetricName.Should().BeEmpty();
        measurement.Description.Should().BeEmpty();
        measurement.MeasurementType.Should().BeEmpty();
        measurement.UnitOfMeasurement.Should().BeEmpty();
        measurement.BaselineValue.Should().Be(0);
        measurement.TargetValue.Should().Be(0);
        measurement.CurrentValue.Should().Be(0);
        measurement.DataSource.Should().BeEmpty();
        measurement.CollectionMethod.Should().BeEmpty();
        measurement.Frequency.Should().BeEmpty();
        measurement.ResponsibleParty.Should().BeEmpty();
        measurement.LastMeasurement.Should().Be(default(DateTime));
        measurement.NextMeasurement.Should().Be(default(DateTime));
        measurement.Status.Should().BeEmpty();
        measurement.Notes.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueId()
    {
        // Act
        var measurement1 = new ImpactMeasurement();
        var measurement2 = new ImpactMeasurement();

        // Assert
        measurement1.Id.Should().NotBeEmpty();
        measurement2.Id.Should().NotBeEmpty();
        measurement1.Id.Should().NotBe(measurement2.Id);
    }

    [Fact]
    public void SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var measurement = new ImpactMeasurement();
        var planId = Guid.NewGuid();
        var metricName = "Community Impact";
        var description = "Measure community impact";
        var measurementType = "Quantitative";
        var unitOfMeasurement = "Count";
        var baselineValue = 0m;
        var targetValue = 500m;
        var currentValue = 100m;
        var dataSource = "Community surveys";
        var collectionMethod = "Survey";
        var frequency = "Monthly";
        var responsibleParty = "Program Manager";
        var lastMeasurement = DateTime.UtcNow.AddDays(-30);
        var nextMeasurement = DateTime.UtcNow.AddDays(30);
        var status = "Active";
        var notes = "Impact measurement notes";

        // Act
        measurement.OBNLBusinessPlanId = planId;
        measurement.MetricName = metricName;
        measurement.Description = description;
        measurement.MeasurementType = measurementType;
        measurement.UnitOfMeasurement = unitOfMeasurement;
        measurement.BaselineValue = baselineValue;
        measurement.TargetValue = targetValue;
        measurement.CurrentValue = currentValue;
        measurement.DataSource = dataSource;
        measurement.CollectionMethod = collectionMethod;
        measurement.Frequency = frequency;
        measurement.ResponsibleParty = responsibleParty;
        measurement.LastMeasurement = lastMeasurement;
        measurement.NextMeasurement = nextMeasurement;
        measurement.Status = status;
        measurement.Notes = notes;

        // Assert
        measurement.OBNLBusinessPlanId.Should().Be(planId);
        measurement.MetricName.Should().Be(metricName);
        measurement.Description.Should().Be(description);
        measurement.MeasurementType.Should().Be(measurementType);
        measurement.UnitOfMeasurement.Should().Be(unitOfMeasurement);
        measurement.BaselineValue.Should().Be(baselineValue);
        measurement.TargetValue.Should().Be(targetValue);
        measurement.CurrentValue.Should().Be(currentValue);
        measurement.DataSource.Should().Be(dataSource);
        measurement.CollectionMethod.Should().Be(collectionMethod);
        measurement.Frequency.Should().Be(frequency);
        measurement.ResponsibleParty.Should().Be(responsibleParty);
        measurement.LastMeasurement.Should().Be(lastMeasurement);
        measurement.NextMeasurement.Should().Be(nextMeasurement);
        measurement.Status.Should().Be(status);
        measurement.Notes.Should().Be(notes);
    }

    [Theory]
    [InlineData("Quantitative")]
    [InlineData("Qualitative")]
    [InlineData("Mixed Methods")]
    [InlineData("Observational")]
    [InlineData("Survey")]
    public void MeasurementType_WithValidTypes_ShouldAcceptAllTypes(string measurementType)
    {
        // Arrange
        var measurement = new ImpactMeasurement();

        // Act
        measurement.MeasurementType = measurementType;

        // Assert
        measurement.MeasurementType.Should().Be(measurementType);
    }

    [Theory]
    [InlineData("Count")]
    [InlineData("Percentage")]
    [InlineData("Hours")]
    [InlineData("People")]
    [InlineData("Events")]
    [InlineData("Dollars")]
    public void UnitOfMeasurement_WithValidUnits_ShouldAcceptAllUnits(string unit)
    {
        // Arrange
        var measurement = new ImpactMeasurement();

        // Act
        measurement.UnitOfMeasurement = unit;

        // Assert
        measurement.UnitOfMeasurement.Should().Be(unit);
    }

    [Theory]
    [InlineData("Daily")]
    [InlineData("Weekly")]
    [InlineData("Monthly")]
    [InlineData("Quarterly")]
    [InlineData("Annually")]
    [InlineData("As Needed")]
    public void Frequency_WithValidFrequencies_ShouldAcceptAllFrequencies(string frequency)
    {
        // Arrange
        var measurement = new ImpactMeasurement();

        // Act
        measurement.Frequency = frequency;

        // Assert
        measurement.Frequency.Should().Be(frequency);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("Inactive")]
    [InlineData("Completed")]
    [InlineData("Suspended")]
    [InlineData("Draft")]
    public void Status_WithValidStatuses_ShouldAcceptAllStatuses(string status)
    {
        // Arrange
        var measurement = new ImpactMeasurement();

        // Act
        measurement.Status = status;

        // Assert
        measurement.Status.Should().Be(status);
    }

    [Fact]
    public void BaselineValue_WithNegativeValue_ShouldAllowNegativeValues()
    {
        // Arrange
        var measurement = new ImpactMeasurement();
        var negativeValue = -50m;

        // Act
        measurement.BaselineValue = negativeValue;

        // Assert
        measurement.BaselineValue.Should().Be(negativeValue);
    }

    [Fact]
    public void TargetValue_WithNegativeValue_ShouldAllowNegativeValues()
    {
        // Arrange
        var measurement = new ImpactMeasurement();
        var negativeValue = -100m;

        // Act
        measurement.TargetValue = negativeValue;

        // Assert
        measurement.TargetValue.Should().Be(negativeValue);
    }

    [Fact]
    public void CurrentValue_WithNegativeValue_ShouldAllowNegativeValues()
    {
        // Arrange
        var measurement = new ImpactMeasurement();
        var negativeValue = -25m;

        // Act
        measurement.CurrentValue = negativeValue;

        // Assert
        measurement.CurrentValue.Should().Be(negativeValue);
    }

    [Fact]
    public void LastMeasurement_WithFutureDate_ShouldAcceptFutureDate()
    {
        // Arrange
        var measurement = new ImpactMeasurement();
        var futureDate = DateTime.UtcNow.AddDays(30);

        // Act
        measurement.LastMeasurement = futureDate;

        // Assert
        measurement.LastMeasurement.Should().Be(futureDate);
    }

    [Fact]
    public void NextMeasurement_WithPastDate_ShouldAcceptPastDate()
    {
        // Arrange
        var measurement = new ImpactMeasurement();
        var pastDate = DateTime.UtcNow.AddDays(-30);

        // Act
        measurement.NextMeasurement = pastDate;

        // Assert
        measurement.NextMeasurement.Should().Be(pastDate);
    }

    [Fact]
    public void CreateImpactMeasurement_WithAllProperties_ShouldCreateCompleteMeasurement()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var metricName = "Community Engagement";
        var description = "Measure community engagement through various activities";
        var measurementType = "Mixed Methods";
        var unitOfMeasurement = "People";
        var baselineValue = 50m;
        var targetValue = 200m;
        var currentValue = 75m;
        var dataSource = "Community surveys and event attendance";
        var collectionMethod = "Survey and observation";
        var frequency = "Monthly";
        var responsibleParty = "Community Engagement Manager";
        var lastMeasurement = DateTime.UtcNow.AddDays(-15);
        var nextMeasurement = DateTime.UtcNow.AddDays(15);
        var status = "Active";
        var notes = "Comprehensive impact measurement for community engagement";

        // Act
        var measurement = new ImpactMeasurement
        {
            OBNLBusinessPlanId = planId,
            MetricName = metricName,
            Description = description,
            MeasurementType = measurementType,
            UnitOfMeasurement = unitOfMeasurement,
            BaselineValue = baselineValue,
            TargetValue = targetValue,
            CurrentValue = currentValue,
            DataSource = dataSource,
            CollectionMethod = collectionMethod,
            Frequency = frequency,
            ResponsibleParty = responsibleParty,
            LastMeasurement = lastMeasurement,
            NextMeasurement = nextMeasurement,
            Status = status,
            Notes = notes
        };

        // Assert
        measurement.OBNLBusinessPlanId.Should().Be(planId);
        measurement.MetricName.Should().Be(metricName);
        measurement.Description.Should().Be(description);
        measurement.MeasurementType.Should().Be(measurementType);
        measurement.UnitOfMeasurement.Should().Be(unitOfMeasurement);
        measurement.BaselineValue.Should().Be(baselineValue);
        measurement.TargetValue.Should().Be(targetValue);
        measurement.CurrentValue.Should().Be(currentValue);
        measurement.DataSource.Should().Be(dataSource);
        measurement.CollectionMethod.Should().Be(collectionMethod);
        measurement.Frequency.Should().Be(frequency);
        measurement.ResponsibleParty.Should().Be(responsibleParty);
        measurement.LastMeasurement.Should().Be(lastMeasurement);
        measurement.NextMeasurement.Should().Be(nextMeasurement);
        measurement.Status.Should().Be(status);
        measurement.Notes.Should().Be(notes);
    }
}
