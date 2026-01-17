#!/usr/bin/env python3
"""
Generate SQL seed script for AI prompts
This script generates a complete SQL file with all default AI prompts
"""

import json
from datetime import datetime

# System prompts
SYSTEM_PROMPT_EN = """You are an expert business plan consultant with 20 years of experience helping entrepreneurs and non-profit organizations create professional, comprehensive business plans. Your expertise includes:
- Strategic planning and market analysis
- Financial projections and funding strategies
- Competitive positioning and value proposition development
- Operational and organizational planning
- Risk assessment and mitigation strategies

Write in a professional, clear, and compelling tone. Use concrete examples and actionable insights. Structure your content with proper headings and bullet points where appropriate. Aim for clarity and persuasiveness."""

SYSTEM_PROMPT_FR = """Vous êtes un consultant expert en plans d'affaires avec 20 ans d'expérience aidant les entrepreneurs et les organismes à but non lucratif à créer des plans d'affaires professionnels et complets. Votre expertise inclut :
- La planification stratégique et l'analyse de marché
- Les projections financières et les stratégies de financement
- Le positionnement concurrentiel et le développement de propositions de valeur
- La planification opérationnelle et organisationnelle
- L'évaluation et l'atténuation des risques

Rédigez dans un ton professionnel, clair et convaincant. Utilisez des exemples concrets et des perspectives actionnables. Structurez votre contenu avec des titres appropriés et des puces lorsque nécessaire. Visez la clarté et la persuasion."""

# English prompts
PROMPTS_EN = {
    "ExecutiveSummary": "Write a compelling executive summary that presents the company, its unique value proposition, target market, competitive advantages, and key financial objectives. The summary should entice the reader to learn more.",
    "ProblemStatement": "Identify and describe the problem or unmet need that your business/organization aims to solve. Explain why this problem is important and urgent for the target market.",
    "Solution": "Present the products or services offered in detail. Explain their features, benefits, how they solve customer problems, and what differentiates them from the competition.",
    "MarketAnalysis": "Analyze the target market: size, growth, trends, segments. Include industry data, opportunities, and challenges. Demonstrate a deep understanding of the market.",
    "CompetitiveAnalysis": "Identify main direct and indirect competitors. Analyze their strengths and weaknesses. Clearly explain the company's competitive positioning and distinctive advantages.",
    "SwotAnalysis": "Conduct a complete SWOT analysis: Strengths (internal assets), Weaknesses (internal limitations), Opportunities (positive external factors), Threats (external risks). Be specific and strategic.",
    "BusinessModel": "Explain the business model: how the company creates, delivers, and captures value. Include revenue streams, cost structure, key resources, and strategic partnerships.",
    "MarketingStrategy": "Describe the complete marketing strategy: positioning, branding, communication channels, customer acquisition tactics, content strategy, and marketing budget.",
    "BrandingStrategy": "Explain the branding strategy: visual identity, tone of communication, brand value proposition, differentiation, and how the brand will resonate with the target audience.",
    "OperationsPlan": "Describe daily operations: facilities, equipment, technologies, key processes, suppliers, supply chain, and quality management.",
    "ManagementTeam": "Present the management team: skills, experiences, roles, and responsibilities. Highlight how the team is positioned to succeed.",
    "FinancialProjections": "Summarize financial projections: expected revenues, main costs, profitability, cash flow needs. Explain the key assumptions behind these projections.",
    "FundingRequirements": "Detail funding needs: required amount, use of funds, potential funding sources, financing structure, and repayment plan or return on investment.",
    "RiskAnalysis": "Identify main risks (market, operational, financial, regulatory) and present concrete mitigation strategies for each.",
    "ExitStrategy": "Explain potential exit options for investors: acquisition, IPO, buyout. Include approximate timeline and valuation factors.",
    "MissionStatement": "Write a clear and inspiring mission statement that explains the organization's purpose, who it serves, and the impact it wishes to create in the community.",
    "SocialImpact": "Describe the expected social impact: positive changes in the community, social success indicators, direct and indirect beneficiaries, and contribution to sustainable development goals.",
    "BeneficiaryProfile": "Draw a detailed portrait of beneficiaries: who they are, their specific needs, the challenges they face, and how the organization will address these needs.",
    "GrantStrategy": "Explain the grant funding strategy: identified sources (government, private foundations), application process, timeline, and anticipated success rate.",
    "SustainabilityPlan": "Describe how the organization will ensure its long-term financial and operational sustainability, beyond initial funding. Include diversified revenue sources and sustainable growth strategy."
}

