-- Script to reset and re-seed all Questionnaire Templates and Questions
-- This script deletes all existing questionnaire data and re-inserts it
-- Use this when you need to reset the questionnaire data or update questions

BEGIN TRANSACTION;

PRINT 'Starting questionnaire reset and re-seeding...';
PRINT '';
PRINT 'WARNING: This will delete all questionnaire templates, questions, and responses!';
PRINT '';

-- Step 1: Delete all Questionnaire Responses (must be deleted first due to foreign key constraints)
DECLARE @DeletedResponsesCount INT;
DELETE FROM [QuestionnaireResponses];
SET @DeletedResponsesCount = @@ROWCOUNT;
PRINT 'Deleted ' + CAST(@DeletedResponsesCount AS NVARCHAR(10)) + ' questionnaire response(s).';

-- Step 2: Delete all Question Templates (must be deleted before templates due to foreign key constraint)
DECLARE @DeletedQuestionsCount INT;
DELETE FROM [QuestionTemplates];
SET @DeletedQuestionsCount = @@ROWCOUNT;
PRINT 'Deleted ' + CAST(@DeletedQuestionsCount AS NVARCHAR(10)) + ' question template(s).';

-- Step 3: Delete all Questionnaire Templates
DECLARE @DeletedTemplatesCount INT;
DELETE FROM [QuestionnaireTemplates];
SET @DeletedTemplatesCount = @@ROWCOUNT;
PRINT 'Deleted ' + CAST(@DeletedTemplatesCount AS NVARCHAR(10)) + ' questionnaire template(s).';
PRINT '';

-- Step 4: Re-seed the BusinessPlan questionnaire template with 20 questions
DECLARE @TemplateId UNIQUEIDENTIFIER = NEWID();
DECLARE @Now DATETIME2 = GETUTCDATE();

PRINT 'Creating new BusinessPlan questionnaire template...';

-- Insert QuestionnaireTemplate for BusinessPlan (Bilingual)
-- These 20 questions are common for all business plans. OBNL-specific questions will be added later.
INSERT INTO [QuestionnaireTemplates] (
    [Id], 
    [Name], 
    [Description], 
    [PlanType], 
    [IsActive], 
    [Version], 
    [Created], 
    [IsDeleted]
)
VALUES (
    @TemplateId,
    'Plan d''affaires - 20 questions communes / Business Plan - 20 Common Questions',
    'Questionnaire complet de 20 questions communes pour créer un plan d''affaires / Complete 20-question common questionnaire to create a business plan',
    'BusinessPlan',
    1, -- IsActive
    1, -- Version
    @Now,
    0  -- IsDeleted
);

PRINT 'Template created with ID: ' + CAST(@TemplateId AS NVARCHAR(36));
PRINT '';

-- Insert all 20 questions with bilingual support (French and English)
PRINT 'Inserting 20 bilingual questions...';

-- Section 1: Mission, vision, valeurs et contexte / Mission, vision, values and context (Questions 1-4)
INSERT INTO [QuestionTemplates] ([Id], [QuestionnaireTemplateId], [QuestionText], [HelpText], [QuestionTextEN], [HelpTextEN], [QuestionType], [Order], [IsRequired], [Section])
VALUES 
    (NEWID(), @TemplateId, 
     'Comment décririez-vous la mission principale de votre organisme ?', 
     'Ex. : Favoriser l''intégration sociale et économique des nouveaux arrivants.',
     'How would you describe the main mission of your organization?',
     'Ex.: Promote the social and economic integration of newcomers.',
     'LongText', 1, 1, 'Mission, vision, valeurs et contexte'),
    
    (NEWID(), @TemplateId, 
     'Quelle est votre vision à long terme pour l''organisation et l''impact que vous souhaitez avoir d''ici 3 à 5 ans ?', 
     'Ex. : Devenir un acteur clé de l''inclusion dans notre région.',
     'What is your long-term vision for the organization and the impact you want to have within 3 to 5 years?',
     'Ex.: Become a key player in inclusion in our region.',
     'LongText', 2, 1, 'Mission, vision, valeurs et contexte'),
    
    (NEWID(), @TemplateId, 
     'Quelles sont les valeurs fondamentales qui guident vos actions et décisions ?', 
     'Ex. : Inclusion, solidarité, innovation, durabilité.',
     'What are the fundamental values that guide your actions and decisions?',
     'Ex.: Inclusion, solidarity, innovation, sustainability.',
     'LongText', 3, 1, 'Mission, vision, valeurs et contexte'),
    
    (NEWID(), @TemplateId, 
     'Quel est le contexte ou les événements qui ont motivé la création de votre organisme et qui influencent aujourd''hui sa mission ?', 
     'Ex. : Répondre au manque de services d''accompagnement pour les familles vulnérables.',
     'What is the context or events that motivated the creation of your organization and that influence its mission today?',
     'Ex.: Respond to the lack of support services for vulnerable families.',
     'LongText', 4, 1, 'Mission, vision, valeurs et contexte');

