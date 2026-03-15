namespace Sqordia.Application.Common.Constants;

/// <summary>
/// Constants for the AI generation pipeline and ML integration.
/// Eliminates magic numbers across services.
/// </summary>
public static class PipelineConstants
{
    /// <summary>Total questions in the STRUCTURE FINALE questionnaire (V3).</summary>
    public const int TotalQuestionnaireQuestions = 22;

    /// <summary>Maximum words per side for the LCS diff algorithm.</summary>
    public const int MaxDiffWords = 2000;

    /// <summary>Minimum edit ratio to record a section edit for ML training (2%).</summary>
    public const double MinEditRatioThreshold = 0.02;

    /// <summary>Minimum ML confidence to trigger auto-regeneration.</summary>
    public const double MinRegenerationConfidence = 0.5;

    /// <summary>Minimum learned preference confidence to include in prompts.</summary>
    public const double MinPreferenceConfidence = 0.3;

    /// <summary>Minimum sample count for a learned preference to be included in prompts.</summary>
    public const int MinPreferenceSamples = 3;

    /// <summary>Maximum tokens for section generation (includes visual element JSON blocks).</summary>
    public const int SectionMaxTokens = 6000;

    /// <summary>Maximum tokens for Business Brief generation (complex 8-object JSON structure).</summary>
    public const int BusinessBriefMaxTokens = 8000;

    /// <summary>Maximum tokens for analysis/review passes.</summary>
    public const int AnalysisMaxTokens = 3000;

    /// <summary>Default max retries for AI calls.</summary>
    public const int DefaultMaxRetries = 3;

    /// <summary>Reduced retries for retry/regen calls.</summary>
    public const int ReducedMaxRetries = 2;

    /// <summary>Temperature for analysis and review passes (low for precision).</summary>
    public const float AnalysisTemperature = 0.3f;

    /// <summary>Minimum temperature floor for ML-triggered regeneration.</summary>
    public const float MinRegenerationTemperature = 0.3f;

    /// <summary>Temperature reduction for ML-triggered regeneration.</summary>
    public const float RegenerationTemperatureReduction = 0.1f;

    /// <summary>Default temperature fallback.</summary>
    public const float DefaultTemperature = 0.6f;

    /// <summary>Maximum character length per answer in section context.</summary>
    public const int SectionAnswerMaxChars = 800;

    /// <summary>Maximum character length per answer in full context.</summary>
    public const int FullContextAnswerMaxChars = 500;

    /// <summary>Maximum character length for business context summary answers.</summary>
    public const int SummaryAnswerMaxChars = 200;

    /// <summary>Maximum character length for sector context in summary.</summary>
    public const int SectorAnswerMaxChars = 150;

    // Pipeline pass identifiers
    public static class PipelinePass
    {
        public const string AnalysisPlan = "Pass1-AnalysisPlan";
        public const string Section = "Pass2-Section";
        public const string LanguageRetry = "Pass2-LangRetry";
        public const string MlRegeneration = "Pass2-MLRegen";
        public const string Review = "Pass3-Review";
        public const string BusinessBrief = "BusinessBrief";
    }
}
