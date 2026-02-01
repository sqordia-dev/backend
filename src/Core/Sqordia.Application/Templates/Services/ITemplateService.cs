using Sqordia.Application.Common.Models;
using Sqordia.Application.Templates.Commands;
using Sqordia.Application.Templates.Queries;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Templates.Services;

public interface ITemplateService
{
    Task<Result<TemplateDto>> CreateTemplateAsync(CreateTemplateCommand command);
    Task<Result<TemplateDto>> GetTemplateByIdAsync(Guid id);
    Task<Result<List<TemplateDto>>> GetTemplatesByCategoryAsync(TemplateCategory category);
    Task<Result<List<TemplateDto>>> GetTemplatesByIndustryAsync(string industry);
    Task<Result<List<TemplateDto>>> GetPublicTemplatesAsync();
    Task<Result<List<TemplateDto>>> SearchTemplatesAsync(string searchTerm);
    Task<Result<TemplateDto>> UpdateTemplateAsync(UpdateTemplateCommand command);
    Task<Result<bool>> DeleteTemplateAsync(Guid id);
    Task<Result<TemplateDto>> CloneTemplateAsync(Guid templateId, string newName);
    Task<Result<TemplateDto>> PublishTemplateAsync(Guid templateId);
    Task<Result<TemplateDto>> ArchiveTemplateAsync(Guid templateId);
    Task<Result<TemplateUsageDto>> RecordTemplateUsageAsync(Guid templateId, string usageType);
    Task<Result<TemplateRatingDto>> RateTemplateAsync(Guid templateId, int rating, string comment);
    Task<Result<List<TemplateDto>>> GetPopularTemplatesAsync(int count = 10);
    Task<Result<List<TemplateDto>>> GetRecentTemplatesAsync(int count = 10);
    Task<Result<List<TemplateDto>>> GetTemplatesByAuthorAsync(string author);
    Task<Result<TemplateAnalyticsDto>> GetTemplateAnalyticsAsync(Guid templateId);

    /// <summary>
    /// Get recommended templates based on persona and industry
    /// </summary>
    /// <param name="persona">User persona (Entrepreneur, Consultant, OBNL)</param>
    /// <param name="industry">Industry sector (optional)</param>
    /// <returns>List of recommended templates</returns>
    Task<Result<List<TemplateDto>>> GetRecommendedTemplatesAsync(string? persona, string? industry);
}
