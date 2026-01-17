-- ============================================================================
-- Sqordia AI Prompts Seed Script (PostgreSQL)
-- ============================================================================
-- This script seeds the database with default AI prompts for business plan
-- content generation. These prompts are used by the BusinessPlanGenerationService.
--
-- Idempotent: Safe to run multiple times (uses WHERE NOT EXISTS)
-- Generated: 2026-01-14T19:42:23.224646
-- ============================================================================

BEGIN;

-- ============================================================================
-- SYSTEM PROMPTS
-- ============================================================================

-- System Prompt: BusinessPlan - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'System Prompt - BusinessPlan - EN',
    'Default system prompt for BusinessPlan in English',
    'SystemPrompt',
    'BusinessPlan',
    'en',
    NULL,
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    '',
    '{}',
    true,
    1,
    0,
    0.0,
    0,
    'Default system prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "Category" = 'SystemPrompt' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
);

-- System Prompt: BusinessPlan - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'System Prompt - BusinessPlan - FR',
    'Default system prompt for BusinessPlan in French',
    'SystemPrompt',
    'BusinessPlan',
    'fr',
    NULL,
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    '',
    '{}',
    true,
    1,
    0,
    0.0,
    0,
    'Default system prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "Category" = 'SystemPrompt' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
);

-- System Prompt: StrategicPlan - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'System Prompt - StrategicPlan - EN',
    'Default system prompt for StrategicPlan in English',
    'SystemPrompt',
    'StrategicPlan',
    'en',
    NULL,
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    '',
    '{}',
    true,
    1,
    0,
    0.0,
    0,
    'Default system prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "Category" = 'SystemPrompt' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
);

-- System Prompt: StrategicPlan - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'System Prompt - StrategicPlan - FR',
    'Default system prompt for StrategicPlan in French',
    'SystemPrompt',
    'StrategicPlan',
    'fr',
    NULL,
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    '',
    '{}',
    true,
    1,
    0,
    0.0,
    0,
    'Default system prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "Category" = 'SystemPrompt' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
);

