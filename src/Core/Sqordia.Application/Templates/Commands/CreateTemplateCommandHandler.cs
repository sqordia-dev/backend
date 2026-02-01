using MediatR;
using Microsoft.EntityFrameworkCore;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Templates.Queries;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Application.Templates.Commands;

public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, Result<TemplateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateTemplateCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TemplateDto>> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        // Verify user exists and is active (admin users bypass email verification)
        if (!string.IsNullOrEmpty(_currentUserService.UserId) && Guid.TryParse(_currentUserService.UserId, out var userId))
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            
            if (user == null)
            {
                return Result.Failure<TemplateDto>(Error.NotFound("User.NotFound", "User not found"));
            }
            
            // Check if user is active
            if (!user.IsActive)
            {
                return Result.Failure<TemplateDto>(Error.Unauthorized("Auth.Error.AccountDisabled", "Your account is disabled. Please contact support."));
            }
            
            // For non-admin users, check email verification
            var isAdmin = user.UserRoles.Any(ur => ur.Role.Name == "Admin");
            if (!isAdmin && !user.IsEmailConfirmed)
            {
                return Result.Failure<TemplateDto>(Error.Unauthorized("Auth.Error.EmailNotVerified", "Please verify your email address before continuing."));
            }
        }
        
        var template = Template.Create(
            name: request.Name,
            description: request.Description,
            content: request.Content,
            category: request.Category,
            type: request.Type,
            author: request.Author,
            authorEmail: request.AuthorEmail,
            createdBy: _currentUserService.UserId ?? "System",
            industry: request.Industry,
            targetAudience: request.TargetAudience,
            language: request.Language,
            country: request.Country,
            isPublic: request.IsPublic,
            tags: request.Tags,
            previewImage: request.PreviewImage,
            version: request.Version,
            changelog: request.Changelog);

        _context.Templates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new TemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Category = template.Category.ToString(),
            Type = template.Type.ToString(),
            Industry = template.Industry,
            TargetAudience = template.TargetAudience,
            Language = template.Language,
            Country = template.Country,
            IsPublic = template.IsPublic,
            Tags = template.Tags,
            PreviewImage = template.PreviewImage,
            Author = template.Author,
            Version = template.Version,
            Status = template.Status.ToString(),
            UsageCount = template.UsageCount,
            Rating = template.Rating,
            RatingCount = template.RatingCount,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };

        return Result.Success(dto);
    }
}
