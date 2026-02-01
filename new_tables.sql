-- Create FinancialCells table
CREATE TABLE IF NOT EXISTS "FinancialCells" (
    "Id" uuid NOT NULL,
    "BusinessPlanId" uuid NOT NULL,
    "SheetName" character varying(100) NOT NULL DEFAULT 'Main',
    "RowId" character varying(200) NOT NULL,
    "ColumnId" character varying(100) NOT NULL,
    "Value" numeric(18,4) NOT NULL,
    "Formula" character varying(1000),
    "IsCalculated" boolean NOT NULL DEFAULT false,
    "CellType" character varying(50) NOT NULL DEFAULT 'number',
    "DisplayFormat" character varying(50),
    "IsLocked" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_FinancialCells" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinancialCells_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_FinancialCells_BusinessPlan_Sheet_Row_Column" ON "FinancialCells" ("BusinessPlanId", "SheetName", "RowId", "ColumnId");
CREATE INDEX IF NOT EXISTS "IX_FinancialCells_BusinessPlanId" ON "FinancialCells" ("BusinessPlanId");

-- Create LocationOverheadRates table
CREATE TABLE IF NOT EXISTS "LocationOverheadRates" (
    "Id" uuid NOT NULL,
    "Province" character varying(100) NOT NULL,
    "ProvinceCode" character varying(10) NOT NULL,
    "OverheadRate" numeric(5,2) NOT NULL,
    "InsuranceRate" numeric(18,2) NOT NULL,
    "TaxRate" numeric(5,2) NOT NULL,
    "OfficeCost" numeric(18,2) NOT NULL,
    "Currency" character varying(10) NOT NULL DEFAULT 'CAD',
    "EffectiveDate" timestamp with time zone NOT NULL,
    "ExpiryDate" timestamp with time zone,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" character varying(100) NOT NULL,
    "UpdatedBy" character varying(100) NOT NULL,
    CONSTRAINT "PK_LocationOverheadRates" PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS "IX_LocationOverheadRates_Province_IsActive" ON "LocationOverheadRates" ("Province", "IsActive");
CREATE INDEX IF NOT EXISTS "IX_LocationOverheadRates_ProvinceCode_IsActive" ON "LocationOverheadRates" ("ProvinceCode", "IsActive");

-- Seed data for LocationOverheadRates (only insert if not exists)
INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000001', 'Alberta', 'AB', 10.0, 200, 15.0, 600, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'AB');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000002', 'British Columbia', 'BC', 12.0, 250, 20.0, 800, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'BC');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000003', 'Manitoba', 'MB', 10.0, 180, 17.0, 450, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'MB');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000004', 'New Brunswick', 'NB', 9.0, 170, 20.0, 400, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'NB');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000005', 'Newfoundland and Labrador', 'NL', 9.0, 175, 20.0, 420, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'NL');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000006', 'Nova Scotia', 'NS', 9.5, 175, 21.0, 450, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'NS');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000007', 'Ontario', 'ON', 12.0, 250, 20.0, 750, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'ON');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000008', 'Prince Edward Island', 'PE', 8.5, 160, 20.0, 380, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'PE');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000009', 'Quebec', 'QC', 11.0, 220, 24.0, 600, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'QC');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000010', 'Saskatchewan', 'SK', 9.5, 180, 16.0, 450, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'SK');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000011', 'Northwest Territories', 'NT', 11.0, 200, 15.0, 700, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'NT');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000012', 'Nunavut', 'NU', 12.0, 220, 15.0, 900, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'NU');

INSERT INTO "LocationOverheadRates" ("Id", "Province", "ProvinceCode", "OverheadRate", "InsuranceRate", "TaxRate", "OfficeCost", "Currency", "EffectiveDate", "IsActive", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy")
SELECT 'a0000000-0000-0000-0000-000000000013', 'Yukon', 'YT', 10.5, 190, 15.0, 650, 'CAD', '2024-01-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2024-01-01 00:00:00+00', 'System', 'System'
WHERE NOT EXISTS (SELECT 1 FROM "LocationOverheadRates" WHERE "ProvinceCode" = 'YT');

-- Update migration history
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260120181400_AddLocationOverheadRatesAndFinancialCells', '8.0.10'
WHERE NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells');
