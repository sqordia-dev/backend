using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Constants;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Domain.Entities.ML;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Persists ML training signals (AI call telemetry and section edit diffs) to the database.
/// </summary>
public class MLDataCollector : IMLDataCollector
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<MLDataCollector> _logger;

    public MLDataCollector(IApplicationDbContext context, ILogger<MLDataCollector> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> RecordAICallAsync(
        AICallTelemetryRecord record,
        CancellationToken cancellationToken = default)
    {
        _context.AICallTelemetryRecords.Add(record);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Persisted AI call telemetry: {Id} | {Provider}/{Model} | {Section} | {Pass} | {InputTokens}+{OutputTokens} tokens | {LatencyMs}ms",
            record.Id, record.Provider, record.ModelUsed, record.SectionType,
            record.PipelinePass, record.InputTokens, record.OutputTokens, record.LatencyMs);

        return record.Id;
    }

    public async Task RecordSectionEditAsync(
        Guid businessPlanId,
        string sectionType,
        string aiGeneratedContent,
        string userEditedContent,
        string language,
        Guid? promptTemplateId = null,
        string? industry = null,
        string? planType = null,
        CancellationToken cancellationToken = default)
    {
        var (editDistance, editRatio) = ComputeWordLevelDiff(aiGeneratedContent, userEditedContent);

        // Skip recording if no meaningful edit
        if (editRatio < PipelineConstants.MinEditRatioThreshold)
        {
            _logger.LogDebug("Skipping section edit record for {Section}: edit ratio {Ratio:P1} below threshold",
                sectionType, editRatio);
            return;
        }

        var record = new SectionEditHistory(
            businessPlanId,
            sectionType,
            aiGeneratedContent,
            userEditedContent,
            editDistance,
            editRatio,
            language,
            promptTemplateId,
            industry,
            planType);

        _context.SectionEditHistories.Add(record);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Recorded section edit: {Section} | EditDistance={Distance} | EditRatio={Ratio:P1} | Industry={Industry}",
            sectionType, editDistance, editRatio, industry ?? "unknown");
    }

    public async Task UpdateCallOutcomeAsync(
        Guid telemetryRecordId,
        bool? wasAccepted,
        bool? wasRegenerated,
        bool? wasEdited,
        double? editRatio,
        decimal? qualityScore,
        CancellationToken cancellationToken = default)
    {
        var record = await _context.AICallTelemetryRecords.FindAsync(
            new object[] { telemetryRecordId }, cancellationToken);

        if (record == null)
        {
            _logger.LogWarning("Telemetry record {Id} not found for outcome update", telemetryRecordId);
            return;
        }

        record.RecordOutcome(wasAccepted, wasRegenerated, wasEdited, editRatio, qualityScore);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Updated outcome for telemetry {Id}: accepted={Accepted}, regenerated={Regen}, edited={Edited}",
            telemetryRecordId, wasAccepted, wasRegenerated, wasEdited);
    }

    /// <summary>
    /// Computes word-level edit distance and ratio between two texts.
    /// Uses a simplified Levenshtein approach on word arrays.
    /// </summary>
    public static (int EditDistance, double EditRatio) ComputeWordLevelDiff(string original, string edited)
    {
        if (string.IsNullOrWhiteSpace(original) && string.IsNullOrWhiteSpace(edited))
            return (0, 0.0);

        if (string.IsNullOrWhiteSpace(original))
            return (edited.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length, 1.0);

        if (string.IsNullOrWhiteSpace(edited))
            return (original.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length, 1.0);

        var originalWords = original.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var editedWords = edited.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var maxLen = Math.Max(originalWords.Length, editedWords.Length);
        if (maxLen == 0) return (0, 0.0);

        // Use LCS-based diff for efficiency with long texts
        var lcsLength = LongestCommonSubsequenceLength(originalWords, editedWords);
        var editDistance = maxLen - lcsLength;
        var editRatio = (double)editDistance / maxLen;

        return (editDistance, Math.Min(editRatio, 1.0));
    }

    /// <summary>
    /// Space-optimized LCS length for word arrays.
    /// O(min(m,n)) space, O(m*n) time — capped at 2000 words per side for safety.
    /// </summary>
    private static int LongestCommonSubsequenceLength(string[] a, string[] b)
    {
        var maxWords = PipelineConstants.MaxDiffWords;
        if (a.Length > maxWords) a = a[..maxWords];
        if (b.Length > maxWords) b = b[..maxWords];

        // Ensure a is the shorter array for space optimization
        if (a.Length > b.Length) (a, b) = (b, a);

        var prev = new int[a.Length + 1];
        var curr = new int[a.Length + 1];

        for (var j = 1; j <= b.Length; j++)
        {
            for (var i = 1; i <= a.Length; i++)
            {
                if (string.Equals(a[i - 1], b[j - 1], StringComparison.OrdinalIgnoreCase))
                    curr[i] = prev[i - 1] + 1;
                else
                    curr[i] = Math.Max(prev[i], curr[i - 1]);
            }

            (prev, curr) = (curr, prev);
            Array.Clear(curr, 0, curr.Length);
        }

        return prev[a.Length];
    }
}
