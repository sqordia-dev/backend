-- =============================================================================
-- CMS Seed Data - Complete Seed File
-- This file seeds all CMS content with proper conflict handling and foreign key respect
-- Version ID: 17a4a74e-4782-4ca0-9493-aebbd22dcc95
-- Block Types: Text=0, RichText=1, Image=2, Link=3, Json=4, Number=5, Boolean=6
-- Status: Draft=0, Published=1, Archived=2
--
-- IMPORTANT: This script empties the CMS tables (CmsContentBlocks, then CmsVersions)
-- and then inserts fresh data. Safe to run multiple times.
-- If you see "current transaction is aborted" (25P02), run ROLLBACK; then re-run this script.
-- =============================================================================

BEGIN;

-- =============================================================================
-- STEP 0: Empty CMS tables (child first, then parent, for FK constraint)
-- =============================================================================
DELETE FROM "CmsContentBlocks";
DELETE FROM "CmsVersions";

-- =============================================================================
-- STEP 1: Insert CmsVersion (must be done first due to FK constraint)
-- =============================================================================
INSERT INTO "CmsVersions" (
    "Id",
    "VersionNumber",
    "Status",
    "CreatedByUserId",
    "PublishedAt",
    "PublishedByUserId",
    "Notes",
    "Created",
    "CreatedBy",
    "LastModified",
    "LastModifiedBy",
    "IsDeleted"
)
VALUES (
    '17a4a74e-4782-4ca0-9493-aebbd22dcc95'::uuid,
    1,
    1, -- Published
    '00000000-0000-0000-0000-000000000000'::uuid, -- System user
    NOW(),
    '00000000-0000-0000-0000-000000000000'::uuid, -- System user
    'Initial CMS seed data',
    NOW(),
    'system',
    NOW(),
    'system',
    false
)
ON CONFLICT ("Id") DO UPDATE SET
    "Status" = EXCLUDED."Status",
    "PublishedAt" = EXCLUDED."PublishedAt",
    "PublishedByUserId" = EXCLUDED."PublishedByUserId",
    "Notes" = EXCLUDED."Notes",
    "LastModified" = NOW(),
    "LastModifiedBy" = 'system',
    "IsDeleted" = false;

-- =============================================================================
-- STEP 2: Insert/Update CmsContentBlocks (with conflict handling)
-- Unique constraint: (CmsVersionId, BlockKey, Language)
-- =============================================================================

-- DASHBOARD - Labels (EN)
INSERT INTO "CmsContentBlocks" (
    "Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", 
    "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted"
)
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.title', 0, 'Dashboard', 'en', 1, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.description', 0, 'Manage your business plans and projects', 'en', 2, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.welcome', 0, 'Welcome Back', 'en', 3, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.subtitle', 0, 'Manage your business plans and track your progress', 'en', 4, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.newPlan', 0, 'New Plan', 'en', 5, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.totalPlans', 0, 'Total Plans', 'en', 6, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.activePlans', 0, 'Active Plans', 'en', 7, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.recentPlans', 0, 'Recent Plans', 'en', 8, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.completionRate', 0, 'Completion Rate', 'en', 9, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createNextPlan', 0, 'Create Your Next Business Plan', 'en', 10, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createNextPlanDesc', 0, 'Start building your success story with AI-powered planning', 'en', 11, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.getStarted', 0, 'Get Started', 'en', 12, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.yourPlans', 0, 'Your Business Plans', 'en', 13, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createFirstPlan', 0, 'Create Your First Plan', 'en', 14, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.view', 0, 'View', 'en', 15, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.resume', 0, 'Resume', 'en', 16, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicate', 0, 'Duplicate', 'en', 17, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.delete', 0, 'Delete', 'en', 18, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deletePlan', 0, 'Delete Business Plan', 'en', 19, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteConfirm', 0, 'Are you sure you want to delete', 'en', 20, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteWarning', 0, 'All associated data will be permanently deleted', 'en', 21, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.cancel', 0, 'Cancel', 'en', 22, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.loading', 0, 'Loading your dashboard...', 'en', 23, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.showTour', 0, 'Show Tour', 'en', 24, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.plan', 0, 'plan', 'en', 28, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.plansTotal', 0, 'plans total', 'en', 29, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicatePlan', 0, 'Duplicate', 'en', 30, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.status.draft', 0, 'Draft', 'en', 31, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.status.completed', 0, 'Completed', 'en', 32, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.status.active', 0, 'Active', 'en', 33, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.status.inProgress', 0, 'In Progress', 'en', 34, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleting', 0, 'Deleting...', 'en', 35, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deletePlanButton', 0, 'Delete Plan', 'en', 36, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteSuccess', 0, 'Plan deleted', 'en', 37, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteSuccessDesc', 0, 'Your plan has been deleted successfully', 'en', 38, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteError', 0, 'Failed to delete plan', 'en', 39, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicateSuccess', 0, 'Plan duplicated', 'en', 40, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicateSuccessDesc', 0, 'Your plan has been duplicated successfully', 'en', 41, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicateError', 0, 'Failed to duplicate plan', 'en', 42, 'dashboard.labels', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET
    "BlockType" = EXCLUDED."BlockType",
    "Content" = EXCLUDED."Content",
    "SortOrder" = EXCLUDED."SortOrder",
    "SectionKey" = EXCLUDED."SectionKey",
    "Metadata" = EXCLUDED."Metadata",
    "LastModified" = NOW(),
    "IsDeleted" = false;

-- DASHBOARD - Labels (FR)
INSERT INTO "CmsContentBlocks" (
    "Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", 
    "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted"
)
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.title', 0, 'Tableau de bord', 'fr', 1, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.description', 0, E'G\u00e9rez vos plans d''affaires et vos projets', 'fr', 2, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.welcome', 0, 'Bon retour', 'fr', 3, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.subtitle', 0, E'G\u00e9rez vos plans d''affaires et suivez votre progression', 'fr', 4, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.newPlan', 0, 'Nouveau plan', 'fr', 5, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.totalPlans', 0, 'Plans totaux', 'fr', 6, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.activePlans', 0, 'Plans actifs', 'fr', 7, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.recentPlans', 0, E'Plans r\u00e9cents', 'fr', 8, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.completionRate', 0, E'Taux de compl\u00e9tion', 'fr', 9, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createNextPlan', 0, E'Cr\u00e9ez votre prochain plan d''affaires', 'fr', 10, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createNextPlanDesc', 0, E'Commencez \u00e0 construire votre histoire de succ\u00e8s avec la planification propuls\u00e9e par l''IA', 'fr', 11, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.getStarted', 0, 'Commencer', 'fr', 12, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.yourPlans', 0, E'Vos plans d''affaires', 'fr', 13, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createFirstPlan', 0, E'Cr\u00e9ez votre premier plan', 'fr', 14, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.view', 0, 'Voir', 'fr', 15, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.resume', 0, 'Reprendre', 'fr', 16, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicate', 0, 'Dupliquer', 'fr', 17, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.delete', 0, 'Supprimer', 'fr', 18, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deletePlan', 0, E'Supprimer le plan d''affaires', 'fr', 19, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteConfirm', 0, E'\u00cates-vous s\u00fbr de vouloir supprimer', 'fr', 20, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteWarning', 0, E'Toutes les donn\u00e9es associ\u00e9es seront d\u00e9finitivement supprim\u00e9es', 'fr', 21, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.cancel', 0, 'Annuler', 'fr', 22, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.loading', 0, 'Chargement de votre tableau de bord...', 'fr', 23, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.showTour', 0, 'Afficher le guide', 'fr', 24, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.plan', 0, 'plan', 'fr', 28, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.plansTotal', 0, 'plans au total', 'fr', 29, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicatePlan', 0, 'Dupliquer', 'fr', 30, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.status.draft', 0, 'Brouillon', 'fr', 31, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.status.completed', 0, E'Termin\u00e9', 'fr', 32, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.status.active', 0, 'Actif', 'fr', 33, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.status.inProgress', 0, 'En cours', 'fr', 34, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleting', 0, 'Suppression...', 'fr', 35, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deletePlanButton', 0, 'Supprimer le plan', 'fr', 36, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteSuccess', 0, E'Plan supprim\u00e9', 'fr', 37, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteSuccessDesc', 0, E'Votre plan a \u00e9t\u00e9 supprim\u00e9 avec succ\u00e8s', 'fr', 38, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteError', 0, E'\u00c9chec de la suppression du plan', 'fr', 39, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicateSuccess', 0, E'Plan dupliqu\u00e9', 'fr', 40, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicateSuccessDesc', 0, E'Votre plan a \u00e9t\u00e9 dupliqu\u00e9 avec succ\u00e8s', 'fr', 41, 'dashboard.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicateError', 0, E'\u00c9chec de la duplication du plan', 'fr', 42, 'dashboard.labels', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET
    "BlockType" = EXCLUDED."BlockType",
    "Content" = EXCLUDED."Content",
    "SortOrder" = EXCLUDED."SortOrder",
    "SectionKey" = EXCLUDED."SectionKey",
    "Metadata" = EXCLUDED."Metadata",
    "LastModified" = NOW(),
    "IsDeleted" = false;

