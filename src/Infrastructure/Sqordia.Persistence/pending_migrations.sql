START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    ALTER TABLE "Users" ADD "Persona" character varying(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    ALTER TABLE "BusinessPlanShares" ADD "AllowDownload" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    ALTER TABLE "BusinessPlanShares" ADD "EnableWatermark" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    ALTER TABLE "BusinessPlanShares" ADD "IsVaultShare" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    ALTER TABLE "BusinessPlanShares" ADD "MaxViews" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    ALTER TABLE "BusinessPlanShares" ADD "PasswordHash" character varying(256);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    ALTER TABLE "BusinessPlanShares" ADD "RequireEmailVerification" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    ALTER TABLE "BusinessPlanShares" ADD "TrackViews" boolean NOT NULL DEFAULT TRUE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    ALTER TABLE "BusinessPlanShares" ADD "WatermarkText" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    CREATE TABLE "FinancialCells" (
        "Id" uuid NOT NULL,
        "BusinessPlanId" uuid NOT NULL,
        "SheetName" character varying(100) NOT NULL DEFAULT 'Main',
        "RowId" character varying(200) NOT NULL,
        "ColumnId" character varying(100) NOT NULL,
        "Value" numeric(18,4) NOT NULL,
        "Formula" character varying(1000),
        "IsCalculated" boolean NOT NULL DEFAULT FALSE,
        "CellType" character varying(50) NOT NULL DEFAULT 'number',
        "DisplayFormat" character varying(50),
        "IsLocked" boolean NOT NULL DEFAULT FALSE,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_FinancialCells" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_FinancialCells_BusinessPlans_BusinessPlanId" FOREIGN KEY ("BusinessPlanId") REFERENCES "BusinessPlans" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    CREATE TABLE "LocationOverheadRates" (
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
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "CreatedBy" character varying(100) NOT NULL,
        "UpdatedBy" character varying(100) NOT NULL,
        CONSTRAINT "PK_LocationOverheadRates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000001', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 200.0, TRUE, 600.0, 10.0, 'Alberta', 'AB', 15.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000002', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 250.0, TRUE, 800.0, 12.0, 'British Columbia', 'BC', 20.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000003', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 180.0, TRUE, 450.0, 10.0, 'Manitoba', 'MB', 17.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000004', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 170.0, TRUE, 400.0, 9.0, 'New Brunswick', 'NB', 20.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000005', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 175.0, TRUE, 420.0, 9.0, 'Newfoundland and Labrador', 'NL', 20.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000006', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 175.0, TRUE, 450.0, 9.5, 'Nova Scotia', 'NS', 21.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000007', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 250.0, TRUE, 750.0, 12.0, 'Ontario', 'ON', 20.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000008', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 160.0, TRUE, 380.0, 8.5, 'Prince Edward Island', 'PE', 20.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000009', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 220.0, TRUE, 600.0, 11.0, 'Quebec', 'QC', 24.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000010', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 180.0, TRUE, 450.0, 9.5, 'Saskatchewan', 'SK', 16.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000011', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 200.0, TRUE, 700.0, 11.0, 'Northwest Territories', 'NT', 15.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000012', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 220.0, TRUE, 900.0, 12.0, 'Nunavut', 'NU', 15.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    INSERT INTO "LocationOverheadRates" ("Id", "CreatedAt", "CreatedBy", "Currency", "EffectiveDate", "ExpiryDate", "InsuranceRate", "IsActive", "OfficeCost", "OverheadRate", "Province", "ProvinceCode", "TaxRate", "UpdatedAt", "UpdatedBy")
    VALUES ('a0000000-0000-0000-0000-000000000013', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System', 'CAD', TIMESTAMPTZ '2024-01-01T00:00:00Z', NULL, 190.0, TRUE, 650.0, 10.5, 'Yukon', 'YT', 15.0, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'System');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    CREATE UNIQUE INDEX "IX_FinancialCells_BusinessPlan_Sheet_Row_Column" ON "FinancialCells" ("BusinessPlanId", "SheetName", "RowId", "ColumnId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    CREATE INDEX "IX_FinancialCells_BusinessPlanId" ON "FinancialCells" ("BusinessPlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    CREATE INDEX "IX_LocationOverheadRates_Province_IsActive" ON "LocationOverheadRates" ("Province", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    CREATE INDEX "IX_LocationOverheadRates_ProvinceCode_IsActive" ON "LocationOverheadRates" ("ProvinceCode", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260120181400_AddLocationOverheadRatesAndFinancialCells') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260120181400_AddLocationOverheadRatesAndFinancialCells', '8.0.10');
    END IF;
END $EF$;
COMMIT;

