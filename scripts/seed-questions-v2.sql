-- Seed QuestionTemplatesV2 data
-- Section 1: Identity & Vision
INSERT INTO "QuestionTemplatesV2" ("Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN", "QuestionType", "Order", "IsRequired", "Section", "Icon", "IsActive", "Created", "IsDeleted")
VALUES
(gen_random_uuid(), NULL, 1, 'Quel est le nom légal et la structure juridique (ex: Corporation)?', 'What is the legal name and business structure (e.g., Corporation)?', 'Indiquez le nom officiel de votre entreprise tel qu''enregistré légalement.', 'Enter the official name of your business as legally registered.', 'ShortText', 1, true, 'Identity & Vision', '⚖️', true, NOW(), false),
(gen_random_uuid(), NULL, 1, 'S''agit-il d''une nouvelle startup, d''une expansion ou d''une acquisition?', 'Is this a new startup, an expansion, or an acquisition?', 'Précisez la nature de votre projet d''affaires.', 'Specify the nature of your business project.', 'SingleChoice', 2, true, 'Identity & Vision', '🚀', true, NOW(), false),
(gen_random_uuid(), NULL, 1, 'Quel est votre énoncé de mission (pourquoi l''entreprise existe)?', 'What is your mission statement (why does the business exist)?', 'Décrivez la raison d''être de votre entreprise en une ou deux phrases.', 'Describe your company''s reason for being in one or two sentences.', 'LongText', 3, true, 'Identity & Vision', '🎯', true, NOW(), false),
(gen_random_uuid(), NULL, 1, 'Quels sont vos trois principaux objectifs pour les 12 premiers mois?', 'What are your top three objectives for the first 12 months?', 'Listez vos objectifs SMART pour la première année.', 'List your SMART objectives for the first year.', 'LongText', 4, true, 'Identity & Vision', '📍', true, NOW(), false);

-- Section 2: The Offering
INSERT INTO "QuestionTemplatesV2" ("Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN", "QuestionType", "Order", "IsRequired", "Section", "Icon", "IsActive", "Created", "IsDeleted")
VALUES
(gen_random_uuid(), NULL, 2, 'Quel problème ou lacune du marché résolvez-vous?', 'What specific market problem or gap are you solving?', 'Identifiez le problème précis que votre entreprise adresse.', 'Identify the specific problem your business addresses.', 'LongText', 1, true, 'The Offering', '🔍', true, NOW(), false),
(gen_random_uuid(), NULL, 2, 'Décrivez votre produit ou service en détail.', 'Describe your product or service in detail.', 'Expliquez ce que vous offrez, ses caractéristiques et avantages.', 'Explain what you offer, its features and benefits.', 'LongText', 2, true, 'The Offering', '📦', true, NOW(), false),
(gen_random_uuid(), NULL, 2, 'Quelle est votre Proposition de Valeur Unique (PVU)?', 'What is your Unique Selling Proposition (USP)?', 'Ce qui vous différencie de vos concurrents.', 'What differentiates you from your competitors.', 'LongText', 3, true, 'The Offering', '✨', true, NOW(), false),
(gen_random_uuid(), NULL, 2, 'Quel est votre modèle de revenus (comment facturez-vous)?', 'What is your revenue model (how do you charge)?', 'Expliquez comment vous générez des revenus.', 'Explain how you generate revenue.', 'LongText', 4, true, 'The Offering', '💸', true, NOW(), false);

-- Section 3: Market Analysis
INSERT INTO "QuestionTemplatesV2" ("Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN", "QuestionType", "Order", "IsRequired", "Section", "Icon", "IsActive", "Created", "IsDeleted")
VALUES
(gen_random_uuid(), NULL, 3, 'Quel est votre lieu principal d''activité (Ville et Province)?', 'What is your primary location (City and Province)?', 'Indiquez où sera basée votre entreprise.', 'Indicate where your business will be based.', 'ShortText', 1, true, 'Market Analysis', '🗺️', true, NOW(), false),
(gen_random_uuid(), NULL, 3, 'Qui est votre client idéal (données démographiques et intérêts)?', 'Who is your ideal customer (demographics and interests)?', 'Décrivez votre persona client cible.', 'Describe your target customer persona.', 'LongText', 2, true, 'Market Analysis', '👥', true, NOW(), false),
(gen_random_uuid(), NULL, 3, 'Qui sont vos trois principaux concurrents?', 'Who are your top three competitors?', 'Identifiez et décrivez vos concurrents directs.', 'Identify and describe your direct competitors.', 'LongText', 3, true, 'Market Analysis', '🏁', true, NOW(), false),
(gen_random_uuid(), NULL, 3, 'Quelle tendance majeure de l''industrie exploitez-vous?', 'What major industry trend are you capitalizing on?', 'Identifiez la tendance du marché qui favorise votre entreprise.', 'Identify the market trend that favors your business.', 'LongText', 4, true, 'Market Analysis', '📈', true, NOW(), false);

