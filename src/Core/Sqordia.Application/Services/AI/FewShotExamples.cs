namespace Sqordia.Application.Services.AI;

/// <summary>
/// Few-shot examples showing good vs bad output per section type.
/// Injected into prompts to calibrate LLM output quality.
/// </summary>
public static class FewShotExamples
{
    public record FewShotPair(string GoodExample, string BadExample);

    private static readonly Dictionary<string, (FewShotPair Fr, FewShotPair En)> Examples = new()
    {
        ["MarketAnalysis"] = (
            new FewShotPair(
                GoodExample: @"Le marché canadien des technologies RH est évalué à 2,3 milliards $ en 2025, avec un TCAC de 11,2 % (source : Mordor Intelligence). Le segment des PME manufacturières (200-500 employés) représente un marché adressable de 340 M$, soit environ 4 200 entreprises au Québec et en Ontario. Trois segments clients se distinguent : (1) les directeurs RH cherchant l'automatisation de la conformité (45 % du marché), (2) les propriétaires-dirigeants de PME sans département RH dédié (35 %), et (3) les consultants RH indépendants desservant plusieurs PME (20 %). La croissance est tirée par le durcissement réglementaire en santé-sécurité et la pénurie de main-d'œuvre, qui pousse les PME à investir dans la rétention.",
                BadExample: @"Le marché des technologies est en pleine croissance. De nombreuses entreprises cherchent des solutions innovantes pour améliorer leur gestion. Notre marché cible est très large et offre de grandes opportunités. La demande ne cesse d'augmenter et les perspectives sont prometteuses pour notre entreprise."),
            new FewShotPair(
                GoodExample: @"The Canadian HR technology market is valued at $2.3B in 2025, growing at 11.2% CAGR (source: Mordor Intelligence). The mid-market manufacturing segment (200-500 employees) represents a $340M addressable market, approximately 4,200 companies in Quebec and Ontario. Three customer segments emerge: (1) HR directors seeking compliance automation (45% of market), (2) SMB owner-operators without dedicated HR departments (35%), and (3) independent HR consultants serving multiple SMBs (20%). Growth is driven by tightening health & safety regulations and labor shortages pushing SMBs to invest in retention.",
                BadExample: @"The technology market is booming. Many companies are looking for innovative solutions to improve their management. Our target market is very large and offers great opportunities. Demand continues to increase and the outlook is promising for our company.")
        ),
        ["FinancialProjections"] = (
            new FewShotPair(
                GoodExample: @"Année 1 : revenus de 180 000 $ basés sur 15 clients à 1 000 $/mois (acquisition progressive : 3 clients/trimestre). Coûts fixes : 120 000 $ (salaires 2 ETP : 90 000 $, loyer : 18 000 $, outils SaaS : 12 000 $). Marge brute : 33 %. Seuil de rentabilité atteint au mois 14 avec 12 clients actifs. Année 3 : revenus de 720 000 $ (60 clients), marge nette de 22 %. Hypothèse clé : taux de rétention de 85 % basé sur les moyennes du secteur SaaS B2B.",
                BadExample: @"Nous prévoyons des revenus de 500 000 $ la première année et de 2 000 000 $ la troisième année. Nos coûts seront bien maîtrisés et notre rentabilité sera atteinte rapidement. Le marché étant en croissance, nos projections sont conservatrices."),
            new FewShotPair(
                GoodExample: @"Year 1: Revenue of $180K based on 15 clients at $1,000/month (progressive acquisition: 3 clients/quarter). Fixed costs: $120K (salaries 2 FTE: $90K, rent: $18K, SaaS tools: $12K). Gross margin: 33%. Break-even reached at month 14 with 12 active clients. Year 3: Revenue of $720K (60 clients), net margin of 22%. Key assumption: 85% retention rate based on B2B SaaS industry averages.",
                BadExample: @"We project revenue of $500K in year one and $2M by year three. Our costs will be well managed and profitability will be reached quickly. Since the market is growing, our projections are conservative.")
        ),
        ["ExecutiveSummary"] = (
            new FewShotPair(
                GoodExample: @"RH-Conforme est une plateforme SaaS d'automatisation de la conformité RH destinée aux PME manufacturières du Québec. Face à un durcissement réglementaire qui coûte en moyenne 45 000 $/an en non-conformité aux PME (CNESST, 2024), notre solution réduit ce risque de 80 % tout en automatisant 70 % des tâches administratives RH. Avec un marché adressable de 340 M$ et un modèle d'abonnement à 1 000 $/mois, nous projetons 720 000 $ de revenus en année 3 avec une marge nette de 22 %. Notre équipe combine 15 ans d'expérience en conformité RH et 10 ans en développement SaaS. Nous sollicitons un financement de 250 000 $ pour accélérer le développement produit et l'acquisition des 50 premiers clients.",
                BadExample: @"Notre entreprise offre des services innovants dans le domaine des ressources humaines. Nous avons une équipe passionnée et expérimentée. Le marché est en pleine croissance et nous sommes bien positionnés pour réussir. Nous cherchons du financement pour développer notre activité."),
            new FewShotPair(
                GoodExample: @"RH-Conforme is a SaaS platform automating HR compliance for Quebec's manufacturing SMBs. Facing regulatory tightening that costs SMBs an average of $45K/year in non-compliance (CNESST, 2024), our solution reduces this risk by 80% while automating 70% of administrative HR tasks. With a $340M addressable market and a $1,000/month subscription model, we project $720K revenue by year 3 at 22% net margin. Our team combines 15 years of HR compliance experience with 10 years of SaaS development. We are seeking $250K in funding to accelerate product development and acquire our first 50 clients.",
                BadExample: @"Our company offers innovative services in the human resources field. We have a passionate and experienced team. The market is growing and we are well positioned to succeed. We are seeking funding to develop our business.")
        ),
        ["CompetitiveAnalysis"] = (
            new FewShotPair(
                GoodExample: @"Trois concurrents directs dominent le marché québécois : (1) ADP — leader établi (35 % de part de marché), force en paie mais faible en conformité SST, tarification élevée (2 500 $/mois min.) inadaptée aux PME. (2) BambooHR — interface moderne, populaire auprès des startups, mais pas de module conformité québécoise (CNESST, LSST). (3) Folks RH — solution locale, bonne intégration Desjardins, mais UX datée et pas d'automatisation réglementaire. Notre positionnement : seule solution combinant conformité CNESST automatisée + gestion RH intégrée à un prix PME (1 000 $/mois vs 2 500 $ pour ADP). Barrière à l'entrée : expertise réglementaire québécoise difficilement réplicable.",
                BadExample: @"Nous avons quelques concurrents sur le marché, mais notre solution est nettement supérieure. Les autres entreprises n'offrent pas la même qualité de service. Notre approche innovante nous différencie clairement de la concurrence."),
            new FewShotPair(
                GoodExample: @"Three direct competitors dominate the Quebec market: (1) ADP — established leader (35% market share), strong in payroll but weak in OHS compliance, high pricing ($2,500/month min.) unsuitable for SMBs. (2) BambooHR — modern interface, popular with startups, but no Quebec compliance module (CNESST, LSST). (3) Folks HR — local solution, good Desjardins integration, but dated UX and no regulatory automation. Our positioning: only solution combining automated CNESST compliance + integrated HR management at SMB pricing ($1,000/month vs $2,500 for ADP). Barrier to entry: Quebec regulatory expertise difficult to replicate.",
                BadExample: @"We have a few competitors in the market, but our solution is clearly superior. Other companies don't offer the same quality of service. Our innovative approach clearly differentiates us from the competition.")
        ),
        ["BusinessModel"] = (
            new FewShotPair(
                GoodExample: @"Modèle SaaS par abonnement mensuel avec 3 paliers : Essentiel (500 $/mois, conformité de base, 1-50 employés), Professionnel (1 000 $/mois, conformité complète + automatisation, 50-200 employés), Entreprise (sur mesure, 200+ employés). Revenus additionnels : formation en conformité (150 $/participant, 4 sessions/an), et intégration personnalisée (forfait unique de 2 000-5 000 $). Coût d'acquisition client estimé : 800 $ (marketing digital B2B). Valeur vie client : 18 000 $ (rétention moyenne 18 mois × 1 000 $/mois). Ratio LTV/CAC : 22,5x.",
                BadExample: @"Notre modèle d'affaires repose sur la vente de nos services. Nous offrirons différents forfaits adaptés aux besoins de nos clients. Notre tarification sera compétitive et nous générerons des revenus récurrents."),
            new FewShotPair(
                GoodExample: @"SaaS subscription model with 3 tiers: Essential ($500/month, basic compliance, 1-50 employees), Professional ($1,000/month, full compliance + automation, 50-200 employees), Enterprise (custom, 200+ employees). Additional revenue: compliance training ($150/participant, 4 sessions/year), and custom integration (one-time $2,000-5,000). Estimated customer acquisition cost: $800 (B2B digital marketing). Customer lifetime value: $18,000 (average 18-month retention × $1,000/month). LTV/CAC ratio: 22.5x.",
                BadExample: @"Our business model is based on selling our services. We will offer different packages adapted to our clients' needs. Our pricing will be competitive and we will generate recurring revenue.")
        ),
    };

    /// <summary>
    /// Gets examples for a section in the specified language. Returns null if none defined.
    /// </summary>
    public static FewShotPair? GetExamples(string sectionName, string language = "fr")
    {
        if (!Examples.TryGetValue(sectionName, out var pair))
            return null;

        return language.Equals("fr", StringComparison.OrdinalIgnoreCase) ? pair.Fr : pair.En;
    }

    /// <summary>
    /// Formats examples as a prompt block.
    /// </summary>
    public static string? FormatForPrompt(string sectionName, string language = "fr")
    {
        var examples = GetExamples(sectionName, language);
        if (examples == null) return null;

        var isFr = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
        return string.Join("\n", new[]
        {
            isFr ? "=== EXEMPLES ===" : "=== EXAMPLES ===",
            "",
            isFr ? "BON EXEMPLE (à suivre):" : "GOOD EXAMPLE (follow this):",
            examples.GoodExample,
            "",
            isFr ? "MAUVAIS EXEMPLE (à éviter):" : "BAD EXAMPLE (avoid this):",
            examples.BadExample
        });
    }
}
