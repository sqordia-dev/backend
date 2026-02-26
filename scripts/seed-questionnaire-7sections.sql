-- ============================================================================
-- Sqordia Questionnaire Seed Script: 7-Section Business Plan Questionnaire
-- ============================================================================
-- This script seeds the QuestionTemplatesV2 and QuestionnaireSteps tables with
-- the 7-section questionnaire structure as per user journey document:
--   1. Business Information
--   2. Problem & Solution
--   3. Market
--   4. Competition
--   5. Revenue Model
--   6. Team
--   7. Financials
--
-- Idempotent: Safe to run multiple times (uses DELETE + INSERT)
-- ============================================================================

BEGIN;

-- ============================================================================
-- 1. CLEAR EXISTING V2 DATA
-- ============================================================================
DELETE FROM "QuestionTemplatesV2";
DELETE FROM "QuestionnaireSteps";

-- ============================================================================
-- 2. SEED QUESTIONNAIRE STEPS (7 sections)
-- ============================================================================
INSERT INTO "QuestionnaireSteps" (
    "Id", "StepNumber", "TitleFR", "TitleEN", "DescriptionFR", "DescriptionEN", "Icon", "IsActive", "Created", "IsDeleted"
)
VALUES
    (
        'a0000001-0000-0000-0000-000000000001'::uuid, 1,
        'Informations sur l''entreprise', 'Business Information',
        'Décrivez les informations de base de votre entreprise', 'Describe the basic information about your business',
        'Building2', true, NOW(), false
    ),
    (
        'a0000001-0000-0000-0000-000000000002'::uuid, 2,
        'Problème et Solution', 'Problem & Solution',
        'Identifiez le problème que vous résolvez et votre solution unique', 'Identify the problem you solve and your unique solution',
        'Lightbulb', true, NOW(), false
    ),
    (
        'a0000001-0000-0000-0000-000000000003'::uuid, 3,
        'Marché', 'Market',
        'Analysez votre marché cible et votre positionnement', 'Analyze your target market and positioning',
        'Target', true, NOW(), false
    ),
    (
        'a0000001-0000-0000-0000-000000000004'::uuid, 4,
        'Concurrence', 'Competition',
        'Évaluez vos concurrents et vos avantages compétitifs', 'Evaluate your competitors and competitive advantages',
        'Users', true, NOW(), false
    ),
    (
        'a0000001-0000-0000-0000-000000000005'::uuid, 5,
        'Modèle de revenus', 'Revenue Model',
        'Définissez comment votre entreprise génère des revenus', 'Define how your business generates revenue',
        'DollarSign', true, NOW(), false
    ),
    (
        'a0000001-0000-0000-0000-000000000006'::uuid, 6,
        'Équipe', 'Team',
        'Présentez votre équipe et vos besoins en recrutement', 'Present your team and hiring needs',
        'UserPlus', true, NOW(), false
    ),
    (
        'a0000001-0000-0000-0000-000000000007'::uuid, 7,
        'Finances', 'Financials',
        'Détaillez vos besoins financiers et projections', 'Detail your financial needs and projections',
        'Calculator', true, NOW(), false
    );

-- ============================================================================
-- 3. SEED QUESTIONS FOR EACH SECTION
-- ============================================================================

-- Section 1: Business Information (4 questions)
INSERT INTO "QuestionTemplatesV2" (
    "Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN",
    "QuestionType", "Order", "IsRequired", "Section", "IsActive", "Created", "IsDeleted"
)
VALUES
    (
        'b0000001-0001-0000-0000-000000000001'::uuid, NULL, 1,
        'Quel est le nom de votre entreprise ?',
        'What is your business name?',
        'Le nom officiel de votre entreprise tel qu''il apparaîtra sur les documents légaux.',
        'The official name of your business as it will appear on legal documents.',
        'ShortText', 1, true, 'Business Information', true, NOW(), false
    ),
    (
        'b0000001-0001-0000-0000-000000000002'::uuid, NULL, 1,
        'Décrivez votre entreprise en quelques phrases.',
        'Describe your business in a few sentences.',
        'Expliquez ce que fait votre entreprise, ses produits ou services principaux.',
        'Explain what your business does, its main products or services.',
        'LongText', 2, true, 'Business Information', true, NOW(), false
    ),
    (
        'b0000001-0001-0000-0000-000000000003'::uuid, NULL, 1,
        'Dans quelle industrie ou secteur opérez-vous ?',
        'What industry or sector do you operate in?',
        'Ex. : Technologie, Santé, Finance, Commerce de détail, Alimentation, Services.',
        'Ex.: Technology, Healthcare, Finance, Retail, Food, Services.',
        'ShortText', 3, true, 'Business Information', true, NOW(), false
    ),
    (
        'b0000001-0001-0000-0000-000000000004'::uuid, NULL, 1,
        'Quelle est la structure juridique de votre entreprise ?',
        'What is your business legal structure?',
        'Ex. : Inc., OBNL, Société à responsabilité limitée, Entreprise individuelle.',
        'Ex.: Inc., Non-profit, LLC, Sole Proprietorship.',
        'ShortText', 4, false, 'Business Information', true, NOW(), false
    );

