namespace Sqordia.Application.Services.AI;

/// <summary>
/// Per-section quality rubrics that define what makes a good section.
/// Injected into prompts to give the LLM explicit success criteria.
/// </summary>
public static class SectionRubrics
{
    public record SectionRubric(
        string[] RequiredElements,
        string[] QualityCriteria,
        string ToneGuidance,
        int MinWordCount,
        int MaxWordCount,
        string[] AntiPatterns);

    private static readonly Dictionary<string, (SectionRubric Fr, SectionRubric En)> Rubrics = new()
    {
        ["ExecutiveSummary"] = (
            new SectionRubric(
                RequiredElements: new[] { "Accroche convaincante", "Concept d'affaires", "Marché cible", "Avantage concurrentiel", "Chiffres financiers clés", "Montant de financement demandé" },
                QualityCriteria: new[] { "Doit synthétiser TOUTES les autres sections", "Inclure au moins 2-3 métriques chiffrées", "Être autonome (compréhensible sans lire le reste)" },
                ToneGuidance: "Convaincant, synthétique, orienté investisseur",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Copier-coller verbatim des autres sections", "Omettre les chiffres financiers", "Langage vague sans données concrètes" }),
            new SectionRubric(
                RequiredElements: new[] { "Compelling hook", "Business concept", "Target market", "Competitive advantage", "Key financial figures", "Funding amount requested" },
                QualityCriteria: new[] { "Must synthesize ALL other sections", "Include at least 2-3 numerical metrics", "Be standalone (understandable without reading the rest)" },
                ToneGuidance: "Compelling, concise, investor-oriented",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Copy-pasting verbatim from other sections", "Omitting financial figures", "Vague language without concrete data" })
        ),
        ["MarketAnalysis"] = (
            new SectionRubric(
                RequiredElements: new[] { "Taille du marché (TAM/SAM/SOM)", "Tendances de croissance avec taux (TCAC)", "Segmentation de la clientèle", "Portée géographique", "Sources de données" },
                QualityCriteria: new[] { "Inclure des chiffres précis, pas des affirmations vagues", "Citer des sources ou estimer avec méthodologie", "Définir au moins 2-3 segments clients" },
                ToneGuidance: "Analytique, basé sur les données, objectif",
                MinWordCount: 600, MaxWordCount: 900,
                AntiPatterns: new[] { "Affirmations vagues sur un 'marché en forte croissance' sans chiffres", "Pas de segmentation client", "Données génériques non liées au secteur spécifique" }),
            new SectionRubric(
                RequiredElements: new[] { "Market size (TAM/SAM/SOM)", "Growth trends with rates (CAGR)", "Customer segmentation", "Geographic scope", "Data sources" },
                QualityCriteria: new[] { "Include precise figures, not vague claims", "Cite sources or estimate with clear methodology", "Define at least 2-3 customer segments" },
                ToneGuidance: "Analytical, data-driven, objective",
                MinWordCount: 600, MaxWordCount: 900,
                AntiPatterns: new[] { "Vague claims about a 'rapidly growing market' without figures", "No customer segmentation", "Generic data not tied to the specific industry" })
        ),
        ["CompetitiveAnalysis"] = (
            new SectionRubric(
                RequiredElements: new[] { "Concurrents directs (3-5)", "Concurrents indirects", "Matrice de positionnement", "Avantages concurrentiels spécifiques", "Barrières à l'entrée" },
                QualityCriteria: new[] { "Nommer des concurrents réels quand possible", "Expliquer pourquoi le positionnement est défendable", "Inclure forces ET faiblesses des concurrents" },
                ToneGuidance: "Stratégique, factuel, honnête",
                MinWordCount: 500, MaxWordCount: 800,
                AntiPatterns: new[] { "Prétendre qu'il n'y a pas de concurrence", "Ne lister que les faiblesses des concurrents", "Positionnement non soutenu par des faits" }),
            new SectionRubric(
                RequiredElements: new[] { "Direct competitors (3-5)", "Indirect competitors", "Positioning matrix", "Specific competitive advantages", "Barriers to entry" },
                QualityCriteria: new[] { "Name real competitors when possible", "Explain why positioning is defensible", "Include strengths AND weaknesses of competitors" },
                ToneGuidance: "Strategic, factual, honest",
                MinWordCount: 500, MaxWordCount: 800,
                AntiPatterns: new[] { "Claiming there is no competition", "Only listing competitors' weaknesses", "Positioning not supported by facts" })
        ),
        ["FinancialProjections"] = (
            new SectionRubric(
                RequiredElements: new[] { "Projections sur 3 ans (revenus, dépenses, bénéfice net)", "Seuil de rentabilité", "Structure des coûts", "Sources de revenus", "Hypothèses clés justifiées" },
                QualityCriteria: new[] { "Chaque chiffre doit être justifié par une hypothèse", "Les projections doivent être cohérentes avec le plan marketing et opérationnel", "Inclure un scénario réaliste" },
                ToneGuidance: "Précis, méthodique, conservateur, crédible",
                MinWordCount: 500, MaxWordCount: 800,
                AntiPatterns: new[] { "Chiffres ronds sans justification", "Projections irréalistes de croissance exponentielle", "Ignorer les coûts fixes importants", "Incohérence avec les autres sections" }),
            new SectionRubric(
                RequiredElements: new[] { "3-year projections (revenue, expenses, net income)", "Break-even timeline", "Cost structure", "Revenue streams", "Key assumptions justified" },
                QualityCriteria: new[] { "Every figure must be backed by an assumption", "Projections must be consistent with marketing and operations plan", "Include a realistic scenario" },
                ToneGuidance: "Precise, methodical, conservative, credible",
                MinWordCount: 500, MaxWordCount: 800,
                AntiPatterns: new[] { "Round numbers without justification", "Unrealistic exponential growth projections", "Ignoring significant fixed costs", "Inconsistency with other sections" })
        ),
        ["BusinessModel"] = (
            new SectionRubric(
                RequiredElements: new[] { "Proposition de valeur", "Sources de revenus et tarification", "Structure des coûts", "Canaux de distribution", "Partenaires clés" },
                QualityCriteria: new[] { "Expliquer clairement comment l'entreprise génère des revenus", "Justifier la stratégie de prix", "Montrer la viabilité économique" },
                ToneGuidance: "Clair, structuré, orienté business",
                MinWordCount: 500, MaxWordCount: 800,
                AntiPatterns: new[] { "Description vague du modèle sans chiffres", "Ignorer la structure des coûts", "Ne pas expliquer la tarification" }),
            new SectionRubric(
                RequiredElements: new[] { "Value proposition", "Revenue streams and pricing", "Cost structure", "Distribution channels", "Key partners" },
                QualityCriteria: new[] { "Clearly explain how the business generates revenue", "Justify pricing strategy", "Show economic viability" },
                ToneGuidance: "Clear, structured, business-oriented",
                MinWordCount: 500, MaxWordCount: 800,
                AntiPatterns: new[] { "Vague model description without numbers", "Ignoring cost structure", "Not explaining pricing" })
        ),
        ["SwotAnalysis"] = (
            new SectionRubric(
                RequiredElements: new[] { "3-5 Forces spécifiques", "3-5 Faiblesses honnêtes", "3-5 Opportunités avec données", "3-5 Menaces réalistes" },
                QualityCriteria: new[] { "Chaque élément doit être spécifique au projet, pas générique", "Les faiblesses doivent être honnêtes avec des plans d'atténuation", "Équilibre entre les 4 quadrants" },
                ToneGuidance: "Analytique, honnête, stratégique",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "SWOT générique applicable à n'importe quelle entreprise", "Déséquilibre (trop de forces, aucune faiblesse)", "Éléments non liés au contexte spécifique" }),
            new SectionRubric(
                RequiredElements: new[] { "3-5 Specific Strengths", "3-5 Honest Weaknesses", "3-5 Data-backed Opportunities", "3-5 Realistic Threats" },
                QualityCriteria: new[] { "Each item must be specific to the project, not generic", "Weaknesses must be honest with mitigation plans", "Balance across all 4 quadrants" },
                ToneGuidance: "Analytical, honest, strategic",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Generic SWOT applicable to any business", "Imbalance (too many strengths, no weaknesses)", "Items not tied to the specific context" })
        ),
        ["MarketingStrategy"] = (
            new SectionRubric(
                RequiredElements: new[] { "Stratégie d'acquisition clients", "Canaux marketing prioritaires", "Budget marketing estimé", "Calendrier de lancement", "KPIs mesurables" },
                QualityCriteria: new[] { "Actions concrètes, pas seulement des intentions", "Budget réaliste aligné avec les projections financières", "KPIs chiffrés et mesurables" },
                ToneGuidance: "Orienté action, créatif mais réaliste",
                MinWordCount: 500, MaxWordCount: 800,
                AntiPatterns: new[] { "Stratégie vague sans actions concrètes", "Budget irréaliste", "Pas de KPIs mesurables" }),
            new SectionRubric(
                RequiredElements: new[] { "Customer acquisition strategy", "Priority marketing channels", "Estimated marketing budget", "Launch timeline", "Measurable KPIs" },
                QualityCriteria: new[] { "Concrete actions, not just intentions", "Realistic budget aligned with financial projections", "Quantified and measurable KPIs" },
                ToneGuidance: "Action-oriented, creative but realistic",
                MinWordCount: 500, MaxWordCount: 800,
                AntiPatterns: new[] { "Vague strategy without concrete actions", "Unrealistic budget", "No measurable KPIs" })
        ),
        ["OperationsPlan"] = (
            new SectionRubric(
                RequiredElements: new[] { "Processus opérationnels clés", "Besoins en équipement et technologie", "Localisation et installations", "Fournisseurs clés", "Gestion de la qualité" },
                QualityCriteria: new[] { "Décrire le flux opérationnel de bout en bout", "Identifier les goulots d'étranglement potentiels", "Coûts opérationnels alignés avec les projections financières" },
                ToneGuidance: "Pratique, détaillé, orienté exécution",
                MinWordCount: 500, MaxWordCount: 800,
                AntiPatterns: new[] { "Description trop théorique sans détails pratiques", "Ignorer les défis logistiques", "Coûts non alignés avec la section financière" }),
            new SectionRubric(
                RequiredElements: new[] { "Key operational processes", "Equipment and technology needs", "Location and facilities", "Key suppliers", "Quality management" },
                QualityCriteria: new[] { "Describe end-to-end operational flow", "Identify potential bottlenecks", "Operational costs aligned with financial projections" },
                ToneGuidance: "Practical, detailed, execution-oriented",
                MinWordCount: 500, MaxWordCount: 800,
                AntiPatterns: new[] { "Too theoretical without practical details", "Ignoring logistics challenges", "Costs not aligned with financial section" })
        ),
        ["ManagementTeam"] = (
            new SectionRubric(
                RequiredElements: new[] { "Profils des fondateurs/promoteurs", "Compétences clés et expérience pertinente", "Rôles et responsabilités", "Plan de recrutement", "Structure organisationnelle" },
                QualityCriteria: new[] { "Mettre en valeur l'expérience pertinente au secteur", "Identifier les lacunes et comment les combler", "Montrer la complémentarité de l'équipe" },
                ToneGuidance: "Professionnel, crédible, inspirant confiance",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "CV génériques sans lien avec le projet", "Ignorer les lacunes de l'équipe", "Pas de plan de recrutement" }),
            new SectionRubric(
                RequiredElements: new[] { "Founders/promoters profiles", "Key skills and relevant experience", "Roles and responsibilities", "Hiring plan", "Organizational structure" },
                QualityCriteria: new[] { "Highlight industry-relevant experience", "Identify gaps and how to fill them", "Show team complementarity" },
                ToneGuidance: "Professional, credible, confidence-inspiring",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Generic CVs unrelated to the project", "Ignoring team gaps", "No hiring plan" })
        ),
        ["ProblemStatement"] = (
            new SectionRubric(
                RequiredElements: new[] { "Problème client clairement défini", "Impact du problème (chiffré si possible)", "Solutions actuelles et leurs limites", "Urgence du problème" },
                QualityCriteria: new[] { "Le problème doit être spécifique et vérifiable", "Montrer l'ampleur avec des données", "Lier le problème au marché cible identifié" },
                ToneGuidance: "Empathique, factuel, convaincant",
                MinWordCount: 300, MaxWordCount: 600,
                AntiPatterns: new[] { "Problème trop vague ou universel", "Pas de données sur l'impact", "Déconnecté du marché cible" }),
            new SectionRubric(
                RequiredElements: new[] { "Clearly defined customer problem", "Problem impact (quantified if possible)", "Current solutions and their limitations", "Problem urgency" },
                QualityCriteria: new[] { "Problem must be specific and verifiable", "Show scale with data", "Link problem to identified target market" },
                ToneGuidance: "Empathetic, factual, compelling",
                MinWordCount: 300, MaxWordCount: 600,
                AntiPatterns: new[] { "Problem too vague or universal", "No data on impact", "Disconnected from target market" })
        ),
        ["Solution"] = (
            new SectionRubric(
                RequiredElements: new[] { "Description claire de la solution", "Différenciation par rapport aux alternatives", "Bénéfices clients concrets", "Faisabilité technique" },
                QualityCriteria: new[] { "La solution doit répondre directement au problème identifié", "Expliquer pourquoi c'est meilleur que l'existant", "Être réaliste et réalisable" },
                ToneGuidance: "Clair, innovant, orienté bénéfices client",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Solution déconnectée du problème", "Trop technique sans bénéfices clients", "Prétentions non soutenues" }),
            new SectionRubric(
                RequiredElements: new[] { "Clear solution description", "Differentiation from alternatives", "Concrete customer benefits", "Technical feasibility" },
                QualityCriteria: new[] { "Solution must directly address the identified problem", "Explain why it's better than existing options", "Be realistic and achievable" },
                ToneGuidance: "Clear, innovative, customer-benefit oriented",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Solution disconnected from problem", "Too technical without customer benefits", "Unsupported claims" })
        ),
        ["BrandingStrategy"] = (
            new SectionRubric(
                RequiredElements: new[] { "Identité de marque", "Positionnement et message clé", "Public cible de la marque", "Canaux de communication" },
                QualityCriteria: new[] { "Cohérent avec la stratégie marketing", "Différencié des concurrents", "Adapté au marché cible" },
                ToneGuidance: "Créatif, stratégique, inspirant",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Branding générique sans personnalité", "Déconnecté de la stratégie marketing", "Pas de cohérence avec le positionnement" }),
            new SectionRubric(
                RequiredElements: new[] { "Brand identity", "Positioning and key message", "Brand target audience", "Communication channels" },
                QualityCriteria: new[] { "Consistent with marketing strategy", "Differentiated from competitors", "Adapted to target market" },
                ToneGuidance: "Creative, strategic, inspiring",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Generic branding without personality", "Disconnected from marketing strategy", "No consistency with positioning" })
        ),
        ["RiskAnalysis"] = (
            new SectionRubric(
                RequiredElements: new[] { "Risques de marché", "Risques financiers", "Risques opérationnels", "Stratégies d'atténuation pour chaque risque", "Plan de contingence" },
                QualityCriteria: new[] { "Risques spécifiques au projet, pas génériques", "Chaque risque doit avoir une stratégie d'atténuation", "Évaluation de probabilité et d'impact" },
                ToneGuidance: "Réaliste, proactif, rassurant",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Risques génériques non liés au projet", "Pas de stratégie d'atténuation", "Minimiser les risques réels" }),
            new SectionRubric(
                RequiredElements: new[] { "Market risks", "Financial risks", "Operational risks", "Mitigation strategies for each risk", "Contingency plan" },
                QualityCriteria: new[] { "Project-specific risks, not generic", "Each risk must have a mitigation strategy", "Probability and impact assessment" },
                ToneGuidance: "Realistic, proactive, reassuring",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Generic risks not tied to project", "No mitigation strategy", "Minimizing real risks" })
        ),
        ["FundingRequirements"] = (
            new SectionRubric(
                RequiredElements: new[] { "Montant total demandé", "Répartition détaillée de l'utilisation des fonds", "Calendrier de déploiement", "Retour sur investissement prévu", "Apport personnel du promoteur" },
                QualityCriteria: new[] { "Montants alignés avec les projections financières", "Justification claire de chaque poste", "Montrer l'engagement du promoteur (apport personnel)" },
                ToneGuidance: "Précis, transparent, orienté banquier",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Montants non justifiés", "Incohérence avec les projections financières", "Pas d'apport personnel mentionné" }),
            new SectionRubric(
                RequiredElements: new[] { "Total amount requested", "Detailed fund usage breakdown", "Deployment timeline", "Expected return on investment", "Promoter's personal contribution" },
                QualityCriteria: new[] { "Amounts aligned with financial projections", "Clear justification for each line item", "Show promoter commitment (personal contribution)" },
                ToneGuidance: "Precise, transparent, banker-oriented",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Unjustified amounts", "Inconsistency with financial projections", "No personal contribution mentioned" })
        ),
        ["ExitStrategy"] = (
            new SectionRubric(
                RequiredElements: new[] { "Options de sortie envisagées", "Horizon temporel", "Valorisation estimée", "Conditions de marché requises" },
                QualityCriteria: new[] { "Cohérent avec les projections financières", "Réaliste pour le secteur", "Plusieurs scénarios envisagés" },
                ToneGuidance: "Stratégique, réaliste, orienté long terme",
                MinWordCount: 300, MaxWordCount: 600,
                AntiPatterns: new[] { "Sortie irréaliste", "Pas de lien avec les finances", "Un seul scénario sans alternatives" }),
            new SectionRubric(
                RequiredElements: new[] { "Exit options considered", "Time horizon", "Estimated valuation", "Required market conditions" },
                QualityCriteria: new[] { "Consistent with financial projections", "Realistic for the industry", "Multiple scenarios considered" },
                ToneGuidance: "Strategic, realistic, long-term oriented",
                MinWordCount: 300, MaxWordCount: 600,
                AntiPatterns: new[] { "Unrealistic exit", "No link to financials", "Single scenario with no alternatives" })
        ),
        // OBNL sections
        ["MissionStatement"] = (
            new SectionRubric(
                RequiredElements: new[] { "Mission claire et inspirante", "Vision à long terme", "Valeurs fondamentales", "Impact social visé" },
                QualityCriteria: new[] { "Mission spécifique, pas générique", "Alignée avec les activités décrites", "Inspirante pour les parties prenantes" },
                ToneGuidance: "Inspirant, clair, orienté impact social",
                MinWordCount: 300, MaxWordCount: 600,
                AntiPatterns: new[] { "Mission vague applicable à tout OBNL", "Déconnectée des activités", "Trop corporative pour un OBNL" }),
            new SectionRubric(
                RequiredElements: new[] { "Clear and inspiring mission", "Long-term vision", "Core values", "Targeted social impact" },
                QualityCriteria: new[] { "Specific mission, not generic", "Aligned with described activities", "Inspiring for stakeholders" },
                ToneGuidance: "Inspiring, clear, social impact oriented",
                MinWordCount: 300, MaxWordCount: 600,
                AntiPatterns: new[] { "Vague mission applicable to any non-profit", "Disconnected from activities", "Too corporate for a non-profit" })
        ),
        ["SocialImpact"] = (
            new SectionRubric(
                RequiredElements: new[] { "Bénéficiaires ciblés", "Indicateurs d'impact mesurables", "Méthode de mesure", "Objectifs d'impact à 3 ans" },
                QualityCriteria: new[] { "Impact quantifiable avec des indicateurs clairs", "Méthodologie de mesure décrite", "Objectifs SMART" },
                ToneGuidance: "Engagé, basé sur les données, orienté résultats",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Impact vague sans indicateurs", "Pas de méthode de mesure", "Objectifs non mesurables" }),
            new SectionRubric(
                RequiredElements: new[] { "Target beneficiaries", "Measurable impact indicators", "Measurement method", "3-year impact goals" },
                QualityCriteria: new[] { "Quantifiable impact with clear indicators", "Measurement methodology described", "SMART objectives" },
                ToneGuidance: "Committed, data-driven, results-oriented",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Vague impact without indicators", "No measurement method", "Non-measurable objectives" })
        ),
        ["BeneficiaryProfile"] = (
            new SectionRubric(
                RequiredElements: new[] { "Profil démographique", "Besoins identifiés", "Nombre estimé de bénéficiaires", "Comment ils seront rejoints" },
                QualityCriteria: new[] { "Profil spécifique basé sur des données", "Besoins clairement articulés", "Stratégie d'atteinte réaliste" },
                ToneGuidance: "Empathique, précis, centré sur les personnes",
                MinWordCount: 300, MaxWordCount: 600,
                AntiPatterns: new[] { "Profil trop large", "Besoins non validés", "Pas d'estimation du nombre" }),
            new SectionRubric(
                RequiredElements: new[] { "Demographic profile", "Identified needs", "Estimated number of beneficiaries", "How they will be reached" },
                QualityCriteria: new[] { "Specific profile based on data", "Clearly articulated needs", "Realistic outreach strategy" },
                ToneGuidance: "Empathetic, precise, people-centered",
                MinWordCount: 300, MaxWordCount: 600,
                AntiPatterns: new[] { "Profile too broad", "Unvalidated needs", "No number estimate" })
        ),
        ["GrantStrategy"] = (
            new SectionRubric(
                RequiredElements: new[] { "Sources de subventions identifiées", "Montants visés par source", "Calendrier de demandes", "Taux de succès estimé" },
                QualityCriteria: new[] { "Sources réelles et accessibles", "Diversification des sources", "Calendrier réaliste" },
                ToneGuidance: "Stratégique, précis, réaliste",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Sources non identifiées", "Dépendance à une seule source", "Calendrier irréaliste" }),
            new SectionRubric(
                RequiredElements: new[] { "Identified grant sources", "Target amounts per source", "Application timeline", "Estimated success rate" },
                QualityCriteria: new[] { "Real and accessible sources", "Source diversification", "Realistic timeline" },
                ToneGuidance: "Strategic, precise, realistic",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Unidentified sources", "Dependence on single source", "Unrealistic timeline" })
        ),
        ["SustainabilityPlan"] = (
            new SectionRubric(
                RequiredElements: new[] { "Modèle de pérennité financière", "Diversification des revenus", "Plan de réduction de dépendance aux subventions", "Objectifs de durabilité à 5 ans" },
                QualityCriteria: new[] { "Plan réaliste et progressif", "Aligné avec la mission sociale", "Indicateurs de progrès définis" },
                ToneGuidance: "Visionnaire, pragmatique, orienté long terme",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Plan irréaliste", "Dépendance totale aux subventions", "Pas d'indicateurs de suivi" }),
            new SectionRubric(
                RequiredElements: new[] { "Financial sustainability model", "Revenue diversification", "Plan to reduce grant dependency", "5-year sustainability goals" },
                QualityCriteria: new[] { "Realistic and progressive plan", "Aligned with social mission", "Progress indicators defined" },
                ToneGuidance: "Visionary, pragmatic, long-term oriented",
                MinWordCount: 400, MaxWordCount: 700,
                AntiPatterns: new[] { "Unrealistic plan", "Total dependence on grants", "No tracking indicators" })
        ),
    };

    /// <summary>
    /// Gets the rubric for a section in the specified language.
    /// Returns null if no rubric is defined for the section.
    /// </summary>
    public static SectionRubric? GetRubric(string sectionName, string language = "fr")
    {
        if (!Rubrics.TryGetValue(sectionName, out var pair))
            return null;

        return language.Equals("fr", StringComparison.OrdinalIgnoreCase) ? pair.Fr : pair.En;
    }

    /// <summary>
    /// Formats a rubric as a prompt block to inject into AI prompts.
    /// </summary>
    public static string? FormatForPrompt(string sectionName, string language = "fr")
    {
        var rubric = GetRubric(sectionName, language);
        if (rubric == null) return null;

        var isFr = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var lines = new List<string>
        {
            isFr ? "=== CRITÈRES DE QUALITÉ ===" : "=== QUALITY CRITERIA ===",
            "",
            isFr ? "Éléments requis:" : "Required elements:"
        };
        foreach (var el in rubric.RequiredElements)
            lines.Add($"  - {el}");

        lines.Add("");
        lines.Add(isFr ? "Critères de qualité:" : "Quality criteria:");
        foreach (var c in rubric.QualityCriteria)
            lines.Add($"  - {c}");

        lines.Add("");
        lines.Add($"{(isFr ? "Ton" : "Tone")}: {rubric.ToneGuidance}");
        lines.Add($"{(isFr ? "Longueur" : "Length")}: {rubric.MinWordCount}-{rubric.MaxWordCount} {(isFr ? "mots" : "words")}");

        lines.Add("");
        lines.Add(isFr ? "À ÉVITER:" : "AVOID:");
        foreach (var ap in rubric.AntiPatterns)
            lines.Add($"  - {ap}");

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Returns all section names that have rubrics defined.
    /// </summary>
    public static IReadOnlyCollection<string> GetAllSectionNames() => Rubrics.Keys;
}
