-- ============================================================================
-- Sqordia AI Prompts Update Script - Visual Elements Support
-- ============================================================================
-- This script removes old prompts and creates new ones with visual element
-- instructions for charts, tables, and metrics.
--
-- Run with: docker exec -i sqordia-db-dev psql -U postgres -d sqordia < scripts/update-prompts-visual-elements.sql
-- ============================================================================

BEGIN;

-- ============================================================================
-- STEP 1: Delete all existing prompts
-- ============================================================================
DELETE FROM "AIPrompts";

-- ============================================================================
-- STEP 2: Insert System Prompts with Visual Elements
-- ============================================================================

-- System Prompt: BusinessPlan - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'System Prompt - BusinessPlan - EN',
    'System prompt for BusinessPlan with visual elements support',
    'SystemPrompt',
    'BusinessPlan',
    'en',
    NULL,
    'You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness.

VISUAL ELEMENTS: When appropriate, include visual elements to enhance the content using the following JSON code block format:

For charts (use for trends, comparisons, projections):
```json:chart
{"chartType": "bar", "title": "Chart Title", "labels": ["Label1", "Label2"], "datasets": [{"label": "Series Name", "data": [100, 200], "color": "#3B82F6"}]}
```

For tables (use for structured data, comparisons, timelines):
```json:table
{"tableType": "comparison", "headers": ["Feature", "Us", "Competitor"], "rows": [{"cells": [{"value": "Price"}, {"value": "$99"}, {"value": "$149"}]}]}
```

For key metrics (use for KPIs, important numbers):
```json:metrics
{"layout": "grid", "metrics": [{"label": "Market Size", "value": 5000000, "format": "currency"}, {"label": "Growth", "value": 25, "format": "percentage", "trend": "up"}]}
```

Chart types: line, bar, stacked-bar, pie, donut, area
Table types: financial, comparison, swot, timeline, pricing, custom
Metric formats: currency, percentage, number, text
Metric trends: up, down, neutral

Include 1-3 relevant visual elements per section where they add value. Always include explanatory prose around each visual element.',
    '',
    '{}',
    true,
    1,
    0,
    0.0,
    0,
    'System prompt with visual elements support',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- System Prompt: BusinessPlan - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'System Prompt - BusinessPlan - FR',
    'System prompt for BusinessPlan in French with visual elements',
    'SystemPrompt',
    'BusinessPlan',
    'fr',
    NULL,
    'Vous êtes un consultant expert en plans d''affaires avec 20 ans d''expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d''affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l''analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L''évaluation et l''atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion.

ÉLÉMENTS VISUELS: Lorsque approprié, incluez des éléments visuels pour enrichir le contenu en utilisant le format JSON suivant:

Pour les graphiques (utilisez pour les tendances, comparaisons, projections):
```json:chart
{"chartType": "bar", "title": "Titre du graphique", "labels": ["Label1", "Label2"], "datasets": [{"label": "Nom de série", "data": [100, 200], "color": "#3B82F6"}]}
```

Pour les tableaux (utilisez pour les données structurées, comparaisons, calendriers):
```json:table
{"tableType": "comparison", "headers": ["Caractéristique", "Nous", "Concurrent"], "rows": [{"cells": [{"value": "Prix"}, {"value": "99$"}, {"value": "149$"}]}]}
```

Pour les métriques clés (utilisez pour les KPI, chiffres importants):
```json:metrics
{"layout": "grid", "metrics": [{"label": "Taille du marché", "value": 5000000, "format": "currency"}, {"label": "Croissance", "value": 25, "format": "percentage", "trend": "up"}]}
```

Types de graphiques: line, bar, stacked-bar, pie, donut, area
Types de tableaux: financial, comparison, swot, timeline, pricing, custom
Formats de métriques: currency, percentage, number, text
Tendances: up, down, neutral

