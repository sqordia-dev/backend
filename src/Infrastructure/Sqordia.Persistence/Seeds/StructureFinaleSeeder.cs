using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Persistence.Seeds;

/// <summary>
/// Seeds the STRUCTURE FINALE business plan structure:
/// - 8 main sections with sub-sections
/// - 22 core questionnaire questions (V3)
/// - Question-to-section mappings
/// </summary>
public class StructureFinaleSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StructureFinaleSeeder> _logger;

    public StructureFinaleSeeder(ApplicationDbContext context, ILogger<StructureFinaleSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting STRUCTURE FINALE seeding...");

        await SeedOrUpdateMainSectionsAsync(cancellationToken);
        await SeedQuestionTemplatesAsync(cancellationToken);
        await SeedDefaultMappingsAsync(cancellationToken);

        _logger.LogInformation("STRUCTURE FINALE seeding completed.");
    }

    private async Task SeedOrUpdateMainSectionsAsync(CancellationToken cancellationToken)
    {
        var existingSections = await _context.MainSections
            .Include(s => s.SubSections)
            .ToListAsync(cancellationToken);
        var newSections = GetMainSectionsWithSubSections();

        int added = 0, updated = 0;

        foreach (var newSection in newSections)
        {
            var existing = existingSections.FirstOrDefault(s => s.Number == newSection.Number);
            if (existing != null)
            {
                // Update existing section titles and descriptions
                existing.UpdateTitles(newSection.TitleFR, newSection.TitleEN);
                existing.UpdateDescriptions(newSection.DescriptionFR, newSection.DescriptionEN);

                // Update sub-sections
                foreach (var newSub in newSection.SubSections)
                {
                    var existingSub = existing.SubSections.FirstOrDefault(s => s.Code == newSub.Code);
                    if (existingSub != null)
                    {
                        existingSub.UpdateTitles(newSub.TitleFR, newSub.TitleEN);
                    }
                    else
                    {
                        var sub = new SubSection(
                            mainSectionId: existing.Id,
                            code: newSub.Code,
                            titleFR: newSub.TitleFR,
                            titleEN: newSub.TitleEN,
                            displayOrder: newSub.DisplayOrder);
                        existing.SubSections.Add(sub);
                    }
                }
                updated++;
            }
            else
            {
                _context.MainSections.Add(newSection);
                added++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Main sections seed completed. Added: {Added}, Updated: {Updated}.", added, updated);
    }

    private async Task SeedQuestionTemplatesAsync(CancellationToken cancellationToken)
    {
        var existingCount = await _context.QuestionTemplates.CountAsync(cancellationToken);
        if (existingCount > 0)
        {
            _logger.LogInformation("Question templates V3 already exist ({Count} found). Skipping.", existingCount);
            return;
        }

        var questions = GetQuestionTemplates();

        foreach (var question in questions)
        {
            _context.QuestionTemplates.Add(question);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} question templates V3.", questions.Count);
    }

    private async Task SeedDefaultMappingsAsync(CancellationToken cancellationToken)
    {
        var existingCount = await _context.QuestionSectionMappings.CountAsync(cancellationToken);
        if (existingCount > 0)
        {
            _logger.LogInformation("Question mappings already exist ({Count} found). Skipping.", existingCount);
            return;
        }

        // Load questions and sub-sections for mapping
        var questions = await _context.QuestionTemplates.ToListAsync(cancellationToken);
        var subSections = await _context.SubSections.Include(s => s.MainSection).ToListAsync(cancellationToken);

        var mappings = CreateDefaultMappings(questions, subSections);

        foreach (var mapping in mappings)
        {
            _context.QuestionSectionMappings.Add(mapping);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} question-section mappings.", mappings.Count);
    }

    private List<MainSection> GetMainSectionsWithSubSections()
    {
        var sections = new List<MainSection>();

        // Section 0: Sommaire Executif (Executive Summary) - Generated LAST
        var execSummary = MainSection.Create(
            number: 0,
            code: "executive_summary",
            titleFR: "Sommaire Exécutif",
            titleEN: "Executive Summary",
            descriptionFR: "Résumé stratégique du plan d'affaires, généré en dernier après toutes les autres sections.",
            descriptionEN: "Strategic summary of the business plan, generated last after all other sections.",
            displayOrder: 0,
            generatedLast: true,
            icon: "📋"
        );
        CreateSubSections(execSummary, new[]
        {
            ("0.1", "Aperçu de l'entreprise", "Company Overview"),
            ("0.2", "Opportunité de marché", "Market Opportunity"),
            ("0.3", "Produits et services", "Products and Services"),
            ("0.4", "Avantages concurrentiels", "Competitive Advantages"),
            ("0.5", "Objectifs financiers", "Financial Objectives"),
            ("0.6", "Besoins de financement", "Funding Requirements"),
            ("0.7", "Conclusion et appel à l'action", "Conclusion and Call to Action"),
        });
        sections.Add(execSummary);

        // Section 1: Le Projet (The Project)
        var project = MainSection.Create(
            number: 1,
            code: "le_projet",
            titleFR: "Le Projet",
            titleEN: "The Project",
            descriptionFR: "Présentation complète du projet d'entreprise, de sa vision et de ses objectifs.",
            descriptionEN: "Complete presentation of the business project, its vision and objectives.",
            displayOrder: 1,
            generatedLast: false,
            icon: "💡"
        );
        CreateSubSections(project, new[]
        {
            ("1.1", "Description de l'entreprise", "Company Description"),
            ("1.2", "Mission et vision", "Mission and Vision"),
            ("1.3", "Objectifs stratégiques", "Strategic Objectives"),
            ("1.4", "Structure juridique", "Legal Structure"),
            ("1.5", "Historique et étapes clés", "History and Key Milestones"),
            ("1.6", "Localisation et installations", "Location and Facilities"),
            ("1.7", "Partenariats stratégiques", "Strategic Partnerships"),
        });
        sections.Add(project);

        // Section 2: Les Promoteurs (The Promoters)
        var promoters = MainSection.Create(
            number: 2,
            code: "promoteurs",
            titleFR: "Les Promoteurs",
            titleEN: "The Promoters",
            descriptionFR: "Présentation de l'équipe fondatrice et de direction.",
            descriptionEN: "Presentation of the founding and management team.",
            displayOrder: 2,
            generatedLast: false,
            icon: "👥"
        );
        CreateSubSections(promoters, new[]
        {
            ("2.1", "Équipe de direction", "Management Team"),
            ("2.2", "Compétences et expériences", "Skills and Experience"),
            ("2.3", "Rôles et responsabilités", "Roles and Responsibilities"),
            ("2.4", "Conseillers et mentors", "Advisors and Mentors"),
        });
        sections.Add(promoters);

        // Section 3: Etude de Marche (Market Study)
        var marketStudy = MainSection.Create(
            number: 3,
            code: "etude_marche",
            titleFR: "Étude de Marché",
            titleEN: "Market Study",
            descriptionFR: "Analyse approfondie du marché cible et de l'environnement concurrentiel.",
            descriptionEN: "In-depth analysis of the target market and competitive environment.",
            displayOrder: 3,
            generatedLast: false,
            icon: "🔍"
        );
        CreateSubSections(marketStudy, new[]
        {
            ("3.1", "Analyse du secteur", "Industry Analysis"),
            ("3.2", "Marché cible", "Target Market"),
            ("3.3", "Segmentation client", "Customer Segmentation"),
            ("3.4", "Analyse de la concurrence", "Competitive Analysis"),
            ("3.5", "Tendances du marché", "Market Trends"),
            ("3.6", "Taille et croissance du marché", "Market Size and Growth"),
            ("3.7", "Analyse PESTEL", "PESTEL Analysis"),
        });
        sections.Add(marketStudy);

        // Section 4: Plan Marketing et Ventes (Marketing and Sales Plan)
        var marketing = MainSection.Create(
            number: 4,
            code: "plan_marketing",
            titleFR: "Plan Marketing et Ventes",
            titleEN: "Marketing and Sales Plan",
            descriptionFR: "Stratégie de mise en marché et d'acquisition de clients.",
            descriptionEN: "Go-to-market strategy and customer acquisition.",
            displayOrder: 4,
            generatedLast: false,
            icon: "🎯"
        );
        CreateSubSections(marketing, new[]
        {
            ("4.1", "Stratégie de positionnement", "Positioning Strategy"),
            ("4.2", "Mix marketing (4P)", "Marketing Mix (4P)"),
            ("4.3", "Stratégie de prix", "Pricing Strategy"),
            ("4.4", "Canaux de distribution", "Distribution Channels"),
            ("4.5", "Stratégie de communication", "Communication Strategy"),
            ("4.6", "Processus de vente", "Sales Process"),
            ("4.7", "Budget marketing", "Marketing Budget"),
        });
        sections.Add(marketing);

        // Section 5: Plan Operationnel (Operational Plan)
        var operations = MainSection.Create(
            number: 5,
            code: "plan_operationnel",
            titleFR: "Plan Opérationnel",
            titleEN: "Operational Plan",
            descriptionFR: "Détails des opérations quotidiennes et de la production.",
            descriptionEN: "Details of daily operations and production.",
            displayOrder: 5,
            generatedLast: false,
            icon: "⚙️"
        );
        CreateSubSections(operations, new[]
        {
            ("5.1", "Processus de production", "Production Process"),
            ("5.2", "Chaîne d'approvisionnement", "Supply Chain"),
            ("5.3", "Gestion des stocks", "Inventory Management"),
            ("5.4", "Contrôle qualité", "Quality Control"),
            ("5.5", "Infrastructure technologique", "Technology Infrastructure"),
            ("5.6", "Ressources humaines", "Human Resources"),
            ("5.7", "Calendrier de mise en œuvre", "Implementation Timeline"),
            ("5.8", "Gestion des risques", "Risk Management"),
            ("5.9", "Conformité réglementaire", "Regulatory Compliance"),
        });
        sections.Add(operations);

        // Section 6: Analyse Financiere (Financial Analysis)
        var financial = MainSection.Create(
            number: 6,
            code: "analyse_financiere",
            titleFR: "Analyse Financière",
            titleEN: "Financial Analysis",
            descriptionFR: "Projections financières et analyse de rentabilité.",
            descriptionEN: "Financial projections and profitability analysis.",
            displayOrder: 6,
            generatedLast: false,
            icon: "💰"
        );
        CreateSubSections(financial, new[]
        {
            ("6.1", "Hypothèses financières", "Financial Assumptions"),
            ("6.2", "Coût de démarrage", "Startup Costs"),
            ("6.3", "Prévisions de revenus", "Revenue Projections"),
            ("6.4", "Prévisions de dépenses", "Expense Projections"),
            ("6.5", "État des résultats prévisionnels", "Pro Forma Income Statement"),
            ("6.6", "Bilan prévisionnel", "Pro Forma Balance Sheet"),
            ("6.7", "Flux de trésorerie", "Cash Flow Projections"),
            ("6.8", "Analyse du seuil de rentabilité", "Break-even Analysis"),
            ("6.9", "Ratios financiers", "Financial Ratios"),
        });
        sections.Add(financial);

        // Section 7: Annexes (Appendices)
        var appendices = MainSection.Create(
            number: 7,
            code: "annexes",
            titleFR: "Annexes",
            titleEN: "Appendices",
            descriptionFR: "Documents supplémentaires et informations de référence.",
            descriptionEN: "Supporting documents and reference information.",
            displayOrder: 7,
            generatedLast: false,
            icon: "📎"
        );
        CreateSubSections(appendices, new[]
        {
            ("7.1", "CV des promoteurs", "Promoter Resumes"),
            ("7.2", "Études de marché détaillées", "Detailed Market Research"),
            ("7.3", "Lettres d'intention", "Letters of Intent"),
            ("7.4", "Contrats et ententes", "Contracts and Agreements"),
            ("7.5", "Brevets et propriété intellectuelle", "Patents and IP"),
            ("7.6", "Tableaux financiers détaillés", "Detailed Financial Tables"),
            ("7.7", "Références et sources", "References and Sources"),
        });
        sections.Add(appendices);

        return sections;
    }

    private void CreateSubSections(MainSection mainSection, (string code, string titleFR, string titleEN)[] subSections)
    {
        for (int i = 0; i < subSections.Length; i++)
        {
            var (code, titleFR, titleEN) = subSections[i];
            var subSection = new SubSection(
                mainSectionId: mainSection.Id,
                code: code,
                titleFR: titleFR,
                titleEN: titleEN,
                displayOrder: i,
                descriptionFR: null,
                descriptionEN: null,
                icon: null
            );
            mainSection.SubSections.Add(subSection);
        }
    }

    private List<QuestionTemplate> GetQuestionTemplates()
    {
        var questions = new List<QuestionTemplate>();

        // Step 1: Le Projet (The Project)
        questions.Add(CreateQuestion(1, 1, QuestionType.ShortText,
            "Quel est le nom de votre entreprise ou projet?",
            "What is the name of your company or project?",
            "Le nom officiel ou le nom de marque prévu.",
            "The official name or planned brand name.",
            "Aidez l'utilisateur à choisir un nom mémorable et distinctif qui reflète l'identité de l'entreprise.",
            "Help the user choose a memorable and distinctive name that reflects the company's identity."
        ));

        questions.Add(CreateQuestion(2, 1, QuestionType.LongText,
            "Décrivez votre entreprise en quelques phrases.",
            "Describe your business in a few sentences.",
            "Expliquez ce que fait votre entreprise et pourquoi elle existe.",
            "Explain what your company does and why it exists.",
            "Guidez l'utilisateur pour formuler une description claire et concise de leur proposition de valeur unique.",
            "Guide the user to formulate a clear and concise description of their unique value proposition."
        ));

        questions.Add(CreateQuestion(3, 1, QuestionType.LongText,
            "Quelle est votre mission d'entreprise?",
            "What is your company mission?",
            "La raison d'être de votre entreprise au-delà du profit.",
            "Your company's reason for being beyond profit.",
            "Aidez à formuler une mission inspirante qui motive l'équipe et résonne avec les clients.",
            "Help formulate an inspiring mission that motivates the team and resonates with customers."
        ));

        questions.Add(CreateQuestion(4, 1, QuestionType.SingleChoice,
            "Quelle est la structure juridique de votre entreprise?",
            "What is the legal structure of your company?",
            "Choisissez le type d'entité juridique.",
            "Choose the type of legal entity.",
            null, null,
            optionsFR: "[\"Entreprise individuelle\",\"Société en nom collectif (SNC)\",\"Société par actions (SPA/Inc)\",\"Coopérative\",\"OBNL\",\"Autre\"]",
            optionsEN: "[\"Sole proprietorship\",\"Partnership\",\"Corporation (Inc)\",\"Cooperative\",\"Non-profit\",\"Other\"]"
        ));

        // Step 2: Les Promoteurs (The Promoters)
        questions.Add(CreateQuestion(5, 2, QuestionType.LongText,
            "Présentez-vous et votre parcours professionnel.",
            "Introduce yourself and your professional background.",
            "Décrivez votre expérience et vos compétences clés.",
            "Describe your experience and key skills.",
            "Aidez à mettre en valeur l'expérience pertinente et les accomplissements qui renforcent la crédibilité.",
            "Help highlight relevant experience and accomplishments that strengthen credibility."
        ));

        questions.Add(CreateQuestion(6, 2, QuestionType.LongText,
            "Qui sont les autres membres clés de votre équipe?",
            "Who are the other key members of your team?",
            "Présentez les cofondateurs, gestionnaires et conseillers clés.",
            "Introduce co-founders, key managers and advisors.",
            "Guidez pour présenter l'équipe de manière à montrer la complémentarité des compétences.",
            "Guide to present the team in a way that shows complementary skills."
        ));

        // Step 3: Etude de Marche (Market Study)
        questions.Add(CreateQuestion(7, 3, QuestionType.LongText,
            "Décrivez votre marché cible.",
            "Describe your target market.",
            "Qui sont vos clients idéaux? Quelles sont leurs caractéristiques?",
            "Who are your ideal customers? What are their characteristics?",
            "Aidez à définir un profil client détaillé avec des caractéristiques démographiques et psychographiques.",
            "Help define a detailed customer profile with demographic and psychographic characteristics."
        ));

        questions.Add(CreateQuestion(8, 3, QuestionType.LongText,
            "Quelle est la taille de votre marché?",
            "What is the size of your market?",
            "Estimez le marché total, le marché disponible et le marché cible.",
            "Estimate the total market, available market and target market.",
            "Guidez pour calculer TAM, SAM et SOM avec des sources fiables.",
            "Guide to calculate TAM, SAM and SOM with reliable sources."
        ));

        questions.Add(CreateQuestion(9, 3, QuestionType.LongText,
            "Qui sont vos principaux concurrents?",
            "Who are your main competitors?",
            "Identifiez les concurrents directs et indirects.",
            "Identify direct and indirect competitors.",
            "Aidez à réaliser une analyse concurrentielle objective et complète.",
            "Help conduct an objective and comprehensive competitive analysis."
        ));

        questions.Add(CreateQuestion(10, 3, QuestionType.LongText,
            "Quels sont vos avantages concurrentiels?",
            "What are your competitive advantages?",
            "Qu'est-ce qui vous différencie de la concurrence?",
            "What differentiates you from the competition?",
            "Guidez pour identifier et articuler des avantages durables et défendables.",
            "Guide to identify and articulate sustainable and defensible advantages."
        ));

        // Step 4: Plan Marketing (Marketing Plan)
        questions.Add(CreateQuestion(11, 4, QuestionType.LongText,
            "Décrivez vos produits ou services.",
            "Describe your products or services.",
            "Détails de votre offre, fonctionnalités et bénéfices.",
            "Details of your offering, features and benefits.",
            "Aidez à présenter l'offre en termes de valeur pour le client plutôt que de caractéristiques techniques.",
            "Help present the offering in terms of customer value rather than technical features."
        ));

        questions.Add(CreateQuestion(12, 4, QuestionType.LongText,
            "Quelle est votre stratégie de prix?",
            "What is your pricing strategy?",
            "Comment avez-vous déterminé vos prix?",
            "How did you determine your prices?",
            "Guidez pour justifier la stratégie de prix par rapport à la valeur et au marché.",
            "Guide to justify pricing strategy relative to value and market."
        ));

        questions.Add(CreateQuestion(13, 4, QuestionType.LongText,
            "Comment allez-vous atteindre vos clients?",
            "How will you reach your customers?",
            "Canaux de distribution et stratégies marketing.",
            "Distribution channels and marketing strategies.",
            "Aidez à définir un mix marketing cohérent et adapté au budget.",
            "Help define a consistent marketing mix adapted to the budget."
        ));

        questions.Add(CreateQuestion(14, 4, QuestionType.LongText,
            "Quel est votre processus de vente?",
            "What is your sales process?",
            "Décrivez les étapes de l'acquisition à la conversion.",
            "Describe the steps from acquisition to conversion.",
            "Guidez pour définir un entonnoir de vente clair et mesurable.",
            "Guide to define a clear and measurable sales funnel."
        ));

        // Step 5: Plan Operationnel (Operational Plan)
        questions.Add(CreateQuestion(15, 5, QuestionType.LongText,
            "Décrivez vos opérations quotidiennes.",
            "Describe your daily operations.",
            "Comment fonctionne votre entreprise au jour le jour?",
            "How does your business operate day to day?",
            "Aidez à décrire les processus opérationnels de manière claire et efficiente.",
            "Help describe operational processes in a clear and efficient manner."
        ));

        questions.Add(CreateQuestion(16, 5, QuestionType.LongText,
            "Quels sont vos besoins en ressources humaines?",
            "What are your human resource needs?",
            "Équipe actuelle et besoins de recrutement futurs.",
            "Current team and future hiring needs.",
            "Guidez pour planifier les besoins en personnel en fonction de la croissance.",
            "Guide to plan staffing needs based on growth."
        ));

        questions.Add(CreateQuestion(17, 5, QuestionType.LongText,
            "Quels sont les principaux risques identifiés?",
            "What are the main identified risks?",
            "Risques opérationnels, financiers, de marché, etc.",
            "Operational, financial, market risks, etc.",
            "Aidez à identifier les risques et proposer des stratégies d'atténuation.",
            "Help identify risks and propose mitigation strategies."
        ));

        // Step 6: Analyse Financiere (Financial Analysis)
        questions.Add(CreateQuestion(18, 6, QuestionType.Currency,
            "Quel est votre investissement initial requis?",
            "What is your required initial investment?",
            "Montant total nécessaire pour démarrer.",
            "Total amount needed to start.",
            "Guidez pour calculer les coûts de démarrage de manière exhaustive.",
            "Guide to calculate startup costs comprehensively."
        ));

        questions.Add(CreateQuestion(19, 6, QuestionType.LongText,
            "Quelles sont vos sources de financement?",
            "What are your funding sources?",
            "Fonds propres, prêts, subventions, investisseurs.",
            "Own funds, loans, grants, investors.",
            "Aidez à identifier toutes les sources de financement possibles.",
            "Help identify all possible funding sources."
        ));

        questions.Add(CreateQuestion(20, 6, QuestionType.LongText,
            "Quelles sont vos projections de revenus?",
            "What are your revenue projections?",
            "Estimations de ventes pour les 3-5 prochaines années.",
            "Sales estimates for the next 3-5 years.",
            "Guidez pour créer des projections réalistes basées sur des hypothèses solides.",
            "Guide to create realistic projections based on solid assumptions."
        ));

        questions.Add(CreateQuestion(21, 6, QuestionType.Number,
            "À quel moment atteindrez-vous le seuil de rentabilité?",
            "When will you reach break-even?",
            "Nombre de mois ou volume de ventes nécessaire.",
            "Number of months or sales volume needed.",
            "Aidez à calculer le point mort et les facteurs clés de rentabilité.",
            "Help calculate break-even point and key profitability factors."
        ));

        // Step 7: Business Model Canvas (optional/advanced)
        questions.Add(CreateQuestion(22, 7, QuestionType.LongText,
            "Résumez votre modèle d'affaires.",
            "Summarize your business model.",
            "Les 9 blocs du Business Model Canvas.",
            "The 9 building blocks of the Business Model Canvas.",
            "Guidez pour compléter chaque bloc du BMC de manière cohérente.",
            "Guide to complete each BMC block consistently."
        ));

        return questions;
    }

    private QuestionTemplate CreateQuestion(
        int questionNumber,
        int stepNumber,
        QuestionType questionType,
        string questionTextFR,
        string questionTextEN,
        string? helpTextFR,
        string? helpTextEN,
        string? coachPromptFR,
        string? coachPromptEN,
        string? optionsFR = null,
        string? optionsEN = null)
    {
        return QuestionTemplate.Create(
            questionNumber: questionNumber,
            personaType: null, // Universal for all personas
            stepNumber: stepNumber,
            questionTextFR: questionTextFR,
            questionTextEN: questionTextEN,
            helpTextFR: helpTextFR,
            helpTextEN: helpTextEN,
            questionType: questionType,
            optionsFR: optionsFR,
            optionsEN: optionsEN,
            validationRules: null,
            conditionalLogic: null,
            coachPromptFR: coachPromptFR,
            coachPromptEN: coachPromptEN,
            expertAdviceFR: null,
            expertAdviceEN: null,
            displayOrder: questionNumber,
            isRequired: questionNumber <= 10, // First 10 questions are required
            icon: null
        );
    }

    private List<QuestionSectionMapping> CreateDefaultMappings(
        List<QuestionTemplate> questions,
        List<SubSection> subSections)
    {
        var mappings = new List<QuestionSectionMapping>();

        // Map questions to relevant sub-sections
        // Question 1 (Company name) -> 1.1 Description, 0.1 Company Overview
        MapQuestion(mappings, questions, 1, new[] { "1.1", "0.1" }, subSections);

        // Question 2 (Business description) -> 1.1 Description, 0.1 Company Overview
        MapQuestion(mappings, questions, 2, new[] { "1.1", "0.1", "0.3" }, subSections);

        // Question 3 (Mission) -> 1.2 Mission and Vision
        MapQuestion(mappings, questions, 3, new[] { "1.2" }, subSections);

        // Question 4 (Legal structure) -> 1.4 Legal Structure
        MapQuestion(mappings, questions, 4, new[] { "1.4" }, subSections);

        // Question 5 (Promoter background) -> 2.1 Management Team, 2.2 Skills
        MapQuestion(mappings, questions, 5, new[] { "2.1", "2.2" }, subSections);

        // Question 6 (Team members) -> 2.1, 2.3
        MapQuestion(mappings, questions, 6, new[] { "2.1", "2.3" }, subSections);

        // Question 7 (Target market) -> 3.2, 3.3
        MapQuestion(mappings, questions, 7, new[] { "3.2", "3.3", "0.2" }, subSections);

        // Question 8 (Market size) -> 3.6
        MapQuestion(mappings, questions, 8, new[] { "3.6", "0.2" }, subSections);

        // Question 9 (Competitors) -> 3.4
        MapQuestion(mappings, questions, 9, new[] { "3.4" }, subSections);

        // Question 10 (Competitive advantages) -> 3.4, 0.4
        MapQuestion(mappings, questions, 10, new[] { "3.4", "0.4" }, subSections);

        // Question 11 (Products/Services) -> 4.2, 0.3
        MapQuestion(mappings, questions, 11, new[] { "4.2", "0.3" }, subSections);

        // Question 12 (Pricing) -> 4.3
        MapQuestion(mappings, questions, 12, new[] { "4.3" }, subSections);

        // Question 13 (Marketing channels) -> 4.4, 4.5
        MapQuestion(mappings, questions, 13, new[] { "4.4", "4.5" }, subSections);

        // Question 14 (Sales process) -> 4.6
        MapQuestion(mappings, questions, 14, new[] { "4.6" }, subSections);

        // Question 15 (Operations) -> 5.1
        MapQuestion(mappings, questions, 15, new[] { "5.1", "5.2" }, subSections);

        // Question 16 (HR needs) -> 5.6
        MapQuestion(mappings, questions, 16, new[] { "5.6" }, subSections);

        // Question 17 (Risks) -> 5.8
        MapQuestion(mappings, questions, 17, new[] { "5.8" }, subSections);

        // Question 18 (Investment) -> 6.2, 0.6
        MapQuestion(mappings, questions, 18, new[] { "6.2", "0.6" }, subSections);

        // Question 19 (Funding sources) -> 6.2, 0.6
        MapQuestion(mappings, questions, 19, new[] { "6.2", "0.6" }, subSections);

        // Question 20 (Revenue projections) -> 6.3, 0.5
        MapQuestion(mappings, questions, 20, new[] { "6.3", "0.5" }, subSections);

        // Question 21 (Break-even) -> 6.8
        MapQuestion(mappings, questions, 21, new[] { "6.8" }, subSections);

        // Question 22 (Business Model) -> Multiple sections
        MapQuestion(mappings, questions, 22, new[] { "1.1", "3.2", "4.2", "6.3" }, subSections);

        return mappings;
    }

    private void MapQuestion(
        List<QuestionSectionMapping> mappings,
        List<QuestionTemplate> questions,
        int questionNumber,
        string[] subSectionCodes,
        List<SubSection> allSubSections)
    {
        var question = questions.FirstOrDefault(q => q.QuestionNumber == questionNumber);
        if (question == null) return;

        for (int i = 0; i < subSectionCodes.Length; i++)
        {
            var subSection = allSubSections.FirstOrDefault(s => s.Code == subSectionCodes[i]);
            if (subSection == null) continue;

            var context = i == 0 ? "primary" : "secondary";
            var weight = i == 0 ? 1.0m : 0.5m;

            var mapping = QuestionSectionMapping.Create(
                questionTemplateV3Id: question.Id,
                subSectionId: subSection.Id,
                mappingContext: context,
                weight: weight,
                transformationHint: null
            );

            mappings.Add(mapping);
        }
    }
}