-- Section 4: Operations & People
INSERT INTO "QuestionTemplatesV2" ("Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN", "QuestionType", "Order", "IsRequired", "Section", "Icon", "IsActive", "Created", "IsDeleted")
VALUES
(gen_random_uuid(), NULL, 4, 'Quelle expérience pertinente l''équipe fondatrice apporte-t-elle?', 'What relevant experience does the founding team bring?', 'Décrivez les compétences et l''expérience de votre équipe.', 'Describe your team''s skills and experience.', 'LongText', 1, true, 'Operations & People', '🎓', true, NOW(), false),
(gen_random_uuid(), NULL, 4, 'Quel est votre plan de dotation pour l''année prochaine?', 'What is your staffing plan for the next year?', 'Indiquez vos besoins en personnel.', 'Indicate your staffing needs.', 'LongText', 2, true, 'Operations & People', '🤝', true, NOW(), false),
(gen_random_uuid(), NULL, 4, 'Quels équipements ou technologies essentiels sont nécessaires?', 'What essential equipment or technologies are needed?', 'Listez les ressources matérielles et technologiques requises.', 'List the required material and technological resources.', 'LongText', 3, true, 'Operations & People', '🛠️', true, NOW(), false),
(gen_random_uuid(), NULL, 4, 'Quels sont vos principaux partenaires ou fournisseurs?', 'Who are your key partners or suppliers?', 'Identifiez vos partenaires stratégiques.', 'Identify your strategic partners.', 'LongText', 4, true, 'Operations & People', '🤝', true, NOW(), false);

-- Section 5: Financial
INSERT INTO "QuestionTemplatesV2" ("Id", "PersonaType", "StepNumber", "QuestionText", "QuestionTextEN", "HelpText", "HelpTextEN", "QuestionType", "Order", "IsRequired", "Section", "Icon", "IsActive", "Created", "IsDeleted")
VALUES
(gen_random_uuid(), NULL, 5, 'Quel est votre investissement initial requis?', 'What is your required initial investment?', 'Indiquez le montant nécessaire pour démarrer.', 'Indicate the amount needed to start.', 'Currency', 1, true, 'Financial', '💰', true, NOW(), false),
(gen_random_uuid(), NULL, 5, 'Quelles sont vos projections de revenus pour les 3 premières années?', 'What are your revenue projections for the first 3 years?', 'Estimez vos revenus annuels prévus.', 'Estimate your expected annual revenues.', 'LongText', 2, true, 'Financial', '📊', true, NOW(), false),
(gen_random_uuid(), NULL, 5, 'Quels sont vos principaux coûts d''exploitation mensuels?', 'What are your main monthly operating costs?', 'Détaillez vos dépenses opérationnelles récurrentes.', 'Detail your recurring operational expenses.', 'LongText', 3, true, 'Financial', '📉', true, NOW(), false),
(gen_random_uuid(), NULL, 5, 'Quand prévoyez-vous atteindre le seuil de rentabilité?', 'When do you expect to reach break-even?', 'Estimez le mois où vos revenus couvriront vos dépenses.', 'Estimate the month when your revenues will cover your expenses.', 'ShortText', 4, true, 'Financial', '⚖️', true, NOW(), false);

-- Set options for single choice question
UPDATE "QuestionTemplatesV2"
SET "Options" = '["Nouvelle startup", "Expansion", "Acquisition"]'::jsonb,
    "OptionsEN" = '["New startup", "Expansion", "Acquisition"]'::jsonb
WHERE "QuestionText" LIKE '%nouvelle startup%expansion%acquisition%';

SELECT COUNT(*) as total_questions FROM "QuestionTemplatesV2";
