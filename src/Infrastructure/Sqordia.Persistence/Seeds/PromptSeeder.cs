using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;
using Sqordia.Persistence.Contexts;

namespace Sqordia.Persistence.Seeds;

/// <summary>
/// Seeds default prompt templates for business plan content generation.
/// Creates prompts for all major section types with Production alias.
/// </summary>
public class PromptSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PromptSeeder> _logger;
    private const string CreatedBy = "System";

    public PromptSeeder(ApplicationDbContext context, ILogger<PromptSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds all default prompt templates if they don't already exist.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting prompt template seeding...");

        var existingCount = await _context.PromptTemplates.CountAsync(cancellationToken);
        if (existingCount > 0)
        {
            _logger.LogInformation("Prompt templates already exist ({Count} found). Skipping seeding.", existingCount);
            return;
        }

        var prompts = GetDefaultPromptTemplates();

        foreach (var prompt in prompts)
        {
            _context.PromptTemplates.Add(prompt);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully seeded {Count} prompt templates.", prompts.Count);
    }

    /// <summary>
    /// Returns all default prompt templates for BusinessPlan type.
    /// </summary>
    private List<PromptTemplate> GetDefaultPromptTemplates()
    {
        var prompts = new List<PromptTemplate>();

        // Add prompts for each section type
        prompts.Add(CreateExecutiveSummaryPrompt());
        prompts.Add(CreateCompanyOverviewPrompt());
        prompts.Add(CreateMarketAnalysisPrompt());
        prompts.Add(CreateProductsServicesPrompt());
        prompts.Add(CreateFinancialProjectionsPrompt());
        prompts.Add(CreateMarketingStrategyPrompt());
        prompts.Add(CreateOperationsPlanPrompt());
        prompts.Add(CreateSWOTAnalysisPrompt());

        // Activate all prompts and set Production alias
        foreach (var prompt in prompts)
        {
            prompt.Activate();
            prompt.SetAlias(PromptAlias.Production);
        }

        return prompts;
    }

    private PromptTemplate CreateExecutiveSummaryPrompt()
    {
        var systemPrompt = @"You are a professional business plan writer specializing in executive summaries. Your role is to create compelling, concise summaries that capture the essence of a business opportunity and convince stakeholders to read further.

Guidelines:
- Write in a professional, confident tone
- Focus on the most impactful points
- Include key metrics and financial highlights
- Make the value proposition immediately clear
- Structure content for quick scanning with clear sections";

        var userPromptTemplate = @"Write an executive summary for a business plan with the following details:

**Company Information:**
- Company Name: {{companyName}}
- Industry: {{industry}}
- Business Description: {{description}}

**Market Opportunity:**
- Target Market: {{targetMarket}}
- Market Size: {{marketSize}}

**Offering:**
- Key Products/Services: {{products}}
- Unique Value Proposition: {{valueProposition}}

**Financial Overview:**
- Funding Required: {{fundingRequest}}
- Revenue Projections: {{revenueProjections}}
- Financial Goals: {{financialGoals}}

**Team:**
- Key Team Members: {{teamMembers}}

**Requirements:**
1. Start with a compelling hook that captures attention
2. Clearly state the business opportunity and market need
3. Highlight competitive advantages and unique differentiators
4. Summarize financial projections and funding requirements
5. End with a clear call-to-action or ask

**Output Format:** Professional prose with key metrics highlighted. Include 2-3 visual element placeholders.
**Length:** 500-700 words";

        var visualElementsJson = @"{
  ""elements"": [
    {
      ""type"": ""metric_cards"",
      ""title"": ""Key Highlights"",
      ""metrics"": [
        { ""label"": ""Market Opportunity"", ""placeholder"": ""{{marketSize}}"", ""format"": ""currency"" },
        { ""label"": ""Revenue Target (Year 3)"", ""placeholder"": ""{{revenueYear3}}"", ""format"": ""currency"" },
        { ""label"": ""Funding Required"", ""placeholder"": ""{{fundingRequest}}"", ""format"": ""currency"" }
      ]
    }
  ]
}";

        return new PromptTemplate(
            SectionType.ExecutiveSummary,
            BusinessPlanType.BusinessPlan,
            "Executive Summary - Standard v1",
            systemPrompt,
            userPromptTemplate,
            OutputFormat.Mixed,
            CreatedBy,
            description: "Standard executive summary generation for general business plans",
            visualElementsJson: visualElementsJson
        );
    }

    private PromptTemplate CreateCompanyOverviewPrompt()
    {
        var systemPrompt = @"You are a professional business writer specializing in company descriptions and organizational profiles. Your role is to create comprehensive yet engaging company overviews that establish credibility and communicate the company's identity, history, and vision.

Guidelines:
- Present the company's story in a compelling narrative
- Highlight key milestones and achievements
- Clearly articulate mission, vision, and values
- Describe the organizational structure professionally
- Balance factual information with aspirational elements";

        var userPromptTemplate = @"Write a company overview section for a business plan with the following details:

**Company Identity:**
- Company Name: {{companyName}}
- Legal Structure: {{legalStructure}}
- Founded/Founding Date: {{foundedDate}}
- Location: {{location}}

**Mission & Vision:**
- Mission Statement: {{missionStatement}}
- Vision Statement: {{visionStatement}}
- Core Values: {{coreValues}}

**History & Background:**
- Company History: {{companyHistory}}
- Key Milestones: {{milestones}}

**Organizational Structure:**
- Ownership Structure: {{ownershipStructure}}
- Key Personnel: {{keyPersonnel}}
- Number of Employees: {{employeeCount}}

**Requirements:**
1. Begin with a strong company identity statement
2. Present the mission and vision clearly
3. Include relevant company history and milestones
4. Describe the organizational structure
5. Highlight what makes the company unique

**Output Format:** Professional prose with structured sections. Include 1-2 visual elements.
**Length:** 400-600 words";

        var visualElementsJson = @"{
  ""elements"": [
    {
      ""type"": ""timeline"",
      ""title"": ""Company Milestones"",
      ""description"": ""Key achievements and milestones in company history""
    },
    {
      ""type"": ""org_chart"",
      ""title"": ""Organizational Structure"",
      ""description"": ""Leadership team and organizational hierarchy""
    }
  ]
}";

        return new PromptTemplate(
            SectionType.CompanyOverview,
            BusinessPlanType.BusinessPlan,
            "Company Overview - Standard v1",
            systemPrompt,
            userPromptTemplate,
            OutputFormat.Mixed,
            CreatedBy,
            description: "Standard company overview generation for business plans",
            visualElementsJson: visualElementsJson
        );
    }

    private PromptTemplate CreateMarketAnalysisPrompt()
    {
        var systemPrompt = @"You are a market research analyst with expertise in competitive analysis, market sizing, and industry trends. Your role is to create data-driven market analysis sections that demonstrate deep understanding of the target market and competitive landscape.

Guidelines:
- Use specific data points and statistics when available
- Structure analysis using TAM/SAM/SOM framework
- Provide objective competitor assessments
- Identify and analyze key market trends
- Support claims with evidence and reasoning";

        var userPromptTemplate = @"Write a comprehensive market analysis section for a business plan with the following details:

**Industry Overview:**
- Industry: {{industry}}
- Industry Description: {{industryDescription}}
- Industry Growth Rate: {{industryGrowthRate}}

**Market Size:**
- Total Addressable Market (TAM): {{tam}}
- Serviceable Addressable Market (SAM): {{sam}}
- Serviceable Obtainable Market (SOM): {{som}}

**Target Market:**
- Target Customer Profile: {{targetCustomer}}
- Customer Demographics: {{demographics}}
- Customer Pain Points: {{painPoints}}
- Buying Behavior: {{buyingBehavior}}

**Competitive Landscape:**
- Direct Competitors: {{directCompetitors}}
- Indirect Competitors: {{indirectCompetitors}}
- Competitive Advantages: {{competitiveAdvantages}}

**Market Trends:**
- Key Trends: {{marketTrends}}
- Opportunities: {{opportunities}}
- Threats: {{threats}}

**Requirements:**
1. Start with industry overview and market context
2. Present market size using TAM/SAM/SOM analysis
3. Define target customer segments clearly
4. Analyze competitive landscape objectively
5. Identify key trends and their implications

**Output Format:** Data-driven prose with charts and tables. Include at least 3 visual elements.
**Length:** 800-1200 words";

        var visualElementsJson = @"{
  ""elements"": [
    {
      ""type"": ""chart"",
      ""chartType"": ""pie"",
      ""title"": ""Market Size Analysis (TAM/SAM/SOM)"",
      ""data"": {
        ""labels"": [""TAM"", ""SAM"", ""SOM""],
        ""datasets"": [{
          ""label"": ""Market Size"",
          ""data"": [""{{tam}}"", ""{{sam}}"", ""{{som}}""]
        }]
      }
    },
    {
      ""type"": ""table"",
      ""tableType"": ""comparison"",
      ""title"": ""Competitive Analysis"",
      ""headers"": [""Competitor"", ""Market Share"", ""Strengths"", ""Weaknesses""],
      ""description"": ""Comparison of key competitors""
    },
    {
      ""type"": ""chart"",
      ""chartType"": ""line"",
      ""title"": ""Market Growth Projection"",
      ""description"": ""5-year market growth trajectory""
    }
  ]
}";

        return new PromptTemplate(
            SectionType.MarketAnalysis,
            BusinessPlanType.BusinessPlan,
            "Market Analysis - Standard v1",
            systemPrompt,
            userPromptTemplate,
            OutputFormat.Mixed,
            CreatedBy,
            description: "Comprehensive market analysis with TAM/SAM/SOM and competitive analysis",
            visualElementsJson: visualElementsJson
        );
    }

    private PromptTemplate CreateProductsServicesPrompt()
    {
        var systemPrompt = @"You are a product marketing expert with experience in articulating product value propositions and service offerings. Your role is to create compelling product and service descriptions that clearly communicate features, benefits, and differentiation.

Guidelines:
- Focus on benefits over features
- Clearly articulate the value proposition
- Use customer-centric language
- Include competitive differentiation
- Address the full product/service lifecycle";

        var userPromptTemplate = @"Write a products and services section for a business plan with the following details:

**Product/Service Overview:**
- Primary Offerings: {{primaryOfferings}}
- Product/Service Description: {{offeringDescription}}
- Product Category: {{productCategory}}

**Value Proposition:**
- Core Value Proposition: {{valueProposition}}
- Key Benefits: {{keyBenefits}}
- Problem Solved: {{problemSolved}}

**Features & Specifications:**
- Key Features: {{keyFeatures}}
- Technical Specifications: {{technicalSpecs}}
- Service Levels: {{serviceLevels}}

**Pricing Strategy:**
- Pricing Model: {{pricingModel}}
- Price Points: {{pricePoints}}
- Pricing Rationale: {{pricingRationale}}

**Product Roadmap:**
- Current Status: {{currentStatus}}
- Future Development: {{futureRoadmap}}
- Innovation Pipeline: {{innovationPipeline}}

**Requirements:**
1. Start with a clear description of offerings
2. Articulate the value proposition strongly
3. Detail features and their benefits
4. Explain the pricing strategy
5. Present the product roadmap

**Output Format:** Professional prose with feature tables and roadmap. Include 2-3 visual elements.
**Length:** 600-900 words";

        var visualElementsJson = @"{
  ""elements"": [
    {
      ""type"": ""table"",
      ""tableType"": ""features"",
      ""title"": ""Product Features & Benefits"",
      ""headers"": [""Feature"", ""Description"", ""Customer Benefit""],
      ""description"": ""Key features mapped to customer benefits""
    },
    {
      ""type"": ""table"",
      ""tableType"": ""pricing"",
      ""title"": ""Pricing Tiers"",
      ""headers"": [""Tier"", ""Price"", ""Features Included""],
      ""description"": ""Pricing structure and tier comparison""
    },
    {
      ""type"": ""timeline"",
      ""title"": ""Product Roadmap"",
      ""description"": ""Planned product development milestones""
    }
  ]
}";

        return new PromptTemplate(
            SectionType.ProductsServices,
            BusinessPlanType.BusinessPlan,
            "Products & Services - Standard v1",
            systemPrompt,
            userPromptTemplate,
            OutputFormat.Mixed,
            CreatedBy,
            description: "Comprehensive products and services section with features, pricing, and roadmap",
            visualElementsJson: visualElementsJson
        );
    }

    private PromptTemplate CreateFinancialProjectionsPrompt()
    {
        var systemPrompt = @"You are a financial analyst with expertise in business financial modeling and projections. Your role is to create credible, well-reasoned financial projections that demonstrate the business's financial viability and growth potential.

Guidelines:
- Base projections on realistic assumptions
- Clearly state all key assumptions
- Use industry benchmarks for validation
- Present multiple scenarios when appropriate
- Include key financial metrics and ratios";

        var userPromptTemplate = @"Write a financial projections section for a business plan with the following details:

**Revenue Model:**
- Revenue Streams: {{revenueStreams}}
- Pricing Strategy: {{pricingStrategy}}
- Sales Projections: {{salesProjections}}

**Cost Structure:**
- Fixed Costs: {{fixedCosts}}
- Variable Costs: {{variableCosts}}
- Operating Expenses: {{operatingExpenses}}

**Financial Projections:**
- Year 1 Revenue: {{year1Revenue}}
- Year 2 Revenue: {{year2Revenue}}
- Year 3 Revenue: {{year3Revenue}}
- Year 1 Expenses: {{year1Expenses}}
- Year 2 Expenses: {{year2Expenses}}
- Year 3 Expenses: {{year3Expenses}}

**Key Metrics:**
- Break-even Point: {{breakEvenPoint}}
- Gross Margin: {{grossMargin}}
- Net Margin: {{netMargin}}
- Customer Acquisition Cost: {{cac}}
- Lifetime Value: {{ltv}}

**Funding Requirements:**
- Total Funding Needed: {{fundingNeeded}}
- Use of Funds: {{useOfFunds}}
- Funding Timeline: {{fundingTimeline}}

**Key Assumptions:**
- Growth Assumptions: {{growthAssumptions}}
- Market Assumptions: {{marketAssumptions}}

**Requirements:**
1. Present clear revenue projections with assumptions
2. Detail the cost structure comprehensively
3. Show profitability path and timeline
4. Include key financial metrics
5. Explain funding requirements and use of funds

**Output Format:** Data-driven prose with financial tables and charts. Include 3-4 visual elements.
**Length:** 800-1100 words";

        var visualElementsJson = @"{
  ""elements"": [
    {
      ""type"": ""chart"",
      ""chartType"": ""bar"",
      ""title"": ""Revenue Projections (3-Year)"",
      ""data"": {
        ""labels"": [""Year 1"", ""Year 2"", ""Year 3""],
        ""datasets"": [
          { ""label"": ""Revenue"", ""data"": [""{{year1Revenue}}"", ""{{year2Revenue}}"", ""{{year3Revenue}}""] },
          { ""label"": ""Expenses"", ""data"": [""{{year1Expenses}}"", ""{{year2Expenses}}"", ""{{year3Expenses}}""] }
        ]
      }
    },
    {
      ""type"": ""table"",
      ""tableType"": ""financial"",
      ""title"": ""Income Statement Summary"",
      ""headers"": [""Category"", ""Year 1"", ""Year 2"", ""Year 3""],
      ""description"": ""Projected income statement""
    },
    {
      ""type"": ""chart"",
      ""chartType"": ""line"",
      ""title"": ""Break-even Analysis"",
      ""description"": ""Path to profitability""
    },
    {
      ""type"": ""metric_cards"",
      ""title"": ""Key Financial Metrics"",
      ""metrics"": [
        { ""label"": ""Gross Margin"", ""placeholder"": ""{{grossMargin}}"", ""format"": ""percentage"" },
        { ""label"": ""Break-even"", ""placeholder"": ""{{breakEvenPoint}}"", ""format"": ""text"" },
        { ""label"": ""LTV:CAC Ratio"", ""placeholder"": ""{{ltvCacRatio}}"", ""format"": ""ratio"" }
      ]
    }
  ]
}";

        return new PromptTemplate(
            SectionType.FinancialProjections,
            BusinessPlanType.BusinessPlan,
            "Financial Projections - Standard v1",
            systemPrompt,
            userPromptTemplate,
            OutputFormat.Mixed,
            CreatedBy,
            description: "Comprehensive financial projections with revenue, costs, and key metrics",
            visualElementsJson: visualElementsJson
        );
    }

    private PromptTemplate CreateMarketingStrategyPrompt()
    {
        var systemPrompt = @"You are a marketing strategist with expertise in go-to-market strategies, customer acquisition, and brand development. Your role is to create actionable marketing strategies that align with business objectives and target market characteristics.

Guidelines:
- Develop strategies aligned with target market insights
- Include specific, actionable tactics
- Define measurable marketing goals
- Balance online and offline channels appropriately
- Consider budget constraints in recommendations";

        var userPromptTemplate = @"Write a marketing strategy section for a business plan with the following details:

**Brand Positioning:**
- Brand Identity: {{brandIdentity}}
- Positioning Statement: {{positioningStatement}}
- Key Messages: {{keyMessages}}

**Target Audience:**
- Primary Audience: {{primaryAudience}}
- Secondary Audience: {{secondaryAudience}}
- Buyer Personas: {{buyerPersonas}}

**Marketing Channels:**
- Digital Channels: {{digitalChannels}}
- Traditional Channels: {{traditionalChannels}}
- Partnership Channels: {{partnershipChannels}}

**Marketing Tactics:**
- Content Strategy: {{contentStrategy}}
- Social Media Strategy: {{socialMediaStrategy}}
- Paid Advertising: {{paidAdvertising}}
- PR and Communications: {{prStrategy}}

**Marketing Budget:**
- Total Marketing Budget: {{marketingBudget}}
- Budget Allocation: {{budgetAllocation}}

**Goals & KPIs:**
- Marketing Goals: {{marketingGoals}}
- Key Performance Indicators: {{kpis}}
- Success Metrics: {{successMetrics}}

**Requirements:**
1. Start with brand positioning and messaging
2. Define target audience segments clearly
3. Detail marketing channels and tactics
4. Present budget allocation strategy
5. Establish measurable goals and KPIs

**Output Format:** Strategic prose with budget tables and channel mix. Include 2-3 visual elements.
**Length:** 600-900 words";

        var visualElementsJson = @"{
  ""elements"": [
    {
      ""type"": ""chart"",
      ""chartType"": ""pie"",
      ""title"": ""Marketing Budget Allocation"",
      ""description"": ""Distribution of marketing spend by channel""
    },
    {
      ""type"": ""table"",
      ""tableType"": ""marketing"",
      ""title"": ""Marketing Channel Strategy"",
      ""headers"": [""Channel"", ""Tactics"", ""Budget"", ""Expected ROI""],
      ""description"": ""Channel-specific marketing plan""
    },
    {
      ""type"": ""timeline"",
      ""title"": ""Marketing Campaign Timeline"",
      ""description"": ""Planned marketing activities and campaigns""
    }
  ]
}";

        return new PromptTemplate(
            SectionType.MarketingStrategy,
            BusinessPlanType.BusinessPlan,
            "Marketing Strategy - Standard v1",
            systemPrompt,
            userPromptTemplate,
            OutputFormat.Mixed,
            CreatedBy,
            description: "Comprehensive marketing strategy with channels, tactics, and budget",
            visualElementsJson: visualElementsJson
        );
    }

    private PromptTemplate CreateOperationsPlanPrompt()
    {
        var systemPrompt = @"You are an operations management expert with experience in business process design, supply chain management, and operational efficiency. Your role is to create practical operations plans that demonstrate how the business will deliver its products or services effectively.

Guidelines:
- Focus on practical, implementable processes
- Address resource requirements realistically
- Include quality control measures
- Consider scalability in planning
- Identify key operational risks and mitigation";

        var userPromptTemplate = @"Write an operations plan section for a business plan with the following details:

**Business Operations:**
- Business Model: {{businessModel}}
- Core Operations: {{coreOperations}}
- Operating Hours: {{operatingHours}}

**Facilities & Location:**
- Location Details: {{locationDetails}}
- Facility Requirements: {{facilityRequirements}}
- Equipment Needs: {{equipmentNeeds}}

**Production/Service Delivery:**
- Production Process: {{productionProcess}}
- Service Delivery Method: {{serviceDelivery}}
- Quality Control: {{qualityControl}}

**Supply Chain:**
- Key Suppliers: {{keySuppliers}}
- Inventory Management: {{inventoryManagement}}
- Logistics: {{logistics}}

**Technology & Systems:**
- Technology Stack: {{technologyStack}}
- Software Systems: {{softwareSystems}}
- Automation Plans: {{automationPlans}}

**Human Resources:**
- Staffing Requirements: {{staffingRequirements}}
- Key Roles: {{keyRoles}}
- Training Programs: {{trainingPrograms}}

**Requirements:**
1. Describe day-to-day operations clearly
2. Detail facility and equipment needs
3. Explain production/service delivery processes
4. Address supply chain and vendor management
5. Include staffing and technology requirements

**Output Format:** Process-oriented prose with workflow diagrams. Include 2-3 visual elements.
**Length:** 600-900 words";

        var visualElementsJson = @"{
  ""elements"": [
    {
      ""type"": ""flowchart"",
      ""title"": ""Operations Workflow"",
      ""description"": ""End-to-end operational process flow""
    },
    {
      ""type"": ""table"",
      ""tableType"": ""resources"",
      ""title"": ""Resource Requirements"",
      ""headers"": [""Resource Type"", ""Description"", ""Quantity"", ""Cost""],
      ""description"": ""Key operational resources needed""
    },
    {
      ""type"": ""timeline"",
      ""title"": ""Operations Timeline"",
      ""description"": ""Key operational milestones and setup phases""
    }
  ]
}";

        return new PromptTemplate(
            SectionType.OperationsPlan,
            BusinessPlanType.BusinessPlan,
            "Operations Plan - Standard v1",
            systemPrompt,
            userPromptTemplate,
            OutputFormat.Mixed,
            CreatedBy,
            description: "Comprehensive operations plan with processes, resources, and timeline",
            visualElementsJson: visualElementsJson
        );
    }

    private PromptTemplate CreateSWOTAnalysisPrompt()
    {
        var systemPrompt = @"You are a strategic planning consultant with expertise in SWOT analysis and strategic assessment. Your role is to create insightful, actionable SWOT analyses that inform strategic decision-making and highlight both opportunities and challenges.

Guidelines:
- Be objective and evidence-based
- Prioritize factors by significance
- Connect SWOT factors to strategic implications
- Provide actionable recommendations
- Balance honesty with strategic positioning";

        var userPromptTemplate = @"Write a SWOT Analysis section for a business plan with the following details:

**Company Context:**
- Company Name: {{companyName}}
- Industry: {{industry}}
- Business Stage: {{businessStage}}

**Strengths:**
- Internal Strengths: {{internalStrengths}}
- Competitive Advantages: {{competitiveAdvantages}}
- Core Competencies: {{coreCompetencies}}
- Resources: {{resources}}

**Weaknesses:**
- Internal Weaknesses: {{internalWeaknesses}}
- Resource Gaps: {{resourceGaps}}
- Skill Gaps: {{skillGaps}}
- Limitations: {{limitations}}

**Opportunities:**
- Market Opportunities: {{marketOpportunities}}
- Industry Trends: {{industryTrends}}
- Growth Potential: {{growthPotential}}
- Strategic Opportunities: {{strategicOpportunities}}

**Threats:**
- Competitive Threats: {{competitiveThreats}}
- Market Risks: {{marketRisks}}
- Economic Factors: {{economicFactors}}
- Regulatory Risks: {{regulatoryRisks}}

**Requirements:**
1. Analyze each SWOT category thoroughly
2. Prioritize factors by business impact
3. Connect factors to strategic implications
4. Provide recommendations for each area
5. Include a strategic summary

**Output Format:** Structured analysis with SWOT matrix. Include 2 visual elements.
**Length:** 600-800 words";

        var visualElementsJson = @"{
  ""elements"": [
    {
      ""type"": ""swot_matrix"",
      ""title"": ""SWOT Analysis Matrix"",
      ""quadrants"": {
        ""strengths"": { ""title"": ""Strengths"", ""color"": ""green"" },
        ""weaknesses"": { ""title"": ""Weaknesses"", ""color"": ""red"" },
        ""opportunities"": { ""title"": ""Opportunities"", ""color"": ""blue"" },
        ""threats"": { ""title"": ""Threats"", ""color"": ""orange"" }
      }
    },
    {
      ""type"": ""table"",
      ""tableType"": ""strategic"",
      ""title"": ""Strategic Implications"",
      ""headers"": [""SWOT Factor"", ""Strategic Implication"", ""Recommended Action""],
      ""description"": ""Key strategic takeaways from SWOT analysis""
    }
  ]
}";

        return new PromptTemplate(
            SectionType.SWOTAnalysis,
            BusinessPlanType.BusinessPlan,
            "SWOT Analysis - Standard v1",
            systemPrompt,
            userPromptTemplate,
            OutputFormat.Mixed,
            CreatedBy,
            description: "Comprehensive SWOT analysis with strategic implications",
            visualElementsJson: visualElementsJson
        );
    }
}
