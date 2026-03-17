using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Domain.Entities.BusinessPlan;
using Sqordia.Domain.Enums;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Persistence.Seeds;

/// <summary>
/// Seeds the 22 STRUCTURE FINALE questionnaire questions with:
/// - French/English question text
/// - Expert advice (Conseil d'expert)
/// - AI Coach prompts for generating suggestions
/// Based on "STRUCTURE FINALE- Questions & prompt.xlsx"
/// </summary>
public class StructureFinaleQuestionsSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StructureFinaleQuestionsSeeder> _logger;

    public StructureFinaleQuestionsSeeder(ApplicationDbContext context, ILogger<StructureFinaleQuestionsSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedOrUpdateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting STRUCTURE FINALE Questions seeding/update...");

        var existingQuestions = await _context.QuestionTemplates.ToListAsync(cancellationToken);
        var newQuestions = GetDetailedQuestionTemplates();

        foreach (var newQuestion in newQuestions)
        {
            var existing = existingQuestions.FirstOrDefault(q => q.QuestionNumber == newQuestion.QuestionNumber);
            if (existing != null)
            {
                // Update existing question with new data
                existing.UpdateQuestionText(newQuestion.QuestionTextFR, newQuestion.QuestionTextEN);
                existing.SetHelpText(newQuestion.HelpTextFR, newQuestion.HelpTextEN);
                existing.SetCoachPrompts(newQuestion.CoachPromptFR, newQuestion.CoachPromptEN);
                existing.SetExpertAdvice(newQuestion.ExpertAdviceFR, newQuestion.ExpertAdviceEN);
                existing.UpdateQuestionType(newQuestion.QuestionType);
                existing.SetOptions(newQuestion.OptionsFR, newQuestion.OptionsEN);
                _logger.LogDebug("Updated question {Number}", newQuestion.QuestionNumber);
            }
            else
            {
                // Add new question
                _context.QuestionTemplates.Add(newQuestion);
                _logger.LogDebug("Added new question {Number}", newQuestion.QuestionNumber);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("STRUCTURE FINALE Questions seeding/update completed. Total: {Count} questions.", newQuestions.Count);
    }

    private List<QuestionTemplate> GetDetailedQuestionTemplates()
    {
        var questions = new List<QuestionTemplate>();

        // Question 1: Company Name
        questions.Add(CreateDetailedQuestion(
            questionNumber: 1,
            stepNumber: 1,
            questionType: QuestionType.ShortText,
            questionTextFR: "Quel est le nom de ton entreprise et comment résumerais-tu ton activité en une phrase simple et percutante ?",
            questionTextEN: "What is the name of your company and how would you summarize your activity in one simple and impactful sentence?",
            helpTextFR: "Identifie le branding et le modèle (mobile).",
            helpTextEN: "Identifies the branding and model (mobile).",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Imagine que tu montes dans un ascenseur et que tu as exactement 10 secondes pour expliquer ce que tu fais à quelqu'un.

Pour réussir ton résumé, utilise cette formule simple :
""Mon entreprise [Nom] aide [Ta Cible] à [Résultat obtenu] grâce à [Ta méthode unique].""

Astuce : Évite les mots trop vagues comme ""services divers"" ou ""solutions de qualité"". Sois concret ! Si on ne comprend pas ton métier en lisant cette phrase, c'est qu'elle est encore trop complexe. Garde ça simple et efficace.",
            expertAdviceEN: @"Expert advice for a good answer:
Imagine you're in an elevator and have exactly 10 seconds to explain what you do to someone.

To succeed with your summary, use this simple formula:
""My company [Name] helps [Your Target] to [Result achieved] through [Your unique method].""

Tip: Avoid vague words like ""various services"" or ""quality solutions"". Be concrete! If someone can't understand your business by reading this sentence, it's still too complex. Keep it simple and effective.",
            coachPromptFR: null,
            coachPromptEN: null
        ));

        // Question 2: Mission & Origin Story
        questions.Add(CreateDetailedQuestion(
            questionNumber: 2,
            stepNumber: 1,
            questionType: QuestionType.LongText,
            questionTextFR: "Quelle est l'histoire à l'origine de ton projet et quelles sont les motivations profondes qui te poussent à le lancer aujourd'hui ? (Explique-nous ta mission et ce qui t'anime réellement).",
            questionTextEN: "What is the story behind your project and what are the deep motivations pushing you to launch it today? (Explain your mission and what truly drives you).",
            helpTextFR: "Reformuler la Mission, Rédige la Vision et les Valeurs.",
            helpTextEN: "Reformulate the Mission, Write the Vision and Values.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Ne cherche pas à écrire un texte ""corporate"" ou formel. Parle avec ton coeur.

Pour t'aider, réfléchis à ces trois points :

Le déclic : Y a-t-il eu un moment précis, une frustration personnelle ou une observation qui t'a fait dire : ""Il faut que quelqu'un règle ce problème"" ?

Ta mission : Au-delà de l'argent, quel impact veux-tu avoir sur la vie de tes clients ou sur ta communauté ?

Le moteur : Dans les moments difficiles, qu'est-ce qui te donnera l'énergie de continuer ?

Astuce : Les investisseurs et les banquiers n'investissent pas seulement dans une idée, ils investissent dans toi. Ton histoire est la preuve de ta détermination et de ton expertise.",
            expertAdviceEN: @"Expert advice for a good answer:
Don't try to write a ""corporate"" or formal text. Speak from the heart.

To help you, think about these three points:

The trigger: Was there a specific moment, a personal frustration or observation that made you say: ""Someone needs to solve this problem""?

Your mission: Beyond money, what impact do you want to have on your clients' lives or your community?

The drive: In difficult times, what will give you the energy to continue?

Tip: Investors and bankers don't just invest in an idea, they invest in you. Your story is proof of your determination and expertise.",
            coachPromptFR: null,
            coachPromptEN: null
        ));

        // Question 3: Customer Problem
        questions.Add(CreateDetailedQuestion(
            questionNumber: 3,
            stepNumber: 1,
            questionType: QuestionType.LongText,
            questionTextFR: "Quel est le problème concret que tu règles pour tes clients ? Raconte-nous la situation difficile ou frustrante qu'ils vivent avant de te découvrir.",
            questionTextEN: "What is the concrete problem you solve for your clients? Tell us about the difficult or frustrating situation they experience before discovering you.",
            helpTextFR: "Argumente la pertinence du projet.",
            helpTextEN: "Argue the project's relevance.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Pour bien répondre, ne fais pas juste une liste de tes services. Imagine que tu as 30 secondes pour convaincre un client qui hésite entre toi et ton plus gros concurrent.

Pose-toi ces deux questions :

Le bénéfice concret : Une fois que tu as terminé ton travail, comment se sent ton client ? (Ex: Est-il fier ? Est-il soulagé ? A-t-il gagné du temps ?)

Le ""Petit Plus"" : Quelle est la chose que tu fais, ou la manière dont tu le fais, que personne d'autre ne propose dans ta région ?

Astuce : Si ta seule différence est ""je suis moins cher"", cherche encore ! La qualité, la rapidité, l'écologie ou une spécialité rare sont de bien meilleures fondations pour ton plan d'affaires.",
            expertAdviceEN: @"Expert advice for a good answer:
To answer well, don't just make a list of your services. Imagine you have 30 seconds to convince a client who is hesitating between you and your biggest competitor.

Ask yourself these two questions:

The concrete benefit: Once you've finished your work, how does your client feel? (Ex: Are they proud? Relieved? Did they save time?)

The ""Little Extra"": What is the thing you do, or the way you do it, that no one else offers in your area?

Tip: If your only difference is ""I'm cheaper"", keep looking! Quality, speed, ecology or a rare specialty are much better foundations for your business plan.",
            coachPromptFR: @"Tu es un expert en stratégie commerciale. Ton rôle est d'aider l'entrepreneur à verbaliser le problème majeur de ses clients en utilisant les informations qu'il a déjà fournies (secteur, équipe, nom de l'entreprise).

Étape 1 : Analyse le secteur d'activité mentionné précédemment.
Étape 2 : Propose deux scénarios de 'douleur client' :

L'Option A (Le manque de temps/Logistique) : Focus sur la vie effrénée du client et la difficulté d'accéder aux solutions actuelles.

L'Option B (La frustration émotionnelle/Image) : Focus sur l'inconfort, la gêne ou le sentiment de négligence que ressent le client.

Format de réponse :
'Identifier le problème est la clé pour prouver que ton entreprise est nécessaire. Voici deux façons d'expliquer ce que vivent tes clients :'",
            coachPromptEN: @"You are an expert in commercial strategy. Your role is to help the entrepreneur verbalize their clients' major problem using the information they have already provided (sector, team, company name).

Step 1: Analyze the previously mentioned business sector.
Step 2: Propose two 'customer pain' scenarios:

Option A (Lack of time/Logistics): Focus on the client's hectic life and difficulty accessing current solutions.

Option B (Emotional frustration/Image): Focus on the discomfort, embarrassment or feeling of neglect that the client experiences.

Response format:
'Identifying the problem is key to proving that your business is necessary. Here are two ways to explain what your clients experience:'"
        ));

        // Question 4: Solution & Differentiation
        questions.Add(CreateDetailedQuestion(
            questionNumber: 4,
            stepNumber: 1,
            questionType: QuestionType.LongText,
            questionTextFR: "Quelle est ta solution pour régler le problème de tes clients et quel est l'élément unique qui te différencie de la concurrence ? (C'est ton \"petit plus\" qui fait que l'on te choisit toi plutôt qu'un autre).",
            questionTextEN: "What is your solution to solve your clients' problem and what is the unique element that differentiates you from the competition? (It's your \"little extra\" that makes people choose you over others).",
            helpTextFR: "Décrit les Produits/Services.",
            helpTextEN: "Describes Products/Services.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Pour répondre, ne te contente pas de dire que tu es ""meilleur"" ou ""moins cher"". Cherche ton avantage injuste.

Pose-toi ces questions :

Le ""Petit Plus"" : Si ton client a le choix entre toi et une grosse entreprise installée depuis 10 ans, quelle est la chose précise qui le fera pencher pour toi ? (Ta flexibilité ? Ton service ultra-personnalisé ? Ta technologie écologique ? Ta spécialisation sur un modèle de voiture précis ?)

La preuve de ta solution : Comment prouves-tu que ta méthode fonctionne mieux que ce qui existe déjà ?

Astuce : Ta différence, c'est ta signature. Si un concurrent peut copier ton idée en une heure, c'est que ta différenciation n'est pas encore assez forte. Creuse ce qui te rend irremplaçable !",
            expertAdviceEN: @"Expert advice for a good answer:
To answer, don't just say you're ""better"" or ""cheaper"". Look for your unfair advantage.

Ask yourself these questions:

The ""Little Extra"": If your client has a choice between you and a big company that's been around for 10 years, what specific thing will make them lean towards you? (Your flexibility? Your ultra-personalized service? Your ecological technology? Your specialization in a specific car model?)

Proof of your solution: How do you prove that your method works better than what already exists?

Tip: Your difference is your signature. If a competitor can copy your idea in an hour, your differentiation isn't strong enough yet. Dig into what makes you irreplaceable!",
            coachPromptFR: @"Tu es un expert en accompagnement de startups. Ton rôle est d'aider l'entrepreneur à formuler sa Solution et sa Différenciation.

Étape 1 : Analyse les informations précédentes (Nom de l'entreprise, secteur, histoire et problème identifié).
Étape 2 : Propose deux options de réponses rédigées de manière professionnelle mais authentique.

L'Option A (Approche Pragmatique) : Focus sur l'efficacité, la rapidité ou le prix.

L'Option B (Approche Premium/Innovante) : Focus sur l'expérience client, la technologie ou l'aspect écologique.

Format de réponse :
'Voici deux suggestions pour t'aider à répondre. Choisis celle qui te ressemble le plus ou mélange les deux !'

Structure de chaque option :
Ma solution : [Description concise]
Ma différence : [L'élément unique]",
            coachPromptEN: @"You are a startup coaching expert. Your role is to help the entrepreneur formulate their Solution and Differentiation.

Step 1: Analyze previous information (Company name, sector, story and identified problem).
Step 2: Propose two answer options written professionally but authentically.

Option A (Pragmatic Approach): Focus on efficiency, speed or price.

Option B (Premium/Innovative Approach): Focus on customer experience, technology or ecological aspects.

Response format:
'Here are two suggestions to help you answer. Choose the one that suits you best or mix both!'

Structure of each option:
My solution: [Concise description]
My difference: [The unique element]"
        ));

        // Question 5: Business Sector
        questions.Add(CreateDetailedQuestion(
            questionNumber: 5,
            stepNumber: 1,
            questionType: QuestionType.SingleChoice,
            questionTextFR: "Quel est ton secteur d'activité précis et dans quel univers ton entreprise évolue-t-elle ? (Ex: Services de proximité, Commerce de détail, Technologie de la santé, etc.)",
            questionTextEN: "What is your precise business sector and in what universe does your company operate? (Ex: Local services, Retail, Health technology, etc.)",
            helpTextFR: "Génère l'Analyse PESTEL.",
            helpTextEN: "Generates the PESTEL Analysis.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Pour bien répondre, ne reste pas trop général. Si tu lances un service de nettoyage de voiture, ton secteur n'est pas juste ""l'automobile"", mais plutôt le ""service d'entretien esthétique automobile"".

Pourquoi c'est important ? Parce que chaque secteur a ses propres règles, ses propres lois et ses propres modes. En étant précis, tu nous aides à identifier les opportunités et les risques spécifiques à ton métier (ex: l'évolution des lois sur l'environnement ou les nouvelles technologies de nettoyage).

Astuce : Si tu hésites, demande-toi dans quelle section de l'annuaire ou sur quel type de site spécialisé on trouverait ton entreprise.",
            expertAdviceEN: @"Expert advice for a good answer:
To answer well, don't stay too general. If you're launching a car cleaning service, your sector isn't just ""automotive"", but rather ""automotive aesthetic maintenance service"".

Why is this important? Because each sector has its own rules, laws and trends. By being precise, you help us identify the opportunities and risks specific to your business (ex: changes in environmental laws or new cleaning technologies).

Tip: If you're unsure, ask yourself in which directory section or on what type of specialized website your company would be found.",
            coachPromptFR: @"Tu es un expert en stratégie d'entreprise. Ta mission est d'aider l'entrepreneur à définir son secteur d'activité.

ÉTAPE 1 : ANALYSE DU CONTEXTE
Lis attentivement toutes les réponses précédentes fournies par l'utilisateur (nom de l'entreprise, mission, problème réglé et solution). Identifie les mots-clés dominants et la nature réelle de son métier.

ÉTAPE 2 : GÉNÉRATION DE RÉPONSES
En fonction de cette analyse, propose deux options de réponse pour la question suivante :
Quel est ton secteur d'activité précis et dans quel univers ton entreprise évolue-t-elle ?

Structure des options :

Option 1 (Approche Marché Global) : Définis le secteur de manière large et rassurante pour un banquier (ex: Services aux entreprises, Commerce de détail).

Option 2 (Approche Marché de Niche) : Définis le secteur de manière plus pointue en soulignant la spécialité du projet (ex: Économie circulaire, Service mobile à la demande).

Consigne de style :
Rédige chaque option comme si l'entrepreneur parlait lui-même. Termine par : 'D'après tes réponses précédentes, voici les deux univers qui te correspondent le mieux. Lequel choisis-tu ?'",
            coachPromptEN: @"You are a business strategy expert. Your mission is to help the entrepreneur define their business sector.

STEP 1: CONTEXT ANALYSIS
Carefully read all previous answers provided by the user (company name, mission, problem solved and solution). Identify the dominant keywords and the true nature of their business.

STEP 2: RESPONSE GENERATION
Based on this analysis, propose two answer options for the following question:
What is your precise business sector and in what universe does your company operate?

Option structure:

Option 1 (Global Market Approach): Define the sector broadly and reassuringly for a banker (ex: Business services, Retail).

Option 2 (Niche Market Approach): Define the sector more precisely by highlighting the project's specialty (ex: Circular economy, On-demand mobile service).

Style instruction:
Write each option as if the entrepreneur were speaking themselves. End with: 'Based on your previous answers, here are the two universes that best suit you. Which one do you choose?'"
        ));

        // Question 6: Customer Profile
        questions.Add(CreateDetailedQuestion(
            questionNumber: 6,
            stepNumber: 3,
            questionType: QuestionType.LongText,
            questionTextFR: "Qui est la personne la plus susceptible de payer pour tes services dès demain ? Précise son profil : son âge, sa localisation, son métier et surtout, ce qu'elle valorise le plus.",
            questionTextEN: "Who is the person most likely to pay for your services tomorrow? Specify their profile: their age, location, job and especially what they value most.",
            helpTextFR: "Génère le portrait Persona.",
            helpTextEN: "Generates the Persona portrait.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Pour réussir ton plan d'affaires, ne réponds pas ""tout le monde"". Si tu essaies de parler à tout le monde, tu ne toucheras personne.

Imagine ton client parfait et décris-le nous :

Son identité : Est-ce un particulier (un parent pressé, un passionné de belles voitures) ou une entreprise (un gestionnaire de flotte, un concessionnaire) ?

Son comportement : Où habite-t-il ? Quel est son budget ? Est-ce qu'il privilégie la rapidité, le prix bas ou la qualité impeccable ?

Sa motivation : Quel est le déclic qui le fait t'appeler ?

Astuce : Plus ton client idéal est précis, plus il sera facile pour nous de créer une stratégie publicitaire qui ne gaspillera pas ton argent.",
            expertAdviceEN: @"Expert advice for a good answer:
To succeed with your business plan, don't answer ""everyone"". If you try to talk to everyone, you won't reach anyone.

Imagine your perfect client and describe them to us:

Their identity: Is it an individual (a busy parent, a car enthusiast) or a business (a fleet manager, a dealer)?

Their behavior: Where do they live? What's their budget? Do they prioritize speed, low price or impeccable quality?

Their motivation: What's the trigger that makes them call you?

Tip: The more precise your ideal client is, the easier it will be for us to create an advertising strategy that won't waste your money.",
            coachPromptFR: @"Tu es un expert en marketing et en ciblage client. Ta mission est d'aider l'entrepreneur à brosser le portrait-robot de son client idéal.

ÉTAPE 1 : ANALYSE DU CONTEXTE
Lis attentivement toutes les réponses précédentes (le nom de l'entreprise, le secteur d'activité, le problème réglé et la solution proposée). Détermine quel type de client serait le plus prêt à payer pour la valeur ajoutée décrite.

ÉTAPE 2 : GÉNÉRATION DE PROFILS
En fonction de cette analyse, propose deux options de profils détaillés pour répondre à la question :
Qui est la personne la plus susceptible de payer pour tes services dès demain ? Précise son profil : son âge, sa localisation, son métier et surtout, ce qu'elle valorise le plus.

Structure des options :

Option 1 (Cible B2C - Particuliers) : Un profil individuel basé sur le gain de temps ou le confort personnel.

Option 2 (Cible B2B ou Niche Spécifique) : Un profil professionnel ou un passionné exigeant, basé sur l'image de marque ou l'expertise.

Consigne de style :
Rédige chaque option de manière très concrète. Termine par : 'D'après ton projet, voici deux types de clients qui seraient tes meilleurs ambassadeurs. Lequel te semble le plus prioritaire ?'",
            coachPromptEN: @"You are an expert in marketing and customer targeting. Your mission is to help the entrepreneur paint a picture of their ideal client.

STEP 1: CONTEXT ANALYSIS
Carefully read all previous answers (company name, business sector, problem solved and proposed solution). Determine which type of client would be most ready to pay for the described added value.

STEP 2: PROFILE GENERATION
Based on this analysis, propose two detailed profile options to answer the question:
Who is the person most likely to pay for your services tomorrow? Specify their profile: their age, location, job and especially what they value most.

Option structure:

Option 1 (B2C Target - Individuals): An individual profile based on time savings or personal comfort.

Option 2 (B2B or Specific Niche Target): A professional or demanding enthusiast profile, based on brand image or expertise.

Style instruction:
Write each option very concretely. End with: 'Based on your project, here are two types of clients who would be your best ambassadors. Which one seems most priority to you?'"
        ));

        // Question 7: Competition
        questions.Add(CreateDetailedQuestion(
            questionNumber: 7,
            stepNumber: 3,
            questionType: QuestionType.LongText,
            questionTextFR: "Si ton entreprise n'existait pas, vers quelle autre solution tes clients se tourneraient-ils ? Cite tes 2 ou 3 concurrents principaux (directs ou indirects).",
            questionTextEN: "If your company didn't exist, what other solution would your clients turn to? Name your 2 or 3 main competitors (direct or indirect).",
            helpTextFR: "Génère le tableau SWOT.",
            helpTextEN: "Generates the SWOT table.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Pour bien répondre, ne cherche pas seulement les entreprises qui font exactement la même chose que toi.

Il existe deux types de concurrents à surveiller :

La concurrence directe : Ceux qui font le même métier (ex: un autre lave-auto mobile).

La concurrence indirecte : Les autres options du client (ex: les stations-service avec lave-auto automatique, ou même le client qui lave sa voiture lui-même).

Astuce : Pour chacun, essaie de trouver une force (pourquoi ils réussissent) et une faiblesse (là où tu peux faire mieux). Ne dis jamais ""je n'ai pas de concurrents"" ; cela inquiète souvent les banquiers car cela signifie soit que le marché n'existe pas, soit que tu ne l'as pas assez étudié !",
            expertAdviceEN: @"Expert advice for a good answer:
To answer well, don't just look for companies that do exactly the same thing as you.

There are two types of competitors to watch:

Direct competition: Those who do the same job (ex: another mobile car wash).

Indirect competition: The client's other options (ex: gas stations with automatic car washes, or even the client washing their own car).

Tip: For each one, try to find a strength (why they succeed) and a weakness (where you can do better). Never say ""I have no competitors""; this often worries bankers because it means either the market doesn't exist, or you haven't studied it enough!",
            coachPromptFR: @"Tu es un analyste de marché expert. Ta mission est d'aider l'entrepreneur à identifier les alternatives qui s'offrent à ses clients.

ÉTAPE 1 : ANALYSE DU CONTEXTE
Lis attentivement les réponses précédentes (secteur d'activité, solution proposée et surtout le profil du client idéal). Comprends quelle est la valeur principale offerte (ex: gain de temps, bas prix, prestige).

ÉTAPE 2 : GÉNÉRATION D'ANALYSES CONCURRENTIELLES
Propose deux options de réponse pour la question suivante :
Si ton entreprise n'existait pas, vers quelle autre solution tes clients se tourneraient-ils ? Cite tes 2 ou 3 concurrents principaux.

Structure des options :

Option 1 (Concurrence Directe & Franchise) : Identifie les acteurs établis qui offrent un service similaire.

Option 2 (Concurrence Indirecte & Statu Quo) : Identifie les solutions alternatives (faire soi-même, services low-cost) qui volent des parts de marché.

Consigne de style :
Rédige chaque option pour qu'elle mette en lumière une opportunité de se différencier. Termine par : 'Identifier tes concurrents permet de prouver que tu es meilleur qu'eux. Voici deux façons d'analyser ton marché :'",
            coachPromptEN: @"You are an expert market analyst. Your mission is to help the entrepreneur identify the alternatives available to their clients.

STEP 1: CONTEXT ANALYSIS
Carefully read previous answers (business sector, proposed solution and especially the ideal client profile). Understand what the main value offered is (ex: time savings, low price, prestige).

STEP 2: COMPETITIVE ANALYSIS GENERATION
Propose two answer options for the following question:
If your company didn't exist, what other solution would your clients turn to? Name your 2 or 3 main competitors.

Option structure:

Option 1 (Direct Competition & Franchise): Identify established players who offer a similar service.

Option 2 (Indirect Competition & Status Quo): Identify alternative solutions (DIY, low-cost services) that steal market share.

Style instruction:
Write each option to highlight an opportunity to differentiate. End with: 'Identifying your competitors allows you to prove you're better than them. Here are two ways to analyze your market:'"
        ));

        // Question 8: Products & Pricing
        questions.Add(CreateDetailedQuestion(
            questionNumber: 8,
            stepNumber: 4,
            questionType: QuestionType.LongText,
            questionTextFR: "Quels sont tes principaux produits ou forfaits et à quel prix comptes-tu les vendre ? Explique-nous aussi comment tu as fixé ces tarifs par rapport au marché.",
            questionTextEN: "What are your main products or packages and at what price do you plan to sell them? Also explain how you set these prices compared to the market.",
            helpTextFR: "Définit la rentabilité.",
            helpTextEN: "Defines profitability.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Pour bien répondre, ne te contente pas de donner un chiffre au hasard. Ton prix raconte une histoire.

Voici comment structurer ta réponse :

La liste : Détaille tes services (ex: Lavage Express, Shampoing Intérieur, Protection Cire).

Le prix : Donne un montant précis pour chaque service.

La logique : Pourquoi ce prix ? Est-ce parce que tes produits coûtent cher, parce que tu y passes beaucoup de temps, ou parce que tu veux être le plus abordable du quartier ?

Astuce : Dans ta Prévision financière, nous devrons calculer ta rentabilité. Si tu vends un service 100 $, assure-toi que tes produits et ton essence ne t'en coûtent pas 90 $ ! Pense à inclure un service ""entrée de gamme"" pour attirer les gens et un service ""haut de gamme"" pour faire ta marge.",
            expertAdviceEN: @"Expert advice for a good answer:
To answer well, don't just give a random number. Your price tells a story.

Here's how to structure your answer:

The list: Detail your services (ex: Express Wash, Interior Shampoo, Wax Protection).

The price: Give a precise amount for each service.

The logic: Why this price? Is it because your products are expensive, because you spend a lot of time on it, or because you want to be the most affordable in the neighborhood?

Tip: In your Financial Forecast, we'll need to calculate your profitability. If you sell a service for $100, make sure your products and gas don't cost you $90! Think about including an ""entry-level"" service to attract people and a ""high-end"" service for your margin.",
            coachPromptFR: @"Tu es un expert en tarification et stratégie commerciale. Ta mission est d'aider l'entrepreneur à bâtir une liste de prix réaliste et rentable.

ÉTAPE 1 : ANALYSE DU CONTEXTE
Lis attentivement les réponses précédentes (secteur d'activité, solution et client idéal). Détermine si le positionnement est 'économique', 'standard' ou 'haut de gamme' d'après la valeur ajoutée décrite.

ÉTAPE 2 : GÉNÉRATION D'OFFRES
Propose deux options de structure de prix pour répondre à la question :
Quels sont tes principaux produits ou forfaits et à quel prix comptes-tu les vendre ? Explique-nous aussi comment tu as fixé ces tarifs par rapport au marché.

Structure des options :

Option 1 (Le Menu ""Standard"" - Volume) : Une offre simple avec un produit d'appel et un produit principal, axée sur la compétitivité.

Option 2 (La Pyramide ""Premium"" - Valeur) : Une offre avec des forfaits graduels (Bronze, Or, Platine) pour maximiser le panier moyen.

Consigne de style :
Suggère des prix réalistes pour le marché québécois (en $). Termine par : 'Définir tes prix est l'étape la plus concrète pour ton futur profit. Voici deux structures qui pourraient convenir à ton projet :'",
            coachPromptEN: @"You are an expert in pricing and commercial strategy. Your mission is to help the entrepreneur build a realistic and profitable price list.

STEP 1: CONTEXT ANALYSIS
Carefully read previous answers (business sector, solution and ideal client). Determine if the positioning is 'economic', 'standard' or 'high-end' based on the described added value.

STEP 2: OFFER GENERATION
Propose two price structure options to answer the question:
What are your main products or packages and at what price do you plan to sell them? Also explain how you set these prices compared to the market.

Option structure:

Option 1 (The ""Standard"" Menu - Volume): A simple offer with a lead product and main product, focused on competitiveness.

Option 2 (The ""Premium"" Pyramid - Value): An offer with graduated packages (Bronze, Gold, Platinum) to maximize the average basket.

Style instruction:
Suggest realistic prices for the Quebec market (in $). End with: 'Setting your prices is the most concrete step for your future profit. Here are two structures that could suit your project:'"
        ));

        // Question 9: Marketing Channels
        questions.Add(CreateDetailedQuestion(
            questionNumber: 9,
            stepNumber: 4,
            questionType: QuestionType.MultipleChoice,
            questionTextFR: "Par quels chemins tes clients vont-ils passer pour découvrir tes services et passer leur première commande ? Identifie les 2 canaux sur lesquels tu vas concentrer tes efforts au démarrage. (Ex : réseaux sociaux, prospection directe, partenariats, publicité locale, etc.).",
            questionTextEN: "What paths will your clients take to discover your services and place their first order? Identify the 2 channels you will focus your efforts on at startup. (Ex: social media, direct prospecting, partnerships, local advertising, etc.).",
            helpTextFR: "Rédige le Plan d'Action Marketing.",
            helpTextEN: "Writes the Marketing Action Plan.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Au début, il vaut mieux être excellent sur deux canaux que moyen sur dix.

Pour bien choisir tes canaux, pose-toi ces questions :

Où se trouve ton client ? Si tu vises des retraités, Instagram n'est peut-être pas le meilleur choix. Si tu vises des entreprises, LinkedIn ou la prospection physique seront plus payants.

Le coût vs le temps : As-tu du budget pour faire de la publicité payante (ex: Facebook Ads) ou as-tu du temps pour faire du contenu gratuit ou de la distribution de flyers ?

Astuce : Un canal de vente, c'est comme un tuyau qui apporte des clients. Assure-toi que ce tuyau est direct et que tu sais comment mesurer s'il fonctionne (ex: ""Je sais que pour 100 flyers distribués, j'obtiens 2 appels"").",
            expertAdviceEN: @"Expert advice for a good answer:
At the beginning, it's better to be excellent on two channels than average on ten.

To choose your channels well, ask yourself these questions:

Where is your client? If you're targeting retirees, Instagram might not be the best choice. If you're targeting businesses, LinkedIn or physical prospecting will be more rewarding.

Cost vs time: Do you have a budget for paid advertising (ex: Facebook Ads) or do you have time to create free content or distribute flyers?

Tip: A sales channel is like a pipe that brings clients. Make sure this pipe is direct and that you know how to measure if it works (ex: ""I know that for 100 flyers distributed, I get 2 calls"").",
            coachPromptFR: @"Tu es un expert en croissance (Growth Marketing). Ta mission est d'aider l'entrepreneur à choisir les deux chemins les plus efficaces pour obtenir ses premiers clients.

ÉTAPE 1 : ANALYSE DU CONTEXTE
Lis attentivement les réponses précédentes (secteur d'activité, solution et surtout le profil du client idéal / Persona). Détermine où ce client passe son temps (en ligne ou physiquement) et comment il prend ses décisions d'achat.

ÉTAPE 2 : GÉNÉRATION DE STRATÉGIES
Propose deux options de combinaisons de canaux pour répondre à la question :
Par quels chemins tes clients vont-ils passer pour découvrir tes services ? Identifie tes 2 canaux prioritaires.

Structure des options :

Option 1 (Le mix ""Visibilité Digitale"") : Focus sur les réseaux sociaux et la publicité ciblée, idéal pour une cible large ou jeune.

Option 2 (Le mix ""Réseau et Proximité"") : Focus sur le bouche-à-oreille, les partenariats locaux ou la prospection directe, idéal pour une cible locale ou haut de gamme.

Consigne de style :
Explique brièvement pourquoi ces canaux sont les meilleurs pour ce projet. Termine par : 'Au début, il vaut mieux être excellent sur deux canaux que moyen sur dix. Voici deux duos gagnants pour ton projet :'",
            coachPromptEN: @"You are a Growth Marketing expert. Your mission is to help the entrepreneur choose the two most effective paths to get their first clients.

STEP 1: CONTEXT ANALYSIS
Carefully read previous answers (business sector, solution and especially the ideal client profile / Persona). Determine where this client spends their time (online or physically) and how they make purchasing decisions.

STEP 2: STRATEGY GENERATION
Propose two channel combination options to answer the question:
What paths will your clients take to discover your services? Identify your 2 priority channels.

Option structure:

Option 1 (The ""Digital Visibility"" mix): Focus on social media and targeted advertising, ideal for a broad or young target.

Option 2 (The ""Network and Proximity"" mix): Focus on word-of-mouth, local partnerships or direct prospecting, ideal for a local or high-end target.

Style instruction:
Briefly explain why these channels are best for this project. End with: 'At the beginning, it's better to be excellent on two channels than average on ten. Here are two winning duos for your project:'"
        ));

        // Question 10: Team & Promoters
        questions.Add(CreateDetailedQuestion(
            questionNumber: 10,
            stepNumber: 2,
            questionType: QuestionType.LongText,
            questionTextFR: "Qui compose l'équipe de direction de ton projet ? Indique le nom de chaque promoteur, son rôle précis, ses compétences clés ainsi que son pourcentage de participation dans l'entreprise.",
            questionTextEN: "Who makes up the management team of your project? Indicate the name of each promoter, their precise role, their key skills and their percentage of participation in the company.",
            helpTextFR: "Valorise le profil du promoteur.",
            helpTextEN: "Highlights the promoter's profile.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Dans un projet d'affaires, la clarté sur ""qui fait quoi"" et ""qui possède quoi"" est un gage de sérieux pour les partenaires financiers.

Pour bien répondre, structure ta réponse ainsi :

Les rôles : Ne donne pas juste des titres, décris les responsabilités (ex: Marie gère les finances et le marketing, Jean gère les opérations et le terrain).

L'expertise : Mets en avant les forces complémentaires. L'idée est de montrer que l'équipe couvre tous les besoins de l'entreprise (Vente, Technique, Gestion).

Le pourcentage : Indique comment le capital est réparti (ex: 50/50 ou 100% si tu es seul).

Astuce : Si tu es seul au démarrage, n'hésite pas à mentionner ton rôle de ""PDG multi-tâches"" tout en précisant tes domaines de prédilection. Les banquiers aiment voir que l'entrepreneur connaît ses forces, mais aussi les expertises qu'il devra aller chercher plus tard.",
            expertAdviceEN: @"Expert advice for a good answer:
In a business project, clarity on ""who does what"" and ""who owns what"" is a sign of seriousness for financial partners.

To answer well, structure your answer like this:

Roles: Don't just give titles, describe responsibilities (ex: Marie manages finances and marketing, Jean manages operations and fieldwork).

Expertise: Highlight complementary strengths. The idea is to show that the team covers all the company's needs (Sales, Technical, Management).

Percentage: Indicate how the capital is distributed (ex: 50/50 or 100% if you're alone).

Tip: If you're alone at startup, don't hesitate to mention your role as ""multi-tasking CEO"" while specifying your areas of expertise. Bankers like to see that the entrepreneur knows their strengths, but also the expertise they'll need to seek later.",
            coachPromptFR: null,
            coachPromptEN: null
        ));

        // Continue with questions 11-22...
        // Question 11: Legal Structure
        questions.Add(CreateDetailedQuestion(
            questionNumber: 11,
            stepNumber: 2,
            questionType: QuestionType.SingleChoice,
            questionTextFR: "Sous quelle forme juridique as-tu décidé d'enregistrer ton entreprise et pour quelle raison as-tu fait ce choix ? (Ex : Entreprise individuelle, Société incorporée/Inc., OBNL, etc.)",
            questionTextEN: "What legal form have you decided to register your business under and why did you make this choice? (Ex: Sole proprietorship, Incorporated company/Inc., Non-profit, etc.)",
            helpTextFR: "Remplit la section juridique.",
            helpTextEN: "Fills the legal section.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Le choix de la structure juridique est comme les fondations d'une maison : il détermine ta responsabilité et ta façon de payer des impôts.

Voici un petit rappel pour t'aider :

Entreprise individuelle : C'est la forme la plus simple et la moins coûteuse au début. Tu es seul maître à bord, mais tu es personnellement responsable de tout.

Société par actions (Inc. / SAS) : L'entreprise est une entité séparée de toi. C'est plus complexe et coûteux à créer, mais cela protège tes biens personnels et permet d'accueillir des associés plus facilement.

OBNL ou Coopérative : Si ton projet a une mission sociale avant tout.

Astuce : Si tu n'es pas encore certain, indique celle qui te semble la plus probable. Dans ton plan d'affaires, nous expliquerons pourquoi ce statut est le plus adapté à la croissance de ton projet.",
            expertAdviceEN: @"Expert advice for a good answer:
The choice of legal structure is like the foundations of a house: it determines your liability and how you pay taxes.

Here's a small reminder to help you:

Sole proprietorship: It's the simplest and least expensive form at the beginning. You're the sole master on board, but you're personally responsible for everything.

Corporation (Inc. / SAS): The company is a separate entity from you. It's more complex and expensive to create, but it protects your personal assets and makes it easier to welcome partners.

Non-profit or Cooperative: If your project has a social mission above all.

Tip: If you're not yet sure, indicate the one that seems most likely to you. In your business plan, we'll explain why this status is best suited for your project's growth.",
            coachPromptFR: @"Tu es un expert en droit des affaires international. Ta mission est d'aider l'entrepreneur à choisir sa structure juridique en fonction de sa localisation.

ÉTAPE 1 : DÉTECTION DE LA ZONE GÉOGRAPHIQUE
Lis les réponses précédentes pour identifier où l'entreprise est située (ex: Québec, France, Côte d'Ivoire, etc.). Si aucune localisation n'est mentionnée, demande-lui de préciser, mais par défaut, base-toi sur le contexte détecté.

ÉTAPE 2 : GÉNÉRATION DES OPTIONS LOCALES
Propose toutes les structures juridiques valides pour ce pays spécifique. Pour chaque option, affiche :
- Le nom exact de la forme juridique.
- Les Avantages / Inconvénients simplifiés.
- Une réponse pré-rédigée à la première personne.

ÉTAPE 3 : ADAPTATION SELON LE PAYS
Si Québec/Canada : Propose Entreprise individuelle, Société par actions (Inc.), SENC, OBNL, Coop.
Si France : Propose Micro-entreprise, SASU/SAS, EURL/SARL, Association loi 1901.
Si Afrique (Zone OHADA) : Propose Entreprise Individuelle, SARL, SAS, SA, Société Coopérative.

Consigne de style :
Présente cela sous forme de cartes cliquables. Termine par : 'Voici les options disponibles pour ton entreprise en [NOM DU PAYS]. Laquelle choisis-tu ?'",
            coachPromptEN: @"You are an expert in international business law. Your mission is to help the entrepreneur choose their legal structure based on their location.

STEP 1: GEOGRAPHIC ZONE DETECTION
Read previous answers to identify where the company is located (ex: Quebec, France, Ivory Coast, etc.). If no location is mentioned, ask them to specify, but by default, base yourself on the detected context.

STEP 2: LOCAL OPTIONS GENERATION
Propose all valid legal structures for this specific country. For each option, display:
- The exact name of the legal form.
- Simplified Advantages / Disadvantages.
- A pre-written first-person answer.

STEP 3: ADAPTATION BY COUNTRY
If Quebec/Canada: Propose Sole proprietorship, Corporation (Inc.), Partnership, Non-profit, Coop.
If France: Propose Micro-enterprise, SASU/SAS, EURL/SARL, Association law 1901.
If Africa (OHADA Zone): Propose Sole Proprietorship, LLC, SAS, SA, Cooperative Society.

Style instruction:
Present this as clickable cards. End with: 'Here are the options available for your business in [COUNTRY NAME]. Which one do you choose?'",
            optionsFR: "[\"Entreprise individuelle\",\"Société par actions (Inc.)\",\"SENC\",\"OBNL\",\"Coopérative\",\"Autre\"]",
            optionsEN: "[\"Sole proprietorship\",\"Corporation (Inc.)\",\"Partnership\",\"Non-profit\",\"Cooperative\",\"Other\"]"
        ));

        // Question 12: Material Needs
        questions.Add(CreateDetailedQuestion(
            questionNumber: 12,
            stepNumber: 5,
            questionType: QuestionType.LongText,
            questionTextFR: "Quels sont tes besoins matériels pour opérer ? Précise si tu as besoin d'un local physique (bureau, entrepôt, boutique) et fais la liste de l'équipement ou de l'inventaire de départ nécessaire.",
            questionTextEN: "What are your material needs to operate? Specify if you need a physical location (office, warehouse, store) and list the equipment or starting inventory needed.",
            helpTextFR: "Calcule les frais fixes (Loyer/Stock).",
            helpTextEN: "Calculates fixed costs (Rent/Stock).",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Cette question prépare ton budget. Sois le plus précis possible pour éviter les mauvaises surprises financières.

Voici comment diviser ta réponse :

Le Lieu : Travailles-tu de la maison, dans un camion (mobile), ou as-tu besoin de louer un local ? Si tu loues, quels sont les critères essentiels (surface, électricité, accès client) ?

L'équipement (Immobilisations) : Ce sont les objets durables que tu achètes une fois (ex : ordinateur, nettoyeur haute pression, remorque).

L'inventaire (Stocks) : Ce sont les produits que tu consommes ou revends (ex : produits chimiques, microfibres, accessoires).

Astuce : N'oublie pas les ""petits"" équipements qui s'additionnent vite. Dans Previsio, nous devrons séparer ce que tu possèdes déjà de ce que tu dois acheter avec le financement demandé.",
            expertAdviceEN: @"Expert advice for a good answer:
This question prepares your budget. Be as precise as possible to avoid bad financial surprises.

Here's how to divide your answer:

The Location: Do you work from home, in a truck (mobile), or do you need to rent premises? If renting, what are the essential criteria (space, electricity, client access)?

Equipment (Fixed Assets): These are durable items you buy once (ex: computer, pressure washer, trailer).

Inventory (Stock): These are products you consume or resell (ex: chemicals, microfibers, accessories).

Tip: Don't forget the ""small"" equipment that adds up quickly. In Previsio, we'll need to separate what you already own from what you need to buy with the requested financing.",
            coachPromptFR: @"Tu es un expert en gestion des opérations et planification financière. Ta mission est d'aider l'entrepreneur à lister tout ce dont il a besoin pour démarrer sans rien oublier.

ÉTAPE 1 : ANALYSE DU CONTEXTE
Lis attentivement les réponses précédentes (secteur d'activité, solution proposée et tarifs). Détermine si le projet nécessite une infrastructure lourde (local commercial) ou légère (service mobile, travail à domicile).

ÉTAPE 2 : GÉNÉRATION DE LISTES D'ÉQUIPEMENT
Propose deux options de structure de besoins matériels :

Option 1 (Le Modèle ""Léger / Mobile"") : Focus sur l'équipement transportable et l'absence de local fixe.

Option 2 (Le Modèle ""Établi / Fixe"") : Focus sur l'aménagement d'un espace de travail dédié.

Consigne de style :
Sois très précis sur les types d'objets. Termine par : 'Bien évaluer tes besoins permet d'éviter les surprises dans ton budget. Voici deux configurations possibles pour ton projet :'",
            coachPromptEN: @"You are an expert in operations management and financial planning. Your mission is to help the entrepreneur list everything they need to start without forgetting anything.

STEP 1: CONTEXT ANALYSIS
Carefully read previous answers (business sector, proposed solution and prices). Determine if the project requires heavy infrastructure (commercial premises) or light (mobile service, work from home).

STEP 2: EQUIPMENT LIST GENERATION
Propose two material needs structure options:

Option 1 (The ""Light / Mobile"" Model): Focus on portable equipment and no fixed premises.

Option 2 (The ""Established / Fixed"" Model): Focus on setting up a dedicated workspace.

Style instruction:
Be very precise about types of items. End with: 'Properly evaluating your needs helps avoid surprises in your budget. Here are two possible configurations for your project:'"
        ));

        // Question 13: Personal Investment
        questions.Add(CreateDetailedQuestion(
            questionNumber: 13,
            stepNumber: 6,
            questionType: QuestionType.Currency,
            questionTextFR: "Quel montant souhaites-tu investir personnellement dans ce projet ? (C'est ton apport personnel qui servira de base pour rassurer tes partenaires financiers sur ton engagement).",
            questionTextEN: "What amount do you want to personally invest in this project? (This is your personal contribution that will serve as a base to reassure your financial partners about your commitment).",
            helpTextFR: "Donnée source pour Previsio.",
            helpTextEN: "Source data for Previsio.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
L'apport personnel est le premier signal que tu envoies aux banquiers ou aux investisseurs.

Voici ce qu'il faut savoir :

La règle d'or : En général, les institutions financières s'attendent à ce que tu finances entre 15 % et 25 % du projet total avec tes propres fonds.

L'origine des fonds : Cela peut provenir de tes économies, d'un don de la famille (love money) ou d'un matériel que tu possèdes déjà et que tu transfères à l'entreprise.

Le levier : Plus ton apport est solide, plus il sera facile d'obtenir un prêt avantageux.

Astuce : Si ton apport est faible, ne te décourage pas. Indique le montant réel dont tu disposes. Dans ton plan d'affaires, nous mettrons l'accent sur ton expertise et ton sérieux pour compenser !",
            expertAdviceEN: @"Expert advice for a good answer:
Personal contribution is the first signal you send to bankers or investors.

Here's what you need to know:

The golden rule: Generally, financial institutions expect you to finance between 15% and 25% of the total project with your own funds.

Source of funds: This can come from your savings, a family gift (love money) or equipment you already own that you transfer to the business.

The leverage: The stronger your contribution, the easier it will be to get a favorable loan.

Tip: If your contribution is low, don't be discouraged. Indicate the real amount you have available. In your business plan, we'll emphasize your expertise and seriousness to compensate!",
            coachPromptFR: null,
            coachPromptEN: null
        ));

        // Question 14: Total Financing Needed
        questions.Add(CreateDetailedQuestion(
            questionNumber: 14,
            stepNumber: 6,
            questionType: QuestionType.Currency,
            questionTextFR: "Quel est le montant total dont tu as besoin pour lancer ou propulser ton entreprise, et comment comptes-tu utiliser cet argent ? (Précise le montant du prêt ou de l'investissement recherché).",
            questionTextEN: "What is the total amount you need to launch or propel your business, and how do you plan to use this money? (Specify the loan or investment amount sought).",
            helpTextFR: "Calcule le besoin de prêt/investissement.",
            helpTextEN: "Calculates loan/investment need.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Un bon montant de financement ne doit être ni trop bas (pour ne pas manquer d'oxygène), ni trop haut (pour ne pas crouler sous les dettes).

Pour bien évaluer ton besoin, pense à ces trois catégories :

Les gros achats (CapEx) : Ton équipement, ton véhicule ou tes rénovations.

Le fonds de roulement : L'argent nécessaire pour payer tes premières factures (essence, marketing, assurances) avant que les premiers revenus importants n'arrivent.

La réserve de sécurité : Un petit coussin pour les imprévus.

Astuce : Dans Previsio, nous allons croiser ce chiffre avec tes prévisions de ventes. Si tu demandes 50 000 $, assure-toi que ton activité générera assez de profits pour rembourser les mensualités chaque mois !",
            expertAdviceEN: @"Expert advice for a good answer:
A good financing amount should be neither too low (to not run out of oxygen), nor too high (to not be crushed by debt).

To properly evaluate your need, think about these three categories:

Big purchases (CapEx): Your equipment, vehicle or renovations.

Working capital: The money needed to pay your first bills (gas, marketing, insurance) before significant revenues arrive.

Safety reserve: A small cushion for unexpected events.

Tip: In Previsio, we'll cross-reference this figure with your sales forecasts. If you're asking for $50,000, make sure your business will generate enough profit to repay the monthly payments!",
            coachPromptFR: @"Tu es un conseiller financier en entrepreneuriat. Ta mission est d'aider l'entrepreneur à estimer le montant total de son financement et à justifier l'utilisation des fonds.

ÉTAPE 1 : ANALYSE DU CONTEXTE
Lis attentivement les réponses précédentes, particulièrement la liste des besoins matériels et le modèle d'affaires. Estime si le projet demande un investissement de départ léger (5k$ - 15k$), modéré (15k$ - 50k$) ou important (50k$+).

ÉTAPE 2 : GÉNÉRATION DE SCÉNARIOS FINANCIERS
Propose deux options de montage financier :

Option 1 (Le Lancement ""Essentiel"") : Un montant minimaliste pour démarrer rapidement avec le strict nécessaire.

Option 2 (Le Lancement ""Propulsion"") : Un montant plus élevé incluant une réserve de sécurité et un budget marketing agressif.

Consigne de style :
Répartis le montant en trois catégories claires : Équipement, Marketing et Fonds de roulement. Termine par : 'Un montant bien justifié est la clé pour convaincre ton banquier. Voici deux scénarios pour ton projet :'",
            coachPromptEN: @"You are an entrepreneurship financial advisor. Your mission is to help the entrepreneur estimate the total financing amount and justify the use of funds.

STEP 1: CONTEXT ANALYSIS
Carefully read previous answers, particularly the list of material needs and business model. Estimate if the project requires light ($5k-$15k), moderate ($15k-$50k) or significant ($50k+) startup investment.

STEP 2: FINANCIAL SCENARIO GENERATION
Propose two financial structure options:

Option 1 (The ""Essential"" Launch): A minimalist amount to start quickly with bare necessities.

Option 2 (The ""Propulsion"" Launch): A higher amount including a safety reserve and aggressive marketing budget.

Style instruction:
Distribute the amount into three clear categories: Equipment, Marketing and Working Capital. End with: 'A well-justified amount is key to convincing your banker. Here are two scenarios for your project:'"
        ));

        // Question 15: Launch Date
        questions.Add(CreateDetailedQuestion(
            questionNumber: 15,
            stepNumber: 5,
            questionType: QuestionType.Date,
            questionTextFR: "Quelle est la date officielle du \"jour 1\" de ton entreprise et combien de mois penses-tu qu'il te reste pour finaliser tes préparatifs ?",
            questionTextEN: "What is the official date of your business's \"day 1\" and how many months do you think you have left to finalize your preparations?",
            helpTextFR: "Génère le calendrier de réalisation.",
            helpTextEN: "Generates the implementation calendar.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Une date de lancement réussie est une date réaliste. Dans un plan d'affaires, on veut voir que tu as planifié la ""montée en puissance"" de ton projet.

Voici comment bien définir ton calendrier :

Le point de départ : Où en es-tu aujourd'hui ?

Les étapes clés : Énumère 3 ou 4 moments importants (ex: ""Février : Signature du prêt"", ""Mars : Réception du camion"", ""Avril : Lancement de la pub"").

La saisonnalité : Si tu lances un service de nettoyage auto, viser le début du printemps est stratégique !

Astuce : Dans la Prévision financière, la date de début détermine le moment où tes revenus commencent à tomber. Si tu prévois deux mois de travaux ou de préparation sans ventes, il faut s'assurer que tu as assez de liquidités pour tenir jusqu'au lancement réel.",
            expertAdviceEN: @"Expert advice for a good answer:
A successful launch date is a realistic date. In a business plan, we want to see that you've planned the ""ramp-up"" of your project.

Here's how to properly define your calendar:

The starting point: Where are you today?

Key milestones: List 3 or 4 important moments (ex: ""February: Loan signing"", ""March: Truck delivery"", ""April: Advertising launch"").

Seasonality: If you're launching a car cleaning service, aiming for early spring is strategic!

Tip: In the Financial Forecast, the start date determines when your revenues begin. If you plan two months of work or preparation without sales, make sure you have enough cash to last until the actual launch.",
            coachPromptFR: null,
            coachPromptEN: null
        ));

        // Question 16: Team Evolution
        questions.Add(CreateDetailedQuestion(
            questionNumber: 16,
            stepNumber: 5,
            questionType: QuestionType.LongText,
            questionTextFR: "Comment vois-tu l'évolution de ton équipe ? Précise si tu prévois d'embaucher des employés dès le départ ou plus tard, en indiquant le nombre de personnes et les responsabilités que tu souhaites leur confier.",
            questionTextEN: "How do you see your team's evolution? Specify if you plan to hire employees from the start or later, indicating the number of people and responsibilities you want to assign them.",
            helpTextFR: "Génère le plan RH.",
            helpTextEN: "Generates the HR plan.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
L'embauche est l'un des plus gros postes de dépenses, mais c'est aussi ce qui permet à ton entreprise de grandir.

Pour bien répondre, pense à ces trois éléments :

Le timing : Est-ce une embauche immédiate (nécessaire pour ouvrir) ou une embauche liée à ton succès (ex: ""J'embauche un deuxième technicien quand j'atteins 15 clients par semaine"") ?

Le type de contrat : Seront-ils à temps plein, à temps partiel ou des travailleurs contractuels/pigistes ?

Le coût réel : N'oublie pas que dans Previsio, nous devrons calculer non seulement le salaire, mais aussi les charges sociales (DAS).

Astuce : Si tu prévois de tout faire seul au début, indique-le clairement, mais précise à quel moment tu penses que tu ne pourras plus suffire à la tâche. Cela montre au banquier que tu as une vision à long terme.",
            expertAdviceEN: @"Expert advice for a good answer:
Hiring is one of the biggest expense items, but it's also what allows your business to grow.

To answer well, think about these three elements:

Timing: Is it an immediate hire (necessary to open) or a success-linked hire (ex: ""I hire a second technician when I reach 15 clients per week"")?

Contract type: Will they be full-time, part-time or contract/freelance workers?

Real cost: Don't forget that in Previsio, we'll need to calculate not only the salary, but also social charges.

Tip: If you plan to do everything alone at first, indicate it clearly, but specify when you think you won't be able to handle the workload anymore. This shows the banker that you have a long-term vision.",
            coachPromptFR: @"Tu es un expert en ressources humaines et en gestion de croissance. Ta mission est d'aider l'entrepreneur à anticiper ses besoins en main-d'oeuvre.

ÉTAPE 1 : ANALYSE DU CONTEXTE
Lis attentivement les réponses précédentes (secteur d'activité, solution et objectifs de financement). Détermine si le projet peut être géré seul au départ ou s'il nécessite immédiatement du soutien.

ÉTAPE 2 : GÉNÉRATION DE SCÉNARIOS D'ÉQUIPE
Propose deux options d'évolution :

Option 1 (Le Modèle ""Solo-Entrepreneur"" évolutif) : Un démarrage seul pour maximiser les profits, avec une embauche prévue une fois un certain seuil de revenus atteint.

Option 2 (Le Modèle ""Équipe Opérationnelle"" dès le départ) : Un démarrage avec un ou plusieurs employés/partenaires pour diviser les tâches.

Consigne de style :
Détaille les rôles précis. Termine par : 'Ton équipe est le moteur de ta croissance. Voici deux trajectoires possibles pour ton organisation :'",
            coachPromptEN: @"You are an expert in human resources and growth management. Your mission is to help the entrepreneur anticipate their workforce needs.

STEP 1: CONTEXT ANALYSIS
Carefully read previous answers (business sector, solution and financing objectives). Determine if the project can be managed alone at first or if it immediately needs support.

STEP 2: TEAM SCENARIO GENERATION
Propose two evolution options:

Option 1 (The Evolving ""Solo-Entrepreneur"" Model): A solo start to maximize profits, with hiring planned once a certain revenue threshold is reached.

Option 2 (The ""Operational Team"" Model from the start): A start with one or more employees/partners to divide tasks.

Style instruction:
Detail precise roles. End with: 'Your team is the engine of your growth. Here are two possible trajectories for your organization:'"
        ));

        // Question 17: First Year Objectives
        questions.Add(CreateDetailedQuestion(
            questionNumber: 17,
            stepNumber: 5,
            questionType: QuestionType.LongText,
            questionTextFR: "Quels sont tes 3 ou 4 grands objectifs prioritaires pour ta première année d'activité ? Décris les étapes clés qui marqueront le succès de ton entreprise durant les 12 prochains mois.",
            questionTextEN: "What are your 3 or 4 major priority objectives for your first year of activity? Describe the key stages that will mark your business's success during the next 12 months.",
            helpTextFR: "Génère le calendrier des objectifs.",
            helpTextEN: "Generates the objectives calendar.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Un bon plan d'affaires ne s'arrête pas au jour de l'ouverture. Les partenaires financiers veulent voir que tu as une vision pour la suite.

Pour définir tes étapes, pense à la méthode ""un jalon par trimestre"" :

Mois 1 à 3 (Lancement) : Finaliser l'installation et réaliser tes premières ventes.

Mois 4 à 6 (Stabilisation) : Fidéliser tes premiers clients et ajuster tes services selon leurs retours.

Mois 7 à 12 (Croissance) : Atteindre ton seuil de rentabilité ou tester un nouveau canal de vente.

Astuce : Sois ambitieux mais réaliste. Il vaut mieux accomplir trois grandes étapes concrètes que de lister dix rêves que tu n'auras pas le temps de réaliser. Chaque étape franchie est une preuve de ta crédibilité en tant que gestionnaire.",
            expertAdviceEN: @"Expert advice for a good answer:
A good business plan doesn't stop at opening day. Financial partners want to see that you have a vision for what comes next.

To define your stages, think about the ""one milestone per quarter"" method:

Months 1-3 (Launch): Finalize setup and make your first sales.

Months 4-6 (Stabilization): Retain your first clients and adjust services based on their feedback.

Months 7-12 (Growth): Reach your break-even point or test a new sales channel.

Tip: Be ambitious but realistic. It's better to accomplish three concrete major stages than to list ten dreams you won't have time to achieve. Each stage completed is proof of your credibility as a manager.",
            coachPromptFR: @"Tu es un expert en gestion de projet et mentor pour entrepreneurs. Ta mission est d'aider l'utilisateur à structurer sa première année d'existence.

ÉTAPE 1 : ANALYSE DU CONTEXTE
Lis attentivement toutes les réponses précédentes (montant du financement, structure de l'équipe, canaux de vente et liste de produits). Identifie le cycle de mise en place nécessaire.

ÉTAPE 2 : GÉNÉRATION DE JALONS (MILESTONES)
Propose deux options de feuilles de route :

Option 1 (Focus ""Lancement et Validation"") : Pour un projet qui doit prouver son concept rapidement avec peu de frais.

Option 2 (Focus ""Structure et Expansion"") : Pour un projet qui vise une part de marché importante dès le départ.

Consigne de style :
Organise les réponses par trimestres (T1, T2, T3, T4). Termine par : 'Une vision claire des 12 prochains mois est le meilleur remède contre le stress du lancement. Voici deux trajectoires pour ta première année :'",
            coachPromptEN: @"You are a project management expert and entrepreneur mentor. Your mission is to help the user structure their first year of existence.

STEP 1: CONTEXT ANALYSIS
Carefully read all previous answers (financing amount, team structure, sales channels and product list). Identify the necessary setup cycle.

STEP 2: MILESTONE GENERATION
Propose two roadmap options:

Option 1 (""Launch and Validation"" Focus): For a project that needs to prove its concept quickly with low costs.

Option 2 (""Structure and Expansion"" Focus): For a project targeting significant market share from the start.

Style instruction:
Organize answers by quarters (Q1, Q2, Q3, Q4). End with: 'A clear vision of the next 12 months is the best remedy against launch stress. Here are two trajectories for your first year:'"
        ));

        // Question 18: Additional Questions (AI-Generated)
        questions.Add(CreateDetailedQuestion(
            questionNumber: 18,
            stepNumber: 3,
            questionType: QuestionType.LongText,
            questionTextFR: "Questions complementaires",
            questionTextEN: "Additional questions",
            helpTextFR: "Diagnostic des zones d'ombre.",
            helpTextEN: "Diagnosis of gray areas.",
            expertAdviceFR: null,
            expertAdviceEN: null,
            coachPromptFR: @"Tu es un consultant senior en stratégie d'affaires. Ta mission est de challenger l'entrepreneur pour rendre son projet indestructible face à un banquier ou un investisseur.

ÉTAPE 1 : ANALYSE DU DOSSIER
Lis attentivement toutes les réponses fournies par l'entrepreneur jusqu'à présent (Secteur, Persona, Solution, Prix, Équipe, Juridique, etc.).

ÉTAPE 2 : IDENTIFICATION DES LACUNES
Repère les informations manquantes, floues ou contradictoires. Pose-toi la question : 'Qu'est-ce qu'un banquier demanderait de plus pour valider la viabilité de ce projet ?'

ÉTAPE 3 : GÉNÉRATION DE QUESTIONS COMPLÉMENTAIRES
Génère entre 3 et 5 questions ultra-ciblées. Ces questions ne doivent pas être génériques. Elles doivent forcer l'entrepreneur à apporter sa vision unique.

Exemples de thèmes à explorer selon le manque détecté :

Opérations : 'Comment vas-tu gérer logistiquement le déplacement entre deux clients ?'

Ventes : 'Quel est ton argument massue quand un client te dit que c'est trop cher ?'

Risques : 'Que se passe-t-il pour ton entreprise si ton équipement principal tombe en panne demain ?'

Ambition : 'Où vois-tu cette entreprise dans 5 ans : un seul camion ou une franchise nationale ?'

Format de sortie :
'Ton projet est solide, mais pour le rendre incontestable, j'aurais besoin que tu précises ces quelques points :'
[Liste de 3 à 5 questions numérotées]",
            coachPromptEN: @"You are a senior business strategy consultant. Your mission is to challenge the entrepreneur to make their project unassailable to a banker or investor.

STEP 1: FILE ANALYSIS
Carefully read all the answers provided by the entrepreneur so far (Sector, Persona, Solution, Pricing, Team, Legal, etc.).

STEP 2: IDENTIFY GAPS
Spot missing, vague or contradictory information. Ask yourself: 'What more would a banker ask to validate this project's viability?'

STEP 3: GENERATE ADDITIONAL QUESTIONS
Generate 3 to 5 ultra-targeted questions. These questions must not be generic. They must force the entrepreneur to bring their unique vision.

Example themes to explore based on detected gaps:

Operations: 'How will you logistically manage moving between two clients?'

Sales: 'What's your knockout argument when a client says it's too expensive?'

Risks: 'What happens to your business if your main equipment breaks down tomorrow?'

Ambition: 'Where do you see this business in 5 years: one truck or a national franchise?'

Output format:
'Your project is solid, but to make it unassailable, I need you to clarify these few points:'
[List of 3 to 5 numbered questions]"
        ));

        // Question 19: Annual Sales Volume
        questions.Add(CreateDetailedQuestion(
            questionNumber: 19,
            stepNumber: 6,
            questionType: QuestionType.LongText,
            questionTextFR: "Quel volume de ventes annuel prévoyez-vous pour chacun de vos produits ou services ? (Indiquez la quantité totale que vous estimez vendre sur une période de 12 mois pour chaque catégorie).",
            questionTextEN: "What annual sales volume do you expect for each of your products or services? (Indicate the total quantity you estimate to sell over a 12-month period for each category).",
            helpTextFR: "Génère les prévisions de ventes.",
            helpTextEN: "Generates sales forecasts.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Un bon plan d'affaires ne s'arrête pas au jour de l'ouverture. Les partenaires financiers veulent voir que tu as une vision pour la suite.

Pour définir tes objectifs de volume, pense à la méthode ""un jalon par trimestre"" :

Mois 1 à 3 (Lancement) : Tes volumes seront plus bas pendant que tu finalises ton installation et réalises tes premières ventes.

Mois 4 à 6 (Stabilisation) : Tes chiffres augmentent grâce au bouche-à-oreille et à la fidélisation.

Mois 7 à 12 (Croissance) : Tu atteins ton rythme de croisière et ton seuil de rentabilité.

Astuce : Ne t'en fais pas si tes estimations ne sont pas parfaites dès maintenant. Tu pourras ajuster ces chiffres avec précision (mois par mois) dans la section ""Prévisions financières"" une fois que la structure globale de ton projet sera en place. Pour l'instant, vise une moyenne annuelle réaliste basée sur ta capacité de production ou de service.",
            expertAdviceEN: @"Expert advice for a good answer:
A good business plan doesn't stop at opening day. Financial partners want to see that you have a vision for what comes next.

To define your volume objectives, think about the ""one milestone per quarter"" method:

Months 1-3 (Launch): Your volumes will be lower while you finalize setup and make your first sales.

Months 4-6 (Stabilization): Your numbers increase through word-of-mouth and retention.

Months 7-12 (Growth): You reach your cruising speed and break-even point.

Tip: Don't worry if your estimates aren't perfect right now. You can adjust these figures precisely (month by month) in the ""Financial Forecasts"" section once your project's overall structure is in place. For now, aim for a realistic annual average based on your production or service capacity.",
            coachPromptFR: @"Tu es un consultant expert en stratégie d'entreprise et rédacteur de plans d'affaires. Ton objectif est d'aider l'entrepreneur à débloquer sa réflexion pour le volume de ventes.

Contexte disponible (Réponses précédentes - ID 8) :
[UTILISE LA RÉPONSE DE L'UTILISATEUR SUR SES PRODUITS ET SES PRIX]

Ta mission :
Propose deux options de réponse distinctes basées sur les produits et tarifs mentionnés. Chaque option doit être rédigée à la première personne.

Option 1 : L'approche ""Standard & Efficace"" - Une projection basée sur une croissance organique et prudente.

Option 2 : L'approche ""Visionnaire & Différenciée"" - Une projection plus ambitieuse, axée sur une capture rapide de parts de marché.

Contrainte de style :
Reste simple, évite le jargon inutile, et assure-toi que les volumes suggérés sont cohérents avec les prix fixés. Termine par : 'Choisis l'option qui te correspond le mieux ou personnalise-la !'",
            coachPromptEN: @"You are a business strategy consultant and business plan writer. Your goal is to help the entrepreneur unlock their thinking on sales volume.

Available context (Previous answers - ID 8):
[USE THE USER'S RESPONSE ON THEIR PRODUCTS AND PRICES]

Your mission:
Propose two distinct answer options based on the mentioned products and prices. Each option should be written in first person.

Option 1: The ""Standard & Effective"" approach - A projection based on organic and cautious growth.

Option 2: The ""Visionary & Differentiated"" approach - A more ambitious projection, focused on rapid market share capture.

Style constraint:
Stay simple, avoid unnecessary jargon, and ensure suggested volumes are consistent with set prices. End with: 'Choose the option that suits you best or customize it!'"
        ));

        // Question 20: Marketing Budget
        questions.Add(CreateDetailedQuestion(
            questionNumber: 20,
            stepNumber: 4,
            questionType: QuestionType.Currency,
            questionTextFR: "Quel budget mensuel moyen prévoyez-vous allouer à vos activités de marketing et de promotion (publicité, réseaux sociaux, matériel promotionnel) ?",
            questionTextEN: "What average monthly budget do you plan to allocate to your marketing and promotion activities (advertising, social media, promotional materials)?",
            helpTextFR: "Génère le budget marketing.",
            helpTextEN: "Generates marketing budget.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Le marketing est le carburant de vos ventes. Pour un partenaire financier, un budget bien défini est la preuve que vous avez un plan concret pour attirer des clients.

Pour établir votre montant, pensez à vos priorités par étape :

Mois 1 à 3 (Lancement) : Prévoyez un budget plus important pour créer un impact fort (lancement des réseaux sociaux, signalisation, publicités locales).

Mois 4 à 12 (Maintien) : Stabilisez vos dépenses pour entretenir la visibilité et fidéliser votre clientèle.

Astuce : Ne voyez pas ce chiffre comme définitif. Vous aurez l'occasion d'affiner ces montants et de les répartir précisément dans la section ""Prévisions financières"". Pour l'instant, visez une moyenne mensuelle qui reflète vos ambitions de croissance. Un bon point de repère se situe souvent entre 5 % et 15 % de votre chiffre d'affaires cible, selon votre secteur.",
            expertAdviceEN: @"Expert advice for a good answer:
Marketing is the fuel for your sales. For a financial partner, a well-defined budget is proof that you have a concrete plan to attract clients.

To establish your amount, think about your priorities by stage:

Months 1-3 (Launch): Plan a larger budget to create a strong impact (social media launch, signage, local advertising).

Months 4-12 (Maintenance): Stabilize your spending to maintain visibility and retain customers.

Tip: Don't see this figure as final. You'll have the opportunity to refine these amounts and distribute them precisely in the ""Financial Forecasts"" section. For now, aim for a monthly average that reflects your growth ambitions. A good benchmark is often between 5% and 15% of your target revenue, depending on your sector.",
            coachPromptFR: @"Tu es un consultant expert en stratégie d'entreprise et rédacteur de plans d'affaires. Ton objectif est d'aider l'entrepreneur à débloquer sa réflexion pour le budget marketing.

Contexte disponible (Réponses précédentes) :
- Produits et Prix (ID 8)
- Objectifs de croissance (ID 17)

Ta mission :
Propose deux options de réponse distinctes. Chaque option doit être rédigée à la première personne.

Option 1 : L'approche ""Standard & Efficace"" - Un budget maîtrisé, axé sur le bouche-à-oreille et une présence numérique organique.

Option 2 : L'approche ""Visionnaire & Différenciée"" - Un investissement plus soutenu pour dominer son créneau, incluant de la publicité payante et du contenu de haute qualité.

Contrainte de style :
Reste simple, assure-toi que les montants suggérés sont proportionnels aux prix des produits. Termine par : 'Choisis l'option qui te correspond le mieux ou personnalise-la !'",
            coachPromptEN: @"You are a business strategy consultant and business plan writer. Your goal is to help the entrepreneur unlock their thinking on marketing budget.

Available context (Previous answers):
- Products and Prices (ID 8)
- Growth objectives (ID 17)

Your mission:
Propose two distinct answer options. Each option should be written in first person.

Option 1: The ""Standard & Effective"" approach - A controlled budget, focused on word-of-mouth and organic digital presence.

Option 2: The ""Visionary & Differentiated"" approach - A more sustained investment to dominate your niche, including paid advertising and high-quality content.

Style constraint:
Stay simple, ensure suggested amounts are proportional to product prices. End with: 'Choose the option that suits you best or customize it!'"
        ));

        // Question 21: Team Salaries
        questions.Add(CreateDetailedQuestion(
            questionNumber: 21,
            stepNumber: 6,
            questionType: QuestionType.LongText,
            questionTextFR: "Détaillez les membres de votre équipe : Pour chaque employé (ou type de poste), quel sera le salaire mensuel brut prévu ?",
            questionTextEN: "Detail your team members: For each employee (or type of position), what will be the expected gross monthly salary?",
            helpTextFR: "Génère la masse salariale.",
            helpTextEN: "Generates payroll.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Une équipe bien structurée est le moteur de votre croissance. En distinguant les salaires par poste, vous démontrez une maîtrise fine de vos coûts d'exploitation et de votre modèle d'affaires.

Pour bien évaluer vos besoins :

Identifiez les rôles : Ne mélangez pas les fonctions. Un technicien n'a pas le même impact financier qu'un agent de service ou qu'un responsable des ventes.

Le coût réel : Indiquez le montant brut (avant impôts). N'oubliez pas qu'au Québec, les charges sociales de l'employeur ajoutent environ 15 % à ce montant.

Astuce : Ne craignez pas d'oublier quelqu'un. Vous pourrez ajouter, supprimer ou modifier chaque employé individuellement dans la section ""Prévisions financières"". Cela vous permettra aussi de tester différents scénarios, comme repousser une embauche de quelques mois pour protéger vos liquidités.",
            expertAdviceEN: @"Expert advice for a good answer:
A well-structured team is the engine of your growth. By distinguishing salaries by position, you demonstrate fine control of your operating costs and business model.

To properly evaluate your needs:

Identify roles: Don't mix functions. A technician doesn't have the same financial impact as a service agent or sales manager.

Real cost: Indicate the gross amount (before taxes). Don't forget that in Quebec, employer social charges add about 15% to this amount.

Tip: Don't be afraid of forgetting someone. You can add, remove or modify each employee individually in the ""Financial Forecasts"" section. This will also allow you to test different scenarios, like postponing a hire by a few months to protect your cash flow.",
            coachPromptFR: @"Tu es un consultant expert en ressources humaines et en planification financière. Ton objectif est d'aider l'entrepreneur à débloquer sa réflexion pour l'équipe et les salaires.

Contexte disponible (Réponses précédentes) :
- Direction et Partenaires (ID 10)
- Plan d'embauche et Évolution (ID 16)

Ta mission :
Propose deux options de réponse distinctes basées sur la vision de croissance fournie, tout en respectant l'équilibre de l'équipe de direction.

Option 1 : L'approche ""Agile & Lean"" - Une structure légère au départ. On mise sur la polyvalence des fondateurs et l'embauche de personnel de soutien à des salaires compétitifs mais prudents.

Option 2 : L'approche ""Structurée & Experte"" - Une structure complète dès le lancement pour garantir une exécution parfaite. On prévoit des salaires plus élevés pour attirer des talents spécialisés.

Contrainte de style :
Reste simple, liste les postes clairement avec les montants mensuels bruts associés. Termine par : 'Choisis l'option qui te correspond le mieux ou personnalise-la !'",
            coachPromptEN: @"You are a human resources and financial planning consultant. Your goal is to help the entrepreneur unlock their thinking on team and salaries.

Available context (Previous answers):
- Management and Partners (ID 10)
- Hiring plan and Evolution (ID 16)

Your mission:
Propose two distinct answer options based on the provided growth vision, while respecting the management team balance.

Option 1: The ""Agile & Lean"" approach - A light structure at the start. Focus on founders' versatility and hiring support staff at competitive but cautious salaries.

Option 2: The ""Structured & Expert"" approach - A complete structure from launch to guarantee perfect execution. Plan higher salaries to attract specialized talent.

Style constraint:
Stay simple, list positions clearly with associated gross monthly amounts. End with: 'Choose the option that suits you best or customize it!'"
        ));

        // Question 22: Premises Costs
        questions.Add(CreateDetailedQuestion(
            questionNumber: 22,
            stepNumber: 5,
            questionType: QuestionType.Currency,
            questionTextFR: "Quel est le coût mensuel total prévu pour l'occupation de votre local (loyer de base, taxes d'affaires et frais communs) ?",
            questionTextEN: "What is the total expected monthly cost for your premises (base rent, business taxes and common fees)?",
            helpTextFR: "Génère les frais de local.",
            helpTextEN: "Generates premises costs.",
            expertAdviceFR: @"Conseil d'expert pour bien répondre :
Le loyer est souvent l'un de vos frais fixes les plus importants. Pour un partenaire financier, la clarté sur ce montant est un indicateur de la solidité de votre planification immobilière.

Pour arriver à un chiffre réaliste, n'oubliez pas d'inclure :

Le type de bail : Assurez-vous de savoir s'il s'agit d'un bail ""net"" (loyer seul) ou ""brut"" (incluant les frais).

Les frais connexes : Pensez à l'électricité, au chauffage, aux assurances et aux taxes d'affaires qui s'ajoutent souvent au loyer de base.

Astuce : Si vous n'avez pas encore signé de bail, indiquez une estimation basée sur les prix du marché pour le secteur visé. Vous aurez la possibilité d'ajuster ce montant avec précision dans la section ""Prévisions financières"" une fois votre emplacement final choisi.",
            expertAdviceEN: @"Expert advice for a good answer:
Rent is often one of your most important fixed costs. For a financial partner, clarity on this amount is an indicator of the strength of your real estate planning.

To arrive at a realistic figure, don't forget to include:

Lease type: Make sure you know if it's a ""net"" lease (rent only) or ""gross"" (including fees).

Related costs: Think about electricity, heating, insurance and business taxes that often add to base rent.

Tip: If you haven't signed a lease yet, indicate an estimate based on market prices for the targeted area. You'll have the opportunity to adjust this amount precisely in the ""Financial Forecasts"" section once your final location is chosen.",
            coachPromptFR: @"Tu es un consultant expert en stratégie d'entreprise et en planification financière. Ton objectif est d'aider l'entrepreneur à débloquer sa réflexion pour le coût d'occupation.

Contexte disponible (Réponses précédentes) :
- Activité et Mission (ID 1 & 2)
- Besoins matériels et Localisation (ID 12)
- Équipe (ID 16)

Ta mission :
Propose deux options de réponse distinctes en fonction de la structure nécessaire pour l'activité détectée.

Option 1 : L'approche ""Optimisée & Flexible"" - Un budget réduit, privilégiant un espace de coworking, un bureau partagé ou un local de taille minimale.

Option 2 : L'approche ""Établie & Stratégique"" - Un budget pour un local commercial ou industriel dédié, offrant de l'espace pour le stockage, une équipe plus large ou une vitrine pour les clients.

Contrainte de style :
Reste simple, décompose l'estimation (loyer + frais) et assure-toi que le montant est réaliste pour le secteur d'activité et la taille de l'équipe. Termine par : 'Choisis l'option qui te correspond le mieux ou personnalise-la !'",
            coachPromptEN: @"You are a business strategy and financial planning consultant. Your goal is to help the entrepreneur unlock their thinking on occupancy costs.

Available context (Previous answers):
- Activity and Mission (ID 1 & 2)
- Material needs and Location (ID 12)
- Team (ID 16)

Your mission:
Propose two distinct answer options based on the structure needed for the detected activity.

Option 1: The ""Optimized & Flexible"" approach - A reduced budget, favoring a coworking space, shared office or minimal-size premises.

Option 2: The ""Established & Strategic"" approach - A budget for dedicated commercial or industrial premises, offering space for storage, a larger team or a storefront for clients.

Style constraint:
Stay simple, break down the estimate (rent + fees) and ensure the amount is realistic for the business sector and team size. End with: 'Choose the option that suits you best or customize it!'"
        ));

        return questions;
    }

    private QuestionTemplate CreateDetailedQuestion(
        int questionNumber,
        int stepNumber,
        QuestionType questionType,
        string questionTextFR,
        string questionTextEN,
        string? helpTextFR,
        string? helpTextEN,
        string? expertAdviceFR,
        string? expertAdviceEN,
        string? coachPromptFR,
        string? coachPromptEN,
        string? optionsFR = null,
        string? optionsEN = null)
    {
        return QuestionTemplate.Create(
            questionNumber: questionNumber,
            personaType: null,
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
            expertAdviceFR: expertAdviceFR,
            expertAdviceEN: expertAdviceEN,
            displayOrder: questionNumber,
            isRequired: questionNumber <= 10,
            icon: null
        );
    }
}