Incluez 1-3 éléments visuels pertinents par section lorsqu''ils ajoutent de la valeur. Incluez toujours du texte explicatif autour de chaque élément visuel.',
    '',
    '{}',
    true,
    1,
    0,
    0.0,
    0,
    'System prompt with visual elements support (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- System Prompt: StrategicPlan - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'System Prompt - StrategicPlan - EN',
    'System prompt for StrategicPlan/OBNL with visual elements',
    'SystemPrompt',
    'StrategicPlan',
    'en',
    NULL,
    'You are an expert consultant specializing in non-profit organizations (OBNL) and strategic planning with 20 years of experience. Your expertise includes:
- Mission-driven strategic planning
- Social impact measurement and reporting
- Grant writing and funding strategies
- Stakeholder engagement and community outreach
- Sustainable organizational development

Write in a professional, clear, and inspiring tone. Focus on social impact, community benefit, and organizational sustainability. Structure your content with proper headings and bullet points where appropriate.

VISUAL ELEMENTS: When appropriate, include visual elements to enhance the content using the following JSON code block format:

For charts (use for impact metrics, funding breakdown, growth):
```json:chart
{"chartType": "bar", "title": "Chart Title", "labels": ["Label1", "Label2"], "datasets": [{"label": "Series Name", "data": [100, 200], "color": "#3B82F6"}]}
```

For tables (use for structured data, comparisons, timelines):
```json:table
{"tableType": "comparison", "headers": ["Metric", "Year 1", "Year 2"], "rows": [{"cells": [{"value": "Beneficiaries"}, {"value": "500"}, {"value": "1000"}]}]}
```

For key metrics (use for KPIs, impact numbers):
```json:metrics
{"layout": "grid", "metrics": [{"label": "Beneficiaries Served", "value": 5000, "format": "number"}, {"label": "Program Growth", "value": 25, "format": "percentage", "trend": "up"}]}
```

Include 1-3 relevant visual elements per section where they add value. Always include explanatory prose around each visual element.',
    '',
    '{}',
    true,
    1,
    0,
    0.0,
    0,
    'System prompt for OBNL/Strategic plans with visual elements',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- System Prompt: StrategicPlan - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'System Prompt - StrategicPlan - FR',
    'System prompt for StrategicPlan/OBNL in French with visual elements',
    'SystemPrompt',
    'StrategicPlan',
    'fr',
    NULL,
    'Vous êtes un consultant expert spécialisé dans les organismes à but non lucratif (OBNL) et la planification stratégique avec 20 ans d''expérience. Votre expertise inclut :
- La planification stratégique axée sur la mission
- La mesure et le rapport d''impact social
- La rédaction de demandes de subventions et les stratégies de financement
- L''engagement des parties prenantes et la sensibilisation communautaire
- Le développement organisationnel durable

Rédigez dans un ton professionnel, clair et inspirant. Concentrez-vous sur l''impact social, les bénéfices communautaires et la durabilité organisationnelle. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire.

ÉLÉMENTS VISUELS: Lorsque approprié, incluez des éléments visuels pour enrichir le contenu:

Pour les graphiques:
```json:chart
{"chartType": "bar", "title": "Titre", "labels": ["Label1", "Label2"], "datasets": [{"label": "Série", "data": [100, 200], "color": "#3B82F6"}]}
```

Pour les tableaux:
```json:table
{"tableType": "comparison", "headers": ["Métrique", "An 1", "An 2"], "rows": [{"cells": [{"value": "Bénéficiaires"}, {"value": "500"}, {"value": "1000"}]}]}
```

Pour les métriques clés:
```json:metrics
{"layout": "grid", "metrics": [{"label": "Bénéficiaires servis", "value": 5000, "format": "number"}, {"label": "Croissance", "value": 25, "format": "percentage", "trend": "up"}]}
```

Incluez 1-3 éléments visuels pertinents par section.',
    '',
    '{}',
    true,
    1,
    0,
    0.0,
    0,
    'System prompt for OBNL/Strategic plans with visual elements (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- ============================================================================
-- STEP 3: Insert Section Prompts with Visual Elements
-- ============================================================================

-- ExecutiveSummary - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Executive Summary - EN',
    'Executive Summary section prompt with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'ExecutiveSummary',
    '',
    'Write a compelling executive summary that presents the company, its unique value proposition, target market, competitive advantages, and key financial objectives. The summary should entice the reader to learn more.

INCLUDE a key metrics visual element showing 3-4 important KPIs (e.g., target revenue, market size, growth rate, funding needed).

