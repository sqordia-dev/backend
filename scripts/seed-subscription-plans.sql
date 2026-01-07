-- ============================================================================
-- Sqordia Subscription Plans Seed Script (PostgreSQL)
-- ============================================================================
-- This script seeds the database with subscription plans:
--   - Free Plan (Free tier)
--   - Pro Plan (Monthly: $29.99 CAD)
--   - Enterprise Plan (Monthly: $99.99 CAD)
--
-- Idempotent: Safe to run multiple times (uses ON CONFLICT)
-- ============================================================================

BEGIN;

-- ============================================================================
-- CREATE SUBSCRIPTION PLANS
-- ============================================================================
DO $$
DECLARE
    v_now TIMESTAMP WITH TIME ZONE := NOW() AT TIME ZONE 'UTC';
BEGIN
    -- Free Plan
    INSERT INTO "SubscriptionPlans" (
        "Id",
        "PlanType",
        "Name",
        "Description",
        "Price",
        "Currency",
        "BillingCycle",
        "MaxUsers",
        "MaxBusinessPlans",
        "MaxStorageGB",
        "Features",
        "IsActive",
        "Created",
        "IsDeleted"
    )
    VALUES (
        'f1e2d3c4-b5a6-4789-f012-345678901234'::uuid,
        'Free',
        'Free Plan',
        'Perfect for getting started',
        0.00,
        'CAD',
        'Monthly',
        1,
        3,
        1,
        '[]',
        true,
        v_now,
        false
    )
    ON CONFLICT ("Id") DO UPDATE SET
        "PlanType" = EXCLUDED."PlanType",
        "Name" = EXCLUDED."Name",
        "Description" = EXCLUDED."Description",
        "Price" = EXCLUDED."Price",
        "Currency" = EXCLUDED."Currency",
        "BillingCycle" = EXCLUDED."BillingCycle",
        "MaxUsers" = EXCLUDED."MaxUsers",
        "MaxBusinessPlans" = EXCLUDED."MaxBusinessPlans",
        "MaxStorageGB" = EXCLUDED."MaxStorageGB",
        "Features" = EXCLUDED."Features",
        "IsActive" = EXCLUDED."IsActive",
        "LastModified" = v_now;

    -- Pro Plan
    INSERT INTO "SubscriptionPlans" (
        "Id",
        "PlanType",
        "Name",
        "Description",
        "Price",
        "Currency",
        "BillingCycle",
        "MaxUsers",
        "MaxBusinessPlans",
        "MaxStorageGB",
        "Features",
        "IsActive",
        "Created",
        "IsDeleted"
    )
    VALUES (
        'a1b2c3d4-e5f6-4789-a012-345678901235'::uuid,
        'Pro',
        'Pro Plan',
        'For growing businesses',
        29.99,
        'CAD',
        'Monthly',
        10,
        50,
        10,
        '["ExportPDF", "ExportWord", "ExportExcel", "AdvancedAI", "PrioritySupport"]',
        true,
        v_now,
        false
    )
    ON CONFLICT ("Id") DO UPDATE SET
        "PlanType" = EXCLUDED."PlanType",
        "Name" = EXCLUDED."Name",
        "Description" = EXCLUDED."Description",
        "Price" = EXCLUDED."Price",
        "Currency" = EXCLUDED."Currency",
        "BillingCycle" = EXCLUDED."BillingCycle",
        "MaxUsers" = EXCLUDED."MaxUsers",
        "MaxBusinessPlans" = EXCLUDED."MaxBusinessPlans",
        "MaxStorageGB" = EXCLUDED."MaxStorageGB",
        "Features" = EXCLUDED."Features",
        "IsActive" = EXCLUDED."IsActive",
        "LastModified" = v_now;

    -- Enterprise Plan
    INSERT INTO "SubscriptionPlans" (
        "Id",
        "PlanType",
        "Name",
        "Description",
        "Price",
        "Currency",
        "BillingCycle",
        "MaxUsers",
        "MaxBusinessPlans",
        "MaxStorageGB",
        "Features",
        "IsActive",
        "Created",
        "IsDeleted"
    )
    VALUES (
        'b2c3d4e5-f6a7-4789-b012-345678901236'::uuid,
        'Enterprise',
        'Enterprise Plan',
        'For large organizations',
        99.99,
        'CAD',
        'Monthly',
        999999,
        999999,
        100,
        '["ExportPDF", "ExportWord", "ExportExcel", "AdvancedAI", "PrioritySupport", "DedicatedSupport", "CustomBranding", "APIAccess"]',
        true,
        v_now,
        false
    )
    ON CONFLICT ("Id") DO UPDATE SET
        "PlanType" = EXCLUDED."PlanType",
        "Name" = EXCLUDED."Name",
        "Description" = EXCLUDED."Description",
        "Price" = EXCLUDED."Price",
        "Currency" = EXCLUDED."Currency",
        "BillingCycle" = EXCLUDED."BillingCycle",
        "MaxUsers" = EXCLUDED."MaxUsers",
        "MaxBusinessPlans" = EXCLUDED."MaxBusinessPlans",
        "MaxStorageGB" = EXCLUDED."MaxStorageGB",
        "Features" = EXCLUDED."Features",
        "IsActive" = EXCLUDED."IsActive",
        "LastModified" = v_now;
END $$;

-- ============================================================================
-- VERIFICATION
-- ============================================================================
DO $$
DECLARE
    v_plan_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO v_plan_count
    FROM "SubscriptionPlans"
    WHERE "IsDeleted" = false AND "IsActive" = true;
    
    IF v_plan_count >= 3 THEN
        RAISE NOTICE '✓ SUCCESS: Subscription plans have been successfully inserted! (Count: %)', v_plan_count;
    ELSE
        RAISE WARNING 'Expected at least 3 plans, but % were found.', v_plan_count;
    END IF;
END $$;

COMMIT;

-- ============================================================================
-- END OF SUBSCRIPTION PLANS SEED SCRIPT
-- ============================================================================
-- Plans Created:
--   - Free Plan (ID: f1e2d3c4-b5a6-4789-f012-345678901234)
--   - Pro Plan (ID: a1b2c3d4-e5f6-4789-a012-345678901235)
--   - Enterprise Plan (ID: b2c3d4e5-f6a7-4789-b012-345678901236)
-- ============================================================================