-- Section 2: Problem & Solution (3 questions)
INSERT INTO "QuestionTemplatesV2" (
    "Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN",
    "QuestionType", "Order", "IsRequired", "Section", "IsActive", "Created", "IsDeleted"
)
VALUES
    (
        'b0000001-0002-0000-0000-000000000001'::uuid, NULL, 2,
        'Quel problème votre entreprise résout-elle ?',
        'What problem does your business solve?',
        'Décrivez le problème principal que vos clients rencontrent et que vous adressez.',
        'Describe the main problem your customers face that you address.',
        'LongText', 1, true, 'Problem & Solution', true, NOW(), false
    ),
    (
        'b0000001-0002-0000-0000-000000000002'::uuid, NULL, 2,
        'Comment votre produit ou service résout-il ce problème ?',
        'How does your product or service solve this problem?',
        'Expliquez votre solution et comment elle répond aux besoins de vos clients.',
        'Explain your solution and how it meets your customers'' needs.',
        'LongText', 2, true, 'Problem & Solution', true, NOW(), false
    ),
    (
        'b0000001-0002-0000-0000-000000000003'::uuid, NULL, 2,
        'Qu''est-ce qui rend votre solution unique ?',
        'What makes your solution unique?',
        'Décrivez votre proposition de valeur unique et ce qui vous différencie.',
        'Describe your unique value proposition and what sets you apart.',
        'LongText', 3, true, 'Problem & Solution', true, NOW(), false
    );

-- Section 3: Market (3 questions)
INSERT INTO "QuestionTemplatesV2" (
    "Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN",
    "QuestionType", "Order", "IsRequired", "Section", "IsActive", "Created", "IsDeleted"
)
VALUES
    (
        'b0000001-0003-0000-0000-000000000001'::uuid, NULL, 3,
        'Qui sont vos clients idéaux ? Décrivez leur profil démographique et leurs comportements.',
        'Who are your ideal customers? Describe their demographics and behaviors.',
        'Soyez précis : âge, revenu, localisation, habitudes d''achat, etc.',
        'Be specific: age, income, location, buying habits, etc.',
        'LongText', 1, true, 'Market', true, NOW(), false
    ),
    (
        'b0000001-0003-0000-0000-000000000002'::uuid, NULL, 3,
        'Quelle est la taille estimée de votre marché ?',
        'What is the estimated size of your market?',
        'Ex. : 2M$ dans la région de Montréal, 50 000 clients potentiels.',
        'Ex.: $2M in the Montreal area, 50,000 potential customers.',
        'ShortText', 2, true, 'Market', true, NOW(), false
    ),
    (
        'b0000001-0003-0000-0000-000000000003'::uuid, NULL, 3,
        'Où allez-vous opérer géographiquement ?',
        'Where will you operate geographically?',
        'Ex. : Local (Montréal), Provincial (Québec), National (Canada), International.',
        'Ex.: Local (Montreal), Provincial (Quebec), National (Canada), International.',
        'ShortText', 3, true, 'Market', true, NOW(), false
    );

-- Section 4: Competition (2 questions)
INSERT INTO "QuestionTemplatesV2" (
    "Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN",
    "QuestionType", "Order", "IsRequired", "Section", "IsActive", "Created", "IsDeleted"
)
VALUES
    (
        'b0000001-0004-0000-0000-000000000001'::uuid, NULL, 4,
        'Qui sont vos principaux concurrents ? Listez des noms spécifiques.',
        'Who are your main competitors? List specific names.',
        'Identifiez 3-5 concurrents directs et indirects avec leurs forces.',
        'Identify 3-5 direct and indirect competitors with their strengths.',
        'LongText', 1, true, 'Competition', true, NOW(), false
    ),
    (
        'b0000001-0004-0000-0000-000000000002'::uuid, NULL, 4,
        'Pourquoi les clients choisiront-ils votre entreprise plutôt que vos concurrents ?',
        'Why will customers choose your business over competitors?',
        'Expliquez vos avantages compétitifs : prix, qualité, service, innovation, etc.',
        'Explain your competitive advantages: price, quality, service, innovation, etc.',
        'LongText', 2, true, 'Competition', true, NOW(), false
    );

