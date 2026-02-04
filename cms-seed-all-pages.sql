-- CMS Content Blocks Seed: All Pages (Dashboard, Profile)
-- Version ID: 17a4a74e-4782-4ca0-9493-aebbd22dcc95
-- Block Types: Text=0, RichText=1, Image=2, Link=3, Json=4, Number=5, Boolean=6

-- ============================================================
-- DASHBOARD - Labels (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.title', 0, 'Dashboard', 'en', 1, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.description', 0, 'Manage your business plans and projects', 'en', 2, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.welcome', 0, 'Welcome Back', 'en', 3, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.subtitle', 0, 'Manage your business plans and track your progress', 'en', 4, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.newPlan', 0, 'New Plan', 'en', 5, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.totalPlans', 0, 'Total Plans', 'en', 6, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.activePlans', 0, 'Active Plans', 'en', 7, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.recentPlans', 0, 'Recent Plans', 'en', 8, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.completionRate', 0, 'Completion Rate', 'en', 9, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createNextPlan', 0, 'Create Your Next Business Plan', 'en', 10, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createNextPlanDesc', 0, 'Start building your success story with AI-powered planning', 'en', 11, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.getStarted', 0, 'Get Started', 'en', 12, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.yourPlans', 0, 'Your Business Plans', 'en', 13, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createFirstPlan', 0, 'Create Your First Plan', 'en', 14, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.view', 0, 'View', 'en', 15, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.resume', 0, 'Resume', 'en', 16, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicate', 0, 'Duplicate', 'en', 17, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.delete', 0, 'Delete', 'en', 18, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deletePlan', 0, 'Delete Business Plan', 'en', 19, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteConfirm', 0, 'Are you sure you want to delete', 'en', 20, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteWarning', 0, 'All associated data will be permanently deleted', 'en', 21, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.cancel', 0, 'Cancel', 'en', 22, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.loading', 0, 'Loading your dashboard...', 'en', 23, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.showTour', 0, 'Show Tour', 'en', 24, 'dashboard.labels', NULL, NOW(), NULL, false);

-- DASHBOARD - Labels (FR)
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.title', 0, 'Tableau de bord', 'fr', 1, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.description', 0, E'G\u00e9rez vos plans d''affaires et vos projets', 'fr', 2, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.welcome', 0, 'Bon retour', 'fr', 3, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.subtitle', 0, E'G\u00e9rez vos plans d''affaires et suivez votre progression', 'fr', 4, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.newPlan', 0, 'Nouveau plan', 'fr', 5, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.totalPlans', 0, 'Plans totaux', 'fr', 6, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.activePlans', 0, 'Plans actifs', 'fr', 7, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.recentPlans', 0, E'Plans r\u00e9cents', 'fr', 8, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.completionRate', 0, E'Taux de compl\u00e9tion', 'fr', 9, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createNextPlan', 0, E'Cr\u00e9ez votre prochain plan d''affaires', 'fr', 10, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createNextPlanDesc', 0, E'Commencez \u00e0 construire votre histoire de succ\u00e8s avec la planification propuls\u00e9e par l''IA', 'fr', 11, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.getStarted', 0, 'Commencer', 'fr', 12, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.yourPlans', 0, E'Vos plans d''affaires', 'fr', 13, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.createFirstPlan', 0, E'Cr\u00e9ez votre premier plan', 'fr', 14, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.view', 0, 'Voir', 'fr', 15, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.resume', 0, 'Reprendre', 'fr', 16, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.duplicate', 0, 'Dupliquer', 'fr', 17, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.delete', 0, 'Supprimer', 'fr', 18, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deletePlan', 0, E'Supprimer le plan d''affaires', 'fr', 19, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteConfirm', 0, E'\u00cates-vous s\u00fbr de vouloir supprimer', 'fr', 20, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.deleteWarning', 0, E'Toutes les donn\u00e9es associ\u00e9es seront d\u00e9finitivement supprim\u00e9es', 'fr', 21, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.cancel', 0, 'Annuler', 'fr', 22, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.loading', 0, 'Chargement de votre tableau de bord...', 'fr', 23, 'dashboard.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.showTour', 0, 'Afficher le guide', 'fr', 24, 'dashboard.labels', NULL, NOW(), NULL, false);

-- DASHBOARD - Empty States (EN)
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noPlans', 0, 'No business plans yet', 'en', 1, 'dashboard.empty_states', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noPlansDesc', 1, 'Get started by creating your first business plan. It only takes a few minutes to begin your journey.', 'en', 2, 'dashboard.empty_states', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noDescription', 0, 'No description provided', 'en', 3, 'dashboard.empty_states', NULL, NOW(), NULL, false);

