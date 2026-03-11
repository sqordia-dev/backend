namespace Sqordia.Application.Contracts.Responses;

/// <summary>
/// Subscription plan DTO — includes all feature limits and pricing info
/// </summary>
public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string PlanType { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string Currency { get; set; } = "CAD";
    public bool IsActive { get; set; }
    public int? DisplayOrder { get; set; }

    // ── Numeric limits ───────────────────────────────────
    public int MaxUsers { get; set; }
    public int MaxBusinessPlans { get; set; }
    public int MaxOrganizations { get; set; }
    public int MaxTeamMembers { get; set; }
    public int MaxStorageGB { get; set; }
    public int MaxAiGenerationsMonthly { get; set; }
    public int MaxAiCoachMessagesMonthly { get; set; }

    // ── Export capabilities ──────────────────────────────
    public bool HasExportHtml { get; set; }
    public bool HasExportPDF { get; set; }
    public bool HasExportWord { get; set; }
    public bool HasExportPowerpoint { get; set; }
    public bool HasExportExcel { get; set; }
    public bool HasExportAgentBlueprints { get; set; }

    // ── AI capabilities ──────────────────────────────────
    public string AiProviderTier { get; set; } = "gemini";
    public bool HasAdvancedAI { get; set; }
    public bool HasPrioritySectionsClaude { get; set; }

    // ── Financial ────────────────────────────────────────
    public bool HasFinancialProjectionsBasic { get; set; }
    public bool HasFinancialProjectionsAdvanced { get; set; }

    // ── Premium features ─────────────────────────────────
    public bool HasCustomBranding { get; set; }
    public bool HasAPIAccess { get; set; }
    public bool HasPrioritySupport { get; set; }
    public bool HasDedicatedSupport { get; set; }
    public bool HasWhiteLabel { get; set; }

    // ── Feature list for display ─────────────────────────
    public List<string> Features { get; set; } = new();
}
