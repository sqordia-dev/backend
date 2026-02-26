using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Contracts.Requests;
using Sqordia.Application.Contracts.Responses;
using Sqordia.Application.Services;
using WebAPI.Controllers;
using System.Security.Claims;
using Xunit;

namespace Sqordia.WebAPI.IntegrationTests.Controllers;

public class SubscriptionControllerTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly Mock<IInvoicePdfService> _invoicePdfServiceMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ILogger<SubscriptionController>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly SubscriptionController _sut;

    public SubscriptionControllerTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _subscriptionServiceMock = new Mock<ISubscriptionService>();
        _stripeServiceMock = new Mock<IStripeService>();
        _invoicePdfServiceMock = new Mock<IInvoicePdfService>();
        _contextMock = new Mock<IApplicationDbContext>();
        _loggerMock = new Mock<ILogger<SubscriptionController>>();
        _configurationMock = new Mock<IConfiguration>();

        _sut = new SubscriptionController(
            _subscriptionServiceMock.Object,
            _stripeServiceMock.Object,
            _invoicePdfServiceMock.Object,
            _contextMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);
    }

    private void SetupAuthenticatedUser(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    private void SetupUnauthenticatedUser()
    {
        var httpContext = new Mock<HttpContext>();
        var user = new Mock<ClaimsPrincipal>();
        user.Setup(x => x.FindFirst(It.IsAny<string>())).Returns((Claim?)null);
        httpContext.Setup(x => x.User).Returns(user.Object);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext.Object
        };
    }

    // ========== GetPlans ==========

    [Fact]
    public async Task GetPlans_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var plans = _fixture.CreateMany<SubscriptionPlanDto>(3).ToList();
        var result = Result.Success(plans);

        _subscriptionServiceMock.Setup(x => x.GetPlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetPlans(CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(plans);
    }

    [Fact]
    public async Task GetPlans_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var error = Error.Failure("Subscription.GetPlans.Failed", "Failed to retrieve plans");
        var result = Result.Failure<List<SubscriptionPlanDto>>(error);

        _subscriptionServiceMock.Setup(x => x.GetPlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetPlans(CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    // ========== GetCurrentSubscription ==========

    [Fact]
    public async Task GetCurrentSubscription_WithValidUser_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscriptionDto = _fixture.Create<SubscriptionDto>();
        var result = Result.Success(subscriptionDto);

        _subscriptionServiceMock.Setup(x => x.GetCurrentSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.GetCurrentSubscription(CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(subscriptionDto);
    }

    [Fact]
    public async Task GetCurrentSubscription_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var error = Error.NotFound("Subscription.NotFound", "No subscription found");
        var result = Result.Failure<SubscriptionDto>(error);

        _subscriptionServiceMock.Setup(x => x.GetCurrentSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.GetCurrentSubscription(CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetCurrentSubscription_WithoutValidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act
        var response = await _sut.GetCurrentSubscription(CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedResult>();
    }

    // ========== Subscribe ==========

    [Fact]
    public async Task Subscribe_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = _fixture.Create<SubscribeRequest>();
        var subscriptionDto = _fixture.Create<SubscriptionDto>();
        var result = Result.Success(subscriptionDto);

        _subscriptionServiceMock.Setup(x => x.SubscribeAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.Subscribe(request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(subscriptionDto);
    }

    [Fact]
    public async Task Subscribe_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = _fixture.Create<SubscribeRequest>();
        var error = Error.Failure("Subscription.Subscribe.Failed", "Subscription failed");
        var result = Result.Failure<SubscriptionDto>(error);

        _subscriptionServiceMock.Setup(x => x.SubscribeAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.Subscribe(request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task Subscribe_WithoutValidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = _fixture.Create<SubscribeRequest>();
        SetupUnauthenticatedUser();

        // Act
        var response = await _sut.Subscribe(request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedResult>();
    }

    // ========== ChangePlan ==========

    [Fact]
    public async Task ChangePlan_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = _fixture.Create<ChangePlanRequest>();
        var subscriptionDto = _fixture.Create<SubscriptionDto>();
        var result = Result.Success(subscriptionDto);

        _subscriptionServiceMock.Setup(x => x.ChangePlanAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.ChangePlan(request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(subscriptionDto);
    }

    [Fact]
    public async Task ChangePlan_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = _fixture.Create<ChangePlanRequest>();
        var error = Error.Failure("Subscription.ChangePlan.Failed", "Plan change failed");
        var result = Result.Failure<SubscriptionDto>(error);

        _subscriptionServiceMock.Setup(x => x.ChangePlanAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.ChangePlan(request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task ChangePlan_WithoutValidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = _fixture.Create<ChangePlanRequest>();
        SetupUnauthenticatedUser();

        // Act
        var response = await _sut.ChangePlan(request, CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedResult>();
    }

    // ========== CancelSubscription ==========

    [Fact]
    public async Task CancelSubscription_WithValidUser_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var result = Result.Success(true);

        _subscriptionServiceMock.Setup(x => x.CancelSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.CancelSubscription(CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(true);
    }

    [Fact]
    public async Task CancelSubscription_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var error = Error.Failure("Subscription.Cancel.Failed", "Cancellation failed");
        var result = Result.Failure<bool>(error);

        _subscriptionServiceMock.Setup(x => x.CancelSubscriptionAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.CancelSubscription(CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task CancelSubscription_WithoutValidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act
        var response = await _sut.CancelSubscription(CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedResult>();
    }

    // ========== GetInvoices ==========

    [Fact]
    public async Task GetInvoices_WithValidUser_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invoices = _fixture.CreateMany<InvoiceDto>(3).ToList();
        var result = Result.Success(invoices);

        _subscriptionServiceMock.Setup(x => x.GetInvoicesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.GetInvoices(CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(invoices);
    }

    [Fact]
    public async Task GetInvoices_WhenServiceFails_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var error = Error.Failure("Subscription.Invoices.Failed", "Failed to retrieve invoices");
        var result = Result.Failure<List<InvoiceDto>>(error);

        _subscriptionServiceMock.Setup(x => x.GetInvoicesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        SetupAuthenticatedUser(userId);

        // Act
        var response = await _sut.GetInvoices(CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(error);
    }

    [Fact]
    public async Task GetInvoices_WithoutValidClaims_ShouldReturnUnauthorized()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act
        var response = await _sut.GetInvoices(CancellationToken.None);

        // Assert
        response.Should().BeOfType<UnauthorizedResult>();
    }

    // ========== GetOrganizationSubscription ==========

    [Fact]
    public async Task GetOrganizationSubscription_WithValidOrganization_ShouldReturnOkResult()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var subscriptionDto = _fixture.Create<SubscriptionDto>();
        var result = Result.Success(subscriptionDto);

        _subscriptionServiceMock.Setup(x => x.GetOrganizationSubscriptionAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetOrganizationSubscription(organizationId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<OkObjectResult>();
        var okResult = response as OkObjectResult;
        okResult!.Value.Should().Be(subscriptionDto);
    }

    [Fact]
    public async Task GetOrganizationSubscription_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var error = Error.NotFound("Subscription.NotFound", "No subscription found");
        var result = Result.Failure<SubscriptionDto>(error);

        _subscriptionServiceMock.Setup(x => x.GetOrganizationSubscriptionAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        var response = await _sut.GetOrganizationSubscription(organizationId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<NotFoundObjectResult>();
    }

    // ========== Service Verification ==========

    [Fact]
    public async Task GetPlans_ShouldCallServiceExactlyOnce()
    {
        // Arrange
        var plans = _fixture.CreateMany<SubscriptionPlanDto>(1).ToList();
        _subscriptionServiceMock.Setup(x => x.GetPlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(plans));

        // Act
        await _sut.GetPlans(CancellationToken.None);

        // Assert
        _subscriptionServiceMock.Verify(x => x.GetPlansAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Subscribe_ShouldCallServiceWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = _fixture.Create<SubscribeRequest>();
        var subscriptionDto = _fixture.Create<SubscriptionDto>();

        _subscriptionServiceMock.Setup(x => x.SubscribeAsync(userId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(subscriptionDto));

        SetupAuthenticatedUser(userId);

        // Act
        await _sut.Subscribe(request, CancellationToken.None);

        // Assert
        _subscriptionServiceMock.Verify(x => x.SubscribeAsync(userId, request, It.IsAny<CancellationToken>()), Times.Once);
    }
}
