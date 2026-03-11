using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Sqordia.Application.Services.Implementations;

namespace Sqordia.Application.UnitTests.Services.AI;

public class ConditionalLogicEvaluatorTests
{
    private readonly ConditionalLogicEvaluator _sut;

    public ConditionalLogicEvaluatorTests()
    {
        var logger = new Mock<ILogger<ConditionalLogicEvaluator>>();
        _sut = new ConditionalLogicEvaluator(logger.Object);
    }

    [Fact]
    public void ShouldShow_NullJson_ReturnsTrue()
    {
        var answers = new Dictionary<int, string>();
        _sut.ShouldShow(null, answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_EmptyJson_ReturnsTrue()
    {
        var answers = new Dictionary<int, string>();
        _sut.ShouldShow("", answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_SimpleEquals_MatchingAnswer_ReturnsTrue()
    {
        var json = """{"showIf": "question5", "equals": "Yes"}""";
        var answers = new Dictionary<int, string> { { 5, "Yes" } };

        _sut.ShouldShow(json, answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_SimpleEquals_NonMatchingAnswer_ReturnsFalse()
    {
        var json = """{"showIf": "question5", "equals": "Yes"}""";
        var answers = new Dictionary<int, string> { { 5, "No" } };

        _sut.ShouldShow(json, answers).Should().BeFalse();
    }

    [Fact]
    public void ShouldShow_SimpleEquals_CaseInsensitive()
    {
        var json = """{"showIf": "question5", "equals": "Yes"}""";
        var answers = new Dictionary<int, string> { { 5, "yes" } };

        _sut.ShouldShow(json, answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_SimpleShowIf_QuestionNotAnswered_ReturnsFalse()
    {
        var json = """{"showIf": "question5", "equals": "Yes"}""";
        var answers = new Dictionary<int, string>();

        _sut.ShouldShow(json, answers).Should().BeFalse();
    }

    [Fact]
    public void ShouldShow_SimpleShowIf_NoComparator_AnswerExists_ReturnsTrue()
    {
        var json = """{"showIf": "question5"}""";
        var answers = new Dictionary<int, string> { { 5, "any answer" } };

        _sut.ShouldShow(json, answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_CompoundAnd_AllTrue_ReturnsTrue()
    {
        var json = """
        {
            "operator": "AND",
            "conditions": [
                {"field": "question5", "operator": "equals", "value": "Yes"},
                {"field": "question7", "operator": "isNotEmpty"}
            ]
        }
        """;
        var answers = new Dictionary<int, string> { { 5, "Yes" }, { 7, "Some value" } };

        _sut.ShouldShow(json, answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_CompoundAnd_OneFalse_ReturnsFalse()
    {
        var json = """
        {
            "operator": "AND",
            "conditions": [
                {"field": "question5", "operator": "equals", "value": "Yes"},
                {"field": "question7", "operator": "equals", "value": "No"}
            ]
        }
        """;
        var answers = new Dictionary<int, string> { { 5, "Yes" }, { 7, "Yes" } };

        _sut.ShouldShow(json, answers).Should().BeFalse();
    }

    [Fact]
    public void ShouldShow_CompoundOr_OneTrue_ReturnsTrue()
    {
        var json = """
        {
            "operator": "OR",
            "conditions": [
                {"field": "question5", "operator": "equals", "value": "Yes"},
                {"field": "question7", "operator": "equals", "value": "No"}
            ]
        }
        """;
        var answers = new Dictionary<int, string> { { 5, "Yes" }, { 7, "Yes" } };

        _sut.ShouldShow(json, answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_Contains_MatchFound_ReturnsTrue()
    {
        var json = """
        {
            "field": "question3",
            "operator": "contains",
            "value": "technology"
        }
        """;
        var answers = new Dictionary<int, string> { { 3, "We are a technology company" } };

        _sut.ShouldShow(json, answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_GreaterThan_NumericValue_ReturnsTrue()
    {
        var json = """
        {
            "field": "question13",
            "operator": "greaterThan",
            "value": "50000"
        }
        """;
        var answers = new Dictionary<int, string> { { 13, "100000" } };

        _sut.ShouldShow(json, answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_GreaterThan_NumericValue_BelowThreshold_ReturnsFalse()
    {
        var json = """
        {
            "field": "question13",
            "operator": "greaterThan",
            "value": "50000"
        }
        """;
        var answers = new Dictionary<int, string> { { 13, "10000" } };

        _sut.ShouldShow(json, answers).Should().BeFalse();
    }

    [Fact]
    public void ShouldShow_IsEmpty_EmptyAnswer_ReturnsTrue()
    {
        var json = """{"field": "question10", "operator": "isEmpty"}""";
        var answers = new Dictionary<int, string>();

        _sut.ShouldShow(json, answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_IsNotEmpty_HasAnswer_ReturnsTrue()
    {
        var json = """{"field": "question10", "operator": "isNotEmpty"}""";
        var answers = new Dictionary<int, string> { { 10, "has content" } };

        _sut.ShouldShow(json, answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_InvalidJson_ReturnsTrue()
    {
        var answers = new Dictionary<int, string>();
        _sut.ShouldShow("not valid json", answers).Should().BeTrue();
    }

    [Theory]
    [InlineData("question5", 5)]
    [InlineData("q5", 5)]
    [InlineData("5", 5)]
    [InlineData("question22", 22)]
    public void ShouldShow_VariousFieldFormats_ParsesCorrectly(string field, int expectedQuestion)
    {
        var json = $$$"""{"showIf": "{{{field}}}", "equals": "Yes"}""";
        var answers = new Dictionary<int, string> { { expectedQuestion, "Yes" } };

        _sut.ShouldShow(json, answers).Should().BeTrue();
    }

    [Fact]
    public void ShouldShow_NumericAnswers_UsedForComparison()
    {
        var json = """{"field": "question13", "operator": "greaterThan", "value": "50000"}""";
        var answers = new Dictionary<int, string> { { 13, "some text" } };
        var numericAnswers = new Dictionary<int, decimal> { { 13, 75000m } };

        _sut.ShouldShow(json, answers, numericAnswers).Should().BeTrue();
    }
}
