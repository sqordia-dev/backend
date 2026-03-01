using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Persistence.Seeds;

/// <summary>
/// Seeds AI generation prompts for business plan sections.
/// Based on "STRUCTURE FINALE- Questions & prompt" HTML files.
/// Each sub-section gets a specific prompt for AI content generation.
/// </summary>
public class SectionPromptSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SectionPromptSeeder> _logger;

    public SectionPromptSeeder(ApplicationDbContext context, ILogger<SectionPromptSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedOrUpdateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Section Prompts seeding/update...");

        // Load all sub-sections to get their IDs
        var subSections = await _context.SubSections
            .Include(s => s.MainSection)
            .ToListAsync(cancellationToken);

        if (!subSections.Any())
        {
            _logger.LogWarning("No sub-sections found. Run StructureFinaleSeeder first.");
            return;
        }

        var existingPrompts = await _context.SectionPrompts.ToListAsync(cancellationToken);
        var promptsToAdd = new List<SectionPrompt>();
        var updatedCount = 0;

        // Get all prompts from STRUCTURE FINALE
        var promptDefinitions = GetPromptDefinitions();

        foreach (var definition in promptDefinitions)
        {
            var subSection = subSections.FirstOrDefault(s =>
                s.Code == definition.SubSectionCode ||
                s.Code.StartsWith(definition.SubSectionCode.Split(' ')[0]));

            if (subSection == null)
            {
                _logger.LogWarning("SubSection not found for code: {Code}", definition.SubSectionCode);
                continue;
            }

            // Check for French prompt
            var existingFr = existingPrompts.FirstOrDefault(p =>
                p.SubSectionId == subSection.Id &&
                p.Language == "fr" &&
                p.PlanType == BusinessPlanType.BusinessPlan);

            if (existingFr != null)
            {
                existingFr.UpdateContent(
                    definition.SystemPromptFR,
                    definition.UserPromptTemplateFR,
                    definition.Description,
                    definition.VariablesJson);
                updatedCount++;
            }
            else
            {
                promptsToAdd.Add(SectionPrompt.CreateOverridePrompt(
                    subSectionId: subSection.Id,
                    planType: BusinessPlanType.BusinessPlan,
                    language: "fr",
                    name: $"{definition.SubSectionCode} - {definition.Name}",
                    systemPrompt: definition.SystemPromptFR,
                    userPromptTemplate: definition.UserPromptTemplateFR,
                    outputFormat: OutputFormat.Prose,
                    description: definition.Description,
                    variablesJson: definition.VariablesJson
                ));
            }

            // Check for English prompt
            var existingEn = existingPrompts.FirstOrDefault(p =>
                p.SubSectionId == subSection.Id &&
                p.Language == "en" &&
                p.PlanType == BusinessPlanType.BusinessPlan);

            if (existingEn != null)
            {
                existingEn.UpdateContent(
                    definition.SystemPromptEN,
                    definition.UserPromptTemplateEN,
                    definition.Description,
                    definition.VariablesJson);
                updatedCount++;
            }
            else
            {
                promptsToAdd.Add(SectionPrompt.CreateOverridePrompt(
                    subSectionId: subSection.Id,
                    planType: BusinessPlanType.BusinessPlan,
                    language: "en",
                    name: $"{definition.SubSectionCode} - {definition.NameEN}",
                    systemPrompt: definition.SystemPromptEN,
                    userPromptTemplate: definition.UserPromptTemplateEN,
                    outputFormat: OutputFormat.Prose,
                    description: definition.DescriptionEN,
                    variablesJson: definition.VariablesJson
                ));
            }
        }

        if (promptsToAdd.Any())
        {
            _context.SectionPrompts.AddRange(promptsToAdd);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Section Prompts seeding completed. Added: {Added}, Updated: {Updated}",
            promptsToAdd.Count, updatedCount);
    }

    private List<PromptDefinition> GetPromptDefinitions()
    {
        var definitions = new List<PromptDefinition>();

        // SECTION 1: LE PROJET
        definitions.Add(new PromptDefinition
        {
            SubSectionCode = "1.1",
            Name = "Description de l'entreprise",
            NameEN = "Company Description",
            Description = "Genere la section Description de l'entreprise",
            DescriptionEN = "Generates the Company Description section",
            VariablesJson = "[\"ID_1\", \"ID_5\", \"ID_10\", \"ID_11\", \"ID_18\"]",
            SystemPromptFR = @"Role : Expert Consultant en Plan d'Affaires

Objectif specifique : Rediger la section 1.1 DESCRIPTION DE L'ENTREPRISE. Ta mission est de definir l'essence de l'organisation, de legitimer sa structure legale et de situer son activite dans son ecosysteme economique de maniere concise, formelle et rassurante pour des partenaires financiers.

DIRECTIVES DE STYLE :
- Assure-toi que le texte soit fluide, professionnel et qu'il reponde aux attentes des investisseurs (clarte, serieux, conformite).
- Utilise des titres, sous-titres et listes a puces pour maximiser la lisibilite.
- Contrainte stricte : Ne traite pas des motivations personnelles, de la concurrence ou du financement dans cette section precise.",
            UserPromptTemplateFR = @"En te basant exclusivement sur les reponses fournies ci-dessus, redige la section 1.1 DESCRIPTION DE L'ENTREPRISE de maniere detaillee et structuree.

Structure attendue :

**Identite Nominale** : Presentation officielle du nom commercial et de la mission primaire.

**Structure Legale** : Detail du statut juridique choisi et mention de la repartition de la propriete/direction.

**Univers d'Affaires** : Description du secteur d'activite et de la portee geographique ou thematique du projet.

DONNEES SOURCES :
{{questionnaire_responses}}",
            SystemPromptEN = @"Role: Expert Business Plan Consultant

Specific objective: Write section 1.1 COMPANY DESCRIPTION. Your mission is to define the essence of the organization, legitimize its legal structure, and situate its activity in its economic ecosystem in a concise, formal, and reassuring manner for financial partners.

STYLE GUIDELINES:
- Ensure the text is fluid, professional, and meets investor expectations (clarity, seriousness, compliance).
- Use titles, subtitles, and bullet points to maximize readability.
- Strict constraint: Do not address personal motivations, competition, or financing in this specific section.",
            UserPromptTemplateEN = @"Based exclusively on the answers provided above, write section 1.1 COMPANY DESCRIPTION in a detailed and structured manner.

Expected structure:

**Nominal Identity**: Official presentation of the trade name and primary mission.

**Legal Structure**: Detail of the chosen legal status and mention of ownership/management distribution.

**Business Universe**: Description of the business sector and geographic or thematic scope of the project.

SOURCE DATA:
{{questionnaire_responses}}"
        });

        definitions.Add(new PromptDefinition
        {
            SubSectionCode = "1.2",
            Name = "Mission, Vision, Valeurs",
            NameEN = "Mission, Vision, Values",
            Description = "Genere la section Mission, Vision, Valeurs",
            DescriptionEN = "Generates the Mission, Vision, Values section",
            VariablesJson = "[\"ID_2\", \"ID_4\", \"ID_17\", \"ID_18\"]",
            SystemPromptFR = @"Role : Expert Consultant en Plan d'Affaires

Objectif specifique : Rediger la section 1.2 MISSION, VISION, VALEURS. Ta mission est d'articuler la raison d'etre de l'entreprise (Mission), son ambition a long terme (Vision) et les principes fondamentaux qui guident ses actions (Valeurs), afin de demontrer la solidite de la culture d'entreprise et son alignement strategique.

DIRECTIVES DE STYLE :
- Assure-toi que le texte soit fluide, professionnel et qu'il transmette un sentiment de confiance et d'authenticite aux investisseurs.
- Utilise des titres, des sous-titres et des listes a puces pour maximiser la clarte et l'impact visuel.
- Contrainte stricte : Ne pas entrer dans les details techniques des produits ou de la forme juridique.",
            UserPromptTemplateFR = @"En te basant exclusivement sur les reponses fournies ci-dessus, redige la section 1.2 MISSION, VISION, VALEURS de maniere detaillee et structuree.

Structure attendue :

**La Mission** : Definis la raison d'etre de l'entreprise. Quel est l'impact que l'organisation souhaite avoir sur ses clients et son marche au quotidien ?

**La Vision** : Projette l'entreprise dans le futur (3-5 ans). Quelle est l'aspiration ultime et la destination visee par les promoteurs ?

**Les Valeurs** : Identifie et decris 3 a 4 valeurs piliers. Explique comment chaque valeur influence les operations et la relation client.

DONNEES SOURCES :
{{questionnaire_responses}}",
            SystemPromptEN = @"Role: Expert Business Plan Consultant

Specific objective: Write section 1.2 MISSION, VISION, VALUES. Your mission is to articulate the company's reason for being (Mission), its long-term ambition (Vision), and the fundamental principles that guide its actions (Values), to demonstrate the strength of the company culture and its strategic alignment.

STYLE GUIDELINES:
- Ensure the text is fluid, professional, and conveys a sense of confidence and authenticity to investors.
- Use titles, subtitles, and bullet points to maximize clarity and visual impact.
- Strict constraint: Do not go into technical details of products or legal form.",
            UserPromptTemplateEN = @"Based exclusively on the answers provided above, write section 1.2 MISSION, VISION, VALUES in a detailed and structured manner.

Expected structure:

**The Mission**: Define the company's reason for being. What impact does the organization want to have on its customers and market daily?

**The Vision**: Project the company into the future (3-5 years). What is the ultimate aspiration and destination targeted by the promoters?

**The Values**: Identify and describe 3 to 4 pillar values. Explain how each value influences operations and customer relationships.

SOURCE DATA:
{{questionnaire_responses}}"
        });

        definitions.Add(new PromptDefinition
        {
            SubSectionCode = "1.3",
            Name = "Proposition de Valeur",
            NameEN = "Value Proposition",
            Description = "Genere la section Proposition de Valeur",
            DescriptionEN = "Generates the Value Proposition section",
            VariablesJson = "[\"ID_3\", \"ID_4\", \"ID_6\", \"ID_18\"]",
            SystemPromptFR = @"Role : Expert Consultant en Plan d'Affaires

Objectif specifique : Rediger la section 1.3 PROPOSITION DE VALEUR. Ta mission est de demontrer comment l'entreprise repond de maniere unique et superieure aux besoins de son marche cible. Tu dois articuler le lien entre les irritants des clients (douleurs) et les benefices generes par la solution (gains).

DIRECTIVES DE STYLE :
- Utilise un ton persuasif, analytique et oriente vers les resultats.
- Le lecteur doit conclure que la solution est indispensable pour la cible visee.
- Utilise des titres, des sous-titres et des listes a puces pour maximiser la lisibilite.
- Contrainte stricte : Ne pas lister les prix detailles ni le plan marketing.",
            UserPromptTemplateFR = @"En te basant exclusivement sur les reponses fournies ci-dessus, redige la section 1.3 PROPOSITION DE VALEUR de maniere detaillee et structuree.

Structure attendue :

**Analyse du Probleme** : Decris avec empathie et precision la situation frustrante vecue par le client type avant l'intervention de l'entreprise.

**La Solution Strategique** : Presente comment l'offre elimine ces points de friction de maniere efficace.

**L'Avantage Distinctif** : Detaille l'element unique (le 'petit plus') qui fait que l'on choisit cette entreprise plutot que les alternatives actuelles.

**Benefices Clients** : Liste les 3 principaux gains pour le client (ex: gain de temps, economie d'argent, tranquillite d'esprit, image de marque).

DONNEES SOURCES :
{{questionnaire_responses}}",
            SystemPromptEN = @"Role: Expert Business Plan Consultant

Specific objective: Write section 1.3 VALUE PROPOSITION. Your mission is to demonstrate how the company uniquely and superiorly meets the needs of its target market. You must articulate the link between customer pain points (pains) and the benefits generated by the solution (gains).

STYLE GUIDELINES:
- Use a persuasive, analytical, and results-oriented tone.
- The reader should conclude that the solution is essential for the targeted audience.
- Use titles, subtitles, and bullet points to maximize readability.
- Strict constraint: Do not list detailed prices or the marketing plan.",
            UserPromptTemplateEN = @"Based exclusively on the answers provided above, write section 1.3 VALUE PROPOSITION in a detailed and structured manner.

Expected structure:

**Problem Analysis**: Describe with empathy and precision the frustrating situation experienced by the typical client before the company's intervention.

**The Strategic Solution**: Present how the offer eliminates these friction points effectively.

**The Distinctive Advantage**: Detail the unique element (the 'little extra') that makes people choose this company over current alternatives.

**Customer Benefits**: List the 3 main gains for the customer (ex: time savings, money savings, peace of mind, brand image).

SOURCE DATA:
{{questionnaire_responses}}"
        });

        // SECTION 2: PROMOTEURS
        definitions.Add(new PromptDefinition
        {
            SubSectionCode = "2.1",
            Name = "Structure Juridique",
            NameEN = "Legal Structure",
            Description = "Genere la section Structure Juridique",
            DescriptionEN = "Generates the Legal Structure section",
            VariablesJson = "[\"ID_1\", \"ID_10\", \"ID_11\", \"ID_18\"]",
            SystemPromptFR = @"Role : Expert Consultant en Plan d'Affaires

Objectif specifique : Rediger la section 2.1 STRUCTURE JURIDIQUE. Ta mission est d'officialiser le cadre legal de l'entreprise. Tu dois expliquer non seulement 'quelle' est la forme juridique, mais surtout 'pourquoi' elle est la plus adaptee au projet, a la protection des actifs et a la vision de croissance des promoteurs.

DIRECTIVES DE STYLE :
- Utilise un ton institutionnel, rigoureux et precis. Le langage doit refleter une maitrise des concepts de droit des affaires.
- Utilise des titres et des sous-titres. Si la structure est une societe par actions avec plusieurs actionnaires, utilise un petit tableau pour la repartition du capital.
- Contrainte stricte : Ne traite pas ici des operations quotidiennes ou des produits.",
            UserPromptTemplateFR = @"En te basant exclusivement sur les reponses fournies ci-dessus, redige la section 2.1 STRUCTURE JURIDIQUE de maniere detaillee et structuree.

Structure attendue :

**Denomination et Immatriculation** : Confirme le nom legal et, si disponible, les informations d'enregistrement officiel.

**Forme Juridique Choisie** : Presente clairement le statut (ex: Societe par actions, Entreprise individuelle, etc.).

**Justification du Statut** : Developpe les arguments strategiques lies a ce choix (protection de la responsabilite, avantages fiscaux, facilite de transfert, ou simplicite administrative).

**Actionnariat et Gouvernance** : Detaille qui detient l'entreprise et dans quelles proportions. Mentionne brievement le role des administrateurs principaux.

DONNEES SOURCES :
{{questionnaire_responses}}",
            SystemPromptEN = @"Role: Expert Business Plan Consultant

Specific objective: Write section 2.1 LEGAL STRUCTURE. Your mission is to formalize the legal framework of the company. You must explain not only 'what' the legal form is, but especially 'why' it is best suited to the project, asset protection, and the promoters' growth vision.

STYLE GUIDELINES:
- Use an institutional, rigorous, and precise tone. The language should reflect mastery of business law concepts.
- Use titles and subtitles. If the structure is a corporation with multiple shareholders, use a small table for capital distribution.
- Strict constraint: Do not address daily operations or products here.",
            UserPromptTemplateEN = @"Based exclusively on the answers provided above, write section 2.1 LEGAL STRUCTURE in a detailed and structured manner.

Expected structure:

**Name and Registration**: Confirm the legal name and, if available, official registration information.

**Chosen Legal Form**: Clearly present the status (ex: Corporation, Sole proprietorship, etc.).

**Status Justification**: Develop strategic arguments related to this choice (liability protection, tax advantages, ease of transfer, or administrative simplicity).

**Shareholding and Governance**: Detail who owns the company and in what proportions. Briefly mention the role of the main directors.

SOURCE DATA:
{{questionnaire_responses}}"
        });

        definitions.Add(new PromptDefinition
        {
            SubSectionCode = "2.2",
            Name = "Profil des Promoteurs",
            NameEN = "Promoter Profile",
            Description = "Genere la section Profil des Promoteurs",
            DescriptionEN = "Generates the Promoter Profile section",
            VariablesJson = "[\"ID_2\", \"ID_10\", \"ID_13\", \"ID_18\"]",
            SystemPromptFR = @"Role : Expert Consultant en Plan d'Affaires

Objectif specifique : Rediger la section 2.2 PROFIL DU (DES) PROMOTEUR(S). Ta mission est de mettre en valeur le capital humain du projet. Tu dois demontrer que le ou les promoteurs possedent l'expertise, la determination et la complementarite necessaires pour mener l'entreprise au succes.

DIRECTIVES DE STYLE :
- Utilise un ton valorisant, professionnel et convaincant. Evite la simple enumeration de taches ; parle de 'leadership' et de 'vision'.
- Utilise des paragraphes distincts par personne ou des listes a puces pour les competences cles afin de rendre la lecture fluide.
- Contrainte stricte : Ne traite pas ici de la structure juridique globale ni du plan de recrutement futur.",
            UserPromptTemplateFR = @"En te basant exclusivement sur les reponses fournies ci-dessus, redige la section 2.2 PROFIL DU (DES) PROMOTEUR(S) de maniere detaillee et structuree.

Structure attendue :

**Presentation Individuelle** : Pour chaque promoteur, redige un profil mettant en lumiere son expertise metier, son parcours academique et ses realisations marquantes.

**Complementarite et Roles** : Explique comment les forces de chacun se combinent (ex: l'un a la gestion, l'autre aux operations). Si le promoteur est seul, souligne sa polyvalence.

**Lien avec le Projet** : Justifie pourquoi ce promoteur est la personne ideale pour porter ce projet specifique (alignement passion/competence).

**Engagement** : Mentionne l'investissement (temps et argent) qui temoigne de la foi du promoteur dans la reussite de l'entreprise.

DONNEES SOURCES :
{{questionnaire_responses}}",
            SystemPromptEN = @"Role: Expert Business Plan Consultant

Specific objective: Write section 2.2 PROMOTER PROFILE. Your mission is to highlight the human capital of the project. You must demonstrate that the promoter(s) possess the expertise, determination, and complementarity necessary to lead the company to success.

STYLE GUIDELINES:
- Use a valuing, professional, and convincing tone. Avoid simple task enumeration; speak of 'leadership' and 'vision'.
- Use separate paragraphs per person or bullet points for key skills to make reading fluid.
- Strict constraint: Do not address overall legal structure or future recruitment plan here.",
            UserPromptTemplateEN = @"Based exclusively on the answers provided above, write section 2.2 PROMOTER PROFILE in a detailed and structured manner.

Expected structure:

**Individual Presentation**: For each promoter, write a profile highlighting their professional expertise, academic background, and notable achievements.

**Complementarity and Roles**: Explain how each person's strengths combine (ex: one in management, another in operations). If the promoter is alone, emphasize their versatility.

**Link to the Project**: Justify why this promoter is the ideal person to carry this specific project (passion/skill alignment).

**Commitment**: Mention the investment (time and money) that demonstrates the promoter's faith in the company's success.

SOURCE DATA:
{{questionnaire_responses}}"
        });

        // SECTION 3: ETUDE DE MARCHE
        definitions.Add(new PromptDefinition
        {
            SubSectionCode = "3.1",
            Name = "Analyse de Marche",
            NameEN = "Market Analysis",
            Description = "Genere la section Analyse de Marche",
            DescriptionEN = "Generates the Market Analysis section",
            VariablesJson = "[\"ID_5\", \"ID_6\", \"ID_7\", \"ID_18\"]",
            SystemPromptFR = @"Role : Expert Consultant en Plan d'Affaires

Objectif specifique : Rediger la section 3.1 ANALYSE DE MARCHE. Ta mission est de brosser un portrait rigoureux de l'industrie. Tu dois demontrer que le marche est en croissance, identifier les grandes tendances de consommation qui favorisent le projet et valider que la demande est reelle et accessible.

DIRECTIVES DE STYLE :
- Utilise un ton objectif, factuel et expert. Evite les generalites ; utilise des termes propres au secteur d'activite mentionne.
- Utilise des titres, des sous-titres et des listes a puces pour organiser les idees de maniere logique.
- Contrainte stricte : Ne detaille pas ici les strategies marketing de l'entreprise ni les prix.",
            UserPromptTemplateFR = @"En te basant exclusivement sur les reponses fournies ci-dessus, redige la section 3.1 ANALYSE DE MARCHE de maniere structuree et analytique.

Structure attendue :

**Etat de l'Industrie** : Decris le secteur. Est-il en croissance ? Quelles sont les nouvelles habitudes des consommateurs qui profitent a ce type de projet ?

**Segmentation du Marche** : A partir du profil client, explique quelle part du marche est visee (ex: marche local, clientele haut de gamme, entreprises B2B).

**Forces et Tendances** : Identifie 2 ou 3 facteurs externes qui favorisent le succes de l'entreprise (ex: manque de temps des clients, virage ecologique, numerisation).

**Synthese de l'Opportunite** : Conclus sur la viabilite du projet en faisant le pont entre la taille de la cible et l'offre proposee.

DONNEES SOURCES :
{{questionnaire_responses}}",
            SystemPromptEN = @"Role: Expert Business Plan Consultant

Specific objective: Write section 3.1 MARKET ANALYSIS. Your mission is to paint a rigorous portrait of the industry. You must demonstrate that the market is growing, identify major consumption trends that favor the project, and validate that demand is real and accessible.

STYLE GUIDELINES:
- Use an objective, factual, and expert tone. Avoid generalities; use terms specific to the mentioned business sector.
- Use titles, subtitles, and bullet points to organize ideas logically.
- Strict constraint: Do not detail the company's marketing strategies or prices here.",
            UserPromptTemplateEN = @"Based exclusively on the answers provided above, write section 3.1 MARKET ANALYSIS in a structured and analytical manner.

Expected structure:

**Industry State**: Describe the sector. Is it growing? What are the new consumer habits that benefit this type of project?

**Market Segmentation**: From the client profile, explain what market share is targeted (ex: local market, high-end clientele, B2B businesses).

**Forces and Trends**: Identify 2 or 3 external factors that favor the company's success (ex: client time shortage, ecological shift, digitization).

**Opportunity Synthesis**: Conclude on the project's viability by bridging the target size and the proposed offer.

SOURCE DATA:
{{questionnaire_responses}}"
        });

        definitions.Add(new PromptDefinition
        {
            SubSectionCode = "3.4",
            Name = "Analyse de la Concurrence",
            NameEN = "Competitive Analysis",
            Description = "Genere la section Analyse de la Concurrence",
            DescriptionEN = "Generates the Competitive Analysis section",
            VariablesJson = "[\"ID_4\", \"ID_7\", \"ID_8\", \"ID_18\"]",
            SystemPromptFR = @"Role : Expert Consultant en Plan d'Affaires

Objectif specifique : Rediger la section 3.7 ANALYSE DE LA CONCURRENCE. Ta mission est de cartographier le paysage concurrentiel. Tu dois demontrer une comprehension fine des forces et faiblesses des adversaires (directs et indirects) afin de valider le positionnement strategique et la superiorite de l'offre proposee.

DIRECTIVES DE STYLE :
- Utilise un ton objectif, analytique et combatif (mais respectueux). Ne denigre pas gratuitement la concurrence, utilise des faits.
- Presente les concurrents sous forme de liste structuree ou d'un tableau comparatif pour une lecture rapide.
- Contrainte stricte : Ne traite pas ici du plan marketing pour battre ces concurrents. Concentre-toi sur l'etat des lieux de la competition actuelle.",
            UserPromptTemplateFR = @"En te basant exclusivement sur les reponses fournies ci-dessus, redige la section 3.7 ANALYSE DE LA CONCURRENCE de maniere structuree et strategique.

Structure attendue :

**Portrait de la concurrence directe** : Identifie les entreprises qui offrent une solution identique. Analyse leurs points forts et, surtout, leurs lacunes (ex: delais trop longs, manque de service mobile, service client impersonnel).

**Analyse de la concurrence indirecte** : Identifie les alternatives (ex: faire le travail soi-meme, grandes chaines generiques).

**Matrice comparative (Positionnement)** : Compare l'entreprise face aux leaders du marche sur des criteres cles : Prix, Qualite, Rapidite, et Service Client.

**Strategie de differenciation** : Conclus sur la 'breche' dans laquelle l'entreprise s'insere pour gagner des clients (ton avantage injuste).

DONNEES SOURCES :
{{questionnaire_responses}}",
            SystemPromptEN = @"Role: Expert Business Plan Consultant

Specific objective: Write section 3.7 COMPETITIVE ANALYSIS. Your mission is to map the competitive landscape. You must demonstrate a fine understanding of competitors' strengths and weaknesses (direct and indirect) to validate the strategic positioning and superiority of the proposed offer.

STYLE GUIDELINES:
- Use an objective, analytical, and combative (but respectful) tone. Don't gratuitously criticize competition, use facts.
- Present competitors as a structured list or comparative table for quick reading.
- Strict constraint: Do not address the marketing plan to beat these competitors here. Focus on the current state of competition.",
            UserPromptTemplateEN = @"Based exclusively on the answers provided above, write section 3.7 COMPETITIVE ANALYSIS in a structured and strategic manner.

Expected structure:

**Direct Competition Portrait**: Identify companies offering an identical solution. Analyze their strengths and especially their gaps (ex: too long delays, lack of mobile service, impersonal customer service).

**Indirect Competition Analysis**: Identify alternatives (ex: DIY, generic big chains).

**Comparative Matrix (Positioning)**: Compare the company against market leaders on key criteria: Price, Quality, Speed, and Customer Service.

**Differentiation Strategy**: Conclude on the 'gap' in which the company inserts itself to win customers (your unfair advantage).

SOURCE DATA:
{{questionnaire_responses}}"
        });

        // SECTION 4: PLAN MARKETING & VENTES
        definitions.Add(new PromptDefinition
        {
            SubSectionCode = "4.1",
            Name = "Strategie Marketing",
            NameEN = "Marketing Strategy",
            Description = "Genere la section Strategie Marketing",
            DescriptionEN = "Generates the Marketing Strategy section",
            VariablesJson = "[\"ID_4\", \"ID_6\", \"ID_9\", \"ID_17\", \"ID_18\"]",
            SystemPromptFR = @"Role : Expert Consultant en Plan d'Affaires

Objectif specifique : Rediger la section 4.1 STRATEGIE MARKETING. Ta mission est de definir le plan d'attaque commercial de l'entreprise. Tu dois demontrer que l'entrepreneur a identifie les canaux les plus rentables pour rejoindre sa cible et qu'il possede un message clair pour transformer des prospects en clients fideles.

DIRECTIVES DE STYLE :
- Utilise un ton creatif, strategique et oriente vers les resultats. Evite le jargon marketing trop complexe ; reste focalise sur l'efficacite.
- Utilise des titres et des listes a puces pour detailler chaque canal marketing.
- Contrainte stricte : Ne traite pas ici des prix detailles ni du processus de vente operationnel. Concentre-toi sur la generation de demande.",
            UserPromptTemplateFR = @"En te basant exclusivement sur les reponses fournies ci-dessus, redige la section 4.1 STRATEGIE MARKETING de maniere structuree et dynamique.

Structure attendue :

**Axe de Communication (Branding)** : Definis le ton et le message principal. Quelle emotion ou quel benefice rationnel est mis en avant (ex: Prestige, Gain de temps, Securite) ?

**Strategie de Visibilite (Canaux)** : Detaille les deux ou trois canaux prioritaires. Explique pourquoi ces choix sont les plus pertinents pour le profil client (ex: Publicite Facebook ciblee pour les residents locaux, LinkedIn pour le B2B).

**Tactiques de Conversion** : Explique comment l'entreprise compte inciter a l'action (ex: offre de lancement, programme de parrainage, marketing de contenu).

**Calendrier Marketing Initial** : Esquisse les actions des 90 premiers jours pour generer les premieres ventes.

DONNEES SOURCES :
{{questionnaire_responses}}",
            SystemPromptEN = @"Role: Expert Business Plan Consultant

Specific objective: Write section 4.1 MARKETING STRATEGY. Your mission is to define the company's commercial attack plan. You must demonstrate that the entrepreneur has identified the most profitable channels to reach their target and has a clear message to transform prospects into loyal customers.

STYLE GUIDELINES:
- Use a creative, strategic, and results-oriented tone. Avoid overly complex marketing jargon; stay focused on effectiveness.
- Use titles and bullet points to detail each marketing channel.
- Strict constraint: Do not address detailed prices or operational sales process here. Focus on demand generation.",
            UserPromptTemplateEN = @"Based exclusively on the answers provided above, write section 4.1 MARKETING STRATEGY in a structured and dynamic manner.

Expected structure:

**Communication Axis (Branding)**: Define the tone and main message. What emotion or rational benefit is highlighted (ex: Prestige, Time savings, Security)?

**Visibility Strategy (Channels)**: Detail the two or three priority channels. Explain why these choices are most relevant for the client profile (ex: Targeted Facebook advertising for local residents, LinkedIn for B2B).

**Conversion Tactics**: Explain how the company plans to incentivize action (ex: launch offer, referral program, content marketing).

**Initial Marketing Calendar**: Outline actions for the first 90 days to generate first sales.

SOURCE DATA:
{{questionnaire_responses}}"
        });

        // SECTION 0: SOMMAIRE EXECUTIF (Generated Last)
        definitions.Add(new PromptDefinition
        {
            SubSectionCode = "0.1",
            Name = "Apercu de l'entreprise",
            NameEN = "Company Overview",
            Description = "Genere l'apercu de l'entreprise pour le sommaire executif",
            DescriptionEN = "Generates the company overview for executive summary",
            VariablesJson = "[\"section_1_1\", \"section_1_2\", \"section_2_1\"]",
            SystemPromptFR = @"Role : Expert en Communication Strategique et Redacteur de 'Pitch Deck' pour investisseurs.

Objectif : Rediger le SOMMAIRE EXECUTIF en synthetisant les sections deja completees du plan d'affaires. Ce document doit etre la vitrine du projet : percutant, professionnel et coherent.

DIRECTIVES DE STYLE :
- Densite : Ne pas depasser un paragraphe par section.
- Impact : Utilise des verbes d'action et un ton determine.
- Fluidite : Assure des transitions fluides entre les sections pour que le document se lise comme une histoire continue.
- Format : Presentation propre avec titres en gras et listes a puces pour les chiffres cles.",
            UserPromptTemplateFR = @"En te basant sur les sections deja redigees du plan d'affaires, synthetise la section APERCU DE L'ENTREPRISE du sommaire executif.

Resume en 2 a 3 phrases la nature de l'entreprise, son secteur d'activite, et les motivations derriere sa creation. Mentionne brievement son emplacement et son stade de developpement.

CONTENU SOURCE :
{{generated_sections}}",
            SystemPromptEN = @"Role: Strategic Communication Expert and 'Pitch Deck' Writer for investors.

Objective: Write the EXECUTIVE SUMMARY by synthesizing the already completed sections of the business plan. This document must be the project's showcase: impactful, professional, and coherent.

STYLE GUIDELINES:
- Density: Do not exceed one paragraph per section.
- Impact: Use action verbs and a determined tone.
- Fluidity: Ensure smooth transitions between sections so the document reads like a continuous story.
- Format: Clean presentation with bold titles and bullet points for key figures.",
            UserPromptTemplateEN = @"Based on the already written sections of the business plan, synthesize the COMPANY OVERVIEW section of the executive summary.

Summarize in 2 to 3 sentences the nature of the company, its business sector, and the motivations behind its creation. Briefly mention its location and stage of development.

SOURCE CONTENT:
{{generated_sections}}"
        });

        return definitions;
    }

    private class PromptDefinition
    {
        public string SubSectionCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string NameEN { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string DescriptionEN { get; set; } = null!;
        public string? VariablesJson { get; set; }
        public string SystemPromptFR { get; set; } = null!;
        public string UserPromptTemplateFR { get; set; } = null!;
        public string SystemPromptEN { get; set; } = null!;
        public string UserPromptTemplateEN { get; set; } = null!;
    }
}
