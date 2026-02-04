namespace Sqordia.Application.Services.Cms;

/// <summary>
/// Static registry of all CMS-managed pages and their sections.
/// Used by the admin UI to build page/section navigation and by
/// the backend to validate section keys.
/// </summary>
public static class CmsPageRegistry
{
    public static readonly IReadOnlyList<CmsPageDefinition> Pages = new List<CmsPageDefinition>
    {
        new("landing", "Landing Page", new[]
        {
            new CmsSectionDefinition("landing.hero", "Hero", 0),
            new CmsSectionDefinition("landing.features", "Features", 1),
            new CmsSectionDefinition("landing.faq", "FAQ", 2),
            new CmsSectionDefinition("landing.testimonials", "Testimonials", 3),
        }),
        new("dashboard", "Dashboard", new[]
        {
            new CmsSectionDefinition("dashboard.labels", "Labels & Titles", 0),
            new CmsSectionDefinition("dashboard.empty_states", "Empty States", 1),
            new CmsSectionDefinition("dashboard.tips", "Tips & Tour", 2),
        }),
        new("profile", "Profile", new[]
        {
            new CmsSectionDefinition("profile.labels", "Labels & Titles", 0),
            new CmsSectionDefinition("profile.security", "Security", 1),
            new CmsSectionDefinition("profile.sessions", "Sessions", 2),
        }),
        new("questionnaire", "Questionnaire Wizard", new[]
        {
            new CmsSectionDefinition("questionnaire.steps", "Step Configuration", 0),
            new CmsSectionDefinition("questionnaire.labels", "Labels & Buttons", 1),
            new CmsSectionDefinition("questionnaire.tips", "Generation Tips", 2),
        }),
        new("create_plan", "Create Plan", new[]
        {
            new CmsSectionDefinition("create_plan.labels", "Labels & Titles", 0),
            new CmsSectionDefinition("create_plan.types", "Plan Types", 1),
        }),
        new("subscription", "Subscription Plans", new[]
        {
            new CmsSectionDefinition("subscription.labels", "Labels & Titles", 0),
            new CmsSectionDefinition("subscription.plans", "Plan Definitions", 1),
        }),
        new("onboarding", "Onboarding", new[]
        {
            new CmsSectionDefinition("onboarding.welcome", "Welcome", 0),
            new CmsSectionDefinition("onboarding.steps", "Steps", 1),
            new CmsSectionDefinition("onboarding.completion", "Completion", 2),
        }),
        new("auth", "Authentication", new[]
        {
            new CmsSectionDefinition("auth.login", "Login", 0),
            new CmsSectionDefinition("auth.register", "Registration", 1),
            new CmsSectionDefinition("auth.forgot_password", "Forgot Password", 2),
            new CmsSectionDefinition("auth.reset_password", "Reset Password", 3),
            new CmsSectionDefinition("auth.verify_email", "Email Verification", 4),
        }),
        new("legal", "Legal Pages", new[]
        {
            new CmsSectionDefinition("legal.terms", "Terms of Service", 0),
            new CmsSectionDefinition("legal.privacy", "Privacy Policy", 1),
        }),
        new("global", "Global / Shared", new[]
        {
            new CmsSectionDefinition("global.branding", "Branding", 0),
            new CmsSectionDefinition("global.social", "Social Links", 1),
            new CmsSectionDefinition("global.contact", "Contact Information", 2),
            new CmsSectionDefinition("global.footer", "Footer", 3),
            new CmsSectionDefinition("global.navigation", "Navigation", 4),
        }),
    };

    /// <summary>
    /// Find a page definition by its key
    /// </summary>
    public static CmsPageDefinition? GetPage(string pageKey)
        => Pages.FirstOrDefault(p => p.Key == pageKey);

    /// <summary>
    /// Check if a section key belongs to a known page
    /// </summary>
    public static bool IsValidSectionKey(string sectionKey)
        => Pages.SelectMany(p => p.Sections).Any(s => s.Key == sectionKey);
}

public record CmsPageDefinition(string Key, string Label, CmsSectionDefinition[] Sections);
public record CmsSectionDefinition(string Key, string Label, int SortOrder);
