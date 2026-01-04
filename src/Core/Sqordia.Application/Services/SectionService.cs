using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Contracts.Requests.BusinessPlan;
using Sqordia.Contracts.Responses.BusinessPlan;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for business plan section management
/// </summary>
public class SectionService : ISectionService
{
    private readonly ILogger<SectionService> _logger;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SectionService(
        ILogger<SectionService> logger,
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<BusinessPlanSectionsDto> GetSectionsAsync(Guid businessPlanId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all sections for business plan {BusinessPlanId}", businessPlanId);

            // Get the business plan
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

            if (businessPlan == null)
            {
                _logger.LogWarning("Business plan {BusinessPlanId} not found", businessPlanId);
                throw new ArgumentException($"Business plan with ID {businessPlanId} not found.");
            }

            // Get section metadata for the plan type
            var sectionMetadata = GetSectionMetadata(businessPlan.PlanType.ToString());
            var sections = new List<BusinessPlanSectionDto>();

            foreach (var (sectionName, metadata) in sectionMetadata)
            {
                var content = GetSectionContent(businessPlan, sectionName);
                var section = new BusinessPlanSectionDto
                {
                    BusinessPlanId = businessPlanId,
                    SectionName = sectionName,
                    Title = metadata.Title,
                    Content = content,
                    HasContent = !string.IsNullOrEmpty(content),
                    WordCount = content?.Split(' ').Length ?? 0,
                    CharacterCount = content?.Length ?? 0,
                    LastUpdated = businessPlan.LastModified ?? businessPlan.Created,
                    LastUpdatedBy = businessPlan.LastModifiedBy ?? businessPlan.CreatedBy,
                    IsRequired = metadata.IsRequired,
                    Order = metadata.Order,
                    Description = metadata.Description,
                    IsAIGenerated = IsAIGenerated(businessPlan, sectionName),
                    IsManuallyEdited = IsManuallyEdited(businessPlan, sectionName),
                    Status = GetSectionStatus(businessPlan, sectionName),
                    Tags = metadata.Tags
                };

                sections.Add(section);
            }

            // Calculate completion statistics
            var sectionsWithContent = sections.Count(s => s.HasContent);
            var completionPercentage = sections.Count > 0 ? (decimal)sectionsWithContent / sections.Count * 100 : 0;

            return new BusinessPlanSectionsDto
            {
                BusinessPlanId = businessPlanId,
                BusinessPlanTitle = businessPlan.Title,
                PlanType = businessPlan.PlanType.ToString(),
                Sections = sections.OrderBy(s => s.Order).ToList(),
                TotalSections = sections.Count,
                SectionsWithContent = sectionsWithContent,
                CompletionPercentage = completionPercentage,
                LastUpdated = businessPlan.LastModified ?? businessPlan.Created
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sections for business plan {BusinessPlanId}", businessPlanId);
            throw;
        }
    }

    public async Task<BusinessPlanSectionDto> GetSectionAsync(Guid businessPlanId, string sectionName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting section {SectionName} for business plan {BusinessPlanId}", sectionName, businessPlanId);

            // Get the business plan
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

            if (businessPlan == null)
            {
                _logger.LogWarning("Business plan {BusinessPlanId} not found", businessPlanId);
                throw new ArgumentException($"Business plan with ID {businessPlanId} not found.");
            }

            // Validate section name
            if (!IsValidSectionName(sectionName, businessPlan.PlanType.ToString()))
            {
                _logger.LogWarning("Invalid section name {SectionName} for plan type {PlanType}", sectionName, businessPlan.PlanType);
                throw new ArgumentException($"Section '{sectionName}' is not valid for plan type '{businessPlan.PlanType}'.");
            }

            // Get section metadata
            var sectionMetadata = GetSectionMetadata(businessPlan.PlanType.ToString());
            if (!sectionMetadata.TryGetValue(sectionName, out var metadata))
            {
                throw new ArgumentException($"Section metadata not found for '{sectionName}'.");
            }

            // Get section content
            var content = GetSectionContent(businessPlan, sectionName);

            return new BusinessPlanSectionDto
            {
                BusinessPlanId = businessPlanId,
                SectionName = sectionName,
                Title = metadata.Title,
                Content = content,
                HasContent = !string.IsNullOrEmpty(content),
                WordCount = content?.Split(' ').Length ?? 0,
                CharacterCount = content?.Length ?? 0,
                LastUpdated = businessPlan.LastModified ?? businessPlan.Created,
                LastUpdatedBy = businessPlan.LastModifiedBy ?? businessPlan.CreatedBy,
                IsRequired = metadata.IsRequired,
                Order = metadata.Order,
                Description = metadata.Description,
                IsAIGenerated = IsAIGenerated(businessPlan, sectionName),
                IsManuallyEdited = IsManuallyEdited(businessPlan, sectionName),
                Status = GetSectionStatus(businessPlan, sectionName),
                Tags = metadata.Tags
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting section {SectionName} for business plan {BusinessPlanId}", sectionName, businessPlanId);
            throw;
        }
    }

    public async Task<BusinessPlanSectionDto> UpdateSectionAsync(Guid businessPlanId, string sectionName, UpdateSectionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating section {SectionName} for business plan {BusinessPlanId}", sectionName, businessPlanId);

            // Get the business plan
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId, cancellationToken);

            if (businessPlan == null)
            {
                _logger.LogWarning("Business plan {BusinessPlanId} not found", businessPlanId);
                throw new ArgumentException($"Business plan with ID {businessPlanId} not found.");
            }

            // Validate section name
            if (!IsValidSectionName(sectionName, businessPlan.PlanType.ToString()))
            {
                _logger.LogWarning("Invalid section name {SectionName} for plan type {PlanType}", sectionName, businessPlan.PlanType);
                throw new ArgumentException($"Section '{sectionName}' is not valid for plan type '{businessPlan.PlanType}'.");
            }

            // Update the section content
            UpdateSectionContent(businessPlan, sectionName, request.Content);

            // Update business plan metadata
            businessPlan.LastModified = DateTime.UtcNow;
            businessPlan.LastModifiedBy = _currentUserService.UserId ?? "System";

            // Save changes
            await _context.SaveChangesAsync(cancellationToken);

            // Get updated section
            return await GetSectionAsync(businessPlanId, sectionName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating section {SectionName} for business plan {BusinessPlanId}", sectionName, businessPlanId);
            throw;
        }
    }

