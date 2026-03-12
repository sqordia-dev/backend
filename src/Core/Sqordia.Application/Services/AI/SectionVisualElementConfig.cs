namespace Sqordia.Application.Services.AI;

/// <summary>
/// Per-section visual element recommendations.
/// Tells the AI which json:chart / json:table / json:metrics / json:swot blocks
/// to embed for each business plan section. Bilingual (FR / EN).
/// </summary>
public static class SectionVisualElementConfig
{
    public record VisualElementRecommendation(
        string[] RequiredElements,
        string[] OptionalElements,
        string Guidance);

    private static readonly Dictionary<string, (VisualElementRecommendation Fr, VisualElementRecommendation En)> Config = new()
    {
        ["ExecutiveSummary"] = (
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:metrics — Chiffres clés : CA prévu année 1, marge brute, seuil de rentabilité, financement demandé"
                },
                OptionalElements: new[]
                {
                    "json:chart (bar) — Aperçu des revenus sur 3 ans"
                },
                Guidance: "Le résumé exécutif doit commencer par des métriques percutantes qui captent l'attention de l'investisseur."),
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:metrics — Key figures: projected Year 1 revenue, gross margin, break-even point, funding requested"
                },
                OptionalElements: new[]
                {
                    "json:chart (bar) — 3-year revenue overview"
                },
                Guidance: "The executive summary should lead with impactful metrics that capture investor attention.")
        ),

        ["MarketAnalysis"] = (
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:chart (pie ou donut) — Segmentation du marché cible",
                    "json:metrics — TAM, SAM, SOM avec taux de croissance (TCAC)"
                },
                OptionalElements: new[]
                {
                    "json:chart (bar) — Croissance du marché sur 5 ans",
                    "json:table (comparison) — Segments de clientèle : taille, besoins, valeur"
                },
                Guidance: "L'analyse de marché doit être riche en données visuelles pour crédibiliser les chiffres."),
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:chart (pie or donut) — Target market segmentation",
                    "json:metrics — TAM, SAM, SOM with growth rates (CAGR)"
                },
                OptionalElements: new[]
                {
                    "json:chart (bar) — 5-year market growth",
                    "json:table (comparison) — Customer segments: size, needs, value"
                },
                Guidance: "Market analysis should be rich in visual data to make the figures credible.")
        ),

        ["CompetitiveAnalysis"] = (
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (comparison) — Matrice concurrentielle : vous vs 3-5 concurrents (caractéristiques, prix, forces, faiblesses)"
                },
                OptionalElements: new[]
                {
                    "json:chart (bar) — Parts de marché des concurrents",
                    "json:infographic (icon-list) — Avantages concurrentiels clés"
                },
                Guidance: "Un tableau comparatif est indispensable pour que le lecteur visualise votre positionnement."),
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (comparison) — Competitive matrix: you vs 3-5 competitors (features, pricing, strengths, weaknesses)"
                },
                OptionalElements: new[]
                {
                    "json:chart (bar) — Competitor market shares",
                    "json:infographic (icon-list) — Key competitive advantages"
                },
                Guidance: "A comparison table is essential for the reader to visualize your positioning.")
        ),

        ["SwotAnalysis"] = (
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:swot — Matrice SWOT complète avec 3-5 éléments par quadrant (forces, faiblesses, opportunités, menaces)"
                },
                OptionalElements: Array.Empty<string>(),
                Guidance: "Utilisez OBLIGATOIREMENT le format json:swot avec tableType='swot'. Chaque quadrant doit avoir 3-5 éléments spécifiques au projet."),
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:swot — Full SWOT matrix with 3-5 items per quadrant (strengths, weaknesses, opportunities, threats)"
                },
                OptionalElements: Array.Empty<string>(),
                Guidance: "You MUST use the json:swot format with tableType='swot'. Each quadrant should have 3-5 items specific to the project.")
        ),

        ["BusinessModel"] = (
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (financial) — Sources de revenus : modèle, prix unitaire, volume estimé, revenu mensuel",
                    "json:metrics — Marge brute, coût d'acquisition client (CAC), valeur vie client (LTV)"
                },
                OptionalElements: new[]
                {
                    "json:chart (pie) — Répartition des sources de revenus",
                    "json:infographic (process-flow) — Flux de valeur client"
                },
                Guidance: "Le modèle d'affaires doit démontrer la viabilité économique avec des chiffres concrets."),
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (financial) — Revenue streams: model, unit price, estimated volume, monthly revenue",
                    "json:metrics — Gross margin, customer acquisition cost (CAC), customer lifetime value (LTV)"
                },
                OptionalElements: new[]
                {
                    "json:chart (pie) — Revenue stream breakdown",
                    "json:infographic (process-flow) — Customer value flow"
                },
                Guidance: "The business model must demonstrate economic viability with concrete figures.")
        ),

        ["FinancialProjections"] = (
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (financial) — Projections sur 3 ans : revenus, coûts directs, frais d'exploitation, bénéfice net",
                    "json:chart (bar ou line) — Évolution revenus vs dépenses sur 3 ans",
                    "json:metrics — Seuil de rentabilité (mois), marge nette année 3, ROI projeté"
                },
                OptionalElements: new[]
                {
                    "json:chart (area) — Flux de trésorerie cumulatif",
                    "json:table (financial) — Hypothèses clés : taux de croissance, prix, volume"
                },
                Guidance: "Les projections financières DOIVENT inclure des tableaux chiffrés et des graphiques. C'est la section la plus scrutée par les investisseurs et banquiers."),
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (financial) — 3-year projections: revenue, direct costs, operating expenses, net income",
                    "json:chart (bar or line) — Revenue vs expenses evolution over 3 years",
                    "json:metrics — Break-even point (months), Year 3 net margin, projected ROI"
                },
                OptionalElements: new[]
                {
                    "json:chart (area) — Cumulative cash flow",
                    "json:table (financial) — Key assumptions: growth rate, pricing, volume"
                },
                Guidance: "Financial projections MUST include data tables and charts. This is the most scrutinized section by investors and bankers.")
        ),

        ["MarketingStrategy"] = (
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (financial) — Budget marketing par canal : budget mensuel, CAC estimé, clients attendus",
                    "json:metrics — Budget marketing total, CAC moyen, taux de conversion cible"
                },
                OptionalElements: new[]
                {
                    "json:chart (pie) — Répartition du budget par canal",
                    "json:infographic (timeline) — Calendrier de lancement marketing"
                },
                Guidance: "La stratégie marketing doit montrer un plan d'action chiffré, pas seulement des intentions."),
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (financial) — Marketing budget by channel: monthly budget, estimated CAC, expected customers",
                    "json:metrics — Total marketing budget, average CAC, target conversion rate"
                },
                OptionalElements: new[]
                {
                    "json:chart (pie) — Budget allocation by channel",
                    "json:infographic (timeline) — Marketing launch timeline"
                },
                Guidance: "Marketing strategy should show a quantified action plan, not just intentions.")
        ),

        ["RiskAnalysis"] = (
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (comparison) — Matrice des risques : risque, probabilité, impact, stratégie d'atténuation"
                },
                OptionalElements: new[]
                {
                    "json:metrics — Nombre de risques critiques, moyens, faibles",
                    "json:infographic (icon-list) — Plans de contingence principaux"
                },
                Guidance: "L'analyse des risques doit être présentée dans un tableau structuré pour montrer la maturité de la réflexion."),
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (comparison) — Risk matrix: risk, probability, impact, mitigation strategy"
                },
                OptionalElements: new[]
                {
                    "json:metrics — Number of critical, medium, low risks",
                    "json:infographic (icon-list) — Key contingency plans"
                },
                Guidance: "Risk analysis must be presented in a structured table to demonstrate thoughtful planning.")
        ),

        ["FundingRequirements"] = (
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (financial) — Utilisation des fonds : catégorie, montant, pourcentage, justification",
                    "json:metrics — Montant total demandé, fonds propres investis, ratio dette/équité"
                },
                OptionalElements: new[]
                {
                    "json:chart (pie) — Répartition de l'utilisation des fonds",
                    "json:chart (bar) — Calendrier de déploiement des fonds"
                },
                Guidance: "Les besoins de financement doivent être détaillés avec un tableau d'utilisation des fonds et les métriques clés."),
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (financial) — Use of funds: category, amount, percentage, justification",
                    "json:metrics — Total amount requested, owner equity invested, debt-to-equity ratio"
                },
                OptionalElements: new[]
                {
                    "json:chart (pie) — Fund allocation breakdown",
                    "json:chart (bar) — Fund deployment timeline"
                },
                Guidance: "Funding requirements must be detailed with a use-of-funds table and key metrics.")
        ),

        ["ManagementTeam"] = (
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (custom) — Équipe : nom, rôle, compétences clés, années d'expérience"
                },
                OptionalElements: new[]
                {
                    "json:infographic (icon-list) — Compétences complémentaires de l'équipe"
                },
                Guidance: "Un tableau de l'équipe donne de la crédibilité. Montrez la complémentarité."),
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:table (custom) — Team: name, role, key skills, years of experience"
                },
                OptionalElements: new[]
                {
                    "json:infographic (icon-list) — Team complementary skills"
                },
                Guidance: "A team table adds credibility. Show complementarity.")
        ),

        ["OperationsPlan"] = (
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:infographic (process-flow) — Flux opérationnel principal : de la commande à la livraison"
                },
                OptionalElements: new[]
                {
                    "json:table (financial) — Coûts opérationnels mensuels par catégorie",
                    "json:metrics — Capacité de production, délai moyen, coût unitaire"
                },
                Guidance: "Le plan opérationnel est mieux compris avec un diagramme de flux de processus."),
            new VisualElementRecommendation(
                RequiredElements: new[]
                {
                    "json:infographic (process-flow) — Main operational flow: from order to delivery"
                },
                OptionalElements: new[]
                {
                    "json:table (financial) — Monthly operational costs by category",
                    "json:metrics — Production capacity, average lead time, unit cost"
                },
                Guidance: "The operations plan is best understood with a process flow diagram.")
        ),

        ["Solution"] = (
            new VisualElementRecommendation(
                RequiredElements: Array.Empty<string>(),
                OptionalElements: new[]
                {
                    "json:infographic (icon-list) — Caractéristiques clés de la solution",
                    "json:table (comparison) — Votre solution vs alternatives existantes"
                },
                Guidance: "Utilisez des éléments visuels pour mettre en valeur les caractéristiques distinctives de la solution."),
            new VisualElementRecommendation(
                RequiredElements: Array.Empty<string>(),
                OptionalElements: new[]
                {
                    "json:infographic (icon-list) — Key solution features",
                    "json:table (comparison) — Your solution vs existing alternatives"
                },
                Guidance: "Use visual elements to highlight the distinctive features of the solution.")
        ),

        ["ProblemStatement"] = (
            new VisualElementRecommendation(
                RequiredElements: Array.Empty<string>(),
                OptionalElements: new[]
                {
                    "json:metrics — Statistiques d'impact du problème",
                    "json:infographic (callout) — Témoignage ou citation client illustrant le problème"
                },
                Guidance: "Des métriques d'impact donnent du poids à la description du problème."),
            new VisualElementRecommendation(
                RequiredElements: Array.Empty<string>(),
                OptionalElements: new[]
                {
                    "json:metrics — Problem impact statistics",
                    "json:infographic (callout) — Customer testimony or quote illustrating the problem"
                },
                Guidance: "Impact metrics add weight to the problem description.")
        ),

        ["ExitStrategy"] = (
            new VisualElementRecommendation(
                RequiredElements: Array.Empty<string>(),
                OptionalElements: new[]
                {
                    "json:infographic (timeline) — Jalons vers la sortie",
                    "json:metrics — Valorisation cible, multiples du secteur"
                },
                Guidance: "Un chronogramme aide à visualiser la trajectoire de sortie."),
            new VisualElementRecommendation(
                RequiredElements: Array.Empty<string>(),
                OptionalElements: new[]
                {
                    "json:infographic (timeline) — Exit milestones",
                    "json:metrics — Target valuation, industry multiples"
                },
                Guidance: "A timeline helps visualize the exit trajectory.")
        ),

        ["BrandingStrategy"] = (
            new VisualElementRecommendation(
                RequiredElements: Array.Empty<string>(),
                OptionalElements: new[]
                {
                    "json:infographic (icon-list) — Piliers de la marque : valeurs, ton, personnalité",
                    "json:table (custom) — Identité de marque : élément, description, raisonnement"
                },
                Guidance: "Les éléments visuels aident à structurer l'identité de marque de façon claire."),
            new VisualElementRecommendation(
                RequiredElements: Array.Empty<string>(),
                OptionalElements: new[]
                {
                    "json:infographic (icon-list) — Brand pillars: values, tone, personality",
                    "json:table (custom) — Brand identity: element, description, rationale"
                },
                Guidance: "Visual elements help structure brand identity clearly.")
        ),
    };

    /// <summary>
    /// Format visual element recommendations for inclusion in an AI prompt.
    /// Returns null if no recommendations exist for the section.
    /// </summary>
    public static string? FormatForPrompt(string sectionName, string language)
    {
        // Normalize: "executive-summary" → "ExecutiveSummary"
        var key = Common.Constants.SectionNames.ToPascalCase(sectionName);

        if (!Config.TryGetValue(key, out var pair))
            return null;

        var isFr = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        var rec = isFr ? pair.Fr : pair.En;

        var lines = new List<string>();

        lines.Add(isFr
            ? "=== ÉLÉMENTS VISUELS REQUIS ==="
            : "=== REQUIRED VISUAL ELEMENTS ===");

        lines.Add(isFr ? rec.Guidance : rec.Guidance);
        lines.Add("");

        if (rec.RequiredElements.Length > 0)
        {
            lines.Add(isFr ? "OBLIGATOIRE — incluez ces éléments visuels :" : "MANDATORY — include these visual elements:");
            foreach (var el in rec.RequiredElements)
                lines.Add($"  • {el}");
            lines.Add("");
        }

        if (rec.OptionalElements.Length > 0)
        {
            lines.Add(isFr ? "RECOMMANDÉ (si pertinent) :" : "RECOMMENDED (if relevant):");
            foreach (var el in rec.OptionalElements)
                lines.Add($"  • {el}");
            lines.Add("");
        }

        lines.Add(isFr
            ? "Rappel : utilisez les blocs ```json:chart```, ```json:table```, ```json:metrics```, ```json:swot```, ```json:infographic``` avec du JSON valide."
            : "Reminder: use ```json:chart```, ```json:table```, ```json:metrics```, ```json:swot```, ```json:infographic``` blocks with valid JSON.");

        return string.Join(Environment.NewLine, lines);
    }
}