-- DASHBOARD - Empty States (FR)
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noPlans', 0, E'Aucun plan d''affaires pour le moment', 'fr', 1, 'dashboard.empty_states', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noPlansDesc', 1, E'Commencez en cr\u00e9ant votre premier plan d''affaires. Cela ne prend que quelques minutes pour commencer votre parcours.', 'fr', 2, 'dashboard.empty_states', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.noDescription', 0, 'Aucune description fournie', 'fr', 3, 'dashboard.empty_states', NULL, NOW(), NULL, false);

-- DASHBOARD - Tips & Tour (EN)
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.welcome.title', 0, 'Welcome to Your Dashboard!', 'en', 1, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.welcome.description', 1, 'This is your command center. Here you can view all your business plans, track progress, and create new plans.', 'en', 2, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.stats.title', 0, 'Track Your Progress', 'en', 3, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.stats.description', 1, 'Monitor your total plans, active projects, recent activity, and completion rates at a glance.', 'en', 4, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.createPlan.title', 0, 'Create New Plans', 'en', 5, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.createPlan.description', 1, 'Click here to start creating a new business plan. Our AI will guide you through the process step by step.', 'en', 6, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.plansList.title', 0, 'Manage Your Plans', 'en', 7, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.plansList.description', 1, 'View, edit, duplicate, or delete your business plans. All your work is organized here.', 'en', 8, 'dashboard.tips', NULL, NOW(), NULL, false);

-- DASHBOARD - Tips & Tour (FR)
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.welcome.title', 0, 'Bienvenue sur votre tableau de bord!', 'fr', 1, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.welcome.description', 1, E'C''est votre centre de commande. Ici, vous pouvez voir tous vos plans d''affaires, suivre la progression et cr\u00e9er de nouveaux plans.', 'fr', 2, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.stats.title', 0, 'Suivez votre progression', 'fr', 3, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.stats.description', 1, E'Surveillez vos plans totaux, projets actifs, activit\u00e9 r\u00e9cente et taux de compl\u00e9tion en un coup d''\u0153il.', 'fr', 4, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.createPlan.title', 0, E'Cr\u00e9er de nouveaux plans', 'fr', 5, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.createPlan.description', 1, E'Cliquez ici pour commencer \u00e0 cr\u00e9er un nouveau plan d''affaires. Notre IA vous guidera \u00e9tape par \u00e9tape.', 'fr', 6, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.plansList.title', 0, E'G\u00e9rez vos plans', 'fr', 7, 'dashboard.tips', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'dashboard.tour.plansList.description', 1, E'Visualisez, modifiez, dupliquez ou supprimez vos plans d''affaires. Tout votre travail est organis\u00e9 ici.', 'fr', 8, 'dashboard.tips', NULL, NOW(), NULL, false);

-- ============================================================
-- PROFILE - Labels (EN)
-- ============================================================
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.page_title', 0, 'Profile Settings', 'en', 1, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.page_description', 1, 'Manage your account settings and preferences', 'en', 2, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.back_to_dashboard', 0, 'Back to Dashboard', 'en', 3, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_profile', 0, 'Profile', 'en', 4, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_security', 0, 'Security', 'en', 5, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_sessions', 0, 'Sessions', 'en', 6, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.profile_picture_label', 0, 'Profile Picture', 'en', 7, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.first_name_label', 0, 'First Name', 'en', 8, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.last_name_label', 0, 'Last Name', 'en', 9, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.email_label', 0, 'Email', 'en', 10, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.phone_number_label', 0, 'Phone Number', 'en', 11, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.company_label', 0, 'Company', 'en', 12, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.address_label', 0, 'Address', 'en', 13, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.profile_type_label', 0, 'Profile Type', 'en', 14, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.entrepreneur_label', 0, 'Entrepreneur / Solopreneur', 'en', 15, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.consultant_label', 0, 'Consultant', 'en', 16, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.obnl_label', 0, 'OBNL / NPO', 'en', 17, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.save_changes_button', 0, 'Save Changes', 'en', 18, 'profile.labels', NULL, NOW(), NULL, false);