{context}

Based on the questionnaire responses above, write a comprehensive Executive Summary section. Make it specific to this business, using the details provided. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Executive Summary with visual elements',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- ExecutiveSummary - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Executive Summary - FR',
    'Executive Summary section prompt in French with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'ExecutiveSummary',
    '',
    'Rédigez un résumé exécutif captivant qui présente l''entreprise, sa proposition de valeur unique, son marché cible, ses avantages concurrentiels et ses objectifs financiers principaux. Le résumé doit donner envie au lecteur d''en savoir plus.

INCLUEZ un élément visuel de métriques clés montrant 3-4 KPI importants (ex: revenus cibles, taille du marché, taux de croissance, financement requis).

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez un résumé exécutif complet. Rendez-le spécifique à cette entreprise. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Executive Summary with visual elements (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- MarketAnalysis - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Market Analysis - EN',
    'Market Analysis section prompt with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'MarketAnalysis',
    '',
    'Analyze the target market: size, growth, trends, segments. Include industry data, opportunities, and challenges. Demonstrate a deep understanding of the market.

INCLUDE visual elements:
1. A bar or pie chart showing market segmentation or market share distribution
2. Key metrics showing TAM (Total Addressable Market), SAM (Serviceable Addressable Market), and SOM (Serviceable Obtainable Market)

{context}

Based on the questionnaire responses above, write a comprehensive Market Analysis section. Make it specific to this business, using the details provided. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Market Analysis with visual elements',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- MarketAnalysis - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Market Analysis - FR',
    'Market Analysis section prompt in French with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'MarketAnalysis',
    '',
    'Analysez le marché cible : taille, croissance, tendances, segments. Incluez des données sur l''industrie, les opportunités et les défis. Démontrez une compréhension approfondie du marché.

INCLUEZ des éléments visuels:
1. Un graphique à barres ou en camembert montrant la segmentation ou les parts de marché
2. Des métriques clés montrant TAM, SAM et SOM

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une analyse de marché complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Market Analysis with visual elements (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- CompetitiveAnalysis - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Competitive Analysis - EN',
    'Competitive Analysis section prompt with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'CompetitiveAnalysis',
    '',
    'Identify main direct and indirect competitors. Analyze their strengths and weaknesses. Clearly explain the company''s competitive positioning and distinctive advantages.

INCLUDE a comparison table showing key features/capabilities vs competitors. Use tableType: "comparison" with columns for your company and 2-3 main competitors.

{context}

Based on the questionnaire responses above, write a comprehensive Competitive Analysis section. Make it specific to this business. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Competitive Analysis with comparison table',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- CompetitiveAnalysis - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Competitive Analysis - FR',
    'Competitive Analysis section prompt in French with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'CompetitiveAnalysis',
    '',
    'Identifiez les principaux concurrents directs et indirects. Analysez leurs forces et faiblesses. Expliquez clairement le positionnement concurrentiel de l''entreprise et ses avantages distinctifs.

INCLUEZ un tableau comparatif montrant les caractéristiques/capacités clés vs les concurrents. Utilisez tableType: "comparison".

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une analyse concurrentielle complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Competitive Analysis with comparison table (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- SwotAnalysis - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'SWOT Analysis - EN',
    'SWOT Analysis section prompt with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'SwotAnalysis',
    '',
    'Conduct a complete SWOT analysis: Strengths (internal assets), Weaknesses (internal limitations), Opportunities (positive external factors), Threats (external risks). Be specific and strategic.

INCLUDE a SWOT table using this format:
```json:table
{"tableType": "swot", "headers": ["Strengths", "Weaknesses", "Opportunities", "Threats"], "rows": [{"cells": [{"value": "• Strength 1\n• Strength 2\n• Strength 3"}, {"value": "• Weakness 1\n• Weakness 2"}, {"value": "• Opportunity 1\n• Opportunity 2"}, {"value": "• Threat 1\n• Threat 2"}]}]}
```

{context}

Based on the questionnaire responses above, write a comprehensive SWOT Analysis section. Make it specific to this business. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'SWOT Analysis with SWOT table',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- SwotAnalysis - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'SWOT Analysis - FR',
    'SWOT Analysis section prompt in French with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'SwotAnalysis',
    '',
    'Réalisez une analyse SWOT complète : Forces (atouts internes), Faiblesses (limites internes), Opportunités (facteurs externes positifs), Menaces (risques externes). Soyez spécifique et stratégique.

INCLUEZ un tableau SWOT avec tableType "swot" contenant les quatre quadrants avec des éléments spécifiques pour chacun.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une analyse SWOT complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'SWOT Analysis with SWOT table (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- FinancialProjections - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Financial Projections - EN',
    'Financial Projections section prompt with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'FinancialProjections',
    '',
    'Summarize financial projections: expected revenues, main costs, profitability, cash flow needs. Explain the key assumptions behind these projections.

INCLUDE visual elements:
1. A line or bar chart showing 3-5 year revenue projections
2. Key metrics showing important financial KPIs (break-even point, gross margin, net profit margin, etc.)
3. A financial table showing yearly projections (revenue, costs, gross profit, net profit)

{context}

Based on the questionnaire responses above, write a comprehensive Financial Projections section. Make it specific to this business. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Financial Projections with charts, table, and metrics',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- FinancialProjections - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Financial Projections - FR',
    'Financial Projections section prompt in French with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'FinancialProjections',
    '',
    'Résumez les projections financières : revenus prévus, coûts principaux, rentabilité, besoins en trésorerie. Expliquez les hypothèses clés derrière ces projections.

INCLUEZ des éléments visuels:
1. Un graphique linéaire ou à barres montrant les projections de revenus sur 3-5 ans
2. Des métriques clés montrant les KPI financiers importants (seuil de rentabilité, marge brute, etc.)
3. Un tableau financier montrant les projections annuelles (revenus, coûts, profit)

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une section Projections Financières complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Financial Projections with charts, table, and metrics (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- FundingRequirements - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Funding Requirements - EN',
    'Funding Requirements section prompt with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'FundingRequirements',
    '',
    'Detail funding needs: required amount, use of funds, potential funding sources, financing structure, and repayment plan or return on investment.

INCLUDE:
1. A pie chart showing use of funds breakdown (e.g., Product Development 30%, Marketing 25%, Operations 20%, etc.)
2. Key metrics showing funding amount, expected ROI, and timeline to profitability

{context}

Based on the questionnaire responses above, write a comprehensive Funding Requirements section. Make it specific to this business. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Funding Requirements with pie chart and metrics',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- FundingRequirements - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Funding Requirements - FR',
    'Funding Requirements section prompt in French with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'FundingRequirements',
    '',
    'Détaillez les besoins de financement : montant requis, utilisation des fonds, sources de financement potentielles, structure de financement et plan de remboursement ou retour sur investissement.

INCLUEZ:
1. Un graphique en camembert montrant la répartition de l''utilisation des fonds
2. Des métriques clés montrant le montant du financement, le ROI attendu, le calendrier

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une section Besoins de Financement complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Funding Requirements with pie chart and metrics (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- RiskAnalysis - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Risk Analysis - EN',
    'Risk Analysis section prompt with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'RiskAnalysis',
    '',
    'Identify main risks (market, operational, financial, regulatory) and present concrete mitigation strategies for each.

INCLUDE a risk assessment table showing:
- Risk name/description
- Category (Market, Operational, Financial, Regulatory)
- Likelihood (High, Medium, Low)
- Impact (High, Medium, Low)
- Mitigation strategy

{context}

Based on the questionnaire responses above, write a comprehensive Risk Analysis section. Make it specific to this business. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Risk Analysis with risk assessment table',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- RiskAnalysis - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Risk Analysis - FR',
    'Risk Analysis section prompt in French with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'RiskAnalysis',
    '',
    'Identifiez les principaux risques (marché, opérationnels, financiers, réglementaires) et présentez des stratégies concrètes d''atténuation pour chacun.

INCLUEZ un tableau d''évaluation des risques montrant:
- Nom/description du risque
- Catégorie
- Probabilité (Élevée, Moyenne, Faible)
- Impact
- Stratégie d''atténuation

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une analyse des risques complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Risk Analysis with risk assessment table (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- BusinessModel - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Business Model - EN',
    'Business Model section prompt with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'BusinessModel',
    '',
    'Explain the business model: how the company creates, delivers, and captures value. Include revenue streams, cost structure, key resources, and strategic partnerships.

INCLUDE a pie chart showing revenue stream breakdown or cost structure distribution.

{context}

Based on the questionnaire responses above, write a comprehensive Business Model section. Make it specific to this business. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Business Model with pie chart',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- BusinessModel - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Business Model - FR',
    'Business Model section prompt in French with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'BusinessModel',
    '',
    'Expliquez le modèle d''affaires : comment l''entreprise crée, délivre et capture de la valeur. Incluez les flux de revenus, la structure de coûts, les ressources clés et les partenariats stratégiques.

INCLUEZ un graphique en camembert montrant la répartition des sources de revenus ou la structure des coûts.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une section Modèle d''Affaires complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Business Model with pie chart (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- MarketingStrategy - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Marketing Strategy - EN',
    'Marketing Strategy section prompt with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'MarketingStrategy',
    '',
    'Describe the complete marketing strategy: positioning, branding, communication channels, customer acquisition tactics, content strategy, and marketing budget.

INCLUDE a chart showing marketing budget allocation across channels or expected customer acquisition over time.

{context}

Based on the questionnaire responses above, write a comprehensive Marketing Strategy section. Make it specific to this business. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Marketing Strategy with budget allocation chart',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- MarketingStrategy - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Marketing Strategy - FR',
    'Marketing Strategy section prompt in French with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'MarketingStrategy',
    '',
    'Décrivez la stratégie marketing complète : positionnement, branding, canaux de communication, tactiques d''acquisition de clients, stratégie de contenu et budget marketing.

INCLUEZ un graphique montrant l''allocation du budget marketing par canal ou l''acquisition de clients prévue dans le temps.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une Stratégie Marketing complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Marketing Strategy with budget allocation chart (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- OperationsPlan - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Operations Plan - EN',
    'Operations Plan section prompt with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'OperationsPlan',
    '',
    'Describe daily operations: facilities, equipment, technologies, key processes, suppliers, supply chain, and quality management.

INCLUDE a timeline table showing key operational milestones or a process flow showing main operational steps.

{context}

Based on the questionnaire responses above, write a comprehensive Operations Plan section. Make it specific to this business. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Operations Plan with timeline table',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- OperationsPlan - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Operations Plan - FR',
    'Operations Plan section prompt in French with visual elements',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'OperationsPlan',
    '',
    'Décrivez les opérations quotidiennes : installations, équipements, technologies, processus clés, fournisseurs, chaîne d''approvisionnement et gestion de la qualité.

INCLUEZ un tableau chronologique montrant les jalons opérationnels clés ou les étapes du processus.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez un Plan des Opérations complet. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Operations Plan with timeline table (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- ProblemStatement - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Problem Statement - EN',
    'Problem Statement section prompt',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'ProblemStatement',
    '',
    'Identify and describe the problem or unmet need that your business/organization aims to solve. Explain why this problem is important and urgent for the target market.

{context}

Based on the questionnaire responses above, write a comprehensive Problem Statement section. Make it specific to this business. Aim for 300-500 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Problem Statement',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- ProblemStatement - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Problem Statement - FR',
    'Problem Statement section prompt in French',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'ProblemStatement',
    '',
    'Identifiez et décrivez le problème ou le besoin non satisfait que votre entreprise/organisation vise à résoudre. Expliquez pourquoi ce problème est important et urgent pour le marché cible.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une section Énoncé du Problème complète. Visez 300-500 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Problem Statement (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- Solution - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Solution - EN',
    'Solution section prompt',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'Solution',
    '',
    'Present the products or services offered in detail. Explain their features, benefits, how they solve customer problems, and what differentiates them from the competition.

{context}

Based on the questionnaire responses above, write a comprehensive Solution section. Make it specific to this business. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Solution',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- Solution - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Solution - FR',
    'Solution section prompt in French',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'Solution',
    '',
    'Présentez en détail les produits ou services offerts. Expliquez leurs caractéristiques, leurs avantages, comment ils résolvent les problèmes des clients et ce qui les différencie de la concurrence.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une section Solution complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Solution (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- ManagementTeam - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Management Team - EN',
    'Management Team section prompt',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'ManagementTeam',
    '',
    'Present the management team: skills, experiences, roles, and responsibilities. Highlight how the team is positioned to succeed.

{context}

Based on the questionnaire responses above, write a comprehensive Management Team section. Make it specific to this business. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Management Team',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- ManagementTeam - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Management Team - FR',
    'Management Team section prompt in French',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'ManagementTeam',
    '',
    'Présentez l''équipe de direction : compétences, expériences, rôles et responsabilités. Mettez en avant comment l''équipe est positionnée pour réussir.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une section Équipe de Direction complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Management Team (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- BrandingStrategy - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Branding Strategy - EN',
    'Branding Strategy section prompt',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'BrandingStrategy',
    '',
    'Explain the branding strategy: visual identity, tone of communication, brand value proposition, differentiation, and how the brand will resonate with the target audience.

{context}

Based on the questionnaire responses above, write a comprehensive Branding Strategy section. Make it specific to this business. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Branding Strategy',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- BrandingStrategy - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Branding Strategy - FR',
    'Branding Strategy section prompt in French',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'BrandingStrategy',
    '',
    'Expliquez la stratégie de marque : identité visuelle, ton de communication, proposition de valeur de la marque, différenciation et comment la marque résonnera avec le public cible.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une Stratégie de Marque complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Branding Strategy (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- ExitStrategy - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Exit Strategy - EN',
    'Exit Strategy section prompt',
    'ContentGeneration',
    'BusinessPlan',
    'en',
    'ExitStrategy',
    '',
    'Explain potential exit options for investors: acquisition, IPO, buyout. Include approximate timeline and valuation factors.

{context}

Based on the questionnaire responses above, write a comprehensive Exit Strategy section. Make it specific to this business. Aim for 300-500 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Exit Strategy',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- ExitStrategy - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Exit Strategy - FR',
    'Exit Strategy section prompt in French',
    'ContentGeneration',
    'BusinessPlan',
    'fr',
    'ExitStrategy',
    '',
    'Expliquez les options de sortie potentielles pour les investisseurs : acquisition, IPO, buyout. Incluez un calendrier approximatif et les facteurs de valorisation.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une Stratégie de Sortie complète. Visez 300-500 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Exit Strategy (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- ============================================================================
-- STEP 4: Insert OBNL/Strategic Plan specific section prompts
-- ============================================================================

-- MissionStatement - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Mission Statement - EN',
    'Mission Statement section prompt for OBNL',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'MissionStatement',
    '',
    'Write a clear and inspiring mission statement that explains the organization''s purpose, who it serves, and the impact it wishes to create in the community.

{context}

Based on the questionnaire responses above, write a comprehensive Mission Statement section. Make it specific to this organization. Aim for 300-500 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Mission Statement for OBNL',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- MissionStatement - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Mission Statement - FR',
    'Mission Statement section prompt for OBNL in French',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'MissionStatement',
    '',
    'Rédigez un énoncé de mission clair et inspirant qui explique la raison d''être de l''organisation, qui elle sert, et l''impact qu''elle souhaite créer dans la communauté.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez un Énoncé de Mission complet. Visez 300-500 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Mission Statement for OBNL (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- SocialImpact - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Social Impact - EN',
    'Social Impact section prompt for OBNL with visual elements',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'SocialImpact',
    '',
    'Describe the expected social impact: positive changes in the community, social success indicators, direct and indirect beneficiaries, and contribution to sustainable development goals.

INCLUDE key metrics showing expected impact numbers (beneficiaries served, outcomes achieved, community reach, etc.).

{context}

Based on the questionnaire responses above, write a comprehensive Social Impact section. Make it specific to this organization. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Social Impact with metrics for OBNL',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- SocialImpact - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Social Impact - FR',
    'Social Impact section prompt for OBNL in French with visual elements',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'SocialImpact',
    '',
    'Décrivez l''impact social attendu : changements positifs dans la communauté, indicateurs de succès social, bénéficiaires directs et indirects, et contribution aux objectifs de développement durable.

INCLUEZ des métriques clés montrant les chiffres d''impact attendus (bénéficiaires servis, résultats atteints, etc.).

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une section Impact Social complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Social Impact with metrics for OBNL (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- GrantStrategy - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Grant Strategy - EN',
    'Grant Strategy section prompt for OBNL with visual elements',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'GrantStrategy',
    '',
    'Explain the grant funding strategy: identified sources (government, private foundations), application process, timeline, and anticipated success rate.

INCLUDE a table showing potential grant sources, amounts, and application timelines.

{context}

Based on the questionnaire responses above, write a comprehensive Grant Strategy section. Make it specific to this organization. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Grant Strategy with funding table for OBNL',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- GrantStrategy - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Grant Strategy - FR',
    'Grant Strategy section prompt for OBNL in French with visual elements',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'GrantStrategy',
    '',
    'Expliquez la stratégie de financement par subventions : sources identifiées (gouvernementales, fondations privées), processus de demande, calendrier et taux de réussite anticipé.

INCLUEZ un tableau montrant les sources potentielles de subventions, les montants et les calendriers de demande.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez une Stratégie de Subventions complète. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Grant Strategy with funding table for OBNL (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- SustainabilityPlan - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Sustainability Plan - EN',
    'Sustainability Plan section prompt for OBNL with visual elements',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'SustainabilityPlan',
    '',
    'Describe how the organization will ensure its long-term financial and operational sustainability, beyond initial funding. Include diversified revenue sources and sustainable growth strategy.

INCLUDE a chart showing projected revenue diversification over time.

{context}

Based on the questionnaire responses above, write a comprehensive Sustainability Plan section. Make it specific to this organization. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Sustainability Plan with chart for OBNL',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- SustainabilityPlan - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Sustainability Plan - FR',
    'Sustainability Plan section prompt for OBNL in French with visual elements',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'SustainabilityPlan',
    '',
    'Décrivez comment l''organisation assurera sa pérennité financière et opérationnelle à long terme, au-delà du financement initial. Incluez les sources de revenus diversifiées et la stratégie de croissance durable.

INCLUEZ un graphique montrant la diversification projetée des revenus dans le temps.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez un Plan de Durabilité complet. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Sustainability Plan with chart for OBNL (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- BeneficiaryProfile - English
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Beneficiary Profile - EN',
    'Beneficiary Profile section prompt for OBNL',
    'ContentGeneration',
    'StrategicPlan',
    'en',
    'BeneficiaryProfile',
    '',
    'Draw a detailed portrait of beneficiaries: who they are, their specific needs, the challenges they face, and how the organization will address these needs.

{context}

Based on the questionnaire responses above, write a comprehensive Beneficiary Profile section. Make it specific to this organization. Aim for 400-600 words.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Beneficiary Profile for OBNL',
    NOW() AT TIME ZONE 'UTC',
    false
);

-- BeneficiaryProfile - French
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
) VALUES (
    gen_random_uuid(),
    'Beneficiary Profile - FR',
    'Beneficiary Profile section prompt for OBNL in French',
    'ContentGeneration',
    'StrategicPlan',
    'fr',
    'BeneficiaryProfile',
    '',
    'Dressez un portrait détaillé des bénéficiaires : qui ils sont, leurs besoins spécifiques, les défis auxquels ils font face, et comment l''organisation répondra à ces besoins.

{context}

Basé sur les réponses au questionnaire ci-dessus, rédigez un Profil des Bénéficiaires complet. Visez 400-600 mots.',
    '{"context": "Questionnaire responses"}',
    true,
    1,
    0,
    0.0,
    0,
    'Beneficiary Profile for OBNL (French)',
    NOW() AT TIME ZONE 'UTC',
    false
);

COMMIT;

-- ============================================================================
-- Verification
-- ============================================================================
SELECT
    "Category",
    "PlanType",
    "Language",
    COUNT(*) as count
FROM "AIPrompts"
GROUP BY "Category", "PlanType", "Language"
ORDER BY "Category", "PlanType", "Language";
