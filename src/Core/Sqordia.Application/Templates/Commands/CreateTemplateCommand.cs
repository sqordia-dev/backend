using MediatR;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Templates.Queries;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Templates.Commands;

public record CreateTemplateCommand : IRequest<Result<TemplateDto>>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public TemplateCategory Category { get; init; }
    public TemplateType Type { get; init; }
    public string Industry { get; init; } = string.Empty;
    public string TargetAudience { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public bool IsPublic { get; init; }
    public string Tags { get; init; } = string.Empty;
    public string PreviewImage { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string AuthorEmail { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Changelog { get; init; } = string.Empty;
}
