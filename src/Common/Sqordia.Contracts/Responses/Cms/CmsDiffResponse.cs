namespace Sqordia.Contracts.Responses.Cms;

/// <summary>
/// Response containing the diff between two CMS versions
/// </summary>
public class CmsDiffResponse
{
    public required Guid SourceVersionId { get; set; }
    public required int SourceVersionNumber { get; set; }
    public required Guid TargetVersionId { get; set; }
    public required int TargetVersionNumber { get; set; }
    public required List<CmsBlockDiffResponse> BlockDiffs { get; set; }
    public required CmsDiffSummary Summary { get; set; }
}

/// <summary>
/// Diff information for a single content block
/// </summary>
public class CmsBlockDiffResponse
{
    public required string BlockKey { get; set; }
    public required string SectionKey { get; set; }
    public required string BlockType { get; set; }
    public required string Language { get; set; }
    public required CmsBlockDiffStatus Status { get; set; }
    public string? SourceContent { get; set; }
    public string? TargetContent { get; set; }
    public Guid? SourceBlockId { get; set; }
    public Guid? TargetBlockId { get; set; }
}

/// <summary>
/// Summary statistics for the diff
/// </summary>
public class CmsDiffSummary
{
    public required int TotalChanges { get; set; }
    public required int AddedCount { get; set; }
    public required int RemovedCount { get; set; }
    public required int ModifiedCount { get; set; }
    public required int UnchangedCount { get; set; }
}

/// <summary>
/// Status of a block in the diff
/// </summary>
public enum CmsBlockDiffStatus
{
    Unchanged = 0,
    Added = 1,
    Removed = 2,
    Modified = 3
}