    public bool IsValidSectionName(string sectionName, string planType)
    {
        var availableSections = GetAvailableSectionNames(planType);
        return availableSections.Contains(sectionName.ToLower());
    }

    public List<string> GetAvailableSectionNames(string planType)
    {
        var metadata = GetSectionMetadata(planType);
        return metadata.Keys.ToList();
    }

    public Dictionary<string, SectionMetadata> GetSectionMetadata(string planType)
    {
        return planType.ToLower() switch
        {
            "businessplan" or "0" => GetBusinessPlanSections(),
            "strategicplan" or "1" => GetStrategicPlanSections(),
            "leancanvas" or "2" => GetLeanCanvasSections(),
            _ => GetBusinessPlanSections() // Default to business plan sections
        };
    }

    private Dictionary<string, SectionMetadata> GetBusinessPlanSections()
    {
        return new Dictionary<string, SectionMetadata>
        {
            ["executive-summary"] = new SectionMetadata
            {
                Name = "executive-summary",
                Title = "Executive Summary",
                Description = "Brief overview of the business",
                IsRequired = true,
                Order = 1,
                Tags = new List<string> { "overview", "summary" },
                Category = "Introduction"
            },
            ["market-analysis"] = new SectionMetadata
            {
                Name = "market-analysis",
                Title = "Market Analysis",
                Description = "Analysis of target market and competition",
                IsRequired = true,
                Order = 2,
                Tags = new List<string> { "market", "analysis" },
                Category = "Market"
            },
            ["competitive-analysis"] = new SectionMetadata
            {
                Name = "competitive-analysis",
                Title = "Competitive Analysis",
                Description = "Analysis of competitors and competitive advantages",
                IsRequired = true,
                Order = 3,
                Tags = new List<string> { "competition", "analysis" },
                Category = "Market"
            },
            ["business-model"] = new SectionMetadata
            {
                Name = "business-model",
                Title = "Business Model",
                Description = "How the business generates revenue",
                IsRequired = true,
                Order = 4,
                Tags = new List<string> { "business", "model" },
                Category = "Strategy"
            },
            ["marketing-strategy"] = new SectionMetadata
            {
                Name = "marketing-strategy",
                Title = "Marketing Strategy",
                Description = "How the business will reach customers",
                IsRequired = true,
                Order = 5,
                Tags = new List<string> { "marketing", "strategy" },
                Category = "Strategy"
            },
            ["operations-plan"] = new SectionMetadata
            {
                Name = "operations-plan",
                Title = "Operations Plan",
                Description = "How the business will operate day-to-day",
                IsRequired = true,
                Order = 6,
                Tags = new List<string> { "operations", "plan" },
                Category = "Operations"
            },
            ["management-team"] = new SectionMetadata
            {
                Name = "management-team",
                Title = "Management Team",
                Description = "Key team members and their roles",
                IsRequired = true,
                Order = 7,
                Tags = new List<string> { "team", "management" },
                Category = "Team"
            },
            ["financial-projections"] = new SectionMetadata
            {
                Name = "financial-projections",
                Title = "Financial Projections",
                Description = "Financial forecasts and projections",
                IsRequired = true,
                Order = 8,
                Tags = new List<string> { "financial", "projections" },
                Category = "Financial"
            },
            ["funding-requirements"] = new SectionMetadata
            {
                Name = "funding-requirements",
                Title = "Funding Requirements",
                Description = "Funding needs and use of funds",
                IsRequired = true,
                Order = 9,
                Tags = new List<string> { "funding", "requirements" },
                Category = "Financial"
            },
            ["risk-analysis"] = new SectionMetadata
            {
                Name = "risk-analysis",
                Title = "Risk Analysis",
                Description = "Potential risks and mitigation strategies",
                IsRequired = true,
                Order = 10,
                Tags = new List<string> { "risk", "analysis" },
                Category = "Risk"
            }
        };
    }