-- Section 2: Analyse stratégique / Strategic Analysis (Questions 5-8)
DECLARE @Question5Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question6Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question7Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question8Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [QuestionTemplates] ([Id], [QuestionnaireTemplateId], [QuestionText], [HelpText], [QuestionTextEN], [HelpTextEN], [QuestionType], [Order], [IsRequired], [Section])
VALUES 
    (@Question5Id, @TemplateId, 
     'Quels sont, selon vous, les principaux besoins, enjeux ou problématiques auxquels votre organisme souhaite répondre ?', 
     'Ex. : Isolement social, accès limité aux services, pauvreté, etc.',
     'What are, in your opinion, the main needs, challenges, or problems that your organization wishes to address?',
     'Ex.: Social isolation, limited access to services, poverty, etc.',
     'LongText', 5, 1, 'Analyse stratégique'),
    
    (@Question6Id, @TemplateId, 
     'Quelles sont vos principales forces et atouts internes (compétences, expertise, partenaires, crédibilité, etc.) ?', 
     'Ex. : Équipe expérimentée, solide réseau communautaire, expertise sectorielle.',
     'What are your main internal strengths and assets (skills, expertise, partners, credibility, etc.)?',
     'Ex.: Experienced team, strong community network, sector expertise.',
     'LongText', 6, 1, 'Analyse stratégique'),
    
    (@Question7Id, @TemplateId, 
     'Quelles sont vos principales faiblesses ou limites internes à améliorer dans les prochaines années ?', 
     'Ex. : Ressources financières limitées, manque de personnel, faible visibilité.',
     'What are your main internal weaknesses or limitations to improve in the coming years?',
     'Ex.: Limited financial resources, lack of personnel, low visibility.',
     'LongText', 7, 1, 'Analyse stratégique'),
    
    (@Question8Id, @TemplateId, 
     'Quels changements dans votre environnement externe (social, politique, économique, technologique) représentent des opportunités ou des menaces pour votre mission ?', 
     'Ex. : Nouvelles politiques publiques favorables, concurrence accrue pour les subventions.',
     'What changes in your external environment (social, political, economic, technological) represent opportunities or threats to your mission?',
     'Ex.: Favorable new public policies, increased competition for grants.',
     'LongText', 8, 1, 'Analyse stratégique');

