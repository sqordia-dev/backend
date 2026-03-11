namespace Sqordia.Application.Services.AI;

/// <summary>
/// Detects conflicts between organization profile and questionnaire answers.
/// When they contradict, the AI receives confused context and produces incoherent output.
/// </summary>
public static class OrgProfileConflictDetector
{
    public record ConflictWarning(string Field, string OrgValue, string QuestionnaireValue, string Message);

    /// <summary>
    /// Detects misalignments between org profile fields and questionnaire answers.
    /// </summary>
    /// <param name="orgIndustry">Industry from org profile (e.g., "Technology")</param>
    /// <param name="orgTeamSize">Team size description from org profile</param>
    /// <param name="orgStage">Business stage from org profile (e.g., "Idea", "Revenue")</param>
    /// <param name="answers">Questionnaire answers by question number</param>
    public static List<ConflictWarning> DetectConflicts(
        string? orgIndustry,
        string? orgTeamSize,
        string? orgStage,
        Dictionary<int, string> answers)
    {
        var conflicts = new List<ConflictWarning>();

        // Q5 = Industry/Sector
        if (!string.IsNullOrWhiteSpace(orgIndustry) && answers.TryGetValue(5, out var sectorAnswer))
        {
            if (!HasOverlap(orgIndustry, sectorAnswer))
            {
                conflicts.Add(new ConflictWarning(
                    "Industry",
                    orgIndustry,
                    sectorAnswer.Length > 100 ? sectorAnswer[..100] + "..." : sectorAnswer,
                    "Organization profile industry does not match questionnaire sector answer. Prioritize questionnaire answer (Q5)."));
            }
        }

        // Q10 = Team/Promoters
        if (!string.IsNullOrWhiteSpace(orgTeamSize) && answers.TryGetValue(10, out var teamAnswer))
        {
            if (IsSoloConflict(orgTeamSize, teamAnswer))
            {
                conflicts.Add(new ConflictWarning(
                    "TeamSize",
                    orgTeamSize,
                    teamAnswer.Length > 100 ? teamAnswer[..100] + "..." : teamAnswer,
                    "Organization profile team size conflicts with questionnaire team description. Prioritize questionnaire answer (Q10)."));
            }
        }

        // Q13/Q14 = Financial context vs stage
        if (!string.IsNullOrWhiteSpace(orgStage))
        {
            var hasRevenue = answers.TryGetValue(13, out var investAnswer) &&
                             !string.IsNullOrWhiteSpace(investAnswer) &&
                             ContainsFinancialIndicators(investAnswer);

            var isIdeaStage = orgStage.Contains("idea", StringComparison.OrdinalIgnoreCase) ||
                              orgStage.Contains("idée", StringComparison.OrdinalIgnoreCase);

            if (isIdeaStage && hasRevenue)
            {
                conflicts.Add(new ConflictWarning(
                    "BusinessStage",
                    orgStage,
                    investAnswer!.Length > 100 ? investAnswer[..100] + "..." : investAnswer!,
                    "Organization marked as 'idea stage' but questionnaire mentions existing revenue/investment. Prioritize questionnaire answers (Q13/Q14)."));
            }
        }

        return conflicts;
    }

    /// <summary>
    /// Formats detected conflicts as a prompt block.
    /// </summary>
    public static string? FormatForPrompt(List<ConflictWarning> conflicts, string language = "fr")
    {
        if (conflicts.Count == 0) return null;

        var isFr = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var lines = new List<string>
        {
            isFr
                ? "=== NOTES SUR LA QUALITÉ DES DONNÉES ==="
                : "=== DATA QUALITY NOTES ===",
            "",
            isFr
                ? "Les incohérences suivantes ont été détectées entre le profil de l'organisation et les réponses au questionnaire. Priorisez les réponses au questionnaire en cas de conflit :"
                : "The following inconsistencies were detected between the organization profile and questionnaire answers. Prioritize questionnaire answers when they conflict:"
        };

        foreach (var c in conflicts)
        {
            lines.Add($"  - {c.Field}: {c.Message}");
        }

        return string.Join("\n", lines);
    }

    private static bool HasOverlap(string orgValue, string questionnaireValue)
    {
        var orgWords = ExtractKeywords(orgValue);
        var qaWords = ExtractKeywords(questionnaireValue);
        return orgWords.Overlaps(qaWords);
    }

    private static bool IsSoloConflict(string orgTeamSize, string teamAnswer)
    {
        var soloIndicators = new[] { "solo", "seul", "1 person", "une personne", "just me", "moi seul" };
        var teamIndicators = new[] { "équipe", "team", "associé", "partner", "cofound", "co-fond", "employé", "employee" };

        var orgIsSolo = soloIndicators.Any(s => orgTeamSize.Contains(s, StringComparison.OrdinalIgnoreCase));
        var answerHasTeam = teamIndicators.Any(s => teamAnswer.Contains(s, StringComparison.OrdinalIgnoreCase));

        return orgIsSolo && answerHasTeam;
    }

    private static bool ContainsFinancialIndicators(string text)
    {
        var indicators = new[] { "$", "revenue", "revenu", "chiffre d'affaires", "ventes", "sales", "profit" };
        return indicators.Any(i => text.Contains(i, StringComparison.OrdinalIgnoreCase));
    }

    private static HashSet<string> ExtractKeywords(string text)
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "de", "la", "le", "les", "du", "des", "et", "en", "un", "une",
            "the", "and", "of", "in", "a", "an", "for", "to", "is", "are"
        };

        return text.Split(new[] { ' ', ',', '.', ';', ':', '-', '/', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.ToLowerInvariant().Trim())
            .Where(w => w.Length > 3 && !stopWords.Contains(w))
            .ToHashSet();
    }
}
