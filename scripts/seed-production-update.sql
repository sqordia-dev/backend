-- =============================================================================
-- Production Seed Update
-- Adds: Steps 6-7, CMS Pages/Sections, Updates questionnaire snapshots
-- Idempotent: Safe to run multiple times
-- =============================================================================

BEGIN;

-- =============================================================================
-- 1. Add missing QuestionnaireSteps 6 and 7
-- =============================================================================
INSERT INTO "QuestionnaireSteps" ("Id", "StepNumber", "TitleFR", "TitleEN", "DescriptionFR", "DescriptionEN", "Icon", "IsActive", "Created", "IsDeleted")
SELECT gen_random_uuid(), 6, 'Équipe', 'Team',
    'Votre équipe et vos ressources humaines', 'Your team and human resources',
    'Users', true, NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "QuestionnaireSteps" WHERE "StepNumber" = 6 AND "IsDeleted" = false);

INSERT INTO "QuestionnaireSteps" ("Id", "StepNumber", "TitleFR", "TitleEN", "DescriptionFR", "DescriptionEN", "Icon", "IsActive", "Created", "IsDeleted")
SELECT gen_random_uuid(), 7, 'Finances', 'Finances',
    'Détails financiers', 'Financial details',
    'Calculator', true, NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "QuestionnaireSteps" WHERE "StepNumber" = 7 AND "IsDeleted" = false);

-- =============================================================================
-- 2. Seed CMS Pages (idempotent via fixed IDs)
-- =============================================================================
INSERT INTO "CmsPages" ("Id", "Key", "Label", "Description", "SortOrder", "IsActive", "IconName", "SpecialRenderer", "Created", "IsDeleted")
VALUES
  ('a0000001-0000-0000-0000-000000000001', 'landing',            'Landing Page',         NULL, 0,  true, 'Globe',           NULL,                 NOW(), false),
  ('a0000001-0000-0000-0000-000000000002', 'dashboard',          'Dashboard',            NULL, 1,  true, 'LayoutDashboard', NULL,                 NOW(), false),
  ('a0000001-0000-0000-0000-000000000003', 'profile',            'Profile',              NULL, 2,  true, 'UserCircle',      NULL,                 NOW(), false),
  ('a0000001-0000-0000-0000-000000000004', 'question_templates', 'Questions',            NULL, 3,  true, 'HelpCircle',      'question-templates', NOW(), false),
  ('a0000001-0000-0000-0000-000000000005', 'questionnaire',      'Questionnaire Wizard', NULL, 4,  true, 'ClipboardList',   NULL,                 NOW(), false),
  ('a0000001-0000-0000-0000-000000000006', 'create_plan',        'Create Plan',          NULL, 5,  true, 'PenLine',         NULL,                 NOW(), false),
  ('a0000001-0000-0000-0000-000000000007', 'subscription',       'Subscription Plans',   NULL, 6,  true, 'CreditCard',      NULL,                 NOW(), false),
  ('a0000001-0000-0000-0000-000000000008', 'onboarding',         'Onboarding',           NULL, 7,  true, 'Rocket',          NULL,                 NOW(), false),
  ('a0000001-0000-0000-0000-000000000009', 'auth',               'Authentication',       NULL, 8,  true, 'LogIn',           NULL,                 NOW(), false),
  ('a0000001-0000-0000-0000-000000000010', 'legal',              'Legal Pages',          NULL, 9,  true, 'Scale',           NULL,                 NOW(), false),
  ('a0000001-0000-0000-0000-000000000011', 'global',             'Global / Shared',      NULL, 10, true, 'Globe',           NULL,                 NOW(), false)
ON CONFLICT ("Id") DO NOTHING;

-- =============================================================================
-- 3. Seed CMS Sections
-- =============================================================================

-- Landing
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'landing.hero',         'Hero',         0, true, 'Type',          NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'landing.hero');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'landing.features',     'Features',     1, true, 'LayoutGrid',    NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'landing.features');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'landing.faq',          'FAQ',          2, true, 'HelpCircle',    NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'landing.faq');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'landing.testimonials', 'Testimonials', 3, true, 'MessageSquare', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'landing.testimonials');

-- Dashboard
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'dashboard.labels',      'Labels & Titles', 0, true, 'Type',      NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'dashboard.labels');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'dashboard.empty_states', 'Empty States',    1, true, 'FileText',  NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'dashboard.empty_states');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'dashboard.tips',         'Tips & Tour',     2, true, 'Lightbulb', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'dashboard.tips');

-- Profile
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'profile.labels',   'Labels & Titles', 0, true, 'Type',        NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'profile.labels');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'profile.security', 'Security',        1, true, 'ShieldCheck', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'profile.security');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'profile.sessions', 'Sessions',        2, true, 'Monitor',     NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'profile.sessions');

-- Question Templates
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'question_templates.step1', 'Step 1: Vision & Mission',      0, true, 'Target', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'question_templates.step1');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'question_templates.step2', 'Step 2: Market & Customers',    1, true, 'Target', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'question_templates.step2');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'question_templates.step3', 'Step 3: Products & Services',   2, true, 'Target', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'question_templates.step3');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'question_templates.step4', 'Step 4: Strategy & Operations', 3, true, 'Target', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'question_templates.step4');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'question_templates.step5', 'Step 5: Financials & Growth',   4, true, 'Target', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'question_templates.step5');