-- Section 3: Bénéficiaires, besoins et impact / Beneficiaries, needs and impact (Questions 9-12)
DECLARE @Question9Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question10Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question11Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question12Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [QuestionTemplates] ([Id], [QuestionnaireTemplateId], [QuestionText], [HelpText], [QuestionTextEN], [HelpTextEN], [QuestionType], [Order], [IsRequired], [Section])
VALUES 
    (@Question9Id, @TemplateId, 
     'Qui sont les bénéficiaires ou groupes cibles que vous servez (ou souhaitez servir) ? Décrivez-les.', 
     'Ex. : Jeunes en difficulté, personnes âgées, familles immigrantes, etc.',
     'Who are the beneficiaries or target groups you serve (or wish to serve)? Describe them.',
     'Ex.: At-risk youth, elderly people, immigrant families, etc.',
     'LongText', 9, 1, 'Bénéficiaires, besoins et impact'),
    
    (@Question10Id, @TemplateId, 
     'Quels sont leurs besoins prioritaires que votre organisme s''engage à combler ?', 
     'Ex. : Soutien psychologique, intégration à l''emploi, accès à l''information, etc.',
     'What are their priority needs that your organization commits to address?',
     'Ex.: Psychological support, employment integration, access to information, etc.',
     'LongText', 10, 1, 'Bénéficiaires, besoins et impact'),
    
    (@Question11Id, @TemplateId, 
     'Quel impact social concret souhaitez-vous générer sur ces bénéficiaires d''ici 3 à 5 ans ?', 
     'Ex. : Réduire l''isolement social de 30 %, augmenter le taux d''intégration à l''emploi.',
     'What concrete social impact do you want to generate on these beneficiaries within 3 to 5 years?',
     'Ex.: Reduce social isolation by 30%, increase employment integration rate.',
     'LongText', 11, 1, 'Bénéficiaires, besoins et impact'),
    
    (@Question12Id, @TemplateId, 
     'Comment comptez-vous mesurer et évaluer cet impact au fil du temps ?', 
     'Ex. : Indicateurs de participation, taux de satisfaction, nombre de bénéficiaires accompagnés.',
     'How do you plan to measure and evaluate this impact over time?',
     'Ex.: Participation indicators, satisfaction rates, number of beneficiaries served.',
     'LongText', 12, 1, 'Bénéficiaires, besoins et impact');

-- Section 4: Orientations, objectifs et plan d'action / Strategic directions, objectives and action plan (Questions 13-16)
DECLARE @Question13Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question14Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question15Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question16Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [QuestionTemplates] ([Id], [QuestionnaireTemplateId], [QuestionText], [HelpText], [QuestionTextEN], [HelpTextEN], [QuestionType], [Order], [IsRequired], [Section])
VALUES 
    (@Question13Id, @TemplateId, 
     'Quels sont les grands enjeux stratégiques ou priorités que votre organisme souhaite aborder dans les prochaines années ?', 
     'Ex. : Développer de nouveaux programmes, élargir la portée géographique, diversifier les revenus.',
     'What are the major strategic challenges or priorities that your organization wishes to address in the coming years?',
     'Ex.: Develop new programs, expand geographic reach, diversify revenue sources.',
     'LongText', 13, 1, 'Orientations, objectifs et plan d''action'),
    
    (@Question14Id, @TemplateId, 
     'Quelles sont les grandes orientations stratégiques que vous voulez mettre en œuvre pour répondre à ces enjeux ?', 
     'Ex. : Renforcer les partenariats, investir dans le numérique, consolider l''équipe.',
     'What are the major strategic directions you want to implement to address these challenges?',
     'Ex.: Strengthen partnerships, invest in digital, consolidate the team.',
     'LongText', 14, 1, 'Orientations, objectifs et plan d''action'),
    
    (@Question15Id, @TemplateId, 
     'Quels objectifs mesurables souhaitez-vous atteindre dans les 3 à 5 prochaines années pour chaque orientation ?', 
     'Ex. : Atteindre 1000 bénéficiaires par an, développer 3 nouveaux programmes, augmenter de 20 % les dons.',
     'What measurable objectives do you want to achieve within the next 3 to 5 years for each direction?',
     'Ex.: Reach 1,000 beneficiaries per year, develop 3 new programs, increase donations by 20%.',
     'LongText', 15, 1, 'Orientations, objectifs et plan d''action'),
    
    (@Question16Id, @TemplateId, 
     'Quelles actions concrètes ou projets majeurs prévoyez-vous pour atteindre ces objectifs ?', 
     'Ex. : Créer un centre d''accueil, lancer un programme de mentorat, organiser des campagnes de financement.',
     'What concrete actions or major projects do you plan to achieve these objectives?',
     'Ex.: Create a welcome center, launch a mentoring program, organize fundraising campaigns.',
     'LongText', 16, 1, 'Orientations, objectifs et plan d''action');

