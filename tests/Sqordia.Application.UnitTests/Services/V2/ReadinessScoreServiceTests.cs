using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services.V2.Implementations;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.Enums;
using Xunit;

namespace Sqordia.Application.UnitTests.Services.V2;

public class ReadinessScoreServiceTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<ReadinessScoreService>> _loggerMock;
    private readonly ReadinessScoreService _sut;

    public ReadinessScoreServiceTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<ReadinessScoreService>>();

        _sut = new ReadinessScoreService(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WhenUserNotAuthenticated_ShouldReturnNotFound()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns((string?)null);

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task CalculateReadinessScoreAsync_WhenUserIdNotValidGuid_ShouldReturnNotFound()
    {
        // Arrange
        var businessPlanId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns("invalid-guid");

        // Act
        var result = await _sut.CalculateReadinessScoreAsync(businessPlanId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Contain("NotFound");
    }
}
