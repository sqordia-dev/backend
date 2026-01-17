-- ============================================================================
-- Reset Admin User Subscription Script (PostgreSQL)
-- ============================================================================
-- This script:
--   1. Deletes all subscriptions (invoices) for the Admin user
--   2. Resets Admin user to Free plan
--   3. Creates a new Free plan subscription for Admin user
--
-- Admin User: admin@sqordia.com
-- User ID: 1367e88c-d3a2-46c4-928b-40156092d0bf
-- Free Plan ID: f1e2d3c4-b5a6-4789-f012-345678901234
-- ============================================================================

BEGIN;

DO $$
DECLARE
    v_admin_user_id UUID := '1367e88c-d3a2-46c4-928b-40156092d0bf'::uuid;
    v_free_plan_id UUID := 'f1e2d3c4-b5a6-4789-f012-345678901234'::uuid;
    v_admin_org_id UUID;
    v_subscription_count INTEGER;
    v_deleted_count INTEGER;
    v_now TIMESTAMP WITH TIME ZONE := NOW() AT TIME ZONE 'UTC';
    v_start_date TIMESTAMP WITH TIME ZONE := DATE_TRUNC('month', v_now) AT TIME ZONE 'UTC';
    v_end_date TIMESTAMP WITH TIME ZONE := (DATE_TRUNC('month', v_now) + INTERVAL '1 month') AT TIME ZONE 'UTC';
BEGIN
    -- Find Admin user's organization
    SELECT om."OrganizationId" INTO v_admin_org_id
    FROM "OrganizationMembers" om
    WHERE om."UserId" = v_admin_user_id
      AND om."IsActive" = true
    LIMIT 1;
    
    -- If Admin user doesn't have an organization, we need to create one or use a default
    IF v_admin_org_id IS NULL THEN
        -- Try to find any organization the user might be associated with (even inactive)
        SELECT om."OrganizationId" INTO v_admin_org_id
        FROM "OrganizationMembers" om
        WHERE om."UserId" = v_admin_user_id
        LIMIT 1;
        
        -- If still no organization, create a default one for Admin
        IF v_admin_org_id IS NULL THEN
            v_admin_org_id := gen_random_uuid();
            
            INSERT INTO "Organizations" (
                "Id",
                "Name",
                "Description",
                "OrganizationType",
                "IsActive",
                "MaxMembers",
                "AllowMemberInvites",
                "RequireEmailVerification",
                "Created",
                "IsDeleted"
            )
            VALUES (
                v_admin_org_id,
                'Admin Organization',
                'Default organization for Admin user',
                'Business',
                true,
                10,
                true,
                true,
                v_now,
                false
            )
            ON CONFLICT ("Id") DO NOTHING;
            
            -- Add Admin user to the organization
            INSERT INTO "OrganizationMembers" (
                "Id",
                "OrganizationId",
                "UserId",
                "Role",
                "IsActive",
                "JoinedAt",
                "Created",
                "IsDeleted"
            )
            VALUES (
                gen_random_uuid(),
                v_admin_org_id,
                v_admin_user_id,
                'Owner',
                true,
                v_now,
                v_now,
                false
            )
            ON CONFLICT DO NOTHING;
            
            RAISE NOTICE 'Created default organization for Admin user: %', v_admin_org_id;
        END IF;
    END IF;
    
    -- Count existing subscriptions for Admin user
    SELECT COUNT(*) INTO v_subscription_count
    FROM "Subscriptions"
    WHERE "UserId" = v_admin_user_id
      AND "IsDeleted" = false;
    
    RAISE NOTICE 'Found % subscription(s) for Admin user', v_subscription_count;
    
    -- Delete all subscriptions for Admin user (these are the "invoices")
    DELETE FROM "Subscriptions"
    WHERE "UserId" = v_admin_user_id;
    
    GET DIAGNOSTICS v_deleted_count = ROW_COUNT;
    RAISE NOTICE 'Deleted % subscription(s) for Admin user', v_deleted_count;
    
    -- Create new Free plan subscription for Admin user
    INSERT INTO "Subscriptions" (
        "Id",
        "UserId",
        "OrganizationId",
        "SubscriptionPlanId",
        "Status",
        "StartDate",
        "EndDate",
        "CancelledAt",
        "CancelledEffectiveDate",
        "IsYearly",
        "Amount",
        "Currency",
        "IsTrial",
        "TrialEndDate",
        "StripeCustomerId",
        "StripeSubscriptionId",
        "StripePriceId",
        "Created",
        "IsDeleted"
    )
    VALUES (
        gen_random_uuid(),
        v_admin_user_id,
        v_admin_org_id,
        v_free_plan_id,
        'Active', -- SubscriptionStatus.Active
        v_start_date,
        v_end_date,
        NULL, -- CancelledAt
        NULL, -- CancelledEffectiveDate
        false, -- IsYearly (monthly)
        0.00, -- Amount (Free plan)
        'CAD', -- Currency
        false, -- IsTrial
        NULL, -- TrialEndDate
        NULL, -- StripeCustomerId
        NULL, -- StripeSubscriptionId
        NULL, -- StripePriceId
        v_now,
        false
    );
    
    RAISE NOTICE '✓ SUCCESS: Admin user subscription reset to Free plan';
    RAISE NOTICE '  - Organization ID: %', v_admin_org_id;
    RAISE NOTICE '  - Plan: Free Plan';
    RAISE NOTICE '  - Start Date: %', v_start_date;
    RAISE NOTICE '  - End Date: %', v_end_date;
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error resetting Admin subscription: %', SQLERRM;
END $$;

COMMIT;

-- ============================================================================
-- VERIFICATION
-- ============================================================================
DO $$
DECLARE
    v_admin_user_id UUID := '1367e88c-d3a2-46c4-928b-40156092d0bf'::uuid;
    v_free_plan_id UUID := 'f1e2d3c4-b5a6-4789-f012-345678901234'::uuid;
    v_subscription_count INTEGER;
    v_active_subscription_count INTEGER;
BEGIN
    -- Count total subscriptions
    SELECT COUNT(*) INTO v_subscription_count
    FROM "Subscriptions"
    WHERE "UserId" = v_admin_user_id
      AND "IsDeleted" = false;
    
    -- Count active Free plan subscriptions
    SELECT COUNT(*) INTO v_active_subscription_count
    FROM "Subscriptions"
    WHERE "UserId" = v_admin_user_id
      AND "SubscriptionPlanId" = v_free_plan_id
      AND "Status" = 'Active'
      AND "IsDeleted" = false;
    
    IF v_subscription_count = 1 AND v_active_subscription_count = 1 THEN
        RAISE NOTICE '✓ VERIFICATION PASSED: Admin user has exactly 1 active Free plan subscription';
    ELSE
        RAISE WARNING 'Verification failed: Found % total subscription(s), % active Free plan subscription(s)', 
            v_subscription_count, v_active_subscription_count;
    END IF;
END $$;

-- ============================================================================
-- END OF SCRIPT
-- ============================================================================
