-- ============================================================================
-- Sqordia Questionnaire Seed Script (PostgreSQL)
-- ============================================================================
-- This script seeds the database with questionnaire templates and questions:
--   - BusinessPlan questionnaire template (20 common questions)
--   - All questions include bilingual support (French and English)
--   - Questions organized into 5 sections
--
-- Idempotent: Safe to run multiple times (uses ON CONFLICT)
-- ============================================================================

BEGIN;

-- ============================================================================
-- 1. CREATE QUESTIONNAIRE TEMPLATE FOR BUSINESS PLAN
-- ============================================================================
-- Fixed UUID for the BusinessPlan questionnaire template
-- This allows the script to be idempotent
DO $$
DECLARE
    v_template_id UUID := 'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid;
    v_now TIMESTAMP WITH TIME ZONE := NOW() AT TIME ZONE 'UTC';
BEGIN
    INSERT INTO "QuestionnaireTemplates" (
        "Id", 
        "Name", 
        "Description", 
        "PlanType", 
        "IsActive", 
        "Version", 
        "Created",
        "IsDeleted"
    )
    VALUES (
        v_template_id,
        'Plan d''affaires - 20 questions communes / Business Plan - 20 Common Questions',
        'Questionnaire complet de 20 questions communes pour créer un plan d''affaires / Complete 20-question common questionnaire to create a business plan',
        'BusinessPlan',
        true,
        1,
        v_now,
        false
    )
    ON CONFLICT ("Id") DO UPDATE SET
        "Name" = EXCLUDED."Name",
        "Description" = EXCLUDED."Description",
        "PlanType" = EXCLUDED."PlanType",
        "IsActive" = EXCLUDED."IsActive",
        "Version" = EXCLUDED."Version",
        "LastModified" = v_now;
END $$;

-- ============================================================================
-- 2. DELETE EXISTING QUESTIONS FOR THIS TEMPLATE (to allow re-seeding)
-- ============================================================================
-- This ensures clean re-seeding if the script is run multiple times
DELETE FROM "QuestionTemplates" 
WHERE "QuestionnaireTemplateId" = 'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid;

-- ============================================================================
-- 3. INSERT ALL 20 QUESTIONS
-- ============================================================================
-- Section 1: Mission, vision, valeurs et contexte / Mission, vision, values and context (Questions 1-4)
INSERT INTO "QuestionTemplates" (
    "Id", 
    "QuestionnaireTemplateId", 
    "QuestionText", 
    "HelpText", 
    "QuestionTextEN", 
    "HelpTextEN", 
    "QuestionType", 
    "Order", 
    "IsRequired", 
    "Section"
)
VALUES 
    -- Question 1
    (
        'b1c2d3e4-f5a6-4789-b012-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Comment décririez-vous la mission principale de votre organisme ?',
        'Ex. : Favoriser l''intégration sociale et économique des nouveaux arrivants.',
        'How would you describe the main mission of your organization?',
        'Ex.: Promote the social and economic integration of newcomers.',
        'LongText',
        1,
        true,
        'Mission, vision, valeurs et contexte'
    ),
    
    -- Question 2
    (
        'c2d3e4f5-a6b7-4789-c012-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quelle est votre vision à long terme pour l''organisation et l''impact que vous souhaitez avoir d''ici 3 à 5 ans ?',
        'Ex. : Devenir un acteur clé de l''inclusion dans notre région.',
        'What is your long-term vision for the organization and the impact you want to have within 3 to 5 years?',
        'Ex.: Become a key player in inclusion in our region.',
        'LongText',
        2,
        true,
        'Mission, vision, valeurs et contexte'
    ),
    
    -- Question 3
    (
        'd3e4f5a6-b7c8-4789-d012-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quelles sont les valeurs fondamentales qui guident vos actions et décisions ?',
        'Ex. : Inclusion, solidarité, innovation, durabilité.',
        'What are the fundamental values that guide your actions and decisions?',
        'Ex.: Inclusion, solidarity, innovation, sustainability.',
        'LongText',
        3,
        true,
        'Mission, vision, valeurs et contexte'
    ),
    
    -- Question 4
    (
        'e4f5a6b7-c8d9-4789-e012-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quel est le contexte ou les événements qui ont motivé la création de votre organisme et qui influencent aujourd''hui sa mission ?',
        'Ex. : Répondre au manque de services d''accompagnement pour les familles vulnérables.',
        'What is the context or events that motivated the creation of your organization and that influence its mission today?',
        'Ex.: Respond to the lack of support services for vulnerable families.',
        'LongText',
        4,
        true,
        'Mission, vision, valeurs et contexte'
    ),

