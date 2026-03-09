namespace Sqordia.Infrastructure.Services.ContentAdaptation;

/// <summary>
/// Bilingual prompt templates for AI content adaptation per export format.
/// </summary>
internal static class ContentAdaptationPrompts
{
    // ── PowerPoint ────────────────────────────────────────────

    internal static string PowerPointSystem(string langInstruction) =>
        $"You are a presentation content summarizer. {langInstruction} " +
        "Extract exactly 3-5 concise bullet points from the given business plan section. " +
        "Each bullet must be one clear sentence (max 15 words). " +
        "Return only the bullet points, one per line, without numbering or bullet characters.";

    internal static string PowerPointUser(string sectionTitle, string content) =>
        $"Section: {sectionTitle}\n\nContent:\n{content}";

    internal const int PowerPointMaxTokens = 500;
    internal const float PowerPointTemperature = 0.3f;

    // ── Word ──────────────────────────────────────────────────

    internal static string WordSystem(string langInstruction) =>
        $"You are a professional document editor. {langInstruction} " +
        "Restructure the business plan section content for a Word document:\n" +
        "- Add clear markdown sub-headings (## or ###) where logical breaks exist\n" +
        "- Convert dense paragraphs into bullet points where appropriate\n" +
        "- Keep ALL detail and data — do not summarize or remove content\n" +
        "- Preserve tables, numbers, and financial data exactly\n" +
        "- Output clean markdown (headings, bold, bullets, numbered lists)\n" +
        "Return only the restructured content.";

    internal static string WordUser(string sectionTitle, string content) =>
        $"Section: {sectionTitle}\n\nContent:\n{content}";

    internal const int WordMaxTokens = 2000;
    internal const float WordTemperature = 0.5f;

    // ── Helpers ───────────────────────────────────────────────

    internal static string LangInstruction(string language) =>
        language == "fr" ? "Réponds en français." : "Respond in English.";
}