-- ExecutiveSummary - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ExecutiveSummary - BusinessPlan - EN',
    'Prompt for generating ExecutiveSummary section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'ExecutiveSummary',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Write a compelling executive summary that presents the company, its unique value proposition, target market, competitive advantages, and key financial objectives. The summary should entice the reader to learn more.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ExecutiveSummary section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ExecutiveSummary' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- ExecutiveSummary - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ExecutiveSummary - BusinessPlan - FR',
    'Prompt for generating ExecutiveSummary section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'ExecutiveSummary',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Rédigez un résumé exécutif captivant qui présente l'entreprise, sa proposition de valeur unique, son marché cible, ses avantages concurrentiels et ses objectifs financiers principaux. Le résumé doit donner envie au lecteur d'en savoir plus.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ExecutiveSummary section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ExecutiveSummary' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- ProblemStatement - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ProblemStatement - BusinessPlan - EN',
    'Prompt for generating ProblemStatement section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'ProblemStatement',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Identify and describe the problem or unmet need that your business/organization aims to solve. Explain why this problem is important and urgent for the target market.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ProblemStatement section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ProblemStatement' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- ProblemStatement - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ProblemStatement - BusinessPlan - FR',
    'Prompt for generating ProblemStatement section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'ProblemStatement',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Identifiez et décrivez le problème ou le besoin non satisfait que votre entreprise/organisation vise à résoudre. Expliquez pourquoi ce problème est important et urgent pour le marché cible.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ProblemStatement section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ProblemStatement' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- Solution - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'Solution - BusinessPlan - EN',
    'Prompt for generating Solution section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'Solution',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Present the products or services offered in detail. Explain their features, benefits, how they solve customer problems, and what differentiates them from the competition.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive Solution section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'Solution' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- Solution - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'Solution - BusinessPlan - FR',
    'Prompt for generating Solution section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'Solution',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Présentez en détail les produits ou services offerts. Expliquez leurs caractéristiques, leurs avantages, comment ils résolvent les problèmes des clients et ce qui les différencie de la concurrence.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive Solution section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'Solution' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- MarketAnalysis - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'MarketAnalysis - BusinessPlan - EN',
    'Prompt for generating MarketAnalysis section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'MarketAnalysis',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Analyze the target market: size, growth, trends, segments. Include industry data, opportunities, and challenges. Demonstrate a deep understanding of the market.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive MarketAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'MarketAnalysis' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- MarketAnalysis - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'MarketAnalysis - BusinessPlan - FR',
    'Prompt for generating MarketAnalysis section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'MarketAnalysis',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Analysez le marché cible : taille, croissance, tendances, segments. Incluez des données sur l'industrie, les opportunités et les défis. Démontrez une compréhension approfondie du marché.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive MarketAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'MarketAnalysis' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- CompetitiveAnalysis - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'CompetitiveAnalysis - BusinessPlan - EN',
    'Prompt for generating CompetitiveAnalysis section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'CompetitiveAnalysis',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Identify main direct and indirect competitors. Analyze their strengths and weaknesses. Clearly explain the company's competitive positioning and distinctive advantages.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive CompetitiveAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'CompetitiveAnalysis' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- CompetitiveAnalysis - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'CompetitiveAnalysis - BusinessPlan - FR',
    'Prompt for generating CompetitiveAnalysis section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'CompetitiveAnalysis',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Identifiez les principaux concurrents directs et indirects. Analysez leurs forces et faiblesses. Expliquez clairement le positionnement concurrentiel de l'entreprise et ses avantages distinctifs.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive CompetitiveAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'CompetitiveAnalysis' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- SwotAnalysis - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'SwotAnalysis - BusinessPlan - EN',
    'Prompt for generating SwotAnalysis section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'SwotAnalysis',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Conduct a complete SWOT analysis: Strengths (internal assets), Weaknesses (internal limitations), Opportunities (positive external factors), Threats (external risks). Be specific and strategic.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive SwotAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'SwotAnalysis' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- SwotAnalysis - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'SwotAnalysis - BusinessPlan - FR',
    'Prompt for generating SwotAnalysis section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'SwotAnalysis',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Réalisez une analyse SWOT complète : Forces (atouts internes), Faiblesses (limites internes), Opportunités (facteurs externes positifs), Menaces (risques externes). Soyez spécifique et stratégique.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive SwotAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'SwotAnalysis' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- BusinessModel - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'BusinessModel - BusinessPlan - EN',
    'Prompt for generating BusinessModel section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'BusinessModel',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Explain the business model: how the company creates, delivers, and captures value. Include revenue streams, cost structure, key resources, and strategic partnerships.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive BusinessModel section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'BusinessModel' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- BusinessModel - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'BusinessModel - BusinessPlan - FR',
    'Prompt for generating BusinessModel section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'BusinessModel',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Expliquez le modèle d'affaires : comment l'entreprise crée, délivre et capture de la valeur. Incluez les flux de revenus, la structure de coûts, les ressources clés et les partenariats stratégiques.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive BusinessModel section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'BusinessModel' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- MarketingStrategy - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'MarketingStrategy - BusinessPlan - EN',
    'Prompt for generating MarketingStrategy section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'MarketingStrategy',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Describe the complete marketing strategy: positioning, branding, communication channels, customer acquisition tactics, content strategy, and marketing budget.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive MarketingStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'MarketingStrategy' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- MarketingStrategy - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'MarketingStrategy - BusinessPlan - FR',
    'Prompt for generating MarketingStrategy section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'MarketingStrategy',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Décrivez la stratégie marketing complète : positionnement, branding, canaux de communication, tactiques d'acquisition de clients, stratégie de contenu et budget marketing.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive MarketingStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'MarketingStrategy' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- BrandingStrategy - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'BrandingStrategy - BusinessPlan - EN',
    'Prompt for generating BrandingStrategy section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'BrandingStrategy',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Explain the branding strategy: visual identity, tone of communication, brand value proposition, differentiation, and how the brand will resonate with the target audience.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive BrandingStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'BrandingStrategy' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- BrandingStrategy - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'BrandingStrategy - BusinessPlan - FR',
    'Prompt for generating BrandingStrategy section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'BrandingStrategy',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Expliquez la stratégie de marque : identité visuelle, ton de communication, proposition de valeur de la marque, différenciation et comment la marque résonnera avec le public cible.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive BrandingStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'BrandingStrategy' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- OperationsPlan - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'OperationsPlan - BusinessPlan - EN',
    'Prompt for generating OperationsPlan section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'OperationsPlan',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Describe daily operations: facilities, equipment, technologies, key processes, suppliers, supply chain, and quality management.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive OperationsPlan section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'OperationsPlan' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- OperationsPlan - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'OperationsPlan - BusinessPlan - FR',
    'Prompt for generating OperationsPlan section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'OperationsPlan',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Décrivez les opérations quotidiennes : installations, équipements, technologies, processus clés, fournisseurs, chaîne d'approvisionnement et gestion de la qualité.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive OperationsPlan section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'OperationsPlan' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- ManagementTeam - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ManagementTeam - BusinessPlan - EN',
    'Prompt for generating ManagementTeam section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'ManagementTeam',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Present the management team: skills, experiences, roles, and responsibilities. Highlight how the team is positioned to succeed.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ManagementTeam section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ManagementTeam' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- ManagementTeam - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ManagementTeam - BusinessPlan - FR',
    'Prompt for generating ManagementTeam section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'ManagementTeam',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Présentez l'équipe de direction : compétences, expériences, rôles et responsabilités. Mettez en avant comment l'équipe est positionnée pour réussir.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ManagementTeam section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ManagementTeam' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- FinancialProjections - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'FinancialProjections - BusinessPlan - EN',
    'Prompt for generating FinancialProjections section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'FinancialProjections',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Summarize financial projections: expected revenues, main costs, profitability, cash flow needs. Explain the key assumptions behind these projections.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive FinancialProjections section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'FinancialProjections' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- FinancialProjections - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'FinancialProjections - BusinessPlan - FR',
    'Prompt for generating FinancialProjections section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'FinancialProjections',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Résumez les projections financières : revenus prévus, coûts principaux, rentabilité, besoins en trésorerie. Expliquez les hypothèses clés derrière ces projections.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive FinancialProjections section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'FinancialProjections' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- FundingRequirements - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'FundingRequirements - BusinessPlan - EN',
    'Prompt for generating FundingRequirements section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'FundingRequirements',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Detail funding needs: required amount, use of funds, potential funding sources, financing structure, and repayment plan or return on investment.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive FundingRequirements section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'FundingRequirements' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- FundingRequirements - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'FundingRequirements - BusinessPlan - FR',
    'Prompt for generating FundingRequirements section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'FundingRequirements',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Détaillez les besoins de financement : montant requis, utilisation des fonds, sources de financement potentielles, structure de financement et plan de remboursement ou retour sur investissement.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive FundingRequirements section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'FundingRequirements' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- RiskAnalysis - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'RiskAnalysis - BusinessPlan - EN',
    'Prompt for generating RiskAnalysis section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'RiskAnalysis',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Identify main risks (market, operational, financial, regulatory) and present concrete mitigation strategies for each.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive RiskAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'RiskAnalysis' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- RiskAnalysis - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'RiskAnalysis - BusinessPlan - FR',
    'Prompt for generating RiskAnalysis section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'RiskAnalysis',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Identifiez les principaux risques (marché, opérationnels, financiers, réglementaires) et présentez des stratégies concrètes d'atténuation pour chacun.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive RiskAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'RiskAnalysis' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- ExitStrategy - BusinessPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ExitStrategy - BusinessPlan - EN',
    'Prompt for generating ExitStrategy section in BusinessPlan plans (en)',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'ExitStrategy',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Explain potential exit options for investors: acquisition, IPO, buyout. Include approximate timeline and valuation factors.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ExitStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ExitStrategy' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- ExitStrategy - BusinessPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ExitStrategy - BusinessPlan - FR',
    'Prompt for generating ExitStrategy section in BusinessPlan plans (fr)',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'ExitStrategy',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Expliquez les options de sortie potentielles pour les investisseurs : acquisition, IPO, buyout. Incluez un calendrier approximatif et les facteurs de valorisation.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ExitStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ExitStrategy' 
    AND "PlanType" = 'BusinessPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- ExecutiveSummary - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ExecutiveSummary - StrategicPlan - EN',
    'Prompt for generating ExecutiveSummary section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'ExecutiveSummary',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Write a compelling executive summary that presents the company, its unique value proposition, target market, competitive advantages, and key financial objectives. The summary should entice the reader to learn more.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ExecutiveSummary section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ExecutiveSummary' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- ExecutiveSummary - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ExecutiveSummary - StrategicPlan - FR',
    'Prompt for generating ExecutiveSummary section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'ExecutiveSummary',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Rédigez un résumé exécutif captivant qui présente l'entreprise, sa proposition de valeur unique, son marché cible, ses avantages concurrentiels et ses objectifs financiers principaux. Le résumé doit donner envie au lecteur d'en savoir plus.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ExecutiveSummary section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ExecutiveSummary' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- ProblemStatement - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ProblemStatement - StrategicPlan - EN',
    'Prompt for generating ProblemStatement section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'ProblemStatement',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Identify and describe the problem or unmet need that your business/organization aims to solve. Explain why this problem is important and urgent for the target market.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ProblemStatement section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ProblemStatement' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- ProblemStatement - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ProblemStatement - StrategicPlan - FR',
    'Prompt for generating ProblemStatement section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'ProblemStatement',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Identifiez et décrivez le problème ou le besoin non satisfait que votre entreprise/organisation vise à résoudre. Expliquez pourquoi ce problème est important et urgent pour le marché cible.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ProblemStatement section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ProblemStatement' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- Solution - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'Solution - StrategicPlan - EN',
    'Prompt for generating Solution section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'Solution',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Present the products or services offered in detail. Explain their features, benefits, how they solve customer problems, and what differentiates them from the competition.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive Solution section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'Solution' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- Solution - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'Solution - StrategicPlan - FR',
    'Prompt for generating Solution section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'Solution',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Présentez en détail les produits ou services offerts. Expliquez leurs caractéristiques, leurs avantages, comment ils résolvent les problèmes des clients et ce qui les différencie de la concurrence.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive Solution section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'Solution' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- MarketAnalysis - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'MarketAnalysis - StrategicPlan - EN',
    'Prompt for generating MarketAnalysis section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'MarketAnalysis',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Analyze the target market: size, growth, trends, segments. Include industry data, opportunities, and challenges. Demonstrate a deep understanding of the market.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive MarketAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'MarketAnalysis' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- MarketAnalysis - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'MarketAnalysis - StrategicPlan - FR',
    'Prompt for generating MarketAnalysis section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'MarketAnalysis',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Analysez le marché cible : taille, croissance, tendances, segments. Incluez des données sur l'industrie, les opportunités et les défis. Démontrez une compréhension approfondie du marché.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive MarketAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'MarketAnalysis' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- CompetitiveAnalysis - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'CompetitiveAnalysis - StrategicPlan - EN',
    'Prompt for generating CompetitiveAnalysis section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'CompetitiveAnalysis',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Identify main direct and indirect competitors. Analyze their strengths and weaknesses. Clearly explain the company's competitive positioning and distinctive advantages.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive CompetitiveAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'CompetitiveAnalysis' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- CompetitiveAnalysis - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'CompetitiveAnalysis - StrategicPlan - FR',
    'Prompt for generating CompetitiveAnalysis section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'CompetitiveAnalysis',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Identifiez les principaux concurrents directs et indirects. Analysez leurs forces et faiblesses. Expliquez clairement le positionnement concurrentiel de l'entreprise et ses avantages distinctifs.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive CompetitiveAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'CompetitiveAnalysis' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- SwotAnalysis - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'SwotAnalysis - StrategicPlan - EN',
    'Prompt for generating SwotAnalysis section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'SwotAnalysis',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Conduct a complete SWOT analysis: Strengths (internal assets), Weaknesses (internal limitations), Opportunities (positive external factors), Threats (external risks). Be specific and strategic.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive SwotAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'SwotAnalysis' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- SwotAnalysis - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'SwotAnalysis - StrategicPlan - FR',
    'Prompt for generating SwotAnalysis section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'SwotAnalysis',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Réalisez une analyse SWOT complète : Forces (atouts internes), Faiblesses (limites internes), Opportunités (facteurs externes positifs), Menaces (risques externes). Soyez spécifique et stratégique.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive SwotAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'SwotAnalysis' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- BusinessModel - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'BusinessModel - StrategicPlan - EN',
    'Prompt for generating BusinessModel section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'BusinessModel',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Explain the business model: how the company creates, delivers, and captures value. Include revenue streams, cost structure, key resources, and strategic partnerships.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive BusinessModel section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'BusinessModel' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- BusinessModel - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'BusinessModel - StrategicPlan - FR',
    'Prompt for generating BusinessModel section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'BusinessModel',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Expliquez le modèle d'affaires : comment l'entreprise crée, délivre et capture de la valeur. Incluez les flux de revenus, la structure de coûts, les ressources clés et les partenariats stratégiques.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive BusinessModel section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'BusinessModel' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- MarketingStrategy - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'MarketingStrategy - StrategicPlan - EN',
    'Prompt for generating MarketingStrategy section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'MarketingStrategy',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Describe the complete marketing strategy: positioning, branding, communication channels, customer acquisition tactics, content strategy, and marketing budget.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive MarketingStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'MarketingStrategy' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- MarketingStrategy - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'MarketingStrategy - StrategicPlan - FR',
    'Prompt for generating MarketingStrategy section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'MarketingStrategy',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Décrivez la stratégie marketing complète : positionnement, branding, canaux de communication, tactiques d'acquisition de clients, stratégie de contenu et budget marketing.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive MarketingStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'MarketingStrategy' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- BrandingStrategy - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'BrandingStrategy - StrategicPlan - EN',
    'Prompt for generating BrandingStrategy section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'BrandingStrategy',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Explain the branding strategy: visual identity, tone of communication, brand value proposition, differentiation, and how the brand will resonate with the target audience.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive BrandingStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'BrandingStrategy' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- BrandingStrategy - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'BrandingStrategy - StrategicPlan - FR',
    'Prompt for generating BrandingStrategy section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'BrandingStrategy',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Expliquez la stratégie de marque : identité visuelle, ton de communication, proposition de valeur de la marque, différenciation et comment la marque résonnera avec le public cible.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive BrandingStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'BrandingStrategy' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- OperationsPlan - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'OperationsPlan - StrategicPlan - EN',
    'Prompt for generating OperationsPlan section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'OperationsPlan',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Describe daily operations: facilities, equipment, technologies, key processes, suppliers, supply chain, and quality management.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive OperationsPlan section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'OperationsPlan' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- OperationsPlan - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'OperationsPlan - StrategicPlan - FR',
    'Prompt for generating OperationsPlan section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'OperationsPlan',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Décrivez les opérations quotidiennes : installations, équipements, technologies, processus clés, fournisseurs, chaîne d'approvisionnement et gestion de la qualité.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive OperationsPlan section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'OperationsPlan' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- ManagementTeam - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ManagementTeam - StrategicPlan - EN',
    'Prompt for generating ManagementTeam section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'ManagementTeam',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Present the management team: skills, experiences, roles, and responsibilities. Highlight how the team is positioned to succeed.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ManagementTeam section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ManagementTeam' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- ManagementTeam - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'ManagementTeam - StrategicPlan - FR',
    'Prompt for generating ManagementTeam section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'ManagementTeam',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Présentez l'équipe de direction : compétences, expériences, rôles et responsabilités. Mettez en avant comment l'équipe est positionnée pour réussir.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive ManagementTeam section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'ManagementTeam' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- FinancialProjections - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'FinancialProjections - StrategicPlan - EN',
    'Prompt for generating FinancialProjections section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'FinancialProjections',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Summarize financial projections: expected revenues, main costs, profitability, cash flow needs. Explain the key assumptions behind these projections.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive FinancialProjections section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'FinancialProjections' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- FinancialProjections - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'FinancialProjections - StrategicPlan - FR',
    'Prompt for generating FinancialProjections section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'FinancialProjections',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Résumez les projections financières : revenus prévus, coûts principaux, rentabilité, besoins en trésorerie. Expliquez les hypothèses clés derrière ces projections.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive FinancialProjections section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'FinancialProjections' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- FundingRequirements - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'FundingRequirements - StrategicPlan - EN',
    'Prompt for generating FundingRequirements section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'FundingRequirements',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Detail funding needs: required amount, use of funds, potential funding sources, financing structure, and repayment plan or return on investment.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive FundingRequirements section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'FundingRequirements' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- FundingRequirements - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'FundingRequirements - StrategicPlan - FR',
    'Prompt for generating FundingRequirements section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'FundingRequirements',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Détaillez les besoins de financement : montant requis, utilisation des fonds, sources de financement potentielles, structure de financement et plan de remboursement ou retour sur investissement.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive FundingRequirements section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'FundingRequirements' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- RiskAnalysis - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'RiskAnalysis - StrategicPlan - EN',
    'Prompt for generating RiskAnalysis section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'RiskAnalysis',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Identify main risks (market, operational, financial, regulatory) and present concrete mitigation strategies for each.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive RiskAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'RiskAnalysis' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- RiskAnalysis - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'RiskAnalysis - StrategicPlan - FR',
    'Prompt for generating RiskAnalysis section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'RiskAnalysis',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Identifiez les principaux risques (marché, opérationnels, financiers, réglementaires) et présentez des stratégies concrètes d'atténuation pour chacun.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive RiskAnalysis section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'RiskAnalysis' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- MissionStatement - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'MissionStatement - StrategicPlan - EN',
    'Prompt for generating MissionStatement section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'MissionStatement',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Write a clear and inspiring mission statement that explains the organization's purpose, who it serves, and the impact it wishes to create in the community.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive MissionStatement section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'MissionStatement' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- MissionStatement - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'MissionStatement - StrategicPlan - FR',
    'Prompt for generating MissionStatement section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'MissionStatement',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Rédigez un énoncé de mission clair et inspirant qui explique la raison d'être de l'organisation, qui elle sert, et l'impact qu'elle souhaite créer dans la communauté.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive MissionStatement section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'MissionStatement' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- SocialImpact - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'SocialImpact - StrategicPlan - EN',
    'Prompt for generating SocialImpact section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'SocialImpact',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Describe the expected social impact: positive changes in the community, social success indicators, direct and indirect beneficiaries, and contribution to sustainable development goals.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive SocialImpact section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'SocialImpact' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- SocialImpact - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'SocialImpact - StrategicPlan - FR',
    'Prompt for generating SocialImpact section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'SocialImpact',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Décrivez l'impact social attendu : changements positifs dans la communauté, indicateurs de succès social, bénéficiaires directs et indirects, et contribution aux objectifs de développement durable.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive SocialImpact section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'SocialImpact' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- BeneficiaryProfile - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'BeneficiaryProfile - StrategicPlan - EN',
    'Prompt for generating BeneficiaryProfile section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'BeneficiaryProfile',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Draw a detailed portrait of beneficiaries: who they are, their specific needs, the challenges they face, and how the organization will address these needs.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive BeneficiaryProfile section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'BeneficiaryProfile' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- BeneficiaryProfile - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'BeneficiaryProfile - StrategicPlan - FR',
    'Prompt for generating BeneficiaryProfile section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'BeneficiaryProfile',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Dressez un portrait détaillé des bénéficiaires : qui ils sont, leurs besoins spécifiques, les défis auxquels ils font face, et comment l'organisation répondra à ces besoins.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive BeneficiaryProfile section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'BeneficiaryProfile' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- GrantStrategy - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'GrantStrategy - StrategicPlan - EN',
    'Prompt for generating GrantStrategy section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'GrantStrategy',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Explain the grant funding strategy: identified sources (government, private foundations), application process, timeline, and anticipated success rate.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive GrantStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'GrantStrategy' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- GrantStrategy - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'GrantStrategy - StrategicPlan - FR',
    'Prompt for generating GrantStrategy section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'GrantStrategy',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Expliquez la stratégie de financement par subventions : sources identifiées (gouvernementales, fondations privées), processus de demande, calendrier et taux de réussite anticipé.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive GrantStrategy section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'GrantStrategy' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