-- Section 5: Revenue Model (3 questions)
INSERT INTO "QuestionTemplatesV2" (
    "Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN",
    "QuestionType", "Order", "IsRequired", "Section", "IsActive", "Created", "IsDeleted"
)
VALUES
    (
        'b0000001-0005-0000-0000-000000000001'::uuid, NULL, 5,
        'Comment votre entreprise génère-t-elle des revenus ?',
        'How does your business generate revenue?',
        'Ex. : Ventes directes, abonnements, commissions, licences, publicité.',
        'Ex.: Direct sales, subscriptions, commissions, licensing, advertising.',
        'LongText', 1, true, 'Revenue Model', true, NOW(), false
    ),
    (
        'b0000001-0005-0000-0000-000000000002'::uuid, NULL, 5,
        'Quels sont vos prix ? Soyez précis.',
        'What are your prices? Be specific.',
        'Listez vos produits/services avec leurs prix et marges estimées.',
        'List your products/services with their prices and estimated margins.',
        'LongText', 2, true, 'Revenue Model', true, NOW(), false
    ),
    (
        'b0000001-0005-0000-0000-000000000003'::uuid, NULL, 5,
        'Comment vendez-vous vos produits ou services ?',
        'How do you sell your products or services?',
        'Ex. : En ligne, magasin physique, vente directe B2B, distributeurs.',
        'Ex.: Online, physical store, direct B2B sales, distributors.',
        'LongText', 3, true, 'Revenue Model', true, NOW(), false
    );

-- Section 6: Team (2 questions)
INSERT INTO "QuestionTemplatesV2" (
    "Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN",
    "QuestionType", "Order", "IsRequired", "Section", "IsActive", "Created", "IsDeleted"
)
VALUES
    (
        'b0000001-0006-0000-0000-000000000001'::uuid, NULL, 6,
        'Qui sont les fondateurs ? Décrivez leur parcours et leurs rôles.',
        'Who are the founders? Describe their background and roles.',
        'Incluez l''expérience pertinente, les compétences clés et les responsabilités.',
        'Include relevant experience, key skills, and responsibilities.',
        'LongText', 1, true, 'Team', true, NOW(), false
    ),
    (
        'b0000001-0006-0000-0000-000000000002'::uuid, NULL, 6,
        'Quels sont vos besoins en recrutement ? Quand prévoyez-vous embaucher ?',
        'What are your hiring needs? When do you plan to hire?',
        'Listez les postes à pourvoir, le calendrier et le budget prévu.',
        'List positions to fill, timeline, and planned budget.',
        'LongText', 2, true, 'Team', true, NOW(), false
    );

-- Section 7: Financials (4 questions)
INSERT INTO "QuestionTemplatesV2" (
    "Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN",
    "QuestionType", "Order", "IsRequired", "Section", "IsActive", "Created", "IsDeleted"
)
VALUES
    (
        'b0000001-0007-0000-0000-000000000001'::uuid, NULL, 7,
        'Quel est le montant total de l''investissement initial nécessaire ?',
        'What is the total initial investment needed?',
        'Incluez tous les coûts de démarrage : équipement, inventaire, marketing, etc.',
        'Include all startup costs: equipment, inventory, marketing, etc.',
        'ShortText', 1, true, 'Financials', true, NOW(), false
    ),
    (
        'b0000001-0007-0000-0000-000000000002'::uuid, NULL, 7,
        'Combien de financement recherchez-vous ?',
        'How much funding are you seeking?',
        'Montant total que vous souhaitez obtenir auprès d''investisseurs ou de prêteurs.',
        'Total amount you wish to obtain from investors or lenders.',
        'ShortText', 2, true, 'Financials', true, NOW(), false
    ),
    (
        'b0000001-0007-0000-0000-000000000003'::uuid, NULL, 7,
        'D''où proviendra le financement ?',
        'Where will the funding come from?',
        'Ex. : Économies personnelles, prêt bancaire, investisseurs providentiels, subventions.',
        'Ex.: Personal savings, bank loan, angel investors, grants.',
        'LongText', 3, true, 'Financials', true, NOW(), false
    ),
    (
        'b0000001-0007-0000-0000-000000000004'::uuid, NULL, 7,
        'Quels sont vos revenus projetés pour la première année ?',
        'What are your projected revenues for year one?',
        'Estimez vos revenus totaux attendus pour les 12 premiers mois d''opération.',
        'Estimate your total expected revenues for the first 12 months of operation.',
        'ShortText', 4, true, 'Financials', true, NOW(), false
    );

-- ============================================================================
-- 4. VERIFICATION
-- ============================================================================
DO $$
DECLARE
    v_step_count INTEGER;
    v_question_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO v_step_count FROM "QuestionnaireSteps";
    SELECT COUNT(*) INTO v_question_count FROM "QuestionTemplatesV2";

    IF v_step_count = 7 AND v_question_count = 21 THEN
        RAISE NOTICE '✓ SUCCESS: 7 steps and 21 questions have been successfully inserted!';
    ELSE
        RAISE WARNING 'Expected 7 steps and 21 questions, but got % steps and % questions.', v_step_count, v_question_count;
    END IF;
END $$;

COMMIT;

-- ============================================================================
-- END OF 7-SECTION QUESTIONNAIRE SEED SCRIPT
-- ============================================================================
-- Steps: 7 sections (Business Info, Problem & Solution, Market, Competition, Revenue Model, Team, Financials)
-- Questions: 21 total (4 + 3 + 3 + 2 + 3 + 2 + 4)
-- All questions include bilingual support (French and English)
-- ============================================================================