-- Section 2: Analyse stratégique / Strategic Analysis (Questions 5-8)
    -- Question 5
    (
        'f5a6b7c8-d9e0-4789-f012-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quels sont, selon vous, les principaux besoins, enjeux ou problématiques auxquels votre organisme souhaite répondre ?',
        'Ex. : Isolement social, accès limité aux services, pauvreté, etc.',
        'What are, in your opinion, the main needs, challenges, or problems that your organization wishes to address?',
        'Ex.: Social isolation, limited access to services, poverty, etc.',
        'LongText',
        5,
        true,
        'Analyse stratégique'
    ),
    
    -- Question 6
    (
        'a6b7c8d9-e0f1-4789-a112-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quelles sont vos principales forces et atouts internes (compétences, expertise, partenaires, crédibilité, etc.) ?',
        'Ex. : Équipe expérimentée, solide réseau communautaire, expertise sectorielle.',
        'What are your main internal strengths and assets (skills, expertise, partners, credibility, etc.)?',
        'Ex.: Experienced team, strong community network, sector expertise.',
        'LongText',
        6,
        true,
        'Analyse stratégique'
    ),
    
    -- Question 7
    (
        'b7c8d9e0-f1a2-4789-b112-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quelles sont vos principales faiblesses ou limites internes à améliorer dans les prochaines années ?',
        'Ex. : Ressources financières limitées, manque de personnel, faible visibilité.',
        'What are your main internal weaknesses or limitations to improve in the coming years?',
        'Ex.: Limited financial resources, lack of personnel, low visibility.',
        'LongText',
        7,
        true,
        'Analyse stratégique'
    ),
    
    -- Question 8
    (
        'c8d9e0f1-a2b3-4789-c112-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quels changements dans votre environnement externe (social, politique, économique, technologique) représentent des opportunités ou des menaces pour votre mission ?',
        'Ex. : Nouvelles politiques publiques favorables, concurrence accrue pour les subventions.',
        'What changes in your external environment (social, political, economic, technological) represent opportunities or threats to your mission?',
        'Ex.: Favorable new public policies, increased competition for grants.',
        'LongText',
        8,
        true,
        'Analyse stratégique'
    ),

-- Section 3: Bénéficiaires, besoins et impact / Beneficiaries, needs and impact (Questions 9-12)
    -- Question 9
    (
        'd9e0f1a2-b3c4-4789-d112-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Qui sont les bénéficiaires ou groupes cibles que vous servez (ou souhaitez servir) ? Décrivez-les.',
        'Ex. : Jeunes en difficulté, personnes âgées, familles immigrantes, etc.',
        'Who are the beneficiaries or target groups you serve (or wish to serve)? Describe them.',
        'Ex.: At-risk youth, elderly people, immigrant families, etc.',
        'LongText',
        9,
        true,
        'Bénéficiaires, besoins et impact'
    ),
    
    -- Question 10
    (
        'e0f1a2b3-c4d5-4789-e112-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quels sont leurs besoins prioritaires que votre organisme s''engage à combler ?',
        'Ex. : Soutien psychologique, intégration à l''emploi, accès à l''information, etc.',
        'What are their priority needs that your organization commits to address?',
        'Ex.: Psychological support, employment integration, access to information, etc.',
        'LongText',
        10,
        true,
        'Bénéficiaires, besoins et impact'
    ),
    
    -- Question 11
    (
        'f1a2b3c4-d5e6-4789-f112-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quel impact social concret souhaitez-vous générer sur ces bénéficiaires d''ici 3 à 5 ans ?',
        'Ex. : Réduire l''isolement social de 30 %, augmenter le taux d''intégration à l''emploi.',
        'What concrete social impact do you want to generate on these beneficiaries within 3 to 5 years?',
        'Ex.: Reduce social isolation by 30%, increase employment integration rate.',
        'LongText',
        11,
        true,
        'Bénéficiaires, besoins et impact'
    ),
    
    -- Question 12
    (
        'a2b3c4d5-e6f7-4789-a212-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Comment comptez-vous mesurer et évaluer cet impact au fil du temps ?',
        'Ex. : Indicateurs de participation, taux de satisfaction, nombre de bénéficiaires accompagnés.',
        'How do you plan to measure and evaluate this impact over time?',
        'Ex.: Participation indicators, satisfaction rates, number of beneficiaries served.',
        'LongText',
        12,
        true,
        'Bénéficiaires, besoins et impact'
    ),

