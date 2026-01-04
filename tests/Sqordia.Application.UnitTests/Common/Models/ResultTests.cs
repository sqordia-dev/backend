using AutoFixture;
using FluentAssertions;
using Sqordia.Application.Common.Models;
using Xunit;

namespace Sqordia.Application.UnitTests.Common.Models;

public class ResultTests
{
    private readonly IFixture _fixture;

    public ResultTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void Success_WithValue_ShouldReturnSuccessResult()
    {
        // Arrange
        var value = _fixture.Create<string>();

        // Act
        var result = Result.Success(value);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Success_WithoutValue_ShouldReturnSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_WithError_ShouldReturnFailureResult()
    {
        // Arrange
        var error = _fixture.Create<Error>();

        // Act
        var result = Result.Failure(error);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WithErrorMessage_ShouldReturnFailureResult()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("General.Failure");
        result.Error.Message.Should().Be(errorMessage);
    }

    [Fact]
    public void Failure_WithGenericError_ShouldReturnFailureResult()
    {
        // Arrange
        var error = _fixture.Create<Error>();

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Value.Should().Be(default(string));
    }

    [Fact]
    public void Failure_WithGenericErrorMessage_ShouldReturnFailureResult()
    {
        // Arrange
        var errorMessage = _fixture.Create<string>();

        // Act
        var result = Result.Failure<string>(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("General.Failure");
        result.Error.Message.Should().Be(errorMessage);
        result.Value.Should().Be(default(string));
    }
}