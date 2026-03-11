namespace Sqordia.Application.Services.AI;

/// <summary>
/// Maps question relationships based on STRUCTURE FINALE section prompts.
/// Each section of the business plan references specific question IDs.
/// This mapper provides context-aware question relationships for AI coaching.
/// </summary>
public static class QuestionContextMapper
{
    /// <summary>
    /// Question descriptions for building context summaries.
    /// Maps question number to what information it captures.
    /// </summary>
    public static readonly Dictionary<int, QuestionInfo> QuestionInfoMap = new()
    {
        { 1, new QuestionInfo("Nom et résumé de l'activité", "Business Name & Activity Summary", "identity") },
        { 2, new QuestionInfo("Histoire et motivations", "Story & Motivations", "identity") },
        { 3, new QuestionInfo("Problème client identifié", "Customer Problem Identified", "market") },
        { 4, new QuestionInfo("Solution et différenciation", "Solution & Differentiation", "offering") },
        { 5, new QuestionInfo("Secteur d'activité et univers", "Industry Sector & Universe", "market") },
        { 6, new QuestionInfo("Profil client cible", "Target Customer Profile", "market") },
        { 7, new QuestionInfo("Concurrence et positionnement", "Competition & Positioning", "market") },
        { 8, new QuestionInfo("Produits/Services et prix", "Products/Services & Pricing", "offering") },
        { 9, new QuestionInfo("Stratégie marketing et ventes", "Marketing & Sales Strategy", "operations") },
        { 10, new QuestionInfo("Équipe et promoteurs", "Team & Promoters", "team") },
        { 11, new QuestionInfo("Forme juridique", "Legal Structure", "identity") },
        { 12, new QuestionInfo("Besoins matériels", "Material Needs & Equipment", "operations") },
        { 13, new QuestionInfo("Apport personnel", "Personal Investment", "financials") },
        { 14, new QuestionInfo("Besoin de financement", "Financing Needs", "financials") },
        { 15, new QuestionInfo("Date de lancement", "Launch Date", "operations") },
        { 16, new QuestionInfo("Évolution RH", "HR Evolution", "team") },
        { 17, new QuestionInfo("Objectifs prioritaires an 1", "Year 1 Priority Objectives", "strategy") },
        { 18, new QuestionInfo("Questions complémentaires", "Additional Details", "general") },
        { 19, new QuestionInfo("Analyse SWOT - Forces", "SWOT - Strengths", "strategy") },
        { 20, new QuestionInfo("Analyse SWOT - Faiblesses", "SWOT - Weaknesses", "strategy") },
        { 21, new QuestionInfo("Analyse SWOT - Opportunités", "SWOT - Opportunities", "strategy") },
        { 22, new QuestionInfo("Analyse SWOT - Menaces", "SWOT - Threats", "strategy") },
    };

    /// <summary>
    /// Section mappings from STRUCTURE FINALE.
    /// Each business plan section uses answers from specific questions.
    /// </summary>
    public static readonly Dictionary<string, int[]> SectionToQuestions = new()
    {
        // 1-LE PROJET sections
        { "1.1_Description", new[] { 1, 5, 10, 11, 18 } },
        { "1.2_MissionVision", new[] { 2, 4, 17, 18 } },
        { "1.3_PropositionValeur", new[] { 3, 4, 6, 18 } },
        { "1.4_ProduitsServices", new[] { 4, 8, 12, 18 } },
        { "1.5_ObjectifsExpansion", new[] { 14, 16, 17, 18 } },
        { "1.6_BesoinsFinancement", new[] { 12, 13, 14, 18 } },
        { "1.7_Calendrier", new[] { 12, 14, 15, 17, 18 } },

        // 2-PROMOTEURS sections
        { "2.1_Historique", new[] { 1, 2, 10 } },
        { "2.2_VisionMission", new[] { 2, 4, 17 } },
        { "2.3_Objectifs", new[] { 17 } },
        { "2.4_StructureLegale", new[] { 10, 11 } },

        // 3-ÉTUDE DE MARCHÉ sections
        { "3.1_ApercuSecteur", new[] { 5, 6 } },
        { "3.2_MarcheGeographique", new[] { 5, 6, 18 } },
        { "3.3_ClienteleCible", new[] { 3, 6 } },
        { "3.4_Solution", new[] { 3, 4 } },
        { "3.5_Concurrence", new[] { 4, 7 } },
        { "3.6_SWOT", new[] { 19, 20, 21, 22 } },

        // 4-PLAN MARKETING & VENTES sections
        { "4.1_StrategieMarketing", new[] { 6, 9 } },
        { "4.2_ProduitsServices", new[] { 4, 8 } },
        { "4.3_Tarification", new[] { 8 } },
        { "4.4_CanauxDistribution", new[] { 9 } },

        // 5-PLAN OPÉRATIONNEL sections
        { "5.1_Processus", new[] { 12 } },
        { "5.2_Equipement", new[] { 12 } },
        { "5.3_Localisation", new[] { 12, 18 } },
        { "5.4_Fournisseurs", new[] { 12 } },
        { "5.5_RH", new[] { 10, 16 } },

        // 6-ANALYSE FINANCIÈRE sections
        { "6.1_CoutsDemarrage", new[] { 12, 13, 14 } },
        { "6.2_StructureCouts", new[] { 8, 12 } },
        { "6.3_Sources Revenus", new[] { 8 } },
        { "6.4_SeuilRentabilite", new[] { 8, 12 } },
        { "6.5_Previsions", new[] { 14, 17 } },
    };

    /// <summary>
    /// Maps generated section names to their relevant STRUCTURE FINALE sections.
    /// This ensures each generated section uses the most relevant questionnaire answers.
    /// </summary>
    public static readonly Dictionary<string, string[]> GeneratedSectionToStructureFinale = new()
    {
        // Executive Summary needs core business identity + financials
        { "ExecutiveSummary", new[] { "1.1_Description", "1.2_MissionVision", "1.6_BesoinsFinancement" } },

        // Problem/Solution
        { "ProblemStatement", new[] { "1.3_PropositionValeur", "3.3_ClienteleCible" } },
        { "Solution", new[] { "1.3_PropositionValeur", "1.4_ProduitsServices", "3.4_Solution" } },

        // Market sections
        { "MarketAnalysis", new[] { "3.1_ApercuSecteur", "3.2_MarcheGeographique", "3.3_ClienteleCible" } },
        { "CompetitiveAnalysis", new[] { "3.5_Concurrence", "1.3_PropositionValeur" } },
        { "SwotAnalysis", new[] { "3.6_SWOT" } },

        // Business model and strategy
        { "BusinessModel", new[] { "1.4_ProduitsServices", "4.3_Tarification", "6.3_Sources Revenus" } },
        { "MarketingStrategy", new[] { "4.1_StrategieMarketing", "4.4_CanauxDistribution" } },
        { "BrandingStrategy", new[] { "4.1_StrategieMarketing", "1.3_PropositionValeur" } },

        // Operations
        { "OperationsPlan", new[] { "5.1_Processus", "5.2_Equipement", "5.3_Localisation", "5.4_Fournisseurs" } },
        { "ManagementTeam", new[] { "2.1_Historique", "5.5_RH", "2.4_StructureLegale" } },

        // Financials
        { "FinancialProjections", new[] { "6.1_CoutsDemarrage", "6.2_StructureCouts", "6.3_Sources Revenus", "6.5_Previsions" } },
        { "FundingRequirements", new[] { "1.6_BesoinsFinancement", "6.1_CoutsDemarrage" } },
        { "RiskAnalysis", new[] { "3.6_SWOT", "3.5_Concurrence" } },

        // Startup-specific
        { "ExitStrategy", new[] { "1.5_ObjectifsExpansion", "6.5_Previsions" } },

        // OBNL/Non-profit specific
        { "MissionStatement", new[] { "1.2_MissionVision", "2.2_VisionMission" } },
        { "SocialImpact", new[] { "1.2_MissionVision", "2.3_Objectifs" } },
        { "BeneficiaryProfile", new[] { "3.3_ClienteleCible", "1.3_PropositionValeur" } },
        { "GrantStrategy", new[] { "1.6_BesoinsFinancement", "6.1_CoutsDemarrage" } },
        { "SustainabilityPlan", new[] { "1.5_ObjectifsExpansion", "6.5_Previsions" } },
    };