-- Section 5: Gouvernance, financement et pérennité / Governance, funding and sustainability (Questions 17-20)
DECLARE @Question17Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question18Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question19Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Question20Id UNIQUEIDENTIFIER = NEWID();

INSERT INTO [QuestionTemplates] ([Id], [QuestionnaireTemplateId], [QuestionText], [HelpText], [QuestionTextEN], [HelpTextEN], [QuestionType], [Order], [IsRequired], [Section])
VALUES 
    (@Question17Id, @TemplateId, 
     'Comment votre organisme est-il structuré au niveau de la gouvernance (CA, direction, comités) et comment comptez-vous renforcer cette structure ?', 
     'Ex. : Recruter de nouveaux membres au CA, mettre en place un comité stratégique.',
     'How is your organization structured at the governance level (board, management, committees) and how do you plan to strengthen this structure?',
     'Ex.: Recruit new board members, establish a strategic committee.',
     'LongText', 17, 1, 'Gouvernance, financement et pérennité'),
    
    (@Question18Id, @TemplateId, 
     'De quelles ressources humaines et matérielles aurez-vous besoin pour mettre en œuvre votre plan stratégique ?', 
     'Ex. : Embauche de personnel, formation, acquisition d''équipements, locaux supplémentaires.',
     'What human and material resources will you need to implement your strategic plan?',
     'Ex.: Hiring staff, training, equipment acquisition, additional premises.',
     'LongText', 18, 1, 'Gouvernance, financement et pérennité'),
    
    (@Question19Id, @TemplateId, 
     'Quelles sont vos principales sources actuelles et prévues de financement, et comment comptez-vous assurer la pérennité financière de l''organisme ?', 
     'Ex. : Subventions publiques, dons privés, activités génératrices de revenus.',
     'What are your main current and planned funding sources, and how do you plan to ensure the financial sustainability of the organization?',
     'Ex.: Public grants, private donations, revenue-generating activities.',
     'LongText', 19, 1, 'Gouvernance, financement et pérennité'),
    
    (@Question20Id, @TemplateId, 
     'Quels sont les principaux risques que vous anticipez (financiers, organisationnels, politiques, etc.) et comment prévoyez-vous les atténuer ?', 
     'Ex. : Diversifier les sources de financement, créer un fonds de réserve, renforcer les partenariats.',
     'What are the main risks you anticipate (financial, organizational, political, etc.) and how do you plan to mitigate them?',
     'Ex.: Diversify funding sources, create a reserve fund, strengthen partnerships.',
     'LongText', 20, 1, 'Gouvernance, financement et pérennité');

-- Verify the insertion
DECLARE @InsertedQuestionsCount INT;
SELECT @InsertedQuestionsCount = COUNT(*) FROM [QuestionTemplates] WHERE [QuestionnaireTemplateId] = @TemplateId;

PRINT '';
PRINT '========================================';
PRINT 'Questionnaire Reset and Re-seeding Complete!';
PRINT '========================================';
PRINT 'Template ID: ' + CAST(@TemplateId AS NVARCHAR(36));
PRINT 'Plan Type: BusinessPlan (common questions for all business plans)';
PRINT 'Questions Inserted: ' + CAST(@InsertedQuestionsCount AS NVARCHAR(10)) + ' / 20';
PRINT '';
PRINT 'All questions include both French (QuestionText/HelpText) and English (QuestionTextEN/HelpTextEN) translations.';
PRINT 'Note: OBNL-specific questions will be added later for StrategicPlan type.';
PRINT '';

-- Check if all questions were inserted correctly
IF @InsertedQuestionsCount = 20
BEGIN
    PRINT '✓ SUCCESS: All 20 questions have been successfully inserted!';
    COMMIT TRANSACTION;
END
ELSE
BEGIN
    PRINT '✗ ERROR: Expected 20 questions, but only ' + CAST(@InsertedQuestionsCount AS NVARCHAR(10)) + ' were inserted.';
    PRINT 'Rolling back transaction...';
    ROLLBACK TRANSACTION;
    PRINT 'Transaction rolled back. Please check the error and try again.';
END

