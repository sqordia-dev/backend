-- Seed PlanFeatureLimits (21 features × 4 tiers = 84 rows)
-- Idempotent: skips if rows already exist

-- Free plan (f1e2d3c4-b5a6-4789-f012-345678901234)
INSERT INTO "PlanFeatureLimits" ("Id", "SubscriptionPlanId", "FeatureKey", "Value", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'f1e2d3c4-b5a6-4789-f012-345678901234', k, v, NOW(), false
FROM (VALUES
  ('MaxBusinessPlans','1'),('MaxOrganizations','1'),('MaxTeamMembers','1'),
  ('MaxAiGenerationsMonthly','1'),('MaxAiCoachMessagesMonthly','10'),('MaxStorageMb','100'),
  ('ExportHtml','true'),('ExportPdf','false'),('ExportWord','false'),
  ('ExportPowerpoint','false'),('ExportExcel','false'),('ExportAgentBlueprints','false'),
  ('AiProviderTier','gemini'),('PrioritySectionsClaude','false'),
  ('FinancialProjectionsBasic','true'),('FinancialProjectionsAdvanced','false'),
  ('CustomBranding','false'),('ApiAccess','false'),('PrioritySupport','false'),
  ('DedicatedSupport','false'),('WhiteLabel','false')
) AS t(k, v)
WHERE NOT EXISTS (
  SELECT 1 FROM "PlanFeatureLimits"
  WHERE "SubscriptionPlanId" = 'f1e2d3c4-b5a6-4789-f012-345678901234' AND "FeatureKey" = t.k
);

-- Starter plan (26c7fa03-188a-4183-9823-c6711cd685a0)
INSERT INTO "PlanFeatureLimits" ("Id", "SubscriptionPlanId", "FeatureKey", "Value", "Created", "IsDeleted")
SELECT gen_random_uuid(), '26c7fa03-188a-4183-9823-c6711cd685a0', k, v, NOW(), false
FROM (VALUES
  ('MaxBusinessPlans','3'),('MaxOrganizations','1'),('MaxTeamMembers','2'),
  ('MaxAiGenerationsMonthly','5'),('MaxAiCoachMessagesMonthly','50'),('MaxStorageMb','1024'),
  ('ExportHtml','true'),('ExportPdf','true'),('ExportWord','true'),
  ('ExportPowerpoint','false'),('ExportExcel','false'),('ExportAgentBlueprints','false'),
  ('AiProviderTier','blended'),('PrioritySectionsClaude','false'),
  ('FinancialProjectionsBasic','true'),('FinancialProjectionsAdvanced','false'),
  ('CustomBranding','false'),('ApiAccess','false'),('PrioritySupport','false'),
  ('DedicatedSupport','false'),('WhiteLabel','false')
) AS t(k, v)
WHERE NOT EXISTS (
  SELECT 1 FROM "PlanFeatureLimits"
  WHERE "SubscriptionPlanId" = '26c7fa03-188a-4183-9823-c6711cd685a0' AND "FeatureKey" = t.k
);

-- Professional plan (a1b2c3d4-e5f6-4789-a012-345678901235)
INSERT INTO "PlanFeatureLimits" ("Id", "SubscriptionPlanId", "FeatureKey", "Value", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'a1b2c3d4-e5f6-4789-a012-345678901235', k, v, NOW(), false
FROM (VALUES
  ('MaxBusinessPlans','-1'),('MaxOrganizations','3'),('MaxTeamMembers','10'),
  ('MaxAiGenerationsMonthly','30'),('MaxAiCoachMessagesMonthly','300'),('MaxStorageMb','5120'),
  ('ExportHtml','true'),('ExportPdf','true'),('ExportWord','true'),
  ('ExportPowerpoint','true'),('ExportExcel','true'),('ExportAgentBlueprints','true'),
  ('AiProviderTier','blended'),('PrioritySectionsClaude','true'),
  ('FinancialProjectionsBasic','true'),('FinancialProjectionsAdvanced','true'),
  ('CustomBranding','false'),('ApiAccess','false'),('PrioritySupport','true'),
  ('DedicatedSupport','false'),('WhiteLabel','false')
) AS t(k, v)
WHERE NOT EXISTS (
  SELECT 1 FROM "PlanFeatureLimits"
  WHERE "SubscriptionPlanId" = 'a1b2c3d4-e5f6-4789-a012-345678901235' AND "FeatureKey" = t.k
);

-- Enterprise plan (b2c3d4e5-f6a7-4789-b012-345678901236)
INSERT INTO "PlanFeatureLimits" ("Id", "SubscriptionPlanId", "FeatureKey", "Value", "Created", "IsDeleted")
SELECT gen_random_uuid(), 'b2c3d4e5-f6a7-4789-b012-345678901236', k, v, NOW(), false
FROM (VALUES
  ('MaxBusinessPlans','-1'),('MaxOrganizations','-1'),('MaxTeamMembers','-1'),
  ('MaxAiGenerationsMonthly','-1'),('MaxAiCoachMessagesMonthly','-1'),('MaxStorageMb','51200'),
  ('ExportHtml','true'),('ExportPdf','true'),('ExportWord','true'),
  ('ExportPowerpoint','true'),('ExportExcel','true'),('ExportAgentBlueprints','true'),
  ('AiProviderTier','claude'),('PrioritySectionsClaude','true'),
  ('FinancialProjectionsBasic','true'),('FinancialProjectionsAdvanced','true'),
  ('CustomBranding','true'),('ApiAccess','true'),('PrioritySupport','true'),
  ('DedicatedSupport','true'),('WhiteLabel','true')
) AS t(k, v)
WHERE NOT EXISTS (
  SELECT 1 FROM "PlanFeatureLimits"
  WHERE "SubscriptionPlanId" = 'b2c3d4e5-f6a7-4789-b012-345678901236' AND "FeatureKey" = t.k
);
