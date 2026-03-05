using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for managing email templates with CRUD, rendering, and AI generation
/// </summary>
public interface IEmailTemplateService
{
    Task<Result<List<EmailTemplateDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<EmailTemplateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<EmailTemplateDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Result<EmailTemplateDto>> CreateAsync(CreateEmailTemplateRequest request, CancellationToken cancellationToken = default);
    Task<Result<EmailTemplateDto>> UpdateAsync(Guid id, UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<string>> RenderAsync(Guid id, string language, Dictionary<string, string> variables, CancellationToken cancellationToken = default);
    Task<Result<EmailTemplateDto>> GenerateWithAiAsync(GenerateEmailTemplateRequest request, CancellationToken cancellationToken = default);
}

public class EmailTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubjectFr { get; set; } = string.Empty;
    public string SubjectEn { get; set; } = string.Empty;
    public string BodyFr { get; set; } = string.Empty;
    public string BodyEn { get; set; } = string.Empty;
    public List<string> Variables { get; set; } = new();
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class CreateEmailTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "transactional";
    public string SubjectFr { get; set; } = string.Empty;
    public string SubjectEn { get; set; } = string.Empty;
    public string BodyFr { get; set; } = string.Empty;
    public string BodyEn { get; set; } = string.Empty;
    public List<string>? Variables { get; set; }
}

public class UpdateEmailTemplateRequest
{
    public string? SubjectFr { get; set; }
    public string? SubjectEn { get; set; }
    public string? BodyFr { get; set; }
    public string? BodyEn { get; set; }
    public List<string>? Variables { get; set; }
    public bool? IsActive { get; set; }
}

public class GenerateEmailTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "transactional";
    public string Description { get; set; } = string.Empty;
    public List<string>? Variables { get; set; }
}
