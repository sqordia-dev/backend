using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Domain.Entities;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Email template CRUD, rendering, and AI generation service
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly ILogger<EmailTemplateService> _logger;

    public EmailTemplateService(
        IApplicationDbContext context,
        IAIService aiService,
        ILogger<EmailTemplateService> logger)
    {
        _context = context;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<Result<List<EmailTemplateDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _context.EmailTemplates
            .Where(t => !t.IsDeleted)
            .OrderByDescending(t => t.Created)
            .Select(t => MapToDto(t))
            .ToListAsync(cancellationToken);

        return Result.Success(templates);
    }

    public async Task<Result<EmailTemplateDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (template == null)
            return Result.Failure<EmailTemplateDto>(Error.NotFound("EmailTemplate.NotFound", "Email template not found"));

        return Result.Success(MapToDto(template));
    }

    public async Task<Result<EmailTemplateDto>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var template = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Name == name && t.IsActive && !t.IsDeleted, cancellationToken);

        if (template == null)
            return Result.Failure<EmailTemplateDto>(Error.NotFound("EmailTemplate.NotFound", "Email template not found"));

        return Result.Success(MapToDto(template));
    }

    public async Task<Result<EmailTemplateDto>> CreateAsync(CreateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _context.EmailTemplates
            .AnyAsync(t => t.Name == request.Name && !t.IsDeleted, cancellationToken);

        if (existing)
            return Result.Failure<EmailTemplateDto>(Error.Conflict("EmailTemplate.NameExists", "A template with this name already exists"));

        var template = new EmailTemplate
        {
            Name = request.Name,
            Category = request.Category,
            SubjectFr = request.SubjectFr,
            SubjectEn = request.SubjectEn,
            BodyFr = request.BodyFr,
            BodyEn = request.BodyEn,
            VariablesJson = request.Variables != null ? JsonSerializer.Serialize(request.Variables) : null,
            IsActive = true,
            Version = 1
        };

        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(template));
    }

    public async Task<Result<EmailTemplateDto>> UpdateAsync(Guid id, UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (template == null)
            return Result.Failure<EmailTemplateDto>(Error.NotFound("EmailTemplate.NotFound", "Email template not found"));

        if (request.SubjectFr != null) template.SubjectFr = request.SubjectFr;
        if (request.SubjectEn != null) template.SubjectEn = request.SubjectEn;
        if (request.BodyFr != null) template.BodyFr = request.BodyFr;
        if (request.BodyEn != null) template.BodyEn = request.BodyEn;
        if (request.Variables != null) template.VariablesJson = JsonSerializer.Serialize(request.Variables);
        if (request.IsActive.HasValue) template.IsActive = request.IsActive.Value;
        template.Version++;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(MapToDto(template));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (template == null)
            return Result.Failure(Error.NotFound("EmailTemplate.NotFound", "Email template not found"));

        template.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<string>> RenderAsync(Guid id, string language, Dictionary<string, string> variables, CancellationToken cancellationToken = default)
    {
        var template = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive && !t.IsDeleted, cancellationToken);

        if (template == null)
            return Result.Failure<string>(Error.NotFound("EmailTemplate.NotFound", "Email template not found"));

        var body = language == "fr" ? template.BodyFr : template.BodyEn;

        // Replace {{variable}} placeholders
        foreach (var kvp in variables)
        {
            body = body.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }

        return Result.Success(body);
    }

    public async Task<Result<EmailTemplateDto>> GenerateWithAiAsync(GenerateEmailTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var variableList = request.Variables != null ? string.Join(", ", request.Variables.Select(v => $"{{{{{v}}}}}")) : "";

            var systemPrompt = @"You are an expert email template designer. Generate professional bilingual (French and English) HTML email templates.
You MUST respond with valid JSON:
{
  ""subjectFr"": ""French subject line"",
  ""subjectEn"": ""English subject line"",
  ""bodyFr"": ""French HTML email body"",
  ""bodyEn"": ""English HTML email body""
}
Use responsive HTML with inline CSS. Include the provided template variables as {{variableName}} placeholders.";

            var userPrompt = $"Template: {request.Name}\nCategory: {request.Category}\nDescription: {request.Description}\nVariables: {variableList}";

            var content = await _aiService.GenerateContentAsync(systemPrompt, userPrompt, 4000, 0.7f, cancellationToken);

            // Parse AI response
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd <= jsonStart)
                return Result.Failure<EmailTemplateDto>(Error.Failure("EmailTemplate.AIError", "AI returned invalid response"));

            var json = content[jsonStart..(jsonEnd + 1)];
            var generated = JsonSerializer.Deserialize<GeneratedEmailContent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (generated == null)
                return Result.Failure<EmailTemplateDto>(Error.Failure("EmailTemplate.AIError", "Failed to parse AI response"));

            // Create the template
            var createRequest = new CreateEmailTemplateRequest
            {
                Name = request.Name,
                Category = request.Category,
                SubjectFr = generated.SubjectFr ?? string.Empty,
                SubjectEn = generated.SubjectEn ?? string.Empty,
                BodyFr = generated.BodyFr ?? string.Empty,
                BodyEn = generated.BodyEn ?? string.Empty,
                Variables = request.Variables
            };

            return await CreateAsync(createRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating email template with AI");
            return Result.Failure<EmailTemplateDto>(Error.Failure("EmailTemplate.AIError", $"AI generation failed: {ex.Message}"));
        }
    }

    private static EmailTemplateDto MapToDto(EmailTemplate t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Category = t.Category,
        SubjectFr = t.SubjectFr,
        SubjectEn = t.SubjectEn,
        BodyFr = t.BodyFr,
        BodyEn = t.BodyEn,
        Variables = !string.IsNullOrEmpty(t.VariablesJson)
            ? JsonSerializer.Deserialize<List<string>>(t.VariablesJson) ?? new()
            : new(),
        IsActive = t.IsActive,
        Version = t.Version,
        Created = t.Created,
        LastModified = t.LastModified
    };

    private class GeneratedEmailContent
    {
        public string? SubjectFr { get; set; }
        public string? SubjectEn { get; set; }
        public string? BodyFr { get; set; }
        public string? BodyEn { get; set; }
    }
}