    private Dictionary<string, SectionMetadata> GetStrategicPlanSections()
    {
        return new Dictionary<string, SectionMetadata>
        {
            ["mission-statement"] = new SectionMetadata
            {
                Name = "mission-statement",
                Title = "Mission Statement",
                Description = "Organization's mission and purpose",
                IsRequired = true,
                Order = 1,
                Tags = new List<string> { "mission", "purpose" },
                Category = "Foundation"
            },
            ["vision-statement"] = new SectionMetadata
            {
                Name = "vision-statement",
                Title = "Vision Statement",
                Description = "Organization's vision for the future",
                IsRequired = true,
                Order = 2,
                Tags = new List<string> { "vision", "future" },
                Category = "Foundation"
            },
            ["social-impact"] = new SectionMetadata
            {
                Name = "social-impact",
                Title = "Social Impact",
                Description = "Social impact goals and measurements",
                IsRequired = true,
                Order = 3,
                Tags = new List<string> { "impact", "social" },
                Category = "Impact"
            },
            ["beneficiary-profile"] = new SectionMetadata
            {
                Name = "beneficiary-profile",
                Title = "Beneficiary Profile",
                Description = "Description of beneficiaries and their needs",
                IsRequired = true,
                Order = 4,
                Tags = new List<string> { "beneficiaries", "needs" },
                Category = "Impact"
            },
            ["grant-strategy"] = new SectionMetadata
            {
                Name = "grant-strategy",
                Title = "Grant Strategy",
                Description = "Grant application and funding strategy",
                IsRequired = true,
                Order = 5,
                Tags = new List<string> { "grants", "funding" },
                Category = "Funding"
            },
            ["sustainability-plan"] = new SectionMetadata
            {
                Name = "sustainability-plan",
                Title = "Sustainability Plan",
                Description = "Long-term sustainability and viability",
                IsRequired = true,
                Order = 6,
                Tags = new List<string> { "sustainability", "viability" },
                Category = "Sustainability"
            }
        };
    }