# French prompts
PROMPTS_FR = {
    "ExecutiveSummary": "Rédigez un résumé exécutif captivant qui présente l'entreprise, sa proposition de valeur unique, son marché cible, ses avantages concurrentiels et ses objectifs financiers principaux. Le résumé doit donner envie au lecteur d'en savoir plus.",
    "ProblemStatement": "Identifiez et décrivez le problème ou le besoin non satisfait que votre entreprise/organisation vise à résoudre. Expliquez pourquoi ce problème est important et urgent pour le marché cible.",
    "Solution": "Présentez en détail les produits ou services offerts. Expliquez leurs caractéristiques, leurs avantages, comment ils résolvent les problèmes des clients et ce qui les différencie de la concurrence.",
    "MarketAnalysis": "Analysez le marché cible : taille, croissance, tendances, segments. Incluez des données sur l'industrie, les opportunités et les défis. Démontrez une compréhension approfondie du marché.",
    "CompetitiveAnalysis": "Identifiez les principaux concurrents directs et indirects. Analysez leurs forces et faiblesses. Expliquez clairement le positionnement concurrentiel de l'entreprise et ses avantages distinctifs.",
    "SwotAnalysis": "Réalisez une analyse SWOT complète : Forces (atouts internes), Faiblesses (limites internes), Opportunités (facteurs externes positifs), Menaces (risques externes). Soyez spécifique et stratégique.",
    "BusinessModel": "Expliquez le modèle d'affaires : comment l'entreprise crée, délivre et capture de la valeur. Incluez les flux de revenus, la structure de coûts, les ressources clés et les partenariats stratégiques.",
    "MarketingStrategy": "Décrivez la stratégie marketing complète : positionnement, branding, canaux de communication, tactiques d'acquisition de clients, stratégie de contenu et budget marketing.",
    "BrandingStrategy": "Expliquez la stratégie de marque : identité visuelle, ton de communication, proposition de valeur de la marque, différenciation et comment la marque résonnera avec le public cible.",
    "OperationsPlan": "Décrivez les opérations quotidiennes : installations, équipements, technologies, processus clés, fournisseurs, chaîne d'approvisionnement et gestion de la qualité.",
    "ManagementTeam": "Présentez l'équipe de direction : compétences, expériences, rôles et responsabilités. Mettez en avant comment l'équipe est positionnée pour réussir.",
    "FinancialProjections": "Résumez les projections financières : revenus prévus, coûts principaux, rentabilité, besoins en trésorerie. Expliquez les hypothèses clés derrière ces projections.",
    "FundingRequirements": "Détaillez les besoins de financement : montant requis, utilisation des fonds, sources de financement potentielles, structure de financement et plan de remboursement ou retour sur investissement.",
    "RiskAnalysis": "Identifiez les principaux risques (marché, opérationnels, financiers, réglementaires) et présentez des stratégies concrètes d'atténuation pour chacun.",
    "ExitStrategy": "Expliquez les options de sortie potentielles pour les investisseurs : acquisition, IPO, buyout. Incluez un calendrier approximatif et les facteurs de valorisation.",
    "MissionStatement": "Rédigez un énoncé de mission clair et inspirant qui explique la raison d'être de l'organisation, qui elle sert, et l'impact qu'elle souhaite créer dans la communauté.",
    "SocialImpact": "Décrivez l'impact social attendu : changements positifs dans la communauté, indicateurs de succès social, bénéficiaires directs et indirects, et contribution aux objectifs de développement durable.",
    "BeneficiaryProfile": "Dressez un portrait détaillé des bénéficiaires : qui ils sont, leurs besoins spécifiques, les défis auxquels ils font face, et comment l'organisation répondra à ces besoins.",
    "GrantStrategy": "Expliquez la stratégie de financement par subventions : sources identifiées (gouvernementales, fondations privées), processus de demande, calendrier et taux de réussite anticipé.",
    "SustainabilityPlan": "Décrivez comment l'organisation assurera sa pérennité financière et opérationnelle à long terme, au-delà du financement initial. Incluez les sources de revenus diversifiées et la stratégie de croissance durable."
}

# Sections for each plan type
BUSINESS_PLAN_SECTIONS = [
    "ExecutiveSummary", "ProblemStatement", "Solution", "MarketAnalysis", "CompetitiveAnalysis",
    "SwotAnalysis", "BusinessModel", "MarketingStrategy", "BrandingStrategy", "OperationsPlan",
    "ManagementTeam", "FinancialProjections", "FundingRequirements", "RiskAnalysis", "ExitStrategy"
]

