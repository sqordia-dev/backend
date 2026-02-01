-- ============================================================================
-- Sqordia Questionnaire Seed Script: StrategicPlan (OBNL)
-- ============================================================================
-- This script seeds the database with a questionnaire template for StrategicPlan
-- (OBNL / non-profit) plans. Content mirrors the BusinessPlan 20-question
-- structure with bilingual support, suitable for strategic/OBNL plans.
--
-- Idempotent: Safe to run multiple times (uses ON CONFLICT)
-- ============================================================================

BEGIN;

-- ============================================================================
-- 1. CREATE QUESTIONNAIRE TEMPLATE FOR STRATEGIC PLAN (OBNL)
-- ============================================================================
DO $$
DECLARE
    v_template_id UUID := 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid;
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
        'Plan stratégique OBNL - 20 questions / Strategic Plan OBNL - 20 Questions',
        'Questionnaire complet pour plans stratégiques et OBNL / Complete questionnaire for strategic and OBNL plans',
        'StrategicPlan',
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
DELETE FROM "QuestionTemplates" 
WHERE "QuestionnaireTemplateId" = 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid;

-- ============================================================================
-- 3. INSERT ALL 20 QUESTIONS (same structure as BusinessPlan, OBNL-oriented)
-- ============================================================================
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
    ('e5f6a7b8-c9d0-4e12-f234-567890123456'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Comment décririez-vous la mission principale de votre organisme ?', 'Ex. : Favoriser l''intégration sociale et économique des nouveaux arrivants.', 'How would you describe the main mission of your organization?', 'Ex.: Promote the social and economic integration of newcomers.', 'LongText', 1, true, 'Mission, vision, valeurs et contexte'),
    ('f6a7b8c9-d0e1-4f23-a345-678901234567'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quelle est votre vision à long terme pour l''organisation et l''impact que vous souhaitez avoir d''ici 3 à 5 ans ?', 'Ex. : Devenir un acteur clé de l''inclusion dans notre région.', 'What is your long-term vision for the organization and the impact you want to have within 3 to 5 years?', 'Ex.: Become a key player in inclusion in our region.', 'LongText', 2, true, 'Mission, vision, valeurs et contexte'),
    ('a7b8c9d0-e1f2-4a45-b456-789012345678'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quelles sont les valeurs fondamentales qui guident vos actions et décisions ?', 'Ex. : Inclusion, solidarité, innovation, durabilité.', 'What are the fundamental values that guide your actions and decisions?', 'Ex.: Inclusion, solidarity, innovation, sustainability.', 'LongText', 3, true, 'Mission, vision, valeurs et contexte'),
    ('b8c9d0e1-f2a3-4b56-c567-890123456789'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quel est le contexte ou les événements qui ont motivé la création de votre organisme et qui influencent aujourd''hui sa mission ?', 'Ex. : Répondre au manque de services d''accompagnement pour les familles vulnérables.', 'What is the context or events that motivated the creation of your organization and that influence its mission today?', 'Ex.: Respond to the lack of support services for vulnerable families.', 'LongText', 4, true, 'Mission, vision, valeurs et contexte'),
    ('c9d0e1f2-a3b4-4c67-d678-901234567890'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quels sont, selon vous, les principaux besoins, enjeux ou problématiques auxquels votre organisme souhaite répondre ?', 'Ex. : Isolement social, accès limité aux services, pauvreté, etc.', 'What are, in your opinion, the main needs, challenges, or problems that your organization wishes to address?', 'Ex.: Social isolation, limited access to services, poverty, etc.', 'LongText', 5, true, 'Analyse stratégique'),
    ('d0e1f2a3-b4c5-4d78-e789-012345678901'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quelles sont vos principales forces et atouts internes (compétences, expertise, partenaires, crédibilité, etc.) ?', 'Ex. : Équipe expérimentée, solide réseau communautaire, expertise sectorielle.', 'What are your main internal strengths and assets (skills, expertise, partners, credibility, etc.)?', 'Ex.: Experienced team, strong community network, sector expertise.', 'LongText', 6, true, 'Analyse stratégique'),
    ('e1f2a3b4-c5d6-4e89-f890-123456789012'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quelles sont vos principales faiblesses ou limites internes à améliorer dans les prochaines années ?', 'Ex. : Ressources financières limitées, manque de personnel, faible visibilité.', 'What are your main internal weaknesses or limitations to improve in the coming years?', 'Ex.: Limited financial resources, lack of personnel, low visibility.', 'LongText', 7, true, 'Analyse stratégique'),
    ('f2a3b4c5-d6e7-4f90-a901-234567890123'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quels changements dans votre environnement externe (social, politique, économique, technologique) représentent des opportunités ou des menaces pour votre mission ?', 'Ex. : Nouvelles politiques publiques favorables, concurrence accrue pour les subventions.', 'What changes in your external environment (social, political, economic, technological) represent opportunities or threats to your mission?', 'Ex.: Favorable new public policies, increased competition for grants.', 'LongText', 8, true, 'Analyse stratégique'),
    ('a3b4c5d6-e7f8-4a01-b012-345678901234'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Qui sont les bénéficiaires ou groupes cibles que vous servez (ou souhaitez servir) ? Décrivez-les.', 'Ex. : Jeunes en difficulté, personnes âgées, familles immigrantes, etc.', 'Who are the beneficiaries or target groups you serve (or wish to serve)? Describe them.', 'Ex.: At-risk youth, elderly people, immigrant families, etc.', 'LongText', 9, true, 'Bénéficiaires, besoins et impact'),
    ('b4c5d6e7-f8a9-4b12-c123-456789012345'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quels sont leurs besoins prioritaires que votre organisme s''engage à combler ?', 'Ex. : Soutien psychologique, intégration à l''emploi, accès à l''information, etc.', 'What are their priority needs that your organization commits to address?', 'Ex.: Psychological support, employment integration, access to information, etc.', 'LongText', 10, true, 'Bénéficiaires, besoins et impact'),
    ('c5d6e7f8-a9b0-4c23-d234-567890123456'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quel impact social concret souhaitez-vous générer sur ces bénéficiaires d''ici 3 à 5 ans ?', 'Ex. : Réduire l''isolement social de 30 %, augmenter le taux d''intégration à l''emploi.', 'What concrete social impact do you want to generate on these beneficiaries within 3 to 5 years?', 'Ex.: Reduce social isolation by 30%, increase employment integration rate.', 'LongText', 11, true, 'Bénéficiaires, besoins et impact'),
    ('d6e7f8a9-b0c1-4d34-e345-678901234567'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Comment comptez-vous mesurer et évaluer cet impact au fil du temps ?', 'Ex. : Indicateurs de participation, taux de satisfaction, nombre de bénéficiaires accompagnés.', 'How do you plan to measure and evaluate this impact over time?', 'Ex.: Participation indicators, satisfaction rates, number of beneficiaries served.', 'LongText', 12, true, 'Bénéficiaires, besoins et impact'),
    ('e7f8a9b0-c1d2-4e45-f456-789012345678'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quels sont les grands enjeux stratégiques ou priorités que votre organisme souhaite aborder dans les prochaines années ?', 'Ex. : Développer de nouveaux programmes, élargir la portée géographique, diversifier les revenus.', 'What are the major strategic challenges or priorities that your organization wishes to address in the coming years?', 'Ex.: Develop new programs, expand geographic reach, diversify revenue sources.', 'LongText', 13, true, 'Orientations, objectifs et plan d''action'),
    ('f8a9b0c1-d2e3-4f56-a567-890123456789'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quelles sont les grandes orientations stratégiques que vous voulez mettre en œuvre pour répondre à ces enjeux ?', 'Ex. : Renforcer les partenariats, investir dans le numérique, consolider l''équipe.', 'What are the major strategic directions you want to implement to address these challenges?', 'Ex.: Strengthen partnerships, invest in digital, consolidate the team.', 'LongText', 14, true, 'Orientations, objectifs et plan d''action'),
    ('a9b0c1d2-e3f4-4a67-b678-901234567890'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quels objectifs mesurables souhaitez-vous atteindre dans les 3 à 5 prochaines années pour chaque orientation ?', 'Ex. : Atteindre 1000 bénéficiaires par an, développer 3 nouveaux programmes, augmenter de 20 % les dons.', 'What measurable objectives do you want to achieve within the next 3 to 5 years for each direction?', 'Ex.: Reach 1,000 beneficiaries per year, develop 3 new programs, increase donations by 20%.', 'LongText', 15, true, 'Orientations, objectifs et plan d''action'),
    ('b0c1d2e3-f4a5-4b78-c789-012345678901'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quelles actions concrètes ou projets majeurs prévoyez-vous pour atteindre ces objectifs ?', 'Ex. : Créer un centre d''accueil, lancer un programme de mentorat, organiser des campagnes de financement.', 'What concrete actions or major projects do you plan to achieve these objectives?', 'Ex.: Create a welcome center, launch a mentoring program, organize fundraising campaigns.', 'LongText', 16, true, 'Orientations, objectifs et plan d''action'),
    ('c1d2e3f4-a5b6-4c89-d890-123456789012'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Comment votre organisme est-il structuré au niveau de la gouvernance (CA, direction, comités) et comment comptez-vous renforcer cette structure ?', 'Ex. : Recruter de nouveaux membres au CA, mettre en place un comité stratégique.', 'How is your organization structured at the governance level (board, management, committees) and how do you plan to strengthen this structure?', 'Ex.: Recruit new board members, establish a strategic committee.', 'LongText', 17, true, 'Gouvernance, financement et pérennité'),
    ('d2e3f4a5-b6c7-4d90-e901-234567890123'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'De quelles ressources humaines et matérielles aurez-vous besoin pour mettre en œuvre votre plan stratégique ?', 'Ex. : Embauche de personnel, formation, acquisition d''équipements, locaux supplémentaires.', 'What human and material resources will you need to implement your strategic plan?', 'Ex.: Hiring staff, training, equipment acquisition, additional premises.', 'LongText', 18, true, 'Gouvernance, financement et pérennité'),
    ('e3f4a5b6-c7d8-4e01-f012-345678901234'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quelles sont vos principales sources actuelles et prévues de financement, et comment comptez-vous assurer la pérennité financière de l''organisme ?', 'Ex. : Subventions publiques, dons privés, activités génératrices de revenus.', 'What are your main current and planned funding sources, and how do you plan to ensure the financial sustainability of the organization?', 'Ex.: Public grants, private donations, revenue-generating activities.', 'LongText', 19, true, 'Gouvernance, financement et pérennité'),
    ('f4a5b6c7-d8e9-4f12-a123-456789012345'::uuid, 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid, 'Quels sont les principaux risques que vous anticipez (financiers, organisationnels, politiques, etc.) et comment prévoyez-vous les atténuer ?', 'Ex. : Diversifier les sources de financement, créer un fonds de réserve, renforcer les partenariats.', 'What are the main risks you anticipate (financial, organizational, political, etc.) and how do you plan to mitigate them?', 'Ex.: Diversify funding sources, create a reserve fund, strengthen partnerships.', 'LongText', 20, true, 'Gouvernance, financement et pérennité');

-- ============================================================================
-- 4. VERIFICATION
-- ============================================================================
DO $$
DECLARE
    v_question_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO v_question_count
    FROM "QuestionTemplates"
    WHERE "QuestionnaireTemplateId" = 'd4e5f6a7-b8c9-4d01-e123-456789012345'::uuid;
    
    IF v_question_count = 20 THEN
        RAISE NOTICE 'SUCCESS: StrategicPlan questionnaire template and 20 questions seeded.';
    ELSE
        RAISE WARNING 'Expected 20 questions, but % were inserted.', v_question_count;
    END IF;
END $$;

COMMIT;

-- ============================================================================
-- END OF STRATEGIC PLAN QUESTIONNAIRE SEED
-- ============================================================================
-- Template ID: d4e5f6a7-b8c9-4d01-e123-456789012345
-- Plan Type: StrategicPlan
-- Questions: 20 (same sections as BusinessPlan, OBNL-oriented)
-- ============================================================================
