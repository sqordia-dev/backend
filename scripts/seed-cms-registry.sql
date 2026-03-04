-- =============================================================================
-- CMS Registry Seed - Pages and Sections
-- Safe to run multiple times (deletes and re-inserts)
-- =============================================================================

BEGIN;

-- Clean existing registry
DELETE FROM "CmsSections";
DELETE FROM "CmsPages";

-- =============================================================================
-- Pages
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
  ('a0000001-0000-0000-0000-000000000011', 'global',             'Global / Shared',      NULL, 10, true, 'Globe',           NULL,                 NOW(), false);

-- =============================================================================
-- Sections
-- =============================================================================

-- Landing
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted") VALUES
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'landing.hero',         'Hero',         0, true, 'Type',          NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'landing.features',     'Features',     1, true, 'LayoutGrid',    NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'landing.faq',          'FAQ',          2, true, 'HelpCircle',    NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000001', 'landing.testimonials', 'Testimonials', 3, true, 'MessageSquare', NOW(), false);

-- Dashboard
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted") VALUES
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'dashboard.labels',      'Labels & Titles', 0, true, 'Type',      NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'dashboard.empty_states', 'Empty States',    1, true, 'FileText',  NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000002', 'dashboard.tips',         'Tips & Tour',     2, true, 'Lightbulb', NOW(), false);

-- Profile
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted") VALUES
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'profile.labels',   'Labels & Titles', 0, true, 'Type',        NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'profile.security', 'Security',        1, true, 'ShieldCheck', NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000003', 'profile.sessions', 'Sessions',        2, true, 'Monitor',     NOW(), false);

-- Question Templates (specialRenderer: question-templates)
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted") VALUES
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'question_templates.step1', 'Step 1: Vision & Mission',      0, true, 'Target', NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'question_templates.step2', 'Step 2: Market & Customers',    1, true, 'Target', NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'question_templates.step3', 'Step 3: Products & Services',   2, true, 'Target', NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'question_templates.step4', 'Step 4: Strategy & Operations', 3, true, 'Target', NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000004', 'question_templates.step5', 'Step 5: Financials & Growth',   4, true, 'Target', NOW(), false);

-- Questionnaire Wizard
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted") VALUES
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'questionnaire.steps',  'Step Configuration', 0, true, 'Layers',    NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'questionnaire.labels', 'Labels & Buttons',   1, true, 'Type',      NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000005', 'questionnaire.tips',   'Generation Tips',    2, true, 'Lightbulb', NOW(), false);

-- Create Plan
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted") VALUES
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'create_plan.labels', 'Labels & Titles', 0, true, 'Type',   NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000006', 'create_plan.types',  'Plan Types',      1, true, 'Target', NOW(), false);

-- Subscription
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted") VALUES
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'subscription.labels', 'Labels & Titles',  0, true, 'Type',       NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000007', 'subscription.plans',  'Plan Definitions', 1, true, 'CreditCard', NOW(), false);

-- Onboarding
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted") VALUES
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'onboarding.welcome',    'Welcome',    0, true, 'Rocket',   NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'onboarding.steps',      'Steps',      1, true, 'Layers',   NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000008', 'onboarding.completion', 'Completion', 2, true, 'MailCheck', NOW(), false);

-- Auth
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted") VALUES
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'auth.login',           'Login',              0, true, 'LogIn',    NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'auth.register',        'Registration',       1, true, 'UserPlus', NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'auth.forgot_password', 'Forgot Password',    2, true, 'KeyRound', NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'auth.reset_password',  'Reset Password',     3, true, 'Lock',     NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000009', 'auth.verify_email',    'Email Verification', 4, true, 'MailCheck', NOW(), false);

-- Legal
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted") VALUES
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'legal.terms',   'Terms of Service', 0, true, 'Scale', NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000010', 'legal.privacy', 'Privacy Policy',   1, true, 'Lock',  NOW(), false);

-- Global
INSERT INTO "CmsSections" ("Id", "CmsPageId", "Key", "Label", "SortOrder", "IsActive", "IconName", "Created", "IsDeleted") VALUES
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000011', 'global.branding',   'Branding',            0, true, 'Palette',    NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000011', 'global.social',     'Social Links',        1, true, 'Globe',      NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000011', 'global.contact',    'Contact Information', 2, true, 'Building2',  NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000011', 'global.footer',     'Footer',              3, true, 'FileText',   NOW(), false),
  (gen_random_uuid(), 'a0000001-0000-0000-0000-000000000011', 'global.navigation', 'Navigation',          4, true, 'Navigation', NOW(), false);

COMMIT;
