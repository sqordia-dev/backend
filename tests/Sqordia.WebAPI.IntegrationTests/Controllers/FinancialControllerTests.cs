using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Commands;
using Sqordia.Application.Financial.Queries;
using Sqordia.Application.Financial.Services;
using Sqordia.Domain.Enums;
using WebAPI.Controllers;
using Xunit;

namespace Sqordia.WebAPI.IntegrationTests.Controllers;

public class FinancialControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IFinancialService> _financialServiceMock;
    private readonly FinancialController _sut;

    public FinancialControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _financialServiceMock = new Mock<IFinancialService>();

        _sut = new FinancialController(_financialServiceMock.Object);
    }

    // ========== CreateFinancialProjection ==========

    [Fact]
    public async Task CreateFinancialProjection_WithValidCommand_ShouldReturnOkResult()
    {
        // Arrange
        var command = _fixture.Create<CreateFinancialProjectionCommand>();
        var projectionDto = _fixture.Create<FinancialProjectionDto>();
        var result = Result.Success(projectionDto);

        _financialServiceMock.Setup(x => x.CreateFinancialProjectionAsync(command))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CreateFinancialProjection(command);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(projectionDto);
    }

    [Fact]
    public async Task CreateFinancialProjection_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var command = _fixture.Create<CreateFinancialProjectionCommand>();
        var error = Error.Failure("Financial.Projection.CreateFailed", "Failed to create financial projection");
        var result = Result.Failure<FinancialProjectionDto>(error);

        _financialServiceMock.Setup(x => x.CreateFinancialProjectionAsync(command))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CreateFinancialProjection(command);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    // ========== GetFinancialProjection ==========

    [Fact]
    public async Task GetFinancialProjection_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var projectionId = Guid.NewGuid();
        var projectionDto = _fixture.Create<FinancialProjectionDto>();
        var result = Result.Success(projectionDto);

        _financialServiceMock.Setup(x => x.GetFinancialProjectionByIdAsync(projectionId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetFinancialProjection(projectionId);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(projectionDto);
    }

    [Fact]
    public async Task GetFinancialProjection_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var projectionId = Guid.NewGuid();
        var error = Error.NotFound("Financial.Projection.NotFound", "Financial projection not found");
        var result = Result.Failure<FinancialProjectionDto>(error);

        _financialServiceMock.Setup(x => x.GetFinancialProjectionByIdAsync(projectionId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetFinancialProjection(projectionId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    // ========== GetFinancialProjectionsByBusinessPlan ==========

    [Fact]
    public async Task GetFinancialProjectionsByBusinessPlan_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var projections = _fixture.CreateMany<FinancialProjectionDto>(3).ToList();
        var result = Result.Success(projections);

        _financialServiceMock.Setup(x => x.GetFinancialProjectionsByBusinessPlanAsync(businessPlanId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetFinancialProjectionsByBusinessPlan(businessPlanId);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(projections);
    }

    [Fact]
    public async Task GetFinancialProjectionsByBusinessPlan_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var error = Error.NotFound("Financial.BusinessPlan.NotFound", "Business plan not found");
        var result = Result.Failure<List<FinancialProjectionDto>>(error);

        _financialServiceMock.Setup(x => x.GetFinancialProjectionsByBusinessPlanAsync(businessPlanId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetFinancialProjectionsByBusinessPlan(businessPlanId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    // ========== GetFinancialProjectionsByScenario ==========

    [Fact]
    public async Task GetFinancialProjectionsByScenario_WithValidParameters_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var scenario = ScenarioType.Optimistic;
        var projections = _fixture.CreateMany<FinancialProjectionDto>(2).ToList();
        var result = Result.Success(projections);

        _financialServiceMock.Setup(x => x.GetFinancialProjectionsByScenarioAsync(businessPlanId, scenario))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetFinancialProjectionsByScenario(businessPlanId, scenario);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(projections);
    }

    // ========== UpdateFinancialProjection ==========

    [Fact]
    public async Task UpdateFinancialProjection_WithValidCommand_ShouldReturnOkResult()
    {
        // Arrange
        var projectionId = Guid.NewGuid();
        var command = _fixture.Create<UpdateFinancialProjectionCommand>();
        var projectionDto = _fixture.Create<FinancialProjectionDto>();
        var result = Result.Success(projectionDto);

        _financialServiceMock.Setup(x => x.UpdateFinancialProjectionAsync(It.Is<UpdateFinancialProjectionCommand>(
                c => c.Id == projectionId)))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateFinancialProjection(projectionId, command);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(projectionDto);
    }

    [Fact]
    public async Task UpdateFinancialProjection_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var projectionId = Guid.NewGuid();
        var command = _fixture.Create<UpdateFinancialProjectionCommand>();
        var error = Error.NotFound("Financial.Projection.NotFound", "Financial projection not found");
        var result = Result.Failure<FinancialProjectionDto>(error);

        _financialServiceMock.Setup(x => x.UpdateFinancialProjectionAsync(It.Is<UpdateFinancialProjectionCommand>(
                c => c.Id == projectionId)))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.UpdateFinancialProjection(projectionId, command);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    // ========== DeleteFinancialProjection ==========

    [Fact]
    public async Task DeleteFinancialProjection_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var projectionId = Guid.NewGuid();
        var result = Result.Success(true);

        _financialServiceMock.Setup(x => x.DeleteFinancialProjectionAsync(projectionId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DeleteFinancialProjection(projectionId);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(true);
    }

    [Fact]
    public async Task DeleteFinancialProjection_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var projectionId = Guid.NewGuid();
        var error = Error.NotFound("Financial.Projection.NotFound", "Financial projection not found");
        var result = Result.Failure<bool>(error);

        _financialServiceMock.Setup(x => x.DeleteFinancialProjectionAsync(projectionId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.DeleteFinancialProjection(projectionId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    // ========== GetAllCurrencies ==========

    [Fact]
    public async Task GetAllCurrencies_ShouldReturnOkResult()
    {
        // Arrange
        var currencies = _fixture.CreateMany<CurrencyDto>(5).ToList();
        var result = Result.Success(currencies);

        _financialServiceMock.Setup(x => x.GetAllCurrenciesAsync())
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetAllCurrencies();

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(currencies);
    }

    [Fact]
    public async Task GetAllCurrencies_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var error = Error.Failure("Financial.Currencies.Failed", "Failed to retrieve currencies");
        var result = Result.Failure<List<CurrencyDto>>(error);

        _financialServiceMock.Setup(x => x.GetAllCurrenciesAsync())
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetAllCurrencies();

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    // ========== GetCurrency ==========

    [Fact]
    public async Task GetCurrency_WithValidCode_ShouldReturnOkResult()
    {
        // Arrange
        var currencyCode = "CAD";
        var currencyDto = _fixture.Create<CurrencyDto>();
        var result = Result.Success(currencyDto);

        _financialServiceMock.Setup(x => x.GetCurrencyAsync(currencyCode))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetCurrency(currencyCode);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(currencyDto);
    }

    [Fact]
    public async Task GetCurrency_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var currencyCode = "INVALID";
        var error = Error.NotFound("Financial.Currency.NotFound", "Currency not found");
        var result = Result.Failure<CurrencyDto>(error);

        _financialServiceMock.Setup(x => x.GetCurrencyAsync(currencyCode))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetCurrency(currencyCode);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    // ========== CalculateKPIs ==========

    [Fact]
    public async Task CalculateKPIs_WithValidBusinessPlanId_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var kpis = _fixture.CreateMany<FinancialKPIDto>(4).ToList();
        var result = Result.Success(kpis);

        _financialServiceMock.Setup(x => x.CalculateKPIsAsync(businessPlanId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CalculateKPIs(businessPlanId);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(kpis);
    }

    [Fact]
    public async Task CalculateKPIs_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var error = Error.NotFound("Financial.KPIs.NotFound", "Business plan not found for KPI calculation");
        var result = Result.Failure<List<FinancialKPIDto>>(error);

        _financialServiceMock.Setup(x => x.CalculateKPIsAsync(businessPlanId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CalculateKPIs(businessPlanId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    // ========== GetKPIByName ==========

    [Fact]
    public async Task GetKPIByName_WithValidParameters_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var kpiName = "GrossMargin";
        var kpiDto = _fixture.Create<FinancialKPIDto>();
        var result = Result.Success(kpiDto);

        _financialServiceMock.Setup(x => x.GetKPIByNameAsync(businessPlanId, kpiName))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetKPIByName(businessPlanId, kpiName);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(kpiDto);
    }

    [Fact]
    public async Task GetKPIByName_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var kpiName = "NonExistent";
        var error = Error.NotFound("Financial.KPI.NotFound", "KPI not found");
        var result = Result.Failure<FinancialKPIDto>(error);

        _financialServiceMock.Setup(x => x.GetKPIByNameAsync(businessPlanId, kpiName))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetKPIByName(businessPlanId, kpiName);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    // ========== GenerateFinancialReport ==========

    [Fact]
    public async Task GenerateFinancialReport_WithValidParameters_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var reportType = "summary";
        var reportDto = _fixture.Create<FinancialReportDto>();
        var result = Result.Success(reportDto);

        _financialServiceMock.Setup(x => x.GenerateFinancialReportAsync(businessPlanId, reportType))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GenerateFinancialReport(businessPlanId, reportType);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(reportDto);
    }

    [Fact]
    public async Task GenerateFinancialReport_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var reportType = "summary";
        var error = Error.NotFound("Financial.Report.NotFound", "Business plan not found for report generation");
        var result = Result.Failure<FinancialReportDto>(error);

        _financialServiceMock.Setup(x => x.GenerateFinancialReportAsync(businessPlanId, reportType))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GenerateFinancialReport(businessPlanId, reportType);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    // ========== GenerateCashFlowReport ==========

    [Fact]
    public async Task GenerateCashFlowReport_WithValidBusinessPlanId_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var reportDto = _fixture.Create<CashFlowReportDto>();
        var result = Result.Success(reportDto);

        _financialServiceMock.Setup(x => x.GenerateCashFlowReportAsync(businessPlanId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GenerateCashFlowReport(businessPlanId);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(reportDto);
    }

    // ========== GenerateProfitLossReport ==========

    [Fact]
    public async Task GenerateProfitLossReport_WithValidBusinessPlanId_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var reportDto = _fixture.Create<ProfitLossReportDto>();
        var result = Result.Success(reportDto);

        _financialServiceMock.Setup(x => x.GenerateProfitLossReportAsync(businessPlanId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GenerateProfitLossReport(businessPlanId);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(reportDto);
    }

    // ========== GenerateBalanceSheetReport ==========

    [Fact]
    public async Task GenerateBalanceSheetReport_WithValidBusinessPlanId_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var reportDto = _fixture.Create<BalanceSheetReportDto>();
        var result = Result.Success(reportDto);

        _financialServiceMock.Setup(x => x.GenerateBalanceSheetReportAsync(businessPlanId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GenerateBalanceSheetReport(businessPlanId);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(reportDto);
    }

    // ========== ConvertCurrency ==========

    [Fact]
    public async Task ConvertCurrency_WithValidParameters_ShouldReturnOkResult()
    {
        // Arrange
        var amount = 100.00m;
        var fromCurrency = "CAD";
        var toCurrency = "USD";
        var convertedAmount = 75.50m;
        var result = Result.Success(convertedAmount);

        _financialServiceMock.Setup(x => x.ConvertCurrencyAsync(amount, fromCurrency, toCurrency))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ConvertCurrency(amount, fromCurrency, toCurrency);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(convertedAmount);
    }

    [Fact]
    public async Task ConvertCurrency_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var amount = 100.00m;
        var fromCurrency = "INVALID";
        var toCurrency = "USD";
        var error = Error.Failure("Financial.Currency.ConversionFailed", "Currency conversion failed");
        var result = Result.Failure<decimal>(error);

        _financialServiceMock.Setup(x => x.ConvertCurrencyAsync(amount, fromCurrency, toCurrency))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.ConvertCurrency(amount, fromCurrency, toCurrency);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    // ========== CalculateBreakEven ==========

    [Fact]
    public async Task CalculateBreakEven_WithValidBusinessPlanId_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var breakEvenDto = _fixture.Create<BreakEvenAnalysisDto>();
        var result = Result.Success(breakEvenDto);

        _financialServiceMock.Setup(x => x.CalculateBreakEvenAsync(businessPlanId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CalculateBreakEven(businessPlanId);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(breakEvenDto);
    }

    [Fact]
    public async Task CalculateBreakEven_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var error = Error.NotFound("Financial.BreakEven.NotFound", "Business plan not found for break-even analysis");
        var result = Result.Failure<BreakEvenAnalysisDto>(error);

        _financialServiceMock.Setup(x => x.CalculateBreakEvenAsync(businessPlanId))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CalculateBreakEven(businessPlanId);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = response as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be(error);
    }

    // ========== CalculateTax ==========

    [Fact]
    public async Task CalculateTax_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = _fixture.Create<TaxCalculationRequest>();
        var taxDto = _fixture.Create<TaxCalculationDto>();
        var result = Result.Success(taxDto);

        _financialServiceMock.Setup(x => x.CalculateTaxAsync(request))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CalculateTax(request);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(taxDto);
    }

    [Fact]
    public async Task CalculateTax_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var request = _fixture.Create<TaxCalculationRequest>();
        var error = Error.Failure("Financial.Tax.CalculationFailed", "Tax calculation failed");
        var result = Result.Failure<TaxCalculationDto>(error);

        _financialServiceMock.Setup(x => x.CalculateTaxAsync(request))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CalculateTax(request);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    // ========== CalculateROI ==========

    [Fact]
    public async Task CalculateROI_WithValidParameters_ShouldReturnOkResult()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        var investmentAmount = 50000m;
        var analysisDto = _fixture.Create<InvestmentAnalysisDto>();
        var result = Result.Success(analysisDto);

        _financialServiceMock.Setup(x => x.CalculateROIAsync(businessPlanId, investmentAmount))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.CalculateROI(businessPlanId, investmentAmount);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(analysisDto);
    }

    // ========== Service Verification ==========

    [Fact]
    public async Task CreateFinancialProjection_ShouldCallServiceExactlyOnce()
    {
        // Arrange
        var command = _fixture.Create<CreateFinancialProjectionCommand>();
        var projectionDto = _fixture.Create<FinancialProjectionDto>();

        _financialServiceMock.Setup(x => x.CreateFinancialProjectionAsync(command))
            .ReturnsAsync(Result.Success(projectionDto));

        // Act
        await _sut.CreateFinancialProjection(command);

        // Assert
        _financialServiceMock.Verify(x => x.CreateFinancialProjectionAsync(command), Times.Once);
    }

    [Fact]
    public async Task GetFinancialProjection_ShouldCallServiceWithCorrectId()
    {
        // Arrange
        var projectionId = Guid.NewGuid();
        var projectionDto = _fixture.Create<FinancialProjectionDto>();

        _financialServiceMock.Setup(x => x.GetFinancialProjectionByIdAsync(projectionId))
            .ReturnsAsync(Result.Success(projectionDto));

        // Act
        await _sut.GetFinancialProjection(projectionId);

        // Assert
        _financialServiceMock.Verify(x => x.GetFinancialProjectionByIdAsync(projectionId), Times.Once);
    }

    [Fact]
    public async Task GetAllCurrencies_ShouldCallServiceExactlyOnce()
    {
        // Arrange
        var currencies = _fixture.CreateMany<CurrencyDto>(3).ToList();

        _financialServiceMock.Setup(x => x.GetAllCurrenciesAsync())
            .ReturnsAsync(Result.Success(currencies));

        // Act
        await _sut.GetAllCurrencies();

        // Assert
        _financialServiceMock.Verify(x => x.GetAllCurrenciesAsync(), Times.Once);
    }
}