-- Section 4: Orientations, objectifs et plan d'action / Strategic directions, objectives and action plan (Questions 13-16)
    -- Question 13
    (
        'b3c4d5e6-f7a8-4789-b212-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quels sont les grands enjeux stratégiques ou priorités que votre organisme souhaite aborder dans les prochaines années ?',
        'Ex. : Développer de nouveaux programmes, élargir la portée géographique, diversifier les revenus.',
        'What are the major strategic challenges or priorities that your organization wishes to address in the coming years?',
        'Ex.: Develop new programs, expand geographic reach, diversify revenue sources.',
        'LongText',
        13,
        true,
        'Orientations, objectifs et plan d''action'
    ),
    
    -- Question 14
    (
        'c4d5e6f7-a8b9-4789-c212-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quelles sont les grandes orientations stratégiques que vous voulez mettre en œuvre pour répondre à ces enjeux ?',
        'Ex. : Renforcer les partenariats, investir dans le numérique, consolider l''équipe.',
        'What are the major strategic directions you want to implement to address these challenges?',
        'Ex.: Strengthen partnerships, invest in digital, consolidate the team.',
        'LongText',
        14,
        true,
        'Orientations, objectifs et plan d''action'
    ),
    
    -- Question 15
    (
        'd5e6f7a8-b9c0-4789-d212-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quels objectifs mesurables souhaitez-vous atteindre dans les 3 à 5 prochaines années pour chaque orientation ?',
        'Ex. : Atteindre 1000 bénéficiaires par an, développer 3 nouveaux programmes, augmenter de 20 % les dons.',
        'What measurable objectives do you want to achieve within the next 3 to 5 years for each direction?',
        'Ex.: Reach 1,000 beneficiaries per year, develop 3 new programs, increase donations by 20%.',
        'LongText',
        15,
        true,
        'Orientations, objectifs et plan d''action'
    ),
    
    -- Question 16
    (
        'e6f7a8b9-c0d1-4789-e212-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quelles actions concrètes ou projets majeurs prévoyez-vous pour atteindre ces objectifs ?',
        'Ex. : Créer un centre d''accueil, lancer un programme de mentorat, organiser des campagnes de financement.',
        'What concrete actions or major projects do you plan to achieve these objectives?',
        'Ex.: Create a welcome center, launch a mentoring program, organize fundraising campaigns.',
        'LongText',
        16,
        true,
        'Orientations, objectifs et plan d''action'
    ),

-- Section 5: Gouvernance, financement et pérennité / Governance, funding and sustainability (Questions 17-20)
    -- Question 17
    (
        'f7a8b9c0-d1e2-4789-f212-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Comment votre organisme est-il structuré au niveau de la gouvernance (CA, direction, comités) et comment comptez-vous renforcer cette structure ?',
        'Ex. : Recruter de nouveaux membres au CA, mettre en place un comité stratégique.',
        'How is your organization structured at the governance level (board, management, committees) and how do you plan to strengthen this structure?',
        'Ex.: Recruit new board members, establish a strategic committee.',
        'LongText',
        17,
        true,
        'Gouvernance, financement et pérennité'
    ),
    
    -- Question 18
    (
        'a8b9c0d1-e2f3-4789-a312-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'De quelles ressources humaines et matérielles aurez-vous besoin pour mettre en œuvre votre plan stratégique ?',
        'Ex. : Embauche de personnel, formation, acquisition d''équipements, locaux supplémentaires.',
        'What human and material resources will you need to implement your strategic plan?',
        'Ex.: Hiring staff, training, equipment acquisition, additional premises.',
        'LongText',
        18,
        true,
        'Gouvernance, financement et pérennité'
    ),
    
    -- Question 19
    (
        'b9c0d1e2-f3a4-4789-b312-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quelles sont vos principales sources actuelles et prévues de financement, et comment comptez-vous assurer la pérennité financière de l''organisme ?',
        'Ex. : Subventions publiques, dons privés, activités génératrices de revenus.',
        'What are your main current and planned funding sources, and how do you plan to ensure the financial sustainability of the organization?',
        'Ex.: Public grants, private donations, revenue-generating activities.',
        'LongText',
        19,
        true,
        'Gouvernance, financement et pérennité'
    ),
    
    -- Question 20
    (
        'c0d1e2f3-a4b5-4789-c312-345678901234'::uuid,
        'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid,
        'Quels sont les principaux risques que vous anticipez (financiers, organisationnels, politiques, etc.) et comment prévoyez-vous les atténuer ?',
        'Ex. : Diversifier les sources de financement, créer un fonds de réserve, renforcer les partenariats.',
        'What are the main risks you anticipate (financial, organizational, political, etc.) and how do you plan to mitigate them?',
        'Ex.: Diversify funding sources, create a reserve fund, strengthen partnerships.',
        'LongText',
        20,
        true,
        'Gouvernance, financement et pérennité'
    );

-- ============================================================================
-- 4. VERIFICATION
-- ============================================================================
DO $$
DECLARE
    v_question_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO v_question_count
    FROM "QuestionTemplates"
    WHERE "QuestionnaireTemplateId" = 'a1b2c3d4-e5f6-4789-a012-345678901234'::uuid;
    
    IF v_question_count = 20 THEN
        RAISE NOTICE '✓ SUCCESS: All 20 questions have been successfully inserted!';
    ELSE
        RAISE WARNING 'Expected 20 questions, but % were inserted.', v_question_count;
    END IF;
END $$;

COMMIT;

-- ============================================================================
-- END OF QUESTIONNAIRE SEED SCRIPT
-- ============================================================================
-- Template ID: a1b2c3d4-e5f6-4789-a012-345678901234
-- Plan Type: BusinessPlan
-- Questions: 20 questions organized into 5 sections
-- All questions include bilingual support (French and English)
-- ============================================================================