-- SustainabilityPlan - StrategicPlan - EN
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'SustainabilityPlan - StrategicPlan - EN',
    'Prompt for generating SustainabilityPlan section in StrategicPlan plans (en)',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'SustainabilityPlan',
    $en_SP$You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.$en_SP$,
    $en_UP$Describe how the organization will ensure its long-term financial and operational sustainability, beyond initial funding. Include diversified revenue sources and sustainable growth strategy.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive SustainabilityPlan section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$en_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'SustainabilityPlan' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'en'
    AND "Category" = 'ContentGeneration'
);

-- SustainabilityPlan - StrategicPlan - FR
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'SustainabilityPlan - StrategicPlan - FR',
    'Prompt for generating SustainabilityPlan section in StrategicPlan plans (fr)',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'SustainabilityPlan',
    $fr_SP$Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.$fr_SP$,
    $fr_UP$Décrivez comment l'organisation assurera sa pérennité financière et opérationnelle à long terme, au-delà du financement initial. Incluez les sources de revenus diversifiées et la stratégie de croissance durable.

{questionnaireContext}

Based on the questionnaire responses above, write a comprehensive SustainabilityPlan section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words.$fr_UP$,
    '{"questionnaireContext": "The questionnaire responses context"}',
    true,
    1,
    0,
    0.0,
    0,
    'Default prompt seeded from hardcoded prompts',
    NOW() AT TIME ZONE 'UTC',
    false
WHERE NOT EXISTS (
    SELECT 1 FROM "AIPrompts" 
    WHERE "SectionName" = 'SustainabilityPlan' 
    AND "PlanType" = 'StrategicPlan' 
    AND "Language" = 'fr'
    AND "Category" = 'ContentGeneration'
);

COMMIT;

-- ============================================================================
-- END OF AI PROMPTS SEED SCRIPT
-- ============================================================================
