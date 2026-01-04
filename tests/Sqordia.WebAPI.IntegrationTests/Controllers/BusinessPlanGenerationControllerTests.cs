using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;
using WebAPI.Controllers;
using Xunit;

namespace Sqordia.WebAPI.IntegrationTests.Controllers;

public class BusinessPlanGenerationControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IBusinessPlanGenerationService> _generationServiceMock;
    private readonly Mock<ILogger<BusinessPlanGenerationController>> _loggerMock;
    private readonly BusinessPlanGenerationController _sut;

    public BusinessPlanGenerationControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _generationServiceMock = new Mock<IBusinessPlanGenerationService>();
        _loggerMock = new Mock<ILogger<BusinessPlanGenerationController>>();

        _sut = new BusinessPlanGenerationController(_generationServiceMock.Object, _loggerMock.Object);
    }

    #region GenerateBusinessPlan Tests

    [Fact]
    public async Task GenerateBusinessPlan_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var businessPlan = CreateBusinessPlan(businessPlanId);
        var result = Result.Success(businessPlan);

        _generationServiceMock
            .Setup(x => x.GenerateBusinessPlanAsync(businessPlanId, "fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GenerateBusinessPlan(businessPlanId, "fr", CancellationToken.None);

        // Assert
        var okResult = response.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(businessPlan);
    }

    [Fact]
    public async Task GenerateBusinessPlan_WithInvalidLanguage_ShouldReturnBadRequest()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();

        // Act
        var response = await _sut.GenerateBusinessPlan(businessPlanId, "es", CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GenerateBusinessPlan_WithFailedGeneration_ShouldReturnBadRequest()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var result = Result.Failure<BusinessPlan>("Generation failed");

        _generationServiceMock
            .Setup(x => x.GenerateBusinessPlanAsync(businessPlanId, "fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GenerateBusinessPlan(businessPlanId, "fr", CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("en")]
    public async Task GenerateBusinessPlan_WithValidLanguages_ShouldCallService(string language)
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var businessPlan = CreateBusinessPlan(businessPlanId);
        var result = Result.Success(businessPlan);

        _generationServiceMock
            .Setup(x => x.GenerateBusinessPlanAsync(businessPlanId, language, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _sut.GenerateBusinessPlan(businessPlanId, language, CancellationToken.None);

        // Assert
        _generationServiceMock.Verify(
            x => x.GenerateBusinessPlanAsync(businessPlanId, language, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region RegenerateSection Tests

    [Fact]
    public async Task RegenerateSection_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var sectionName = "ExecutiveSummary";
        var businessPlan = CreateBusinessPlan(businessPlanId);
        var result = Result.Success(businessPlan);

        _generationServiceMock
            .Setup(x => x.RegenerateSectionAsync(businessPlanId, sectionName, "fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.RegenerateSection(businessPlanId, sectionName, "fr", CancellationToken.None);

        // Assert
        var okResult = response.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(businessPlan);
    }

    [Fact]
    public async Task RegenerateSection_WithEmptySectionName_ShouldReturnBadRequest()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();

        // Act
        var response = await _sut.RegenerateSection(businessPlanId, "", "fr", CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RegenerateSection_WithInvalidLanguage_ShouldReturnBadRequest()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();

        // Act
        var response = await _sut.RegenerateSection(businessPlanId, "ExecutiveSummary", "de", CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData("ExecutiveSummary")]
    [InlineData("MarketAnalysis")]
    [InlineData("FinancialProjections")]
    [InlineData("ExitStrategy")]
    public async Task RegenerateSection_WithValidSectionNames_ShouldCallService(string sectionName)
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var businessPlan = CreateBusinessPlan(businessPlanId);
        var result = Result.Success(businessPlan);

        _generationServiceMock
            .Setup(x => x.RegenerateSectionAsync(businessPlanId, sectionName, "fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _sut.RegenerateSection(businessPlanId, sectionName, "fr", CancellationToken.None);

        // Assert
        _generationServiceMock.Verify(
            x => x.RegenerateSectionAsync(businessPlanId, sectionName, "fr", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region GetGenerationStatus Tests

    [Fact]
    public async Task GetGenerationStatus_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var status = new BusinessPlanGenerationStatus
        {
            BusinessPlanId = businessPlanId,
            Status = "Generated",
            TotalSections = 15,
            CompletedSections = 15,
            CompletionPercentage = 100
        };
        var result = Result.Success(status);

        _generationServiceMock
            .Setup(x => x.GetGenerationStatusAsync(businessPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetGenerationStatus(businessPlanId, CancellationToken.None);

        // Assert
        var okResult = response.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(status);
    }

    [Fact]
    public async Task GetGenerationStatus_WithFailure_ShouldReturnBadRequest()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var result = Result.Failure<BusinessPlanGenerationStatus>("Business plan not found");

        _generationServiceMock
            .Setup(x => x.GetGenerationStatusAsync(businessPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetGenerationStatus(businessPlanId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetAvailableSections Tests

    [Fact]
    public void GetAvailableSections_ForBusinessPlan_ShouldReturnCorrectSections()
    {
        // Arrange
        var sections = new List<string>
        {
            "ExecutiveSummary", "ProblemStatement", "Solution", "MarketAnalysis",
            "CompetitiveAnalysis", "SwotAnalysis", "BusinessModel", "MarketingStrategy",
            "BrandingStrategy", "OperationsPlan", "ManagementTeam", "FinancialProjections",
            "FundingRequirements", "RiskAnalysis", "ExitStrategy"
        };

        _generationServiceMock
            .Setup(x => x.GetAvailableSections("0"))
            .Returns(sections);

        // Act
        var response = _sut.GetAvailableSections("0");

        // Assert
        var okResult = response.Should().BeOfType<OkObjectResult>().Subject;
        var returnedSections = okResult.Value.Should().BeAssignableTo<List<string>>().Subject;
        returnedSections.Should().Contain("ExecutiveSummary");
        returnedSections.Should().Contain("ExitStrategy");
    }

    [Fact]
    public void GetAvailableSections_ForStrategicPlan_ShouldReturnCorrectSections()
    {
        // Arrange
        var sections = new List<string>
        {
            "ExecutiveSummary", "ProblemStatement", "Solution", "MarketAnalysis",
            "CompetitiveAnalysis", "SwotAnalysis", "BusinessModel", "MarketingStrategy",
            "BrandingStrategy", "OperationsPlan", "ManagementTeam", "FinancialProjections",
            "FundingRequirements", "RiskAnalysis", "MissionStatement", "SocialImpact",
            "BeneficiaryProfile", "GrantStrategy", "SustainabilityPlan"
        };

        _generationServiceMock
            .Setup(x => x.GetAvailableSections("2"))
            .Returns(sections);

        // Act
        var response = _sut.GetAvailableSections("2");

        // Assert
        var okResult = response.Should().BeOfType<OkObjectResult>().Subject;
        var returnedSections = okResult.Value.Should().BeAssignableTo<List<string>>().Subject;
        returnedSections.Should().Contain("MissionStatement");
        returnedSections.Should().Contain("SocialImpact");
    }

    #endregion

    #region Helper Methods

    private BusinessPlan CreateBusinessPlan(Guid id)
    {
        var businessPlan = new BusinessPlan(
            "Test Business Plan",
            BusinessPlanType.BusinessPlan,
            Guid.NewGuid(),
            "Test Description");

        // Use reflection to set ID
        var idProperty = typeof(BusinessPlan).BaseType?.GetProperty("Id");
        idProperty?.SetValue(businessPlan, id);

        return businessPlan;
    }

    #endregion
}

