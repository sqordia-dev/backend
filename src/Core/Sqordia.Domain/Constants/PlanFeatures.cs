namespace Sqordia.Domain.Constants;

/// <summary>
/// Well-known feature keys used in PlanFeatureLimit.
/// Adding a new feature = add constant here + insert rows in seed data. No schema change.
/// </summary>
public static class PlanFeatures
{
    // ── Numeric limits ───────────────────────────────────
    public const string MaxBusinessPlans = "max_business_plans";
    public const string MaxOrganizations = "max_organizations";
    public const string MaxTeamMembers = "max_team_members";
    public const string MaxAiGenerationsMonthly = "max_ai_generations_monthly";
    public const string MaxAiCoachMessagesMonthly = "max_ai_coach_messages_monthly";
    public const string MaxStorageMb = "max_storage_mb";

    // ── Export capabilities ──────────────────────────────
    public const string ExportHtml = "export_html";
    public const string ExportPdf = "export_pdf";
    public const string ExportWord = "export_word";
    public const string ExportPowerpoint = "export_powerpoint";
    public const string ExportExcel = "export_excel";
    public const string ExportAgentBlueprints = "export_agent_blueprints";

    // ── AI capabilities ──────────────────────────────────
    /// <summary>
    /// Values: "gemini", "blended", "claude"
    /// </summary>
    public const string AiProviderTier = "ai_provider_tier";
    public const string PrioritySectionsClaude = "priority_sections_claude";

    // ── Financial ────────────────────────────────────────
    public const string FinancialProjectionsBasic = "financial_projections_basic";
    public const string FinancialProjectionsAdvanced = "financial_projections_advanced";

    // ── Premium features ─────────────────────────────────
    public const string CustomBranding = "custom_branding";
    public const string ApiAccess = "api_access";
    public const string PrioritySupport = "priority_support";
    public const string DedicatedSupport = "dedicated_support";
    public const string WhiteLabel = "white_label";

    /// <summary>
    /// Sentinel value meaning "unlimited" for numeric limits.
    /// </summary>
    public const string Unlimited = "-1";
}
