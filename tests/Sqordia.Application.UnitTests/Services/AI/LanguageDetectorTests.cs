using FluentAssertions;
using Sqordia.Application.Services.AI;

namespace Sqordia.Application.UnitTests.Services.AI;

public class LanguageDetectorTests
{
    [Fact]
    public void DetectLanguageMismatch_FrenchTextExpectingFrench_ReturnsCorrect()
    {
        var frenchText = "Le marché des technologies est en pleine croissance. Les entreprises cherchent des solutions innovantes pour améliorer leur gestion. Notre marché cible est très large et offre de grandes opportunités dans le secteur manufacturier du Québec.";

        var result = LanguageDetector.DetectLanguageMismatch(frenchText, "fr");

        result.IsCorrectLanguage.Should().BeTrue();
        result.DetectedLanguage.Should().Be("fr");
    }

    [Fact]
    public void DetectLanguageMismatch_EnglishTextExpectingEnglish_ReturnsCorrect()
    {
        var englishText = "The technology market is growing rapidly. Companies are looking for innovative solutions to improve their management processes. Our target market is very large and offers great opportunities in the manufacturing sector.";

        var result = LanguageDetector.DetectLanguageMismatch(englishText, "en");

        result.IsCorrectLanguage.Should().BeTrue();
        result.DetectedLanguage.Should().Be("en");
    }

    [Fact]
    public void DetectLanguageMismatch_EnglishTextExpectingFrench_ReturnsMismatch()
    {
        var englishText = "The technology market is growing rapidly. Companies are looking for innovative solutions to improve their management processes. Our target market is very large and offers great opportunities in the manufacturing sector.";

        var result = LanguageDetector.DetectLanguageMismatch(englishText, "fr");

        result.IsCorrectLanguage.Should().BeFalse();
        result.DetectedLanguage.Should().Be("en");
    }

    [Fact]
    public void DetectLanguageMismatch_FrenchTextExpectingEnglish_ReturnsMismatch()
    {
        var frenchText = "Le marché des technologies est en pleine croissance. Les entreprises cherchent des solutions innovantes pour améliorer leur gestion. Notre marché cible est très large et offre de grandes opportunités dans le secteur manufacturier du Québec.";

        var result = LanguageDetector.DetectLanguageMismatch(frenchText, "en");

        result.IsCorrectLanguage.Should().BeFalse();
        result.DetectedLanguage.Should().Be("fr");
    }

    [Fact]
    public void DetectLanguageMismatch_EmptyContent_ReturnsCorrect()
    {
        var result = LanguageDetector.DetectLanguageMismatch("", "fr");

        result.IsCorrectLanguage.Should().BeTrue();
    }

    [Fact]
    public void DetectLanguageMismatch_FewWords_ReturnsCorrectWithLowConfidence()
    {
        var result = LanguageDetector.DetectLanguageMismatch("Hello world", "en");

        result.IsCorrectLanguage.Should().BeTrue();
        result.Confidence.Should().BeLessThanOrEqualTo(0.5);
    }

    [Fact]
    public void DetectLanguageMismatch_FrenchWithTechnicalEnglishTerms_AllowsMixedContent()
    {
        // French text with some English technical terms should still be detected as French
        var mixedText = "Le marché du SaaS B2B est en pleine croissance avec un CAGR de 11%. Les solutions de compliance management sont très demandées par les PME. Notre API REST et notre dashboard offrent une expérience utilisateur optimale.";

        var result = LanguageDetector.DetectLanguageMismatch(mixedText, "fr");

        result.IsCorrectLanguage.Should().BeTrue();
    }
}
