-- =============================================================================
-- Migration: 20260204173221_AddCmsVersioningSystem
-- Run this against the production PostgreSQL database.
-- Everything is wrapped in a transaction — if any step fails, nothing is applied.
-- =============================================================================

BEGIN;

-- =============================================
-- 1. ALTER "Templates" columns
-- =============================================

-- 1a. UpdatedBy: text -> varchar(200) (safe, string-to-string)
ALTER TABLE "Templates"
    ALTER COLUMN "UpdatedBy" TYPE character varying(200);

-- 1b. Type: integer -> varchar(50) (requires CASE mapping)
ALTER TABLE "Templates"
    ALTER COLUMN "Type" TYPE character varying(50)
    USING CASE "Type"
        WHEN 1 THEN 'Standard'
        WHEN 2 THEN 'Premium'
        WHEN 3 THEN 'Custom'
        WHEN 4 THEN 'IndustrySpecific'
        WHEN 5 THEN 'Regional'
        WHEN 6 THEN 'LanguageSpecific'
        WHEN 7 THEN 'SizeSpecific'
        WHEN 8 THEN 'SectorSpecific'
        WHEN 9 THEN 'ComplianceSpecific'
        WHEN 10 THEN 'FundingSpecific'
        WHEN 11 THEN 'Other'
        ELSE 'Standard'
    END;

-- 1c. Tags: text -> varchar(500) (safe, string-to-string)
ALTER TABLE "Templates"
    ALTER COLUMN "Tags" TYPE character varying(500);

-- 1d. Status: integer -> varchar(50) (requires CASE mapping)
ALTER TABLE "Templates"
    ALTER COLUMN "Status" TYPE character varying(50)
    USING CASE "Status"
        WHEN 1 THEN 'Draft'
        WHEN 2 THEN 'Review'
        WHEN 3 THEN 'Approved'
        WHEN 4 THEN 'Published'
        WHEN 5 THEN 'Archived'
        WHEN 6 THEN 'Deprecated'
        WHEN 7 THEN 'UnderMaintenance'
        WHEN 8 THEN 'PendingApproval'
        WHEN 9 THEN 'Rejected'
        WHEN 10 THEN 'Other'
        ELSE 'Draft'
    END;
ALTER TABLE "Templates" ALTER COLUMN "Status" SET DEFAULT 'Draft';
ALTER TABLE "Templates" ALTER COLUMN "Status" SET NOT NULL;

-- 1e. PreviewImage: text -> varchar(500) (safe, string-to-string)
ALTER TABLE "Templates"
    ALTER COLUMN "PreviewImage" TYPE character varying(500);

-- 1f. Description: varchar(1000) -> varchar(2000) (safe, widening)
ALTER TABLE "Templates"
    ALTER COLUMN "Description" TYPE character varying(2000);

-- 1g. CreatedBy: text -> varchar(200) (safe, string-to-string)
ALTER TABLE "Templates"
    ALTER COLUMN "CreatedBy" TYPE character varying(200);

-- 1h. Category: integer -> varchar(50) (requires CASE mapping)
ALTER TABLE "Templates"
    ALTER COLUMN "Category" TYPE character varying(50)
    USING CASE "Category"
        WHEN 1 THEN 'BusinessPlan'
        WHEN 2 THEN 'FinancialProjection'
        WHEN 3 THEN 'MarketingPlan'
        WHEN 4 THEN 'OperationsPlan'
        WHEN 5 THEN 'RiskAssessment'
        WHEN 6 THEN 'ExecutiveSummary'
        WHEN 7 THEN 'CompanyProfile'
        WHEN 8 THEN 'MarketAnalysis'
        WHEN 9 THEN 'CompetitiveAnalysis'
        WHEN 10 THEN 'SalesPlan'
        WHEN 11 THEN 'HRPlan'
        WHEN 12 THEN 'TechnologyPlan'
        WHEN 13 THEN 'SustainabilityPlan'
        WHEN 14 THEN 'ExitStrategy'
        WHEN 15 THEN 'LegalCompliance'
        WHEN 16 THEN 'Other'
        ELSE 'BusinessPlan'
    END;

-- =============================================
-- 2. ALTER "FinancialCells" column
-- =============================================

-- CellType: change default from 'number' to 'Number' (case change)
ALTER TABLE "FinancialCells" ALTER COLUMN "CellType" SET DEFAULT 'Number';