-- =============================================================================
-- DASHBOARD - Empty States (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noPlans', 0, 'No business plans yet', 'en', 1, 'dashboard.empty_states', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noPlansDesc', 1, 'Get started by creating your first business plan. It only takes a few minutes to begin your journey.', 'en', 2, 'dashboard.empty_states', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noDescription', 0, 'No description provided', 'en', 3, 'dashboard.empty_states', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noPlans', 0, E'Aucun plan d''affaires pour le moment', 'fr', 1, 'dashboard.empty_states', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noPlansDesc', 1, E'Commencez en cr\u00e9ant votre premier plan d''affaires. Cela ne prend que quelques minutes.', 'fr', 2, 'dashboard.empty_states', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noDescription', 0, 'Aucune description fournie', 'fr', 3, 'dashboard.empty_states', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- DASHBOARD - Tips & Tour (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.welcome.title', 0, 'Welcome to Your Dashboard!', 'en', 1, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.welcome.description', 1, 'This is your command center. Here you can view all your business plans, track progress, and create new plans.', 'en', 2, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.stats.title', 0, 'Track Your Progress', 'en', 3, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.stats.description', 1, 'Monitor your total plans, active projects, recent activity, and completion rates at a glance.', 'en', 4, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.createPlan.title', 0, 'Create New Plans', 'en', 5, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.createPlan.description', 1, 'Click here to start creating a new business plan. Our AI will guide you through the process step by step.', 'en', 6, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.plansList.title', 0, 'Manage Your Plans', 'en', 7, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.plansList.description', 1, 'View, edit, duplicate, or delete your business plans. All your work is organized here.', 'en', 8, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.welcome.title', 0, 'Bienvenue sur votre tableau de bord!', 'fr', 1, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.welcome.description', 1, E'C''est votre centre de commande. Ici vous pouvez voir tous vos plans d''affaires, suivre la progression et cr\u00e9er de nouveaux plans.', 'fr', 2, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.stats.title', 0, 'Suivez votre progression', 'fr', 3, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.stats.description', 1, E'Surveillez vos plans totaux, projets actifs, activit\u00e9 r\u00e9cente et taux de compl\u00e9tion en un coup d''\u0153il.', 'fr', 4, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.createPlan.title', 0, E'Cr\u00e9er de nouveaux plans', 'fr', 5, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.createPlan.description', 1, E'Cliquez ici pour commencer \u00e0 cr\u00e9er un nouveau plan d''affaires. Notre IA vous guidera \u00e9tape par \u00e9tape.', 'fr', 6, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.plansList.title', 0, E'G\u00e9rez vos plans', 'fr', 7, 'dashboard.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.plansList.description', 1, E'Visualisez, modifiez, dupliquez ou supprimez vos plans d''affaires. Tout votre travail est organis\u00e9 ici.', 'fr', 8, 'dashboard.tips', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- PROFILE - Labels, Security, Sessions (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.page_title', 0, 'Profile Settings', 'en', 1, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.page_description', 1, 'Manage your account settings and preferences', 'en', 2, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.back_to_dashboard', 0, 'Back to Dashboard', 'en', 3, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_profile', 0, 'Profile', 'en', 4, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_security', 0, 'Security', 'en', 5, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_sessions', 0, 'Sessions', 'en', 6, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.profile_picture_label', 0, 'Profile Picture', 'en', 7, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.first_name_label', 0, 'First Name', 'en', 8, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.last_name_label', 0, 'Last Name', 'en', 9, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.email_label', 0, 'Email', 'en', 10, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.phone_number_label', 0, 'Phone Number', 'en', 11, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.company_label', 0, 'Company', 'en', 12, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.address_label', 0, 'Address', 'en', 13, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.profile_type_label', 0, 'Profile Type', 'en', 14, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.entrepreneur_label', 0, 'Entrepreneur / Solopreneur', 'en', 15, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.consultant_label', 0, 'Consultant', 'en', 16, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.obnl_label', 0, 'OBNL / NPO', 'en', 17, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.save_changes_button', 0, 'Save Changes', 'en', 18, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.page_title', 0, E'Param\u00e8tres du Profil', 'fr', 1, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.page_description', 1, E'G\u00e9rez vos param\u00e8tres de compte et vos pr\u00e9f\u00e9rences', 'fr', 2, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.back_to_dashboard', 0, 'Retour au Tableau de Bord', 'fr', 3, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_profile', 0, 'Profil', 'fr', 4, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_security', 0, E'S\u00e9curit\u00e9', 'fr', 5, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_sessions', 0, 'Sessions', 'fr', 6, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.profile_picture_label', 0, 'Photo de Profil', 'fr', 7, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.first_name_label', 0, E'Pr\u00e9nom', 'fr', 8, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.last_name_label', 0, 'Nom de Famille', 'fr', 9, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.email_label', 0, 'Adresse E-mail', 'fr', 10, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.phone_number_label', 0, E'Num\u00e9ro de T\u00e9l\u00e9phone', 'fr', 11, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.company_label', 0, 'Entreprise', 'fr', 12, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.address_label', 0, 'Adresse', 'fr', 13, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.profile_type_label', 0, 'Type de Profil', 'fr', 14, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.entrepreneur_label', 0, E'Entrepreneur / Travailleur Ind\u00e9pendant', 'fr', 15, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.consultant_label', 0, 'Consultant', 'fr', 16, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.obnl_label', 0, 'OBNL / ONG', 'fr', 17, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.save_changes_button', 0, 'Enregistrer les Modifications', 'fr', 18, 'profile.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.change_password_heading', 0, 'Change Password', 'en', 1, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.current_password_label', 0, 'Current Password', 'en', 2, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.new_password_label', 0, 'New Password', 'en', 3, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.confirm_new_password_label', 0, 'Confirm New Password', 'en', 4, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.update_password_button', 0, 'Update Password', 'en', 5, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.two_factor_heading', 0, 'Two-Factor Authentication', 'en', 6, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.two_factor_description', 1, 'Add an extra layer of security to your account', 'en', 7, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.change_password_heading', 0, 'Changer le Mot de Passe', 'fr', 1, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.current_password_label', 0, 'Mot de Passe Actuel', 'fr', 2, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.new_password_label', 0, 'Nouveau Mot de Passe', 'fr', 3, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.confirm_new_password_label', 0, 'Confirmer le Nouveau Mot de Passe', 'fr', 4, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.update_password_button', 0, E'Mettre \u00e0 Jour le Mot de Passe', 'fr', 5, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.two_factor_heading', 0, E'Authentification \u00e0 Deux Facteurs', 'fr', 6, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.two_factor_description', 1, E'Ajoutez une couche de s\u00e9curit\u00e9 suppl\u00e9mentaire \u00e0 votre compte', 'fr', 7, 'profile.security', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.active_sessions_heading', 0, 'Active Sessions', 'en', 1, 'profile.sessions', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.active_sessions_description', 1, 'Manage your active sessions across devices', 'en', 2, 'profile.sessions', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.revoke_all_sessions_button', 0, 'Revoke All Other Sessions', 'en', 3, 'profile.sessions', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.unknown_device', 0, 'Unknown Device', 'en', 4, 'profile.sessions', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.current_session_badge', 0, 'Current', 'en', 5, 'profile.sessions', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.revoke_session_button', 0, 'Revoke', 'en', 6, 'profile.sessions', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.active_sessions_heading', 0, 'Sessions Actives', 'fr', 1, 'profile.sessions', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.active_sessions_description', 1, E'G\u00e9rez vos sessions actives sur tous les appareils', 'fr', 2, 'profile.sessions', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.revoke_all_sessions_button', 0, E'R\u00e9voquer Toutes les Autres Sessions', 'fr', 3, 'profile.sessions', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.unknown_device', 0, 'Appareil Inconnu', 'fr', 4, 'profile.sessions', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.current_session_badge', 0, 'Actuelle', 'fr', 5, 'profile.sessions', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.revoke_session_button', 0, E'R\u00e9voquer', 'fr', 6, 'profile.sessions', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- LANDING - Hero (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.badge_trusted', 0, 'Trusted by 10,000+ entrepreneurs', 'en', 1, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.badge_rating', 0, '4.9/5 rating', 'en', 2, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.headline_line1', 0, 'A Bank-Ready Business Plan', 'en', 3, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.headline_highlight', 0, 'in Under 60 Minutes', 'en', 4, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.subheadline', 0, 'Choose your AI engine. Answer 20 simple questions. Get investor-ready business plans with automated financial projections, market analysis, and bank-readiness scoring.', 'en', 5, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.cta_primary', 0, 'Create Your Plan Free', 'en', 6, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.cta_secondary', 0, 'See Example Plans', 'en', 7, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.trust_nocard', 0, 'No credit card required', 'en', 8, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.trust_trial', 0, '14-day free trial', 'en', 9, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.trust_cancel', 0, 'Cancel anytime', 'en', 10, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.badge_trusted', 0, 'Plus de 10 000 entrepreneurs nous font confiance', 'fr', 1, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.badge_rating', 0, '4,9/5', 'fr', 2, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.headline_line1', 0, E'Un plan d''affaires pr\u00eat pour la banque', 'fr', 3, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.headline_highlight', 0, 'en moins de 60 minutes', 'fr', 4, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.subheadline', 0, E'Choisissez votre moteur IA. R\u00e9pondez \u00e0 20 questions simples. Obtenez des plans d''affaires pr\u00eats pour les investisseurs avec projections financi\u00e8res, analyse de march\u00e9 et score de pr\u00eat bancaire.', 'fr', 5, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.cta_primary', 0, E'Cr\u00e9ez votre plan gratuitement', 'fr', 6, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.cta_secondary', 0, 'Voir des exemples de plans', 'fr', 7, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.trust_nocard', 0, 'Aucune carte de cr\u00e9dit requise', 'fr', 8, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.trust_trial', 0, 'Essai gratuit de 14 jours', 'fr', 9, 'landing.hero', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.hero.trust_cancel', 0, 'Annulez \u00e0 tout moment', 'fr', 10, 'landing.hero', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- LANDING - Features (EN + FR) - 3 steps with titles, subtitles, descriptions, benefits
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
-- Header content (EN)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.badge', 0, 'How It Works', 'en', 1, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.title', 0, 'Three Simple Steps to', 'en', 2, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.title.highlight', 0, 'Your Business Plan', 'en', 3, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.subtitle', 0, 'Transform your business idea into a professional, bank-ready plan with AI-powered guidance.', 'en', 4, 'landing.features', NULL, NOW(), NOW(), false),
-- Step 1 (EN)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.title', 0, 'Answer Smart Questions', 'en', 10, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.subtitle', 0, 'Guided Questionnaire', 'en', 11, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.description', 0, 'Our intelligent questionnaire adapts to your business type and provides AI-powered suggestions as you answer. No business expertise required.', 'en', 12, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.benefit1', 0, 'Multiple-choice answers with smart suggestions', 'en', 13, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.benefit2', 0, 'AI explains why each question matters', 'en', 14, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.benefit3', 0, 'Auto-save progress as you go', 'en', 15, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.benefit4', 0, 'Complete in under 20 minutes', 'en', 16, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.imageAlt', 0, 'Sqordia questionnaire interface showing smart questions', 'en', 17, 'landing.features', NULL, NOW(), NOW(), false),
-- Step 2 (EN)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.title', 0, 'AI Generates Your Plan', 'en', 20, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.subtitle', 0, 'Intelligent Generation', 'en', 21, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.description', 0, 'Choose from multiple AI engines (GPT-4, Claude, Gemini) to generate a comprehensive business plan with market analysis, financial projections, and strategy.', 'en', 22, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.benefit1', 0, 'Multiple AI engine options for best results', 'en', 23, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.benefit2', 0, 'Real-time market data integration', 'en', 24, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.benefit3', 0, 'Automated financial projections', 'en', 25, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.benefit4', 0, 'Bank-readiness scoring included', 'en', 26, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.imageAlt', 0, 'AI-powered plan generation with live preview', 'en', 27, 'landing.features', NULL, NOW(), NOW(), false),
-- Step 3 (EN)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.title', 0, 'Export & Present', 'en', 30, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.subtitle', 0, 'Professional Output', 'en', 31, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.description', 0, 'Export your plan in multiple formats - PDF, Word, PowerPoint. Every section is fully editable before you present to investors or banks.', 'en', 32, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.benefit1', 0, 'Export to PDF, Word, and PowerPoint', 'en', 33, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.benefit2', 0, 'Fully editable before export', 'en', 34, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.benefit3', 0, 'Professional formatting included', 'en', 35, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.benefit4', 0, 'Investor-ready presentation mode', 'en', 36, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.imageAlt', 0, 'Dashboard showing export options and business plans', 'en', 37, 'landing.features', NULL, NOW(), NOW(), false),
-- Header content (FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.badge', 0, 'Comment ça marche', 'fr', 1, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.title', 0, E'Trois \u00e9tapes simples vers', 'fr', 2, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.title.highlight', 0, E'votre plan d''affaires', 'fr', 3, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.subtitle', 0, E'Transformez votre id\u00e9e d''entreprise en un plan professionnel pr\u00eat pour la banque avec l''aide de l''IA.', 'fr', 4, 'landing.features', NULL, NOW(), NOW(), false),
-- Step 1 (FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.title', 0, E'R\u00e9pondez aux questions intelligentes', 'fr', 10, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.subtitle', 0, E'Questionnaire guid\u00e9', 'fr', 11, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.description', 0, E'Notre questionnaire intelligent s''adapte \u00e0 votre type d''entreprise et fournit des suggestions propuls\u00e9es par l''IA au fur et \u00e0 mesure. Aucune expertise commerciale requise.', 'fr', 12, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.benefit1', 0, E'R\u00e9ponses \u00e0 choix multiples avec suggestions intelligentes', 'fr', 13, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.benefit2', 0, E'L''IA explique pourquoi chaque question est importante', 'fr', 14, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.benefit3', 0, E'Sauvegarde automatique au fur et \u00e0 mesure', 'fr', 15, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.benefit4', 0, E'Compl\u00e9tez en moins de 20 minutes', 'fr', 16, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step1.imageAlt', 0, 'Interface du questionnaire Sqordia avec questions intelligentes', 'fr', 17, 'landing.features', NULL, NOW(), NOW(), false),
-- Step 2 (FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.title', 0, E'L''IA g\u00e9n\u00e8re votre plan', 'fr', 20, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.subtitle', 0, E'G\u00e9n\u00e9ration intelligente', 'fr', 21, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.description', 0, E'Choisissez parmi plusieurs moteurs IA (GPT-4, Claude, Gemini) pour g\u00e9n\u00e9rer un plan d''affaires complet avec analyse de march\u00e9, projections financi\u00e8res et strat\u00e9gie.', 'fr', 22, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.benefit1', 0, E'Plusieurs moteurs IA pour les meilleurs r\u00e9sultats', 'fr', 23, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.benefit2', 0, E'Int\u00e9gration de donn\u00e9es de march\u00e9 en temps r\u00e9el', 'fr', 24, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.benefit3', 0, E'Projections financi\u00e8res automatis\u00e9es', 'fr', 25, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.benefit4', 0, E'Score de pr\u00eat bancaire inclus', 'fr', 26, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step2.imageAlt', 0, E'G\u00e9n\u00e9ration de plan propuls\u00e9e par l''IA avec aper\u00e7u en direct', 'fr', 27, 'landing.features', NULL, NOW(), NOW(), false),
-- Step 3 (FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.title', 0, E'Exportez et pr\u00e9sentez', 'fr', 30, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.subtitle', 0, 'Sortie professionnelle', 'fr', 31, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.description', 0, E'Exportez votre plan dans plusieurs formats - PDF, Word, PowerPoint. Chaque section est enti\u00e8rement modifiable avant de pr\u00e9senter aux investisseurs ou aux banques.', 'fr', 32, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.benefit1', 0, 'Exportez en PDF, Word et PowerPoint', 'fr', 33, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.benefit2', 0, E'Enti\u00e8rement modifiable avant l''export', 'fr', 34, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.benefit3', 0, 'Mise en forme professionnelle incluse', 'fr', 35, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.benefit4', 0, E'Mode pr\u00e9sentation pr\u00eat pour les investisseurs', 'fr', 36, 'landing.features', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.features.step3.imageAlt', 0, E'Tableau de bord montrant les options d''exportation et les plans d''affaires', 'fr', 37, 'landing.features', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- LANDING - Value Props (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
-- Header (EN)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.badge', 0, 'Why Sqordia', 'en', 1, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.title', 0, 'Everything You Need to', 'en', 2, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.title.highlight', 0, 'Succeed', 'en', 3, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.subtitle', 0, 'Powerful features designed to help entrepreneurs create professional business plans that get results.', 'en', 4, 'landing.valueProps', NULL, NOW(), NOW(), false),
-- Props (EN)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.1.title', 0, 'AI-Powered Intelligence', 'en', 10, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.1.description', 0, 'Multiple AI engines analyze your business and generate comprehensive, data-driven plans with market insights.', 'en', 11, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.2.title', 0, 'Strategic Precision', 'en', 12, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.2.description', 0, 'Bank-readiness scoring ensures your plan meets the standards that investors and lenders expect.', 'en', 13, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.3.title', 0, 'Built for Everyone', 'en', 14, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.3.description', 0, 'Whether you''re an entrepreneur, consultant, or nonprofit, Sqordia adapts to your unique needs.', 'en', 15, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.4.title', 0, 'Multiple Plan Types', 'en', 16, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.4.description', 0, 'Create traditional business plans, strategic plans, or lean canvases—all from one platform.', 'en', 17, 'landing.valueProps', NULL, NOW(), NOW(), false),
-- Header (FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.badge', 0, 'Pourquoi Sqordia', 'fr', 1, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.title', 0, E'Tout ce qu''il vous faut pour', 'fr', 2, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.title.highlight', 0, E'r\u00e9ussir', 'fr', 3, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.subtitle', 0, E'Des fonctionnalit\u00e9s puissantes con\u00e7ues pour aider les entrepreneurs \u00e0 cr\u00e9er des plans d''affaires professionnels qui obtiennent des r\u00e9sultats.', 'fr', 4, 'landing.valueProps', NULL, NOW(), NOW(), false),
-- Props (FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.1.title', 0, E'Intelligence propuls\u00e9e par l''IA', 'fr', 10, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.1.description', 0, E'Plusieurs moteurs IA analysent votre entreprise et g\u00e9n\u00e8rent des plans complets bas\u00e9s sur les donn\u00e9es.', 'fr', 11, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.2.title', 0, E'Pr\u00e9cision strat\u00e9gique', 'fr', 12, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.2.description', 0, E'Le score de pr\u00eat bancaire garantit que votre plan r\u00e9pond aux normes attendues par les investisseurs et les pr\u00eateurs.', 'fr', 13, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.3.title', 0, E'Con\u00e7u pour tous', 'fr', 14, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.3.description', 0, E'Que vous soyez entrepreneur, consultant ou OBNL, Sqordia s''adapte \u00e0 vos besoins uniques.', 'fr', 15, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.4.title', 0, 'Plusieurs types de plans', 'fr', 16, 'landing.valueProps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.valueProps.4.description', 0, E'Cr\u00e9ez des plans d''affaires traditionnels, des plans strat\u00e9giques ou des lean canvas—le tout depuis une seule plateforme.', 'fr', 17, 'landing.valueProps', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- LANDING - Stats / LogoCloud (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
-- Stats (EN)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.heading', 0, 'Trusted by entrepreneurs worldwide', 'en', 1, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.plans.value', 0, '10,000+', 'en', 2, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.plans.label', 0, 'Business Plans Created', 'en', 3, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.funding.value', 0, '$50M+', 'en', 4, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.funding.label', 0, 'Funding Secured', 'en', 5, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.countries.value', 0, '50+', 'en', 6, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.countries.label', 0, 'Countries', 'en', 7, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.rating.value', 0, '4.9/5', 'en', 8, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.rating.label', 0, 'User Rating', 'en', 9, 'landing.stats', NULL, NOW(), NOW(), false),
-- Stats (FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.heading', 0, E'La confiance des entrepreneurs du monde entier', 'fr', 1, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.plans.value', 0, '10 000+', 'fr', 2, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.plans.label', 0, E'Plans d''affaires cr\u00e9\u00e9s', 'fr', 3, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.funding.value', 0, '50M$+', 'fr', 4, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.funding.label', 0, E'Financement obtenu', 'fr', 5, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.countries.value', 0, '50+', 'fr', 6, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.countries.label', 0, 'Pays', 'fr', 7, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.rating.value', 0, '4,9/5', 'fr', 8, 'landing.stats', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.stats.rating.label', 0, E'\u00c9valuation utilisateurs', 'fr', 9, 'landing.stats', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- LANDING - Final CTA (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
-- Final CTA (EN)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.badge', 0, 'Start Free Today', 'en', 1, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.headline', 0, 'Ready to Build Your Business Plan?', 'en', 2, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.subheadline', 0, 'Join thousands of entrepreneurs who have transformed their ideas into investor-ready business plans.', 'en', 3, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.cta', 0, 'Get Started Free', 'en', 4, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.trust.noCard', 0, 'No credit card required', 'en', 5, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.trust.trial', 0, '14-day free trial', 'en', 6, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.trust.cancel', 0, 'Cancel anytime', 'en', 7, 'landing.finalCta', NULL, NOW(), NOW(), false),
-- Final CTA (FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.badge', 0, E'Commencez gratuitement aujourd''hui', 'fr', 1, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.headline', 0, E'Pr\u00eat \u00e0 cr\u00e9er votre plan d''affaires?', 'fr', 2, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.subheadline', 0, E'Rejoignez des milliers d''entrepreneurs qui ont transform\u00e9 leurs id\u00e9es en plans d''affaires pr\u00eats pour les investisseurs.', 'fr', 3, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.cta', 0, 'Commencer gratuitement', 'fr', 4, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.trust.noCard', 0, E'Aucune carte de cr\u00e9dit requise', 'fr', 5, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.trust.trial', 0, 'Essai gratuit de 14 jours', 'fr', 6, 'landing.finalCta', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.finalCta.trust.cancel', 0, E'Annulez \u00e0 tout moment', 'fr', 7, 'landing.finalCta', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- LANDING - FAQ (EN + FR) + JSON items
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.faq.badge', 0, 'FAQ', 'en', 1, 'landing.faq', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.faq.title', 0, 'Frequently Asked', 'en', 2, 'landing.faq', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.faq.title_highlight', 0, 'Questions', 'en', 3, 'landing.faq', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.faq.subtitle', 0, 'Everything you need to know about creating your business plan with Sqordia.', 'en', 4, 'landing.faq', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.faq.badge', 0, 'FAQ', 'fr', 1, 'landing.faq', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.faq.title', 0, 'Questions', 'fr', 2, 'landing.faq', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.faq.title_highlight', 0, E'fr\u00e9quentes', 'fr', 3, 'landing.faq', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.faq.subtitle', 0, E'Tout ce que vous devez savoir sur la cr\u00e9ation de votre plan d''affaires avec Sqordia.', 'fr', 4, 'landing.faq', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.faq.items', 4, '[{"question":"Do I need to be an expert?","answer":"No. Sqordia uses multiple-choice responses and intelligent suggestions to lead you through the process."},{"question":"Is the AI transparent?","answer":"Yes. Every strategy suggested includes a ''Why this strategy?'' explanation."},{"question":"Can I edit the result?","answer":"Absolutely. Every plan, graph, and image is fully editable before and after export."}]', 'en', 10, 'landing.faq', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.faq.items', 4, E'[{"question":"Dois-je \u00eatre un expert?","answer":"Non. Sqordia utilise des r\u00e9ponses \u00e0 choix multiples et des suggestions intelligentes."},{"question":"L''IA est-elle transparente?","answer":"Oui. Chaque strat\u00e9gie sugg\u00e9r\u00e9e inclut une explication."},{"question":"Puis-je modifier le r\u00e9sultat?","answer":"Absolument. Chaque plan, graphique et image est enti\u00e8rement \u00e9ditable."}]', 'fr', 10, 'landing.faq', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- LANDING - Testimonials (EN + FR) + JSON items
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.testimonials.badge', 0, 'Success Stories', 'en', 1, 'landing.testimonials', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.testimonials.title', 0, 'Real Results from', 'en', 2, 'landing.testimonials', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.testimonials.title_highlight', 0, 'Real Entrepreneurs', 'en', 3, 'landing.testimonials', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.testimonials.subtitle', 0, 'From seed funding to grant applications—see how Sqordia helped them get funded.', 'en', 4, 'landing.testimonials', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.testimonials.badge', 0, E'T\u00e9moignages de succ\u00e8s', 'fr', 1, 'landing.testimonials', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.testimonials.title', 0, E'Des r\u00e9sultats r\u00e9els d''', 'fr', 2, 'landing.testimonials', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.testimonials.title_highlight', 0, 'entrepreneurs r\u00e9els', 'fr', 3, 'landing.testimonials', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.testimonials.subtitle', 0, E'Du financement d''amor\u00e7age aux demandes de subventions—d\u00e9couvrez comment Sqordia les a aid\u00e9s \u00e0 obtenir du financement.', 'fr', 4, 'landing.testimonials', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.testimonials.items', 4, '[{"name":"Sarah Chen","role":"Founder, TechStart","quote":"Sqordia helped me create a business plan that secured $500K in funding. The AI suggestions were incredibly helpful.","avatar":"https://images.pexels.com/photos/774909/pexels-photo-774909.jpeg?auto=compress&cs=tinysrgb&w=200&h=200&fit=crop"},{"name":"Michael Rodriguez","role":"Executive Director, Community First","quote":"As a nonprofit, we needed a strategic plan fast. Sqordia delivered a professional document in under an hour.","avatar":"https://images.pexels.com/photos/2379004/pexels-photo-2379004.jpeg?auto=compress&cs=tinysrgb&w=200&h=200&fit=crop"},{"name":"Emily Watson","role":"CEO, GreenTech Solutions","quote":"The financial projections feature saved me weeks of work. Investors were impressed with the detail.","avatar":"https://images.pexels.com/photos/1222271/pexels-photo-1222271.jpeg?auto=compress&cs=tinysrgb&w=200&h=200&fit=crop"}]', 'en', 10, 'landing.testimonials', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'landing.testimonials.items', 4, E'[{"name":"Sarah Chen","role":"Fondatrice, TechStart","quote":"Sqordia m''a aid\u00e9e \u00e0 cr\u00e9er un plan d''affaires qui a s\u00e9curis\u00e9 500K$. Les suggestions IA \u00e9taient incroyablement utiles."},{"name":"Michael Rodriguez","role":"Directeur, Community First","quote":"En tant qu''OBNL, nous avions besoin d''un plan strat\u00e9gique rapidement. Sqordia a livr\u00e9 un document professionnel en moins d''une heure."},{"name":"Emily Watson","role":"PDG, GreenTech Solutions","quote":"Les projections financi\u00e8res m''ont \u00e9conomis\u00e9 des semaines de travail."}]', 'fr', 10, 'landing.testimonials', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- AUTH - Login (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.panel_tagline', 0, 'AI-Powered Strategic Planning', 'en', 1, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.panel_subtitle', 0, 'Create bank-ready business plans in under 60 minutes', 'en', 2, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.title', 0, 'Welcome back', 'en', 3, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.subtitle', 0, 'Sign in to continue to your dashboard', 'en', 4, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.email_label', 0, 'Email Address', 'en', 5, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.email_placeholder', 0, 'you@company.com', 'en', 6, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.password_label', 0, 'Password', 'en', 7, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.password_placeholder', 0, 'Enter your password', 'en', 8, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.remember', 0, 'Remember me', 'en', 9, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.forgot_password', 0, 'Forgot password?', 'en', 10, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.button', 0, 'Sign In', 'en', 11, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.signing_in', 0, 'Signing in...', 'en', 12, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.divider', 0, 'or continue with', 'en', 13, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.no_account', 0, 'Don''t have an account?', 'en', 14, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.create_account', 0, 'Create one now', 'en', 15, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.back_to_home', 0, '← Back to home', 'en', 16, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.panel_tagline', 0, E'Planification strat\u00e9gique propuls\u00e9e par l''IA', 'fr', 1, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.panel_subtitle', 0, E'Cr\u00e9ez des plans d''affaires pr\u00eats pour la banque en moins de 60 minutes', 'fr', 2, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.title', 0, 'Bon retour', 'fr', 3, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.subtitle', 0, 'Connectez-vous pour accéder à votre tableau de bord', 'fr', 4, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.email_label', 0, 'Adresse e-mail', 'fr', 5, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.email_placeholder', 0, 'vous@entreprise.com', 'fr', 6, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.password_label', 0, 'Mot de passe', 'fr', 7, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.password_placeholder', 0, 'Entrez votre mot de passe', 'fr', 8, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.remember', 0, 'Se souvenir de moi', 'fr', 9, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.forgot_password', 0, 'Mot de passe oublié?', 'fr', 10, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.button', 0, 'Se connecter', 'fr', 11, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.signing_in', 0, 'Connexion en cours...', 'fr', 12, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.divider', 0, 'ou continuer avec', 'fr', 13, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.no_account', 0, 'Vous n''avez pas de compte?', 'fr', 14, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.create_account', 0, 'Créer un compte', 'fr', 15, 'auth.login', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.login.back_to_home', 0, '← Retour à l''accueil', 'fr', 16, 'auth.login', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- AUTH - Signup, Forgot/Reset/Verify (EN + FR) - key blocks only
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.title', 0, 'Create your account', 'en', 1, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.subtitle', 0, 'Get started with your free 14-day trial', 'en', 2, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.panel_tagline', 0, 'Start Your Strategic Journey', 'en', 3, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.panel_subtitle', 0, 'Join thousands of entrepreneurs creating professional business plans', 'en', 4, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.firstname_label', 0, 'First Name', 'en', 5, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.lastname_label', 0, 'Last Name', 'en', 6, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.email_label', 0, 'Email Address', 'en', 7, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.organization_label', 0, 'Organization', 'en', 8, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.optional', 0, '(Optional)', 'en', 9, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.password_label', 0, 'Password', 'en', 10, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.confirm_label', 0, 'Confirm Password', 'en', 11, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.terms_prefix', 0, 'I agree to the', 'en', 12, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.terms_link', 0, 'Terms of Service', 'en', 13, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.and', 0, 'and', 'en', 14, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.privacy_link', 0, 'Privacy Policy', 'en', 15, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.button', 0, 'Create Account', 'en', 16, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.creating', 0, 'Creating account...', 'en', 17, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.divider', 0, 'or sign up with', 'en', 18, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.has_account', 0, 'Already have an account?', 'en', 19, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.signin_link', 0, 'Sign in instead', 'en', 20, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.back', 0, '← Back to home', 'en', 21, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.success', 0, 'Registration successful! Please sign in.', 'en', 22, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.description', 0, 'Create your free Sqordia account and start building professional business plans in under 60 minutes', 'en', 23, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.firstname_placeholder', 0, 'John', 'en', 24, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.lastname_placeholder', 0, 'Doe', 'en', 25, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.email_placeholder', 0, 'you@company.com', 'en', 26, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.organization_placeholder', 0, 'Your Company', 'en', 27, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.password_placeholder', 0, 'Create a strong password', 'en', 28, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.confirm_placeholder', 0, 'Re-enter your password', 'en', 29, 'auth.register', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.title', 0, E'Cr\u00e9ez votre compte', 'fr', 1, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.subtitle', 0, E'Commencez avec votre essai gratuit de 14 jours', 'fr', 2, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.panel_tagline', 0, 'Commencez votre parcours stratégique', 'fr', 3, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.panel_subtitle', 0, E'Rejoignez des milliers d''entrepreneurs qui cr\u00e9ent des plans d''affaires professionnels', 'fr', 4, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.firstname_label', 0, E'Pr\u00e9nom', 'fr', 5, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.lastname_label', 0, 'Nom de famille', 'fr', 6, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.email_label', 0, 'Adresse e-mail', 'fr', 7, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.organization_label', 0, 'Organisation', 'fr', 8, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.optional', 0, '(Optionnel)', 'fr', 9, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.password_label', 0, 'Mot de passe', 'fr', 10, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.confirm_label', 0, 'Confirmer le mot de passe', 'fr', 11, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.terms_prefix', 0, E'J''accepte les', 'fr', 12, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.terms_link', 0, E'Conditions d''utilisation', 'fr', 13, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.and', 0, 'et la', 'fr', 14, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.privacy_link', 0, 'Politique de confidentialité', 'fr', 15, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.button', 0, 'Créer un compte', 'fr', 16, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.creating', 0, 'Création du compte...', 'fr', 17, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.divider', 0, 'ou inscrivez-vous avec', 'fr', 18, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.has_account', 0, 'Vous avez déjà un compte?', 'fr', 19, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.signin_link', 0, 'Se connecter', 'fr', 20, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.back', 0, '← Retour à l''accueil', 'fr', 21, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.success', 0, 'Inscription réussie! Veuillez vous connecter.', 'fr', 22, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.description', 0, E'Cr\u00e9ez votre compte Sqordia gratuit et commencez \u00e0 r\u00e9diger des plans d''affaires professionnels en moins de 60 minutes', 'fr', 23, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.firstname_placeholder', 0, 'Jean', 'fr', 24, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.lastname_placeholder', 0, 'Dupont', 'fr', 25, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.email_placeholder', 0, 'vous@entreprise.com', 'fr', 26, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.organization_placeholder', 0, 'Votre entreprise', 'fr', 27, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.password_placeholder', 0, 'Créez un mot de passe fort', 'fr', 28, 'auth.register', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.signup.confirm_placeholder', 0, 'Confirmez votre mot de passe', 'fr', 29, 'auth.register', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- AUTH - Forgot password, Reset password, Verify email (EN only - same keys used with fallbacks)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.forgot_password.title', 0, 'Forgot password?', 'en', 1, 'auth.forgot_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.forgot_password.subtitle', 0, 'No worries! Enter your email and we''ll send you a reset link.', 'en', 2, 'auth.forgot_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.forgot_password.email_label', 0, 'Email Address', 'en', 3, 'auth.forgot_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.forgot_password.email_placeholder', 0, 'you@company.com', 'en', 4, 'auth.forgot_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.forgot_password.button', 0, 'Send Reset Link', 'en', 5, 'auth.forgot_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.forgot_password.sending', 0, 'Sending...', 'en', 6, 'auth.forgot_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.forgot_password.success_title', 0, 'Check your email', 'en', 7, 'auth.forgot_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.forgot_password.success_message', 0, 'We''ve sent a password reset link to', 'en', 8, 'auth.forgot_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.forgot_password.success_instructions', 0, 'Please check your inbox and click the link to reset your password.', 'en', 9, 'auth.forgot_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.forgot_password.success_back_to_login', 0, 'Back to Login', 'en', 10, 'auth.forgot_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.forgot_password.back_to_login', 0, 'Back to Login', 'en', 11, 'auth.forgot_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.title', 0, 'Set new password', 'en', 1, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.subtitle', 0, 'Create a strong password for your account', 'en', 2, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.new_password_label', 0, 'New Password', 'en', 3, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.confirm_password_label', 0, 'Confirm New Password', 'en', 4, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.button', 0, 'Reset Password', 'en', 5, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.resetting', 0, 'Resetting password...', 'en', 6, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.back_to_login', 0, 'Back to Login', 'en', 7, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.success_title', 0, 'Password reset successful!', 'en', 8, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.success_message', 0, 'Your password has been successfully updated. You can now sign in with your new password.', 'en', 9, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.success_redirecting', 0, 'Redirecting to login in', 'en', 10, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.success_seconds', 0, 'seconds', 'en', 11, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.success_second', 0, 'second', 'en', 12, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.token_error', 0, 'Invalid or missing reset token. Please', 'en', 13, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.token_error_request_new', 0, 'request a new link', 'en', 14, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.new_password_placeholder', 0, 'Enter new password', 'en', 15, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.reset_password.confirm_password_placeholder', 0, 'Re-enter new password', 'en', 16, 'auth.reset_password', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.loading_title', 0, 'Verifying your email...', 'en', 1, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.loading_message', 0, 'Please wait while we confirm your email address.', 'en', 2, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.success_title', 0, 'Email verified!', 'en', 3, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.success_message', 0, 'Your email address has been successfully verified. You can now access all features of your account.', 'en', 4, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.success_redirecting', 0, 'Redirecting to login in', 'en', 5, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.success_seconds', 0, 'seconds', 'en', 6, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.success_second', 0, 'second', 'en', 7, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.login_link', 0, 'Go to login now', 'en', 8, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.error_title', 0, 'Verification failed', 'en', 9, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.error_default_message', 0, 'The verification link may have expired or is invalid.', 'en', 10, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.resend_title', 0, 'Verify your email', 'en', 11, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.resend_message', 0, 'Please check your inbox for the verification email. Click the link to verify your email address.', 'en', 12, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.resend_button', 0, 'Resend Verification Email', 'en', 13, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.resend_sending', 0, 'Sending...', 'en', 14, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.resend_success', 0, 'Verification email sent! Please check your inbox.', 'en', 15, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.already_verified', 0, 'Already verified?', 'en', 16, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.help_text', 0, 'Didn''t receive an email? Check your spam folder or', 'en', 17, 'auth.verify_email', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'auth.verify_email.contact_support', 0, 'contact support', 'en', 18, 'auth.verify_email', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- QUESTIONNAIRE - Labels (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.loading', 0, 'Loading your questionnaire...', 'en', 1, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.seo_title', 0, 'Questionnaire', 'en', 2, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.seo_description', 0, 'Answer questions to create your business plan.', 'en', 3, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.progress', 0, 'Progress', 'en', 4, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.sections', 0, 'sections', 'en', 5, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.generation_title', 0, 'Generating Your Business Plan...', 'en', 6, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.generation_error_title', 0, 'Generation Error', 'en', 7, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.generation_complete_title', 0, 'Generation Complete!', 'en', 8, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.generation_redirecting', 0, 'Redirecting to your business plan...', 'en', 9, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.generation_preparing', 0, 'Preparing your personalized business plan', 'en', 10, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.did_you_know', 0, 'Did you know?', 'en', 11, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.cancel', 0, 'Cancel', 'en', 12, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.retry', 0, 'Retry', 'en', 13, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.estimated_time', 0, 'Estimated: 1-2 minutes remaining', 'en', 14, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.back_to_dashboard', 0, 'Back to Dashboard', 'en', 15, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.show_preview', 0, 'Show Preview', 'en', 16, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.hide_preview', 0, 'Hide Preview', 'en', 17, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.answered', 0, 'answered', 'en', 18, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.processing', 0, 'Processing', 'en', 19, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.generation_progress_label', 0, 'Generation progress', 'en', 20, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.generation_error_sr', 0, 'Generation error', 'en', 21, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.generation_complete_sr', 0, 'Generation complete, redirecting', 'en', 22, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.loading_questions', 0, 'Loading questions...', 'en', 23, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.live_preview', 0, 'Live Preview', 'en', 24, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.auto_updating', 0, 'Auto-updating', 'en', 25, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.empty_preview', 0, 'Start answering questions to see your preview here...', 'en', 26, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.no_questions_error', 0, 'No questions found for this persona. Please try again.', 'en', 27, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.generation_failed', 0, 'Business plan generation failed. Please try again.', 'en', 28, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.generation_start_failed', 0, 'Failed to start business plan generation. Please try again.', 'en', 29, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.loading', 0, 'Chargement de votre questionnaire...', 'fr', 1, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.seo_title', 0, 'Questionnaire', 'fr', 2, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.seo_description', 0, E'R\u00e9pondez aux questions pour cr\u00e9er votre plan d''affaires.', 'fr', 3, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.progress', 0, 'Progression', 'fr', 4, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.sections', 0, 'sections', 'fr', 5, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.back_to_dashboard', 0, 'Retour au tableau de bord', 'fr', 15, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.show_preview', 0, 'Afficher l''aperçu', 'fr', 16, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.hide_preview', 0, 'Masquer l''aperçu', 'fr', 17, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.answered', 0, 'répondu', 'fr', 18, 'questionnaire.labels', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- QUESTIONNAIRE - Step titles (EN + FR) - used in wizard progress
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.step_1_title', 0, 'Identity & Vision', 'en', 31, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.step_2_title', 0, 'The Offering', 'en', 32, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.step_3_title', 0, 'Market Analysis', 'en', 33, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.step_4_title', 0, 'Operations & People', 'en', 34, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.step_5_title', 0, 'Financials & Risks', 'en', 35, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.step_1_title', 0, E'Identit\u00e9 et Vision', 'fr', 31, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.step_2_title', 0, E'L''Offre', 'fr', 32, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.step_3_title', 0, E'Analyse du March\u00e9', 'fr', 33, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.step_4_title', 0, E'Op\u00e9rations et \u00c9quipe', 'fr', 34, 'questionnaire.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.step_5_title', 0, 'Finances et Risques', 'fr', 35, 'questionnaire.labels', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- QUESTIONNAIRE - Step Configuration (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
-- Step Configuration (EN)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step1.title', 0, 'Vision & Mission', 'en', 1, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step1.description', 0, 'Define your business identity, vision, and core mission', 'en', 2, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step1.icon', 0, 'target', 'en', 3, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step2.title', 0, 'Market & Customers', 'en', 4, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step2.description', 0, 'Identify your target market and understand your customers', 'en', 5, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step2.icon', 0, 'users', 'en', 6, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step3.title', 0, 'Products & Services', 'en', 7, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step3.description', 0, 'Detail your offerings and value proposition', 'en', 8, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step3.icon', 0, 'package', 'en', 9, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step4.title', 0, 'Strategy & Operations', 'en', 10, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step4.description', 0, 'Plan your operational strategy and execution approach', 'en', 11, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step4.icon', 0, 'settings', 'en', 12, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step5.title', 0, 'Financials & Growth', 'en', 13, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step5.description', 0, 'Project your financials and growth trajectory', 'en', 14, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step5.icon', 0, 'trending-up', 'en', 15, 'questionnaire.steps', NULL, NOW(), NOW(), false),
-- Step Configuration (FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step1.title', 0, 'Vision et Mission', 'fr', 1, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step1.description', 0, E'D\u00e9finissez l''identit\u00e9, la vision et la mission de votre entreprise', 'fr', 2, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step2.title', 0, E'March\u00e9 et Clients', 'fr', 4, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step2.description', 0, 'Identifiez votre marché cible et comprenez vos clients', 'fr', 5, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step3.title', 0, 'Produits et Services', 'fr', 7, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step3.description', 0, E'D\u00e9taillez vos offres et votre proposition de valeur', 'fr', 8, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step4.title', 0, E'Strat\u00e9gie et Op\u00e9rations', 'fr', 10, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step4.description', 0, E'Planifiez votre strat\u00e9gie op\u00e9rationnelle et votre approche d''ex\u00e9cution', 'fr', 11, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step5.title', 0, 'Finances et Croissance', 'fr', 13, 'questionnaire.steps', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.steps.step5.description', 0, E'Projetez vos finances et votre trajectoire de croissance', 'fr', 14, 'questionnaire.steps', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- QUESTIONNAIRE - Generation Tips (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
-- Generation Tips (EN)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.title', 0, 'Tips for Better Results', 'en', 1, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip1.title', 0, 'Be Specific', 'en', 2, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip1.description', 0, 'The more details you provide, the better your generated plan will be tailored to your business.', 'en', 3, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip2.title', 0, 'Use Real Numbers', 'en', 4, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip2.description', 0, 'Include actual financial figures, market sizes, and growth projections for more accurate analysis.', 'en', 5, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip3.title', 0, 'Review & Refine', 'en', 6, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip3.description', 0, 'You can always come back and update your answers to regenerate sections of your plan.', 'en', 7, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.did_you_know.title', 0, 'Did You Know?', 'en', 8, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.did_you_know.content', 0, 'Business plans that include detailed market research are 2x more likely to secure funding.', 'en', 9, 'questionnaire.tips', NULL, NOW(), NOW(), false),
-- Generation Tips (FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.title', 0, E'Conseils pour de meilleurs r\u00e9sultats', 'fr', 1, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip1.title', 0, E'Soyez pr\u00e9cis', 'fr', 2, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip1.description', 0, E'Plus vous fournissez de d\u00e9tails, mieux votre plan g\u00e9n\u00e9r\u00e9 sera adapt\u00e9 \u00e0 votre entreprise.', 'fr', 3, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip2.title', 0, E'Utilisez des chiffres r\u00e9els', 'fr', 4, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip2.description', 0, E'Incluez des chiffres financiers r\u00e9els, des tailles de march\u00e9 et des projections de croissance pour une analyse plus pr\u00e9cise.', 'fr', 5, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip3.title', 0, E'R\u00e9visez et affinez', 'fr', 6, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.tip3.description', 0, E'Vous pouvez toujours revenir et mettre \u00e0 jour vos r\u00e9ponses pour r\u00e9g\u00e9n\u00e9rer des sections de votre plan.', 'fr', 7, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.did_you_know.title', 0, 'Le saviez-vous?', 'fr', 8, 'questionnaire.tips', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'questionnaire.tips.did_you_know.content', 0, E'Les plans d''affaires incluant une \u00e9tude de march\u00e9 d\u00e9taill\u00e9e ont 2x plus de chances d''obtenir un financement.', 'fr', 9, 'questionnaire.tips', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- QUESTION TEMPLATES - Step Labels (EN + FR)
-- These labels appear in the CMS sidebar under "Questions" section
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
-- Step 1 Labels (EN + FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'question_templates.step1.label', 0, 'Step 1: Vision & Mission', 'en', 1, 'question_templates.step1', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'question_templates.step1.label', 0, E'\u00c9tape 1: Vision et Mission', 'fr', 1, 'question_templates.step1', NULL, NOW(), NOW(), false),
-- Step 2 Labels (EN + FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'question_templates.step2.label', 0, 'Step 2: Market & Customers', 'en', 1, 'question_templates.step2', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'question_templates.step2.label', 0, E'\u00c9tape 2: March\u00e9 et Clients', 'fr', 1, 'question_templates.step2', NULL, NOW(), NOW(), false),
-- Step 3 Labels (EN + FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'question_templates.step3.label', 0, 'Step 3: Products & Services', 'en', 1, 'question_templates.step3', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'question_templates.step3.label', 0, E'\u00c9tape 3: Produits et Services', 'fr', 1, 'question_templates.step3', NULL, NOW(), NOW(), false),
-- Step 4 Labels (EN + FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'question_templates.step4.label', 0, 'Step 4: Strategy & Operations', 'en', 1, 'question_templates.step4', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'question_templates.step4.label', 0, E'\u00c9tape 4: Strat\u00e9gie et Op\u00e9rations', 'fr', 1, 'question_templates.step4', NULL, NOW(), NOW(), false),
-- Step 5 Labels (EN + FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'question_templates.step5.label', 0, 'Step 5: Financials & Growth', 'en', 1, 'question_templates.step5', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'question_templates.step5.label', 0, E'\u00c9tape 5: Finances et Croissance', 'fr', 1, 'question_templates.step5', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- CREATE PLAN - Labels (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.seo_title', 0, 'Create Plan', 'en', 1, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.seo_description', 0, 'Create a new business plan or strategic plan with Sqordia.', 'en', 2, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.back', 0, 'Back to Dashboard', 'en', 3, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.ai_badge', 0, 'AI-Powered Business Plan', 'en', 4, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.header_title', 0, 'Create Your Business Plan', 'en', 5, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.subtitle', 0, 'Choose your plan type and answer guided questions to generate a comprehensive plan', 'en', 6, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.plan_type', 0, 'Plan Type', 'en', 7, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.create_organization', 0, 'Create Your Organization', 'en', 8, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.details', 0, 'Details', 'en', 9, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.business_plan_title', 0, 'Business Plan', 'en', 10, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.business_plan_desc', 0, 'Perfect for startups and growing businesses seeking funding or strategic direction', 'en', 11, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.business_features_1', 0, 'Market analysis & strategy', 'en', 12, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.business_features_2', 0, 'Financial projections', 'en', 13, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.business_features_3', 0, 'Investor-ready format', 'en', 14, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.get_started', 0, 'Get Started', 'en', 15, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.obnl_plan_title', 0, 'OBNL Strategic Plan', 'en', 16, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.obnl_plan_desc', 0, 'Designed for nonprofits seeking grants, donations, or strategic planning', 'en', 17, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.obnl_features_1', 0, 'Mission & impact focus', 'en', 18, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.obnl_features_2', 0, 'Grant application ready', 'en', 19, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.obnl_features_3', 0, 'Compliance tracking', 'en', 20, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.create_organization_desc', 0, 'Set up your organization to get started', 'en', 21, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.organization_name', 0, 'Organization Name', 'en', 22, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.org_name_placeholder', 0, 'e.g., TechStart Inc.', 'en', 23, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.organization_desc', 0, 'Description', 'en', 24, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.optional', 0, '(Optional)', 'en', 25, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.error', 0, 'Error', 'en', 26, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.error_creating_org', 0, 'Failed to create organization. Please try again.', 'en', 27, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.select_org_first', 0, 'Please create or select an organization first.', 'en', 28, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.header_title', 0, E'Cr\u00e9ez votre plan d''affaires', 'fr', 5, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.subtitle', 0, E'Choisissez le type de plan et r\u00e9pondez aux questions guid\u00e9es pour g\u00e9n\u00e9rer un plan complet', 'fr', 6, 'create_plan.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'create_plan.back', 0, 'Retour au tableau de bord', 'fr', 3, 'create_plan.labels', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- SUBSCRIPTION - Labels (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.seo_title', 0, 'Choose Your Plan | Sqordia', 'en', 1, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.seo_description', 0, 'Select the plan that fits your business. Start free, upgrade when you need more.', 'en', 2, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.page_title', 0, 'Choose Your Plan', 'en', 3, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.monthly', 0, 'Monthly', 'en', 4, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.yearly', 0, 'Yearly', 'en', 5, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.popular', 0, 'Most Popular', 'en', 6, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.login_required', 0, 'Please log in to subscribe to a plan.', 'en', 7, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.org_required', 0, 'Please create an organization first. Refreshing page...', 'en', 8, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.plan_changed', 0, 'Successfully changed to {plan}!', 'en', 9, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.subscribed', 0, 'Successfully subscribed to {plan}!', 'en', 10, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.error', 0, 'Failed to subscribe', 'en', 11, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.processing', 0, 'Processing...', 'en', 12, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.get_started', 0, 'Get Started', 'en', 13, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.subscribe', 0, 'Subscribe', 'en', 14, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.page_title', 0, E'Choisissez votre plan', 'fr', 3, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.monthly', 0, 'Mensuel', 'fr', 4, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.yearly', 0, 'Annuel', 'fr', 5, 'subscription.labels', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'subscription.popular', 0, 'Le plus populaire', 'fr', 6, 'subscription.labels', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- GLOBAL - Branding, Social, Footer (used by Header, Footer)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.branding.logo_url', 3, '', 'en', 1, 'global.branding', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.social.twitter', 3, 'https://twitter.com/sqordia', 'en', 1, 'global.social', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.social.linkedin', 3, 'https://linkedin.com/company/sqordia', 'en', 2, 'global.social', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.social.facebook', 3, 'https://facebook.com/sqordia', 'en', 3, 'global.social', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.social.instagram', 3, 'https://instagram.com/sqordia', 'en', 4, 'global.social', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.social.email', 3, 'mailto:hello@sqordia.com', 'en', 5, 'global.social', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.footer.tagline', 0, 'AI-enhanced strategic planning that transforms your ideas into investor-ready business plans in under 60 minutes.', 'en', 1, 'global.footer', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.footer.copyright', 0, 'All rights reserved.', 'en', 2, 'global.footer', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.branding.logo_url', 3, '', 'fr', 1, 'global.branding', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.footer.tagline', 0, E'Planification strat\u00e9gique am\u00e9lior\u00e9e par l''IA qui transforme vos id\u00e9es en plans d''affaires pr\u00eats pour investisseurs en moins de 60 minutes.', 'fr', 1, 'global.footer', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.footer.copyright', 0, 'Tous droits r\u00e9serv\u00e9s.', 'fr', 2, 'global.footer', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- GLOBAL - Contact Information (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
-- Contact info (EN)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.contact.email', 0, 'hello@sqordia.com', 'en', 1, 'global.contact', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.contact.phone', 0, '+1 (514) 555-0123', 'en', 2, 'global.contact', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.contact.address', 0, 'Montreal, QC, Canada', 'en', 3, 'global.contact', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.contact.support_email', 0, 'support@sqordia.com', 'en', 4, 'global.contact', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.contact.support_hours', 0, 'Monday - Friday, 9am - 5pm EST', 'en', 5, 'global.contact', NULL, NOW(), NOW(), false),
-- Contact info (FR)
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.contact.email', 0, 'hello@sqordia.com', 'fr', 1, 'global.contact', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.contact.phone', 0, '+1 (514) 555-0123', 'fr', 2, 'global.contact', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.contact.address', 0, E'Montr\u00e9al, QC, Canada', 'fr', 3, 'global.contact', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.contact.support_email', 0, 'support@sqordia.com', 'fr', 4, 'global.contact', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'global.contact.support_hours', 0, 'Lundi - Vendredi, 9h - 17h HNE', 'fr', 5, 'global.contact', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- GLOBAL - Sidebar / Navigation (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.main_menu', 0, 'Main Menu', 'en', 1, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.admin', 0, 'Administration', 'en', 2, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.logout', 0, 'Logout', 'en', 3, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.dashboard', 0, 'Dashboard', 'en', 4, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.create_plan', 0, 'Create Plan', 'en', 5, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.my_plans', 0, 'My Plans', 'en', 6, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.subscription', 0, 'Subscription', 'en', 7, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.invoices', 0, 'Invoices', 'en', 8, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.profile', 0, 'Profile', 'en', 9, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.settings', 0, 'Settings', 'en', 10, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.admin_panel', 0, 'Admin Panel', 'en', 11, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.main_menu', 0, 'Menu principal', 'fr', 1, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.admin', 0, 'Administration', 'fr', 2, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.logout', 0, 'Déconnexion', 'fr', 3, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.dashboard', 0, 'Tableau de bord', 'fr', 4, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.create_plan', 0, 'Créer un plan', 'fr', 5, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.my_plans', 0, 'Mes plans', 'fr', 6, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.subscription', 0, 'Abonnement', 'fr', 7, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.invoices', 0, 'Factures', 'fr', 8, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.profile', 0, 'Profil', 'fr', 9, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.settings', 0, 'Paramètres', 'fr', 10, 'global.navigation', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'sidebar.admin_panel', 0, 'Panneau d''administration', 'fr', 11, 'global.navigation', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

-- =============================================================================
-- LEGAL - Terms & Privacy (EN + FR)
-- =============================================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'legal.back_to_home', 0, 'Back to Home', 'en', 1, 'legal.terms', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'legal.terms_title', 0, 'Terms of Service', 'en', 2, 'legal.terms', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'legal.terms_last_updated', 0, 'Last updated: ', 'en', 3, 'legal.terms', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'legal.privacy_title', 0, 'Privacy Policy', 'en', 4, 'legal.privacy', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'legal.privacy_last_updated', 0, 'Last updated: ', 'en', 5, 'legal.privacy', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'legal.back_to_home', 0, E'Retour \u00e0 l''accueil', 'fr', 1, 'legal.terms', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'legal.terms_title', 0, E'Conditions d''utilisation', 'fr', 2, 'legal.terms', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'legal.terms_last_updated', 0, 'Dernière mise à jour : ', 'fr', 3, 'legal.terms', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'legal.privacy_title', 0, 'Politique de confidentialité', 'fr', 4, 'legal.privacy', NULL, NOW(), NOW(), false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'legal.privacy_last_updated', 0, 'Dernière mise à jour : ', 'fr', 5, 'legal.privacy', NULL, NOW(), NOW(), false)
ON CONFLICT ("CmsVersionId", "BlockKey", "Language") DO UPDATE SET "BlockType" = EXCLUDED."BlockType", "Content" = EXCLUDED."Content", "SortOrder" = EXCLUDED."SortOrder", "SectionKey" = EXCLUDED."SectionKey", "Metadata" = EXCLUDED."Metadata", "LastModified" = NOW(), "IsDeleted" = false;

COMMIT;