STRATEGIC_PLAN_SECTIONS = [
    "ExecutiveSummary", "ProblemStatement", "Solution", "MarketAnalysis", "CompetitiveAnalysis",
    "SwotAnalysis", "BusinessModel", "MarketingStrategy", "BrandingStrategy", "OperationsPlan",
    "ManagementTeam", "FinancialProjections", "FundingRequirements", "RiskAnalysis",
    "MissionStatement", "SocialImpact", "BeneficiaryProfile", "GrantStrategy", "SustainabilityPlan"
]

def escape_sql(text):
    """Escape text for SQL using dollar quoting"""
    # Replace dollar signs and backslashes
    text = text.replace('$', '$$')
    return text

def generate_sql():
    sql = """-- ============================================================================
-- Sqordia AI Prompts Seed Script (PostgreSQL)
-- ============================================================================
-- This script seeds the database with default AI prompts for business plan
-- content generation. These prompts are used by the BusinessPlanGenerationService.
--
-- Idempotent: Safe to run multiple times (uses WHERE NOT EXISTS)
-- Generated: {timestamp}
-- ============================================================================

BEGIN;

-- ============================================================================
-- SYSTEM PROMPTS
-- ============================================================================

""".format(timestamp=datetime.now().isoformat())
    
    # System prompts
    for plan_type in ["BusinessPlan", "StrategicPlan"]:
        for lang, lang_code in [("English", "en"), ("French", "fr")]:
            system_prompt = SYSTEM_PROMPT_EN if lang_code == "en" else SYSTEM_PROMPT_FR
            sql += f"""-- System Prompt: {plan_type} - {lang}
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    'System Prompt - {plan_type} - {lang_code.upper()}',
    'Default system prompt for {plan_type} in {lang}',
    'SystemPrompt',
    '{plan_type}',
    '{lang_code}',
    NULL,
    ${lang_code}_SP${escape_sql(system_prompt)}${lang_code}_SP$,
    '',
    '{{}}',
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
    AND "PlanType" = '{plan_type}' 
    AND "Language" = '{lang_code}'
);

"""
    
    # Section prompts
    for plan_type, sections in [("BusinessPlan", BUSINESS_PLAN_SECTIONS), ("StrategicPlan", STRATEGIC_PLAN_SECTIONS)]:
        for section in sections:
            for lang_code, prompts_dict in [("en", PROMPTS_EN), ("fr", PROMPTS_FR)]:
                if section not in prompts_dict:
                    continue
                
                template = prompts_dict[section]
                lang_name = "English" if lang_code == "en" else "French"
                system_prompt = SYSTEM_PROMPT_EN if lang_code == "en" else SYSTEM_PROMPT_FR
                
                user_prompt_template = f"{template}\n\n{{questionnaireContext}}\n\nBased on the questionnaire responses above, write a comprehensive {section} section for this business plan. Make it specific to this business, using the details provided. Aim for 400-600 words."
                
                sql += f"""-- {section} - {plan_type} - {lang_code.upper()}
INSERT INTO "AIPrompts" (
    "Id", "Name", "Description", "Category", "PlanType", "Language", "SectionName",
    "SystemPrompt", "UserPromptTemplate", "Variables", "IsActive", "Version",
    "UsageCount", "AverageRating", "RatingCount", "Notes", "Created", "IsDeleted"
)
SELECT 
    gen_random_uuid(),
    '{section} - {plan_type} - {lang_code.upper()}',
    'Prompt for generating {section} section in {plan_type} plans ({lang_code})',
    'ContentGeneration',
    '{plan_type}',
    '{lang_code}',
    '{section}',
    ${lang_code}_SP${escape_sql(system_prompt)}${lang_code}_SP$,
    ${lang_code}_UP${escape_sql(user_prompt_template)}${lang_code}_UP$,
    '{{"questionnaireContext": "The questionnaire responses context"}}',
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
    WHERE "SectionName" = '{section}' 
    AND "PlanType" = '{plan_type}' 
    AND "Language" = '{lang_code}'
    AND "Category" = 'ContentGeneration'
);

"""
    
    sql += """COMMIT;

-- ============================================================================
-- END OF AI PROMPTS SEED SCRIPT
-- ============================================================================
"""
    
    return sql

if __name__ == "__main__":
    output = generate_sql()
    with open("seed-ai-prompts.sql", "w", encoding="utf-8") as f:
        f.write(output)
    print("Generated seed-ai-prompts.sql successfully!")