-- PROFILE - Labels (FR)
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.page_title', 0, E'Param\u00e8tres du Profil', 'fr', 1, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.page_description', 1, E'G\u00e9rez vos param\u00e8tres de compte et vos pr\u00e9f\u00e9rences', 'fr', 2, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.back_to_dashboard', 0, 'Retour au Tableau de Bord', 'fr', 3, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_profile', 0, 'Profil', 'fr', 4, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_security', 0, E'S\u00e9curit\u00e9', 'fr', 5, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.tab_sessions', 0, 'Sessions', 'fr', 6, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.profile_picture_label', 0, 'Photo de Profil', 'fr', 7, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.first_name_label', 0, E'Pr\u00e9nom', 'fr', 8, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.last_name_label', 0, 'Nom de Famille', 'fr', 9, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.email_label', 0, 'Adresse E-mail', 'fr', 10, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.phone_number_label', 0, E'Num\u00e9ro de T\u00e9l\u00e9phone', 'fr', 11, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.company_label', 0, 'Entreprise', 'fr', 12, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.address_label', 0, 'Adresse', 'fr', 13, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.profile_type_label', 0, 'Type de Profil', 'fr', 14, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.entrepreneur_label', 0, E'Entrepreneur / Travailleur Ind\u00e9pendant', 'fr', 15, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.consultant_label', 0, 'Consultant', 'fr', 16, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.obnl_label', 0, 'OBNL / ONG', 'fr', 17, 'profile.labels', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.save_changes_button', 0, 'Enregistrer les Modifications', 'fr', 18, 'profile.labels', NULL, NOW(), NULL, false);

-- PROFILE - Security (EN)
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.change_password_heading', 0, 'Change Password', 'en', 1, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.current_password_label', 0, 'Current Password', 'en', 2, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.new_password_label', 0, 'New Password', 'en', 3, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.confirm_new_password_label', 0, 'Confirm New Password', 'en', 4, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.update_password_button', 0, 'Update Password', 'en', 5, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.two_factor_heading', 0, 'Two-Factor Authentication', 'en', 6, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.two_factor_description', 1, 'Add an extra layer of security to your account', 'en', 7, 'profile.security', NULL, NOW(), NULL, false);

-- PROFILE - Security (FR)
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.change_password_heading', 0, 'Changer le Mot de Passe', 'fr', 1, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.current_password_label', 0, 'Mot de Passe Actuel', 'fr', 2, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.new_password_label', 0, 'Nouveau Mot de Passe', 'fr', 3, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.confirm_new_password_label', 0, 'Confirmer le Nouveau Mot de Passe', 'fr', 4, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.update_password_button', 0, E'Mettre \u00e0 Jour le Mot de Passe', 'fr', 5, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.two_factor_heading', 0, E'Authentification \u00e0 Deux Facteurs', 'fr', 6, 'profile.security', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.two_factor_description', 1, E'Ajoutez une couche de s\u00e9curit\u00e9 suppl\u00e9mentaire \u00e0 votre compte', 'fr', 7, 'profile.security', NULL, NOW(), NULL, false);

-- PROFILE - Sessions (EN)
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.active_sessions_heading', 0, 'Active Sessions', 'en', 1, 'profile.sessions', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.active_sessions_description', 1, 'Manage your active sessions across devices', 'en', 2, 'profile.sessions', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.revoke_all_sessions_button', 0, 'Revoke All Other Sessions', 'en', 3, 'profile.sessions', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.unknown_device', 0, 'Unknown Device', 'en', 4, 'profile.sessions', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.current_session_badge', 0, 'Current', 'en', 5, 'profile.sessions', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.revoke_session_button', 0, 'Revoke', 'en', 6, 'profile.sessions', NULL, NOW(), NULL, false);

-- PROFILE - Sessions (FR)
INSERT INTO "CmsContentBlocks" ("Id", "CmsVersionId", "BlockKey", "BlockType", "Content", "Language", "SortOrder", "SectionKey", "Metadata", "Created", "LastModified", "IsDeleted")
VALUES
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.active_sessions_heading', 0, 'Sessions Actives', 'fr', 1, 'profile.sessions', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.active_sessions_description', 1, E'G\u00e9rez vos sessions actives sur tous les appareils', 'fr', 2, 'profile.sessions', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.revoke_all_sessions_button', 0, E'R\u00e9voquer Toutes les Autres Sessions', 'fr', 3, 'profile.sessions', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.unknown_device', 0, 'Appareil Inconnu', 'fr', 4, 'profile.sessions', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.current_session_badge', 0, 'Actuelle', 'fr', 5, 'profile.sessions', NULL, NOW(), NULL, false),
(gen_random_uuid(), '17a4a74e-4782-4ca0-9493-aebbd22dcc95', 'profile.revoke_session_button', 0, E'R\u00e9voquer', 'fr', 6, 'profile.sessions', NULL, NOW(), NULL, false);
