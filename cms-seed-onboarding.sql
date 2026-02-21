-- =============================================================================
-- CMS Seed Data - Onboarding Sections
-- Adds content blocks for: onboarding.welcome, onboarding.steps, onboarding.completion
-- Version ID: 17a4a74e-4782-4ca0-9493-aebbd22dcc95 (must exist)
-- =============================================================================

-- ONBOARDING - Welcome Section (EN)
INSERT INTO "CmsContentBlocks" (
    "Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language",
    "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted"
)
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.welcome.title', 0, 'Welcome to Sqordia', 'en', 1, 'onboarding.welcome', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.welcome.subtitle', 0, 'Let''s get your business plan started', 'en', 2, 'onboarding.welcome', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.welcome.description', 0, 'We''ll guide you through a few quick steps to personalize your experience and help you create a professional business plan.', 'en', 3, 'onboarding.welcome', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.welcome.button', 0, 'Get Started', 'en', 4, 'onboarding.welcome', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET
    "Content" = EXCLUDED."Content",
    "LastModified" = NOW();

-- ONBOARDING - Welcome Section (FR)
INSERT INTO "CmsContentBlocks" (
    "Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language",
    "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted"
)
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.welcome.title', 0, 'Bienvenue sur Sqordia', 'fr', 1, 'onboarding.welcome', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.welcome.subtitle', 0, 'Commencez votre plan d''affaires', 'fr', 2, 'onboarding.welcome', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.welcome.description', 0, 'Nous vous guiderons a travers quelques etapes rapides pour personnaliser votre experience et vous aider a creer un plan d''affaires professionnel.', 'fr', 3, 'onboarding.welcome', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.welcome.button', 0, 'Commencer', 'fr', 4, 'onboarding.welcome', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET
    "Content" = EXCLUDED."Content",
    "LastModified" = NOW();

-- ONBOARDING - Steps Section (EN)
INSERT INTO "CmsContentBlocks" (
    "Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language",
    "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted"
)
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.title', 0, 'Tell Us About Yourself', 'en', 1, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.step1_title', 0, 'Choose Your Persona', 'en', 2, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.step1_description', 0, 'Are you an entrepreneur, consultant, or non-profit organization?', 'en', 3, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.step2_title', 0, 'Business Information', 'en', 4, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.step2_description', 0, 'Tell us about your business name and location', 'en', 5, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.next_button', 0, 'Next', 'en', 6, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.back_button', 0, 'Back', 'en', 7, 'onboarding.steps', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET
    "Content" = EXCLUDED."Content",
    "LastModified" = NOW();

-- ONBOARDING - Steps Section (FR)
INSERT INTO "CmsContentBlocks" (
    "Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language",
    "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted"
)
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.title', 0, 'Parlez-nous de vous', 'fr', 1, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.step1_title', 0, 'Choisissez votre persona', 'fr', 2, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.step1_description', 0, 'Etes-vous entrepreneur, consultant ou organisme a but non lucratif?', 'fr', 3, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.step2_title', 0, 'Informations sur l''entreprise', 'fr', 4, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.step2_description', 0, 'Parlez-nous du nom et de l''emplacement de votre entreprise', 'fr', 5, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.next_button', 0, 'Suivant', 'fr', 6, 'onboarding.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.steps.back_button', 0, 'Retour', 'fr', 7, 'onboarding.steps', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET
    "Content" = EXCLUDED."Content",
    "LastModified" = NOW();

-- ONBOARDING - Completion Section (EN)
INSERT INTO "CmsContentBlocks" (
    "Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language",
    "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted"
)
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.title', 0, 'You''re All Set!', 'en', 1, 'onboarding.completion', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.subtitle', 0, 'Your account is ready to go', 'en', 2, 'onboarding.completion', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.description', 0, 'You can now start creating your business plan. Our AI-powered tools will help you every step of the way.', 'en', 3, 'onboarding.completion', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.button', 0, 'Go to Dashboard', 'en', 4, 'onboarding.completion', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.tip_title', 0, 'Pro Tip', 'en', 5, 'onboarding.completion', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.tip_text', 0, 'Start with the questionnaire to quickly generate your first business plan draft.', 'en', 6, 'onboarding.completion', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET
    "Content" = EXCLUDED."Content",
    "LastModified" = NOW();

-- ONBOARDING - Completion Section (FR)
INSERT INTO "CmsContentBlocks" (
    "Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language",
    "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted"
)
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.title', 0, 'Vous etes pret!', 'fr', 1, 'onboarding.completion', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.subtitle', 0, 'Votre compte est pret', 'fr', 2, 'onboarding.completion', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.description', 0, 'Vous pouvez maintenant commencer a creer votre plan d''affaires. Nos outils alimentes par l''IA vous aideront a chaque etape.', 'fr', 3, 'onboarding.completion', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.button', 0, 'Aller au tableau de bord', 'fr', 4, 'onboarding.completion', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.tip_title', 0, 'Conseil Pro', 'fr', 5, 'onboarding.completion', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'onboarding.completion.tip_text', 0, 'Commencez par le questionnaire pour generer rapidement votre premier brouillon de plan d''affaires.', 'fr', 6, 'onboarding.completion', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET
    "Content" = EXCLUDED."Content",
    "LastModified" = NOW();
