using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Services.Implementations;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.UnitTests.Services.AI;

public class QuestionnaireContextServiceTests
{
    private readonly QuestionnaireContextService _sut;

    public QuestionnaireContextServiceTests()
    {
        var mockContext = new Mock<IApplicationDbContext>();
        var mockLogger = new Mock<ILogger<QuestionnaireContextService>>();
        _sut = new QuestionnaireContextService(mockContext.Object, mockLogger.Object);
    }

    [Fact]
    public void BuildAnswersDictionary_EmptyCollection_ReturnsEmptyDictionary()
    {
        var responses = new List<QuestionnaireResponse>();

        var result = _sut.BuildAnswersDictionary(responses);

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildAnswersDictionary_NullCollection_ReturnsEmptyDictionary()
    {
        var result = _sut.BuildAnswersDictionary(null!);

        result.Should().BeEmpty();
    }

    [Fact]
    public void BuildAnswersDictionary_ResponsesWithNoTemplates_UsesFallbackOrdering()
    {
        var planId = Guid.NewGuid();
        var responses = new List<QuestionnaireResponse>
        {
            CreateResponseWithText(planId, "First answer"),
            CreateResponseWithText(planId, "Second answer"),
        };

        var result = _sut.BuildAnswersDictionary(responses);

        result.Should().HaveCount(2);
        result[1].Should().Be("First answer");
        result[2].Should().Be("Second answer");
    }

    [Fact]
    public void BuildAnswersDictionary_SkipsBlankResponses()
    {
        var planId = Guid.NewGuid();
        var responses = new List<QuestionnaireResponse>
        {
            CreateResponseWithText(planId, "Answer one"),
            CreateResponseWithText(planId, ""),
            CreateResponseWithText(planId, "Answer three"),
        };

        var result = _sut.BuildAnswersDictionary(responses);

        result.Should().HaveCount(2);
        result[1].Should().Be("Answer one");
        result[2].Should().Be("Answer three");
    }

    private static QuestionnaireResponse CreateResponseWithText(Guid planId, string text)
    {
        // Use V1 constructor (simplest path for fallback testing)
        return new QuestionnaireResponse(planId, Guid.NewGuid(), text);
    }
}
