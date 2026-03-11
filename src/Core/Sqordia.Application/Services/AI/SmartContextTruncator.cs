namespace Sqordia.Application.Services.AI;

/// <summary>
/// Relevance-based context truncation. Replaces arbitrary Substring(0, N) truncation
/// with intelligent paragraph selection based on keyword relevance to the target section.
/// </summary>
public static class SmartContextTruncator
{
    /// <summary>
    /// Truncates content by keeping the most relevant paragraphs for the target section.
    /// Uses rubric keywords to score paragraph relevance.
    /// </summary>
    public static string TruncateWithRelevance(string content, string targetSectionName, int maxCharacters, string language = "fr")
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length <= maxCharacters)
            return content;

        var paragraphs = SplitIntoParagraphs(content);
        if (paragraphs.Count <= 1)
            return content.Length > maxCharacters ? content[..maxCharacters] + "..." : content;

        // Get relevance keywords from the rubric
        var rubric = SectionRubrics.GetRubric(targetSectionName, language);
        var keywords = rubric != null
            ? rubric.RequiredElements
                .SelectMany(e => e.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Select(w => w.ToLowerInvariant().TrimEnd(',', '.', ':', ';'))
                .Where(w => w.Length > 3)
                .ToHashSet()
            : new HashSet<string>();

        // Score each paragraph
        var scored = paragraphs
            .Select((p, i) => (Paragraph: p, Score: ScoreParagraph(p, keywords, i == 0), Index: i))
            .OrderByDescending(x => x.Score)
            .ToList();

        // Build result keeping highest-scoring paragraphs in original order
        var selected = new List<(string Paragraph, int Index)>();
        var totalLength = 0;
        var truncatedCount = 0;

        foreach (var item in scored)
        {
            if (totalLength + item.Paragraph.Length + 2 <= maxCharacters)
            {
                selected.Add((item.Paragraph, item.Index));
                totalLength += item.Paragraph.Length + 2; // +2 for newlines
            }
            else
            {
                truncatedCount++;
            }
        }

        // Re-sort by original order
        selected.Sort((a, b) => a.Index.CompareTo(b.Index));

        var result = string.Join("\n\n", selected.Select(s => s.Paragraph));

        if (truncatedCount > 0)
        {
            var lang = language.Equals("fr", StringComparison.OrdinalIgnoreCase);
            result += lang
                ? $"\n\n[...{truncatedCount} paragraphes omis]"
                : $"\n\n[...{truncatedCount} paragraphs omitted]";
        }

        return result;
    }

    /// <summary>
    /// Summarizes content for the review pass by preserving topic sentences and numeric data.
    /// Better than arbitrary Substring(0, 1500) for coherence assessment.
    /// </summary>
    public static string SummarizeForReview(string content, int maxCharacters)
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length <= maxCharacters)
            return content;

        var paragraphs = SplitIntoParagraphs(content);
        var result = new List<string>();
        var totalLength = 0;

        foreach (var paragraph in paragraphs)
        {
            // Extract the first sentence (topic sentence)
            var topicSentence = ExtractFirstSentence(paragraph);

            // Also capture any sentences with numbers (data-rich)
            var dataSentences = ExtractDataSentences(paragraph)
                .Where(s => s != topicSentence);

            var entry = topicSentence;
            foreach (var ds in dataSentences)
            {
                if (entry.Length + ds.Length + 2 < 400)
                    entry += " " + ds;
            }

            if (totalLength + entry.Length + 2 <= maxCharacters)
            {
                result.Add(entry);
                totalLength += entry.Length + 2;
            }
        }

        return string.Join("\n\n", result);
    }

    private static List<string> SplitIntoParagraphs(string content)
    {
        return content
            .Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();
    }

    private static double ScoreParagraph(string paragraph, HashSet<string> keywords, bool isFirst)
    {
        var lower = paragraph.ToLowerInvariant();
        double score = 0;

        // First paragraph bonus (usually most important)
        if (isFirst) score += 2.0;

        // Keyword matches
        foreach (var kw in keywords)
        {
            if (lower.Contains(kw))
                score += 1.0;
        }

        // Numeric data bonus (paragraphs with numbers are data-rich)
        var digitCount = paragraph.Count(char.IsDigit);
        if (digitCount > 3) score += 1.5;

        // Currency/percentage symbols bonus
        if (paragraph.Contains('$') || paragraph.Contains('%') || paragraph.Contains('€'))
            score += 1.0;

        // Length penalty for very short paragraphs
        if (paragraph.Length < 50) score -= 1.0;

        return score;
    }

    private static string ExtractFirstSentence(string paragraph)
    {
        var sentenceEnders = new[] { ". ", ".\n", ".\r" };
        var minEnd = paragraph.Length;

        foreach (var ender in sentenceEnders)
        {
            var idx = paragraph.IndexOf(ender, StringComparison.Ordinal);
            if (idx >= 0 && idx + 1 < minEnd)
                minEnd = idx + 1;
        }

        return paragraph[..minEnd].Trim();
    }

    private static IEnumerable<string> ExtractDataSentences(string paragraph)
    {
        var sentences = paragraph.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
        return sentences.Where(s => s.Any(char.IsDigit));
    }
}