-- =============================================
-- 3. CREATE CMS tables
-- =============================================

CREATE TABLE "CmsAssets" (
    "Id"               uuid                        NOT NULL,
    "FileName"         character varying(500)       NOT NULL,
    "ContentType"      character varying(100)       NOT NULL,
    "Url"              character varying(2000)      NOT NULL,
    "FileSize"         bigint                       NOT NULL,
    "UploadedByUserId" uuid                        NOT NULL,
    "Category"         character varying(100)       NOT NULL,
    "Created"          timestamp with time zone     NOT NULL,
    "CreatedBy"        text,
    "LastModified"     timestamp with time zone,
    "LastModifiedBy"   text,
    "IsDeleted"        boolean                      NOT NULL,
    "DeletedAt"        timestamp with time zone,
    "DeletedBy"        text,
    CONSTRAINT "PK_CmsAssets" PRIMARY KEY ("Id")
);

CREATE TABLE "CmsVersions" (
    "Id"              uuid                        NOT NULL,
    "VersionNumber"   integer                     NOT NULL,
    "Status"          integer                     NOT NULL,
    "CreatedByUserId" uuid                        NOT NULL,
    "PublishedAt"     timestamp with time zone,
    "PublishedByUserId" uuid,
    "Notes"           character varying(500),
    "Created"         timestamp with time zone     NOT NULL,
    "CreatedBy"       text,
    "LastModified"    timestamp with time zone,
    "LastModifiedBy"  text,
    "IsDeleted"       boolean                      NOT NULL,
    "DeletedAt"       timestamp with time zone,
    "DeletedBy"       text,
    CONSTRAINT "PK_CmsVersions" PRIMARY KEY ("Id")
);

CREATE TABLE "CmsContentBlocks" (
    "Id"             uuid                        NOT NULL,
    "CmsVersionId"   uuid                        NOT NULL,
    "BlockKey"       character varying(200)       NOT NULL,
    "BlockType"      integer                     NOT NULL,
    "Content"        text                        NOT NULL,
    "Language"       character varying(5)         NOT NULL,
    "SortOrder"      integer                     NOT NULL,
    "SectionKey"     character varying(200)       NOT NULL,
    "Metadata"       character varying(2000),
    "Created"        timestamp with time zone     NOT NULL,
    "CreatedBy"      text,
    "LastModified"   timestamp with time zone,
    "LastModifiedBy" text,
    "IsDeleted"      boolean                      NOT NULL,
    "DeletedAt"      timestamp with time zone,
    "DeletedBy"      text,
    CONSTRAINT "PK_CmsContentBlocks" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CmsContentBlocks_CmsVersions_CmsVersionId"
        FOREIGN KEY ("CmsVersionId") REFERENCES "CmsVersions" ("Id") ON DELETE CASCADE
);

-- =============================================
-- 4. CREATE indexes
-- =============================================

-- Templates indexes
CREATE INDEX "IX_Templates_Author"          ON "Templates" ("Author");
CREATE INDEX "IX_Templates_Category"        ON "Templates" ("Category");
CREATE INDEX "IX_Templates_Category_Status" ON "Templates" ("Category", "Status");
CREATE INDEX "IX_Templates_IsPublic"        ON "Templates" ("IsPublic");
CREATE INDEX "IX_Templates_Name"            ON "Templates" ("Name");
CREATE INDEX "IX_Templates_Status"          ON "Templates" ("Status");

-- CmsAssets indexes
CREATE INDEX "IX_CmsAssets_Category"         ON "CmsAssets" ("Category");
CREATE INDEX "IX_CmsAssets_UploadedByUserId" ON "CmsAssets" ("UploadedByUserId");

-- CmsContentBlocks indexes
CREATE UNIQUE INDEX "IX_CmsContentBlocks_CmsVersionId_BlockKey_Language"
    ON "CmsContentBlocks" ("CmsVersionId", "BlockKey", "Language");
CREATE INDEX "IX_CmsContentBlocks_SectionKey" ON "CmsContentBlocks" ("SectionKey");

-- CmsVersions indexes
CREATE INDEX "IX_CmsVersions_Status" ON "CmsVersions" ("Status");

-- =============================================
-- 5. Record migration in EF Core history
--    This tells EF Core this migration has been applied,
--    so it won't try to run it again on next startup.
-- =============================================

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260204173221_AddCmsVersioningSystem', '8.0.10');

COMMIT;