-- Questionnaire Wizard
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'questionnaire.steps',  'Step Configuration', 0, true, 'Layers',    NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'questionnaire.steps');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'questionnaire.labels', 'Labels & Buttons',   1, true, 'Type',      NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'questionnaire.labels');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'questionnaire.tips',   'Generation Tips',    2, true, 'Lightbulb', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'questionnaire.tips');

-- Create Plan
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'create_plan.labels', 'Labels & Titles', 0, true, 'Type',   NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'create_plan.labels');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'create_plan.types',  'Plan Types',      1, true, 'Target', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'create_plan.types');

-- Subscription
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'subscription.labels', 'Labels & Titles',  0, true, 'Type',       NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'subscription.labels');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'subscription.plans',  'Plan Definitions', 1, true, 'CreditCard', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'subscription.plans');

-- Onboarding
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'onboarding.welcome',    'Welcome',    0, true, 'Rocket',   NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'onboarding.welcome');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'onboarding.steps',      'Steps',      1, true, 'Layers',   NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'onboarding.steps');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'onboarding.completion', 'Completion', 2, true, 'MailCheck', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'onboarding.completion');

-- Auth
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'auth.login',           'Login',              0, true, 'LogIn',    NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'auth.login');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'auth.register',        'Registration',       1, true, 'UserPlus', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'auth.register');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'auth.forgot_password', 'Forgot Password',    2, true, 'KeyRound', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'auth.forgot_password');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'auth.reset_password',  'Reset Password',     3, true, 'Lock',     NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'auth.reset_password');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'auth.verify_email',    'Email Verification', 4, true, 'MailCheck', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'auth.verify_email');

-- Legal
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'legal.terms',   'Terms of Service', 0, true, 'Scale', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'legal.terms');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'legal.privacy', 'Privacy Policy',   1, true, 'Lock',  NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'legal.privacy');

-- Global
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000011', 'global.branding',   'Branding',            0, true, 'Palette',    NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'global.branding');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000011', 'global.social',     'Social Links',        1, true, 'Globe',      NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'global.social');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000011', 'global.contact',    'Contact Information', 2, true, 'Building2',  NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'global.contact');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000011', 'global.footer',     'Footer',              3, true, 'FileText',   NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'global.footer');
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a0000001-0000-0000-0000-000000000011', 'global.navigation', 'Navigation',          4, true, 'Navigation', NOW(), false
WHERE NOT EXISTS (SELECT 1 FROM "CmsSections" WHERE "Key" = 'global.navigation');

-- =============================================================================
-- 4. Update QuestionsSnapshot from V3 data for ALL versions
-- =============================================================================
WITH v3_snapshot AS (
  SELECT jsonb_agg(
    jsonb_build_object(
      'id', q."Id"::text,
      'questionText', q."QuestionTextFR",
      'questionTextEN', q."QuestionTextEN",
      'helpText', q."HelpTextFR",
      'helpTextEN', q."HelpTextEN",
      'questionType', q."QuestionType",
      'stepNumber', q."StepNumber",
      'personaType', q."PersonaType",
      'order', q."DisplayOrder",
      'isRequired', q."IsRequired",
      'section', q."SectionGroup",
      'icon', q."Icon",
      'options', q."OptionsFR",
      'optionsEN', q."OptionsEN",
      'validationRules', q."ValidationRules",
      'conditionalLogic', q."ConditionalLogic",
      'expertAdviceFR', q."ExpertAdviceFR",
      'expertAdviceEN', q."ExpertAdviceEN",
      'coachPromptFR', q."CoachPromptFR",
      'coachPromptEN', q."CoachPromptEN",
      'isActive', q."IsActive",
      'created', q."Created",
      'lastModified', q."LastModified"
    ) ORDER BY q."StepNumber", q."DisplayOrder"
  ) as snapshot
  FROM "QuestionTemplatesV3" q
  WHERE q."IsActive" = true
)
UPDATE "QuestionnaireVersions"
SET "QuestionsSnapshot" = (SELECT snapshot::text FROM v3_snapshot)
WHERE "Status" IN (0, 1);

-- =============================================================================
-- 5. Update StepsSnapshot to include all 7 steps for ALL versions
-- =============================================================================
WITH step_questions AS (
  SELECT "StepNumber", COUNT(*) as q_count
  FROM "QuestionTemplatesV3"
  WHERE "IsActive" = true
  GROUP BY "StepNumber"
),
steps_json AS (
  SELECT jsonb_agg(
    jsonb_build_object(
      'id', s."Id"::text,
      'stepNumber', s."StepNumber",
      'titleFR', s."TitleFR",
      'titleEN', s."TitleEN",
      'descriptionFR', s."DescriptionFR",
      'descriptionEN', s."DescriptionEN",
      'icon', s."Icon",
      'isActive', s."IsActive",
      'questionCount', COALESCE(sq.q_count, 0)
    ) ORDER BY s."StepNumber"
  ) as steps
  FROM "QuestionnaireSteps" s
  LEFT JOIN step_questions sq ON sq."StepNumber" = s."StepNumber"
  WHERE s."IsActive" = true AND s."IsDeleted" = false
)
UPDATE "QuestionnaireVersions"
SET "StepsSnapshot" = (SELECT steps::text FROM steps_json)
WHERE "Status" IN (0, 1);

COMMIT;
