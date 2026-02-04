-- Missing Dashboard blocks (EN)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.plan',0,'plan','en',28,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.plansTotal',0,'plans total','en',29,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.duplicatePlan',0,'Duplicate','en',30,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.status.draft',0,'Draft','en',31,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.status.completed',0,'Completed','en',32,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.status.active',0,'Active','en',33,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.status.inProgress',0,'In Progress','en',34,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.deleting',0,'Deleting...','en',35,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.deletePlanButton',0,'Delete Plan','en',36,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.deleteSuccess',0,'Plan deleted','en',37,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.deleteSuccessDesc',0,'Your plan has been deleted successfully','en',38,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.deleteError',0,'Failed to delete plan','en',39,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.duplicateSuccess',0,'Plan duplicated','en',40,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.duplicateSuccessDesc',0,'Your plan has been duplicated successfully','en',41,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.duplicateError',0,'Failed to duplicate plan','en',42,'dashboard.labels',NULL,NOW(),NOW(),false);

-- Missing Dashboard blocks (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.plan',0,'plan','fr',28,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.plansTotal',0,'plans au total','fr',29,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.duplicatePlan',0,'Dupliquer','fr',30,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.status.draft',0,'Brouillon','fr',31,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.status.completed',0,E'Termin\u00e9','fr',32,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.status.active',0,'Actif','fr',33,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.status.inProgress',0,'En cours','fr',34,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.deleting',0,'Suppression...','fr',35,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.deletePlanButton',0,'Supprimer le plan','fr',36,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.deleteSuccess',0,E'Plan supprim\u00e9','fr',37,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.deleteSuccessDesc',0,E'Votre plan a \u00e9t\u00e9 supprim\u00e9 avec succ\u00e8s','fr',38,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.deleteError',0,E'\u00c9chec de la suppression du plan','fr',39,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.duplicateSuccess',0,E'Plan dupliqu\u00e9','fr',40,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.duplicateSuccessDesc',0,E'Votre plan a \u00e9t\u00e9 dupliqu\u00e9 avec succ\u00e8s','fr',41,'dashboard.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','dashboard.duplicateError',0,E'\u00c9chec de la duplication du plan','fr',42,'dashboard.labels',NULL,NOW(),NOW(),false);

-- Missing Profile blocks (EN)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.uploading',0,'Uploading...','en',19,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.upload_from_device',0,'Upload from device','en',20,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.url_placeholder',0,'Or enter a URL to your profile picture','en',21,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.picture_help',0,'Upload an image (max 5MB) or enter a URL. Supported formats: JPEG, PNG, GIF, WebP','en',22,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.email_cant_change',0,'Email cannot be changed','en',23,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.saving',0,'Saving...','en',24,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.entrepreneur_desc',0,'Building your own business from the ground up','en',25,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.consultant_desc',0,'Managing multiple clients and strategic planning','en',26,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.obnl_desc',0,'Strategic planning for non-profit organizations','en',27,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.no_profile_type',0,'No profile type selected','en',28,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.select_profile_type',0,'Select profile type','en',29,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.change',0,'Change','en',30,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.updating',0,'Updating...','en',8,'profile.security',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.enabled',0,'Enabled','en',9,'profile.security',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.disabled',0,'Disabled','en',10,'profile.security',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.no_sessions',0,'No active sessions','en',7,'profile.sessions',NULL,NOW(),NOW(),false);

-- Missing Profile blocks (FR)
INSERT INTO "CmsContentBlocks" ("Id","CmsVersionId","BlockKey","BlockType","Content","Language","SortOrder","SectionKey","Metadata","Created","LastModified","IsDeleted")
VALUES
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.uploading',0,E'T\u00e9l\u00e9versement...','fr',19,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.upload_from_device',0,E'T\u00e9l\u00e9verser depuis l''appareil','fr',20,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.url_placeholder',0,'Ou entrez une URL vers votre photo de profil','fr',21,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.picture_help',0,E'T\u00e9l\u00e9versez une image (max 5 Mo) ou entrez une URL. Formats : JPEG, PNG, GIF, WebP','fr',22,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.email_cant_change',0,E'L''email ne peut pas \u00eatre modifi\u00e9','fr',23,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.saving',0,'Enregistrement...','fr',24,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.entrepreneur_desc',0,E'Construisez votre entreprise \u00e0 partir de z\u00e9ro','fr',25,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.consultant_desc',0,E'G\u00e9rez plusieurs clients et la planification strat\u00e9gique','fr',26,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.obnl_desc',0,E'Planification strat\u00e9gique pour les organismes \u00e0 but non lucratif','fr',27,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.no_profile_type',0,E'Aucun type de profil s\u00e9lectionn\u00e9','fr',28,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.select_profile_type',0,E'S\u00e9lectionner un type de profil','fr',29,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.change',0,'Modifier','fr',30,'profile.labels',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.updating',0,E'Mise \u00e0 jour...','fr',8,'profile.security',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.enabled',0,E'Activ\u00e9','fr',9,'profile.security',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.disabled',0,E'D\u00e9sactiv\u00e9','fr',10,'profile.security',NULL,NOW(),NOW(),false),
(gen_random_uuid(),'17a4a74e-4782-4ca0-9493-aebbd22dcc95','profile.no_sessions',0,'Aucune session active','fr',7,'profile.sessions',NULL,NOW(),NOW(),false);