    /// <summary>
    /// Gets the relevant question numbers for a generated section.
    /// Aggregates all question IDs from the mapped STRUCTURE FINALE sections.
    /// </summary>
    public static int[] GetQuestionsForSection(string sectionName)
    {
        if (!GeneratedSectionToStructureFinale.TryGetValue(sectionName, out var structureSections))
        {
            // Fallback: return core business questions if section not mapped
            return new[] { 1, 2, 4, 5, 6, 8, 10, 17 };
        }

        var questionNumbers = new HashSet<int>();

        // Always include core identity (Q1, Q5) for context
        questionNumbers.Add(1);
        questionNumbers.Add(5);

        foreach (var structureSection in structureSections)
        {
            if (SectionToQuestions.TryGetValue(structureSection, out var questions))
            {
                foreach (var q in questions)
                {
                    questionNumbers.Add(q);
                }
            }
        }

        return questionNumbers.OrderBy(q => q).ToArray();
    }

    /// <summary>
    /// Builds a focused context for a specific generated section.
    /// Only includes answers relevant to that section based on STRUCTURE FINALE mapping.
    /// </summary>
    public static string BuildSectionContext(
        string sectionName,
        Dictionary<int, string> allAnswers,
        string language = "fr")
    {
        var relevantQuestions = GetQuestionsForSection(sectionName);
        var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);

        var contextParts = new List<string>();

        // Header
        var header = isFrench
            ? $"=== CONTEXTE POUR LA SECTION: {sectionName} ==="
            : $"=== CONTEXT FOR SECTION: {sectionName} ===";
        contextParts.Add(header);
        contextParts.Add("");

        // Add relevant answers in order
        foreach (var qNum in relevantQuestions)
        {
            if (allAnswers.TryGetValue(qNum, out var answer) && !string.IsNullOrWhiteSpace(answer))
            {
                if (QuestionInfoMap.TryGetValue(qNum, out var info))
                {
                    var label = isFrench ? info.LabelFr : info.LabelEn;
                    contextParts.Add($"[Q{qNum}: {label}]");
                    contextParts.Add(TruncateAnswer(answer, 800)); // Allow longer answers for section generation
                    contextParts.Add("");
                }
            }
        }

        if (contextParts.Count <= 2) // Only header, no actual answers
        {
            var noData = isFrench
                ? "(Aucune réponse pertinente disponible pour cette section)"
                : "(No relevant answers available for this section)";
            contextParts.Add(noData);
        }