    private Dictionary<string, SectionMetadata> GetLeanCanvasSections()
    {
        return new Dictionary<string, SectionMetadata>
        {
            ["problem"] = new SectionMetadata
            {
                Name = "problem",
                Title = "Problem",
                Description = "Top 3 problems your customers have",
                IsRequired = true,
                Order = 1,
                Tags = new List<string> { "problem", "customer" },
                Category = "Problem"
            },
            ["solution"] = new SectionMetadata
            {
                Name = "solution",
                Title = "Solution",
                Description = "Top 3 features of your solution",
                IsRequired = true,
                Order = 2,
                Tags = new List<string> { "solution", "features" },
                Category = "Solution"
            },
            ["key-metrics"] = new SectionMetadata
            {
                Name = "key-metrics",
                Title = "Key Metrics",
                Description = "Key activities you measure",
                IsRequired = true,
                Order = 3,
                Tags = new List<string> { "metrics", "measurement" },
                Category = "Metrics"
            },
            ["unique-value-proposition"] = new SectionMetadata
            {
                Name = "unique-value-proposition",
                Title = "Unique Value Proposition",
                Description = "Single, clear, compelling message",
                IsRequired = true,
                Order = 4,
                Tags = new List<string> { "value", "proposition" },
                Category = "Value"
            },
            ["unfair-advantage"] = new SectionMetadata
            {
                Name = "unfair-advantage",
                Title = "Unfair Advantage",
                Description = "Something that cannot be easily copied",
                IsRequired = true,
                Order = 5,
                Tags = new List<string> { "advantage", "competitive" },
                Category = "Advantage"
            },
            ["channels"] = new SectionMetadata
            {
                Name = "channels",
                Title = "Channels",
                Description = "Path to customers",
                IsRequired = true,
                Order = 6,
                Tags = new List<string> { "channels", "distribution" },
                Category = "Channels"
            },
            ["customer-segments"] = new SectionMetadata
            {
                Name = "customer-segments",
                Title = "Customer Segments",
                Description = "Target customers and users",
                IsRequired = true,
                Order = 7,
                Tags = new List<string> { "customers", "segments" },
                Category = "Customers"
            },
            ["cost-structure"] = new SectionMetadata
            {
                Name = "cost-structure",
                Title = "Cost Structure",
                Description = "Customer acquisition costs and distribution costs",
                IsRequired = true,
                Order = 8,
                Tags = new List<string> { "costs", "structure" },
                Category = "Costs"
            },
            ["revenue-streams"] = new SectionMetadata
            {
                Name = "revenue-streams",
                Title = "Revenue Streams",
                Description = "Revenue model and pricing strategy",
                IsRequired = true,
                Order = 9,
                Tags = new List<string> { "revenue", "pricing" },
                Category = "Revenue"
            }
        };
    }

    private string? GetSectionContent(Domain.Entities.BusinessPlan.BusinessPlan businessPlan, string sectionName)
    {
        return sectionName.ToLower() switch
        {
            "executive-summary" => businessPlan.ExecutiveSummary,
            "problem-statement" => businessPlan.ProblemStatement,
            "solution" => businessPlan.Solution,
            "market-analysis" => businessPlan.MarketAnalysis,
            "competitive-analysis" => businessPlan.CompetitiveAnalysis,
            "swot-analysis" => businessPlan.SwotAnalysis,
            "business-model" => businessPlan.BusinessModel,
            "marketing-strategy" => businessPlan.MarketingStrategy,
            "branding-strategy" => businessPlan.BrandingStrategy,
            "operations-plan" => businessPlan.OperationsPlan,
            "management-team" => businessPlan.ManagementTeam,
            "financial-projections" => businessPlan.FinancialProjections,
            "funding-requirements" => businessPlan.FundingRequirements,
            "risk-analysis" => businessPlan.RiskAnalysis,
            "exit-strategy" => businessPlan.ExitStrategy,
            "appendix-data" => businessPlan.AppendixData,
            "mission-statement" => businessPlan.MissionStatement,
            "social-impact" => businessPlan.SocialImpact,
            "beneficiary-profile" => businessPlan.BeneficiaryProfile,
            "grant-strategy" => businessPlan.GrantStrategy,
            "sustainability-plan" => businessPlan.SustainabilityPlan,
            _ => null
        };
    }

    private void UpdateSectionContent(Domain.Entities.BusinessPlan.BusinessPlan businessPlan, string sectionName, string content)
    {
        // Note: Section updates are handled via BusinessPlan entity update methods
        _logger.LogInformation("Updating section {SectionName} for business plan {BusinessPlanId} with content length {ContentLength}", 
            sectionName, businessPlan.Id, content.Length);
        
        // In a real implementation, this would call methods like:
        // businessPlan.UpdateExecutiveSummary(content);
        // businessPlan.UpdateMarketAnalysis(content);
        // etc.
    }

    private bool IsAIGenerated(Domain.Entities.BusinessPlan.BusinessPlan businessPlan, string sectionName)
    {
        // This would be determined by checking if the content was generated by AI
        // For now, we'll assume all content is AI-generated if it exists
        return !string.IsNullOrEmpty(GetSectionContent(businessPlan, sectionName));
    }

    private bool IsManuallyEdited(Domain.Entities.BusinessPlan.BusinessPlan businessPlan, string sectionName)
    {
        // This would be determined by checking edit history
        // For now, we'll assume content is manually edited if it exists and was modified after creation
        var content = GetSectionContent(businessPlan, sectionName);
        return !string.IsNullOrEmpty(content) && businessPlan.LastModified > businessPlan.Created;
    }

    private string GetSectionStatus(Domain.Entities.BusinessPlan.BusinessPlan businessPlan, string sectionName)
    {
        var content = GetSectionContent(businessPlan, sectionName);
        return string.IsNullOrEmpty(content) ? "draft" : "review";
    }
}