        return string.Join("\n", contextParts);
    }

    /// <summary>
    /// Builds a complete context with all answers for full plan generation.
    /// Use this when you need all questionnaire data, not just section-specific.
    /// </summary>
    public static string BuildFullContext(
        Dictionary<int, string> allAnswers,
        string language = "fr")
    {
        var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);

        var contextParts = new List<string>();

        var header = isFrench
            ? "=== RÉPONSES AU QUESTIONNAIRE ==="
            : "=== QUESTIONNAIRE RESPONSES ===";
        contextParts.Add(header);
        contextParts.Add("");

        // Add all answers in question number order
        foreach (var qNum in allAnswers.Keys.OrderBy(k => k))
        {
            if (allAnswers.TryGetValue(qNum, out var answer) && !string.IsNullOrWhiteSpace(answer))
            {
                if (QuestionInfoMap.TryGetValue(qNum, out var info))
                {
                    var label = isFrench ? info.LabelFr : info.LabelEn;
                    contextParts.Add($"[Q{qNum}: {label}]");
                    contextParts.Add(TruncateAnswer(answer, 500));
                    contextParts.Add("");
                }
            }
        }

        return string.Join("\n", contextParts);
    }

    /// <summary>
    /// Builds a section context enriched with the Business Brief.
    /// Prepends the brief summary before section-specific answers for holistic AI understanding.
    /// </summary>
    public static string BuildSectionContextWithBrief(
        string sectionName,
        Dictionary<int, string> allAnswers,
        string businessBriefJson,
        string language = "fr")
    {
        var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var contextParts = new List<string>();

        // 1. Add Business Brief summary as top-level context
        if (!string.IsNullOrWhiteSpace(businessBriefJson))
        {
            var briefHeader = isFrench
                ? "=== SYNTHÈSE GLOBALE DE L'ENTREPRISE (Business Brief) ==="
                : "=== BUSINESS OVERVIEW SYNTHESIS (Business Brief) ===";
            contextParts.Add(briefHeader);
            contextParts.Add("");

            // Parse the brief and format key sections relevant to this section
            contextParts.Add(FormatBriefForSection(businessBriefJson, sectionName, isFrench));
            contextParts.Add("");
        }

        // 2. Add section-specific context (existing logic)
        var sectionContext = BuildSectionContext(sectionName, allAnswers, language);
        contextParts.Add(sectionContext);

        return string.Join("\n", contextParts);
    }

    /// <summary>
    /// Formats relevant parts of the Business Brief for a specific section.
    /// Different sections need different aspects of the brief emphasized.
    /// </summary>
    private static string FormatBriefForSection(string briefJson, string sectionName, bool isFrench)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(briefJson);
            var root = doc.RootElement;
            var parts = new List<string>();

            // Always include company profile and business concept
            if (root.TryGetProperty("companyProfile", out var profile))
            {
                var label = isFrench ? "Profil de l'entreprise" : "Company Profile";
                parts.Add($"[{label}]");
                AppendJsonProperties(parts, profile, isFrench);
                parts.Add("");
            }

            if (root.TryGetProperty("businessConcept", out var concept))
            {
                var label = isFrench ? "Concept d'affaires" : "Business Concept";
                parts.Add($"[{label}]");
                AppendJsonProperties(parts, concept, isFrench);
                parts.Add("");
            }

            // Section-specific emphasis
            switch (sectionName)
            {
                case "MarketAnalysis":
                case "CompetitiveAnalysis":
                case "SwotAnalysis":
                    AppendBriefSection(parts, root, "marketContext", isFrench ? "Contexte du marché" : "Market Context", isFrench);
                    AppendBriefSection(parts, root, "strategicContext", isFrench ? "Contexte stratégique" : "Strategic Context", isFrench);
                    break;

                case "FinancialProjections":
                case "FundingRequirements":
                case "BusinessModel":
                    AppendBriefSection(parts, root, "financialContext", isFrench ? "Contexte financier" : "Financial Context", isFrench);
                    AppendBriefSection(parts, root, "marketContext", isFrench ? "Contexte du marché" : "Market Context", isFrench);
                    break;

                case "OperationsPlan":
                case "ManagementTeam":
                    AppendBriefSection(parts, root, "operationalContext", isFrench ? "Contexte opérationnel" : "Operational Context", isFrench);
                    break;

                case "MarketingStrategy":
                case "BrandingStrategy":
                    AppendBriefSection(parts, root, "marketContext", isFrench ? "Contexte du marché" : "Market Context", isFrench);
                    AppendBriefSection(parts, root, "operationalContext", isFrench ? "Contexte opérationnel" : "Operational Context", isFrench);
                    break;

                case "RiskAnalysis":
                case "ExitStrategy":
                    AppendBriefSection(parts, root, "strategicContext", isFrench ? "Contexte stratégique" : "Strategic Context", isFrench);
                    AppendBriefSection(parts, root, "financialContext", isFrench ? "Contexte financier" : "Financial Context", isFrench);
                    break;

                case "ExecutiveSummary":
                    // Executive summary gets everything
                    AppendBriefSection(parts, root, "marketContext", isFrench ? "Contexte du marché" : "Market Context", isFrench);
                    AppendBriefSection(parts, root, "financialContext", isFrench ? "Contexte financier" : "Financial Context", isFrench);
                    AppendBriefSection(parts, root, "strategicContext", isFrench ? "Contexte stratégique" : "Strategic Context", isFrench);
                    AppendBriefSection(parts, root, "operationalContext", isFrench ? "Contexte opérationnel" : "Operational Context", isFrench);
                    break;

                default:
                    // For OBNL and other sections, include all context
                    AppendBriefSection(parts, root, "marketContext", isFrench ? "Contexte du marché" : "Market Context", isFrench);
                    AppendBriefSection(parts, root, "financialContext", isFrench ? "Contexte financier" : "Financial Context", isFrench);
                    AppendBriefSection(parts, root, "strategicContext", isFrench ? "Contexte stratégique" : "Strategic Context", isFrench);
                    break;
            }

            // Always include generation guidance and maturity assessment
            AppendBriefSection(parts, root, "maturityAssessment", isFrench ? "Évaluation de maturité" : "Maturity Assessment", isFrench);
            AppendBriefSection(parts, root, "generationGuidance", isFrench ? "Directives de génération" : "Generation Guidance", isFrench);

            return string.Join("\n", parts);
        }
        catch
        {
            // If parsing fails, return the raw brief
            return briefJson;
        }
    }

    private static void AppendBriefSection(
        List<string> parts,
        System.Text.Json.JsonElement root,
        string propertyName,
        string label,
        bool isFrench)
    {
        if (root.TryGetProperty(propertyName, out var section))
        {
            parts.Add($"[{label}]");
            AppendJsonProperties(parts, section, isFrench);
            parts.Add("");
        }
    }

    private static void AppendJsonProperties(
        List<string> parts,
        System.Text.Json.JsonElement element,
        bool isFrench)
    {
        if (element.ValueKind != System.Text.Json.JsonValueKind.Object)
            return;

        foreach (var prop in element.EnumerateObject())
        {
            if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var value = prop.Value.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    parts.Add($"  - {FormatPropertyName(prop.Name)}: {value}");
                }
            }
            else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                parts.Add($"  - {FormatPropertyName(prop.Name)}: {prop.Value}");
            }
            else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var items = new List<string>();
                foreach (var item in prop.Value.EnumerateArray())
                {
                    if (item.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        var val = item.GetString();
                        if (!string.IsNullOrWhiteSpace(val))
                            items.Add(val);
                    }
                }
                if (items.Any())
                {
                    parts.Add($"  - {FormatPropertyName(prop.Name)}: {string.Join("; ", items)}");
                }
            }
            else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                parts.Add($"  [{FormatPropertyName(prop.Name)}]");
                AppendJsonProperties(parts, prop.Value, isFrench);
            }
        }
    }

    private static string FormatPropertyName(string camelCase)
    {
        // Convert camelCase to Title Case with spaces
        var result = new System.Text.StringBuilder();
        for (var i = 0; i < camelCase.Length; i++)
        {
            if (i > 0 && char.IsUpper(camelCase[i]))
            {
                result.Append(' ');
            }
            result.Append(i == 0 ? char.ToUpper(camelCase[i]) : camelCase[i]);
        }
        return result.ToString();
    }

    /// <summary>
    /// Gets questions that provide relevant context for a given question.
    /// Returns question numbers that should be included in AI context.
    /// </summary>
    public static int[] GetRelatedQuestions(int questionNumber)
    {
        // Core identity questions (1, 2, 5) provide context for most other questions
        var coreContext = new HashSet<int> { 1, 5 };

        // Add questions that directly relate to the current question
        var related = questionNumber switch
        {
            // Identity questions - minimal context needed
            1 => Array.Empty<int>(),
            2 => new[] { 1 },

            // Problem/Solution - needs identity + market context
            3 => new[] { 1, 5, 6 },
            4 => new[] { 1, 3, 5, 6 },

            // Sector/Universe - needs identity
            5 => new[] { 1, 2 },

            // Target customer - needs problem, sector
            6 => new[] { 1, 3, 5 },

            // Competition - needs solution, sector, target
            7 => new[] { 1, 4, 5, 6 },

            // Products/Pricing - needs solution, target, competition
            8 => new[] { 1, 4, 5, 6, 7 },

            // Marketing strategy - needs all market info
            9 => new[] { 1, 4, 5, 6, 7, 8 },

            // Team - needs identity, objectives
            10 => new[] { 1, 2, 5 },

            // Legal structure - needs team
            11 => new[] { 1, 10 },

            // Material needs - needs products, operations context
            12 => new[] { 1, 4, 5, 8 },

            // Personal investment - needs financing context
            13 => new[] { 1, 12, 14 },

            // Financing needs - needs material needs, objectives
            14 => new[] { 1, 12, 13, 17 },

            // Launch date - needs operations context
            15 => new[] { 1, 12, 14 },

            // HR evolution - needs team, objectives
            16 => new[] { 1, 10, 17 },

            // Year 1 objectives - needs full business context
            17 => new[] { 1, 4, 5, 6, 8, 10, 14 },

            // Additional questions - comprehensive context
            18 => new[] { 1, 2, 3, 4, 5, 6, 8 },

            // SWOT - needs comprehensive business understanding
            19 => new[] { 1, 4, 5, 6, 7, 8, 10 },
            20 => new[] { 1, 4, 5, 7, 10, 12 },
            21 => new[] { 1, 5, 6, 7, 17 },
            22 => new[] { 1, 5, 7, 14, 17 },

            _ => Array.Empty<int>()
        };

        return related;
    }

    /// <summary>
    /// Builds a business context summary from accumulated answers.
    /// Used to provide AI with understanding of the user's specific business.
    /// </summary>
    public static string BuildBusinessContextSummary(
        Dictionary<int, string> answers,
        int currentQuestionNumber,
        string language = "fr")
    {
        var relatedQuestions = GetRelatedQuestions(currentQuestionNumber);
        if (relatedQuestions.Length == 0 && answers.Count == 0)
            return string.Empty;

        var isFrench = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var contextParts = new List<string>();

        // Always include business identity if available (Q1)
        if (answers.TryGetValue(1, out var businessIdentity) && !string.IsNullOrWhiteSpace(businessIdentity))
        {
            var label = isFrench ? "Entreprise" : "Business";
            contextParts.Add($"[{label}] {TruncateAnswer(businessIdentity, 200)}");
        }

        // Include sector if available (Q5)
        if (answers.TryGetValue(5, out var sector) && !string.IsNullOrWhiteSpace(sector))
        {
            var label = isFrench ? "Secteur" : "Sector";
            contextParts.Add($"[{label}] {TruncateAnswer(sector, 150)}");
        }

        // Add related question answers
        foreach (var qNum in relatedQuestions.Where(q => q != 1 && q != 5)) // Skip 1 and 5, already added
        {
            if (answers.TryGetValue(qNum, out var answer) && !string.IsNullOrWhiteSpace(answer))
            {
                if (QuestionInfoMap.TryGetValue(qNum, out var info))
                {
                    var label = isFrench ? info.LabelFr : info.LabelEn;
                    contextParts.Add($"[Q{qNum}: {label}] {TruncateAnswer(answer, 150)}");
                }
            }
        }

        if (contextParts.Count == 0)
            return string.Empty;

        var header = isFrench
            ? "CONTEXTE DU PROJET (Réponses précédentes de l'utilisateur):"
            : "PROJECT CONTEXT (User's previous answers):";

        return $"{header}\n\n{string.Join("\n\n", contextParts)}";
    }

    /// <summary>
    /// Converts kebab-case section name to PascalCase for mapping lookups.
    /// e.g., "executive-summary" → "ExecutiveSummary"
    /// </summary>
    public static string ToPascalCase(string sectionName)
    {
        if (string.IsNullOrEmpty(sectionName) || !sectionName.Contains('-'))
            return sectionName;

        return string.Concat(
            sectionName.Split('-')
                .Select(part => string.IsNullOrEmpty(part)
                    ? part
                    : char.ToUpperInvariant(part[0]) + part[1..]));
    }

    private static string TruncateAnswer(string answer, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(answer))
            return string.Empty;

        answer = answer.Trim();
        if (answer.Length <= maxLength)
            return answer;

        return answer.Substring(0, maxLength - 3) + "...";
    }
}

/// <summary>
/// Information about a question for context building
/// </summary>
public record QuestionInfo(string LabelFr, string LabelEn, string Category);
