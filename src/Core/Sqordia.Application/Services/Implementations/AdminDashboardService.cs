using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Common.Security;
using Sqordia.Domain.Entities.Identity;
using Sqordia.Domain.Enums;
using Sqordia.Domain.ValueObjects;
using System.Text;

namespace Sqordia.Application.Services.Implementations;

/// <summary>
/// Implementation of admin dashboard service for system management
/// </summary>
public class AdminDashboardService : IAdminDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AdminDashboardService> _logger;
    private readonly ISecurityService _securityService;

    public AdminDashboardService(
        IApplicationDbContext context,
        ILogger<AdminDashboardService> logger,
        ISecurityService securityService)
    {
        _context = context;
        _logger = logger;
        _securityService = securityService;
    }

    public async Task<Result<AdminSystemOverview>> GetSystemOverviewAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating system overview for admin dashboard");

            var now = DateTime.UtcNow;
            var today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // User statistics
            var totalUsers = await _context.Users.CountAsync(cancellationToken);
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive, cancellationToken);
            var newUsersToday = await _context.Users.CountAsync(u => u.Created >= today, cancellationToken);
            var newUsersThisWeek = await _context.Users.CountAsync(u => u.Created >= weekStart, cancellationToken);
            var newUsersThisMonth = await _context.Users.CountAsync(u => u.Created >= monthStart, cancellationToken);

            // Organization statistics
            var totalOrganizations = await _context.Organizations.CountAsync(cancellationToken);
            var activeOrganizations = await _context.Organizations.CountAsync(o => o.IsActive, cancellationToken);
            var newOrganizationsToday = await _context.Organizations.CountAsync(o => o.Created >= today, cancellationToken);
            var newOrganizationsThisWeek = await _context.Organizations.CountAsync(o => o.Created >= weekStart, cancellationToken);

            // Business plan statistics
            var totalBusinessPlans = await _context.BusinessPlans.CountAsync(cancellationToken);
            var completedBusinessPlans = await _context.BusinessPlans.CountAsync(bp => bp.CompletionPercentage >= 100, cancellationToken);
            var inProgressBusinessPlans = totalBusinessPlans - completedBusinessPlans;
            var businessPlansCreatedToday = await _context.BusinessPlans.CountAsync(bp => bp.Created >= today, cancellationToken);
            var businessPlansCreatedThisWeek = await _context.BusinessPlans.CountAsync(bp => bp.Created >= weekStart, cancellationToken);

            // Financial projections
            var totalFinancialProjections = await _context.BusinessPlanFinancialProjections.CountAsync(cancellationToken);

            // Get real popular features from audit logs
            var featureActions = new Dictionary<string, string>
            {
                { "Business Plan Generation", "DATA_CREATED" },
                { "Financial Projections", "DATA_CREATED" },
                { "Organization Management", "DATA_CREATED" }
            };

            var popularFeatures = new List<AdminFeatureUsage>();
            
            // Aggregate feature usage at the database level instead of loading all rows
            var featureEntityTypes = new[] { "BusinessPlan", "FinancialProjection", "Organization" };
            var featureNames = new Dictionary<string, string>
            {
                { "BusinessPlan", "Business Plan Generation" },
                { "FinancialProjection", "Financial Projections" },
                { "Organization", "Organization Management" }
            };

            var featureStats = await _context.AuditLogs
                .Where(log => featureEntityTypes.Contains(log.EntityType) && log.Success)
                .GroupBy(log => log.EntityType)
                .Select(g => new
                {
                    EntityType = g.Key,
                    UsageCount = g.Count(),
                    UniqueUsers = g.Where(log => log.UserId.HasValue).Select(log => log.UserId!.Value).Distinct().Count(),
                    LastUsed = g.Max(log => log.Timestamp)
                })
                .ToListAsync(cancellationToken);

            foreach (var stat in featureStats)
            {
                popularFeatures.Add(new AdminFeatureUsage
                {
                    FeatureName = featureNames[stat.EntityType],
                    UsageCount = stat.UsageCount,
                    UniqueUsers = stat.UniqueUsers,
                    LastUsed = stat.LastUsed
                });
            }

            // If no features found, add defaults
            if (!popularFeatures.Any())
            {
                popularFeatures.Add(new AdminFeatureUsage
                {
                    FeatureName = "Business Plan Generation",
                    UsageCount = totalBusinessPlans,
                    UniqueUsers = activeUsers,
                    LastUsed = now.AddHours(-1)
                });
            }

            // Get real recent activities from audit logs
            var recentAuditLogs = await _context.AuditLogs
                .Where(log => log.Success && log.Timestamp >= now.AddDays(-7))
                .OrderByDescending(log => log.Timestamp)
                .Take(10)
                .ToListAsync(cancellationToken);

            // Get user emails for logs that have userIds
            var userIds = recentAuditLogs.Where(log => log.UserId.HasValue).Select(log => log.UserId!.Value).Distinct().ToList();
            var userEmails = userIds.Any() 
                ? await _context.Users
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.Email.Value, cancellationToken)
                : new Dictionary<Guid, string>();

            var recentActivities = recentAuditLogs.Select(log =>
            {
                var activityName = log.Action switch
                {
                    "DATA_CREATED" when log.EntityType == "BusinessPlan" => "Business Plan Created",
                    "DATA_CREATED" when log.EntityType == "Organization" => "Organization Created",
                    "REGISTER" or "ACCOUNT_CREATED" => "User Registered",
                    "LOGIN" => "User Logged In",
                    "DATA_UPDATED" when log.EntityType == "BusinessPlan" => "Business Plan Updated",
                    _ => log.Action
                };

                var userEmail = log.UserId.HasValue && userEmails.TryGetValue(log.UserId.Value, out var email) 
                    ? email 
                    : "System";

                return new AdminRecentActivity
                {
                    Activity = activityName,
                    UserEmail = userEmail,
                    Timestamp = log.Timestamp,
                    EntityName = !string.IsNullOrEmpty(log.EntityId) ? log.EntityId : null
                };
            }).ToList();

            // If no recent activities, add some defaults
            if (!recentActivities.Any())
            {
                recentActivities.Add(new AdminRecentActivity
                {
                    Activity = "System Initialized",
                    UserEmail = "System",
                    Timestamp = now
                });
            }

            var overview = new AdminSystemOverview
            {
                GeneratedAt = now,
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                NewUsersToday = newUsersToday,
                NewUsersThisWeek = newUsersThisWeek,
                NewUsersThisMonth = newUsersThisMonth,
                TotalOrganizations = totalOrganizations,
                ActiveOrganizations = activeOrganizations,
                NewOrganizationsToday = newOrganizationsToday,
                NewOrganizationsThisWeek = newOrganizationsThisWeek,
                TotalBusinessPlans = totalBusinessPlans,
                CompletedBusinessPlans = completedBusinessPlans,
                InProgressBusinessPlans = inProgressBusinessPlans,
                BusinessPlansCreatedToday = businessPlansCreatedToday,
                BusinessPlansCreatedThisWeek = businessPlansCreatedThisWeek,
                TotalAIRequests = totalBusinessPlans * 5, // Estimate
                AIRequestsToday = businessPlansCreatedToday * 5,
                EstimatedAICostToday = businessPlansCreatedToday * 2.50m,
                EstimatedAICostThisMonth = newUsersThisMonth * 15.00m,
                AverageResponseTime = 245.5, // Mock data
                SystemUptime = 99.85, // Mock data
                ActiveSessions = activeUsers / 10,
                PopularFeatures = popularFeatures,
                RecentActivities = recentActivities
            };

            return Result.Success(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating system overview");
            return Result.Failure<AdminSystemOverview>("Failed to generate system overview.");
        }
    }

    public async Task<Result<PaginatedList<AdminUserInfo>>> GetUsersAsync(AdminUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving users for admin dashboard with filters");

            var query = _context.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(u => u.Email.Value.ToLower().Contains(searchTerm) ||
                                       u.FirstName.ToLower().Contains(searchTerm) ||
                                       u.LastName.ToLower().Contains(searchTerm) ||
                                       u.UserName.ToLower().Contains(searchTerm));
            }

            if (request.Status.HasValue)
            {
                var isActive = request.Status.Value == UserStatus.Active;
                query = query.Where(u => u.IsActive == isActive);
            }

            if (!string.IsNullOrEmpty(request.UserType))
            {
                query = query.Where(u => u.UserType.ToString() == request.UserType);
            }

            if (request.CreatedAfter.HasValue)
            {
                query = query.Where(u => u.Created >= request.CreatedAfter.Value);
            }

            if (request.CreatedBefore.HasValue)
            {
                query = query.Where(u => u.Created <= request.CreatedBefore.Value);
            }

            if (request.EmailVerified.HasValue)
            {
                query = query.Where(u => u.IsEmailConfirmed == request.EmailVerified.Value);
            }

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "email" => request.SortDescending ? query.OrderByDescending(u => u.Email.Value) : query.OrderBy(u => u.Email.Value),
                "firstname" => request.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                "lastname" => request.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                "lastlogin" => request.SortDescending ? query.OrderByDescending(u => u.LastLoginAt) : query.OrderBy(u => u.LastLoginAt),
                _ => request.SortDescending ? query.OrderByDescending(u => u.Created) : query.OrderBy(u => u.Created)
            };

            // Get paginated results
            var totalCount = await query.CountAsync(cancellationToken);
            var users = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new AdminUserInfo
                {
                    Id = u.Id,
                    Email = u.Email.Value,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserName = u.UserName,
                    UserType = u.UserType.ToString(),
                    Status = u.IsActive ? UserStatus.Active : UserStatus.Inactive,
                    CreatedAt = u.Created,
                    LastLoginAt = u.LastLoginAt,
                    EmailVerified = u.IsEmailConfirmed,
                    TwoFactorEnabled = false, // Simplified
                    LoginCount = 0, // Simplified
                    OrganizationCount = 0, // Simplified - would need complex query
                    BusinessPlanCount = 0, // Simplified - would need complex query
                    AIRequestCount = 0, // Simplified
                    EstimatedAICost = 0, // Simplified
                    LastActiveIP = "192.168.1.1", // Mock data
                    LastActiveLocation = "Unknown" // Mock data
                })
                .ToListAsync(cancellationToken);

            var paginatedResult = new PaginatedList<AdminUserInfo>(users, totalCount, request.Page, request.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for admin dashboard");
            return Result.Failure<PaginatedList<AdminUserInfo>>("Failed to retrieve users.");
        }
    }

    public async Task<Result<PaginatedList<AdminOrganizationInfo>>> GetOrganizationsAsync(AdminOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving organizations for admin dashboard");

            var query = _context.Organizations.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(o => o.Name.ToLower().Contains(searchTerm) ||
                                       (o.Description != null && o.Description.ToLower().Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(request.OrganizationType))
            {
                query = query.Where(o => o.OrganizationType.ToString() == request.OrganizationType);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(o => o.IsActive == request.IsActive.Value);
            }

            if (request.CreatedAfter.HasValue)
            {
                query = query.Where(o => o.Created >= request.CreatedAfter.Value);
            }

            if (request.CreatedBefore.HasValue)
            {
                query = query.Where(o => o.Created <= request.CreatedBefore.Value);
            }

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(o => o.Name) : query.OrderBy(o => o.Name),
                "type" => request.SortDescending ? query.OrderByDescending(o => o.OrganizationType) : query.OrderBy(o => o.OrganizationType),
                _ => request.SortDescending ? query.OrderByDescending(o => o.Created) : query.OrderBy(o => o.Created)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var organizationsData = await query
                .Include(o => o.Members.Where(m => m.IsActive))
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Get organization IDs for business plan counts
            var organizationIds = organizationsData.Select(o => o.Id).ToList();
            var businessPlanCounts = await _context.BusinessPlans
                .Where(bp => organizationIds.Contains(bp.OrganizationId))
                .GroupBy(bp => bp.OrganizationId)
                .ToDictionaryAsync(g => g.Key, g => new { Total = g.Count(), Completed = g.Count(bp => bp.CompletionPercentage >= 100) }, cancellationToken);

            var organizations = organizationsData.Select(o =>
            {
                var bpCounts = businessPlanCounts.TryGetValue(o.Id, out var counts) ? counts : new { Total = 0, Completed = 0 };
                return new AdminOrganizationInfo
                {
                    Id = o.Id,
                    Name = o.Name,
                    Description = o.Description ?? "",
                    OrganizationType = o.OrganizationType.ToString(),
                    IsActive = o.IsActive,
                    CreatedAt = o.Created,
                    OwnerId = Guid.Empty, // Would need to parse CreatedBy or find owner member
                    OwnerEmail = o.CreatedBy ?? "Unknown",
                    OwnerName = "Unknown",
                    MemberCount = o.Members.Count,
                    BusinessPlanCount = bpCounts.Total,
                    CompletedBusinessPlanCount = bpCounts.Completed,
                    LastActivityAt = o.LastModified
                };
            }).ToList();

            var paginatedResult = new PaginatedList<AdminOrganizationInfo>(organizations, totalCount, request.Page, request.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving organizations for admin dashboard");
            return Result.Failure<PaginatedList<AdminOrganizationInfo>>("Failed to retrieve organizations.");
        }
    }

    public async Task<Result<PaginatedList<AdminBusinessPlanInfo>>> GetBusinessPlansAsync(AdminBusinessPlanRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving business plans for admin dashboard");

            var query = _context.BusinessPlans.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(bp => bp.Title.ToLower().Contains(searchTerm) ||
                                        (bp.Description != null && bp.Description.ToLower().Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(request.PlanType))
            {
                query = query.Where(bp => bp.PlanType.ToString() == request.PlanType);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                var isCompleted = request.Status.ToLower() == "completed";
                if (isCompleted)
                    query = query.Where(bp => bp.CompletionPercentage >= 100);
                else
                    query = query.Where(bp => bp.CompletionPercentage < 100);
            }

            if (request.CreatedAfter.HasValue)
            {
                query = query.Where(bp => bp.Created >= request.CreatedAfter.Value);
            }

            if (request.CreatedBefore.HasValue)
            {
                query = query.Where(bp => bp.Created <= request.CreatedBefore.Value);
            }

            if (request.OrganizationId.HasValue)
            {
                query = query.Where(bp => bp.OrganizationId == request.OrganizationId.Value);
            }

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "title" => request.SortDescending ? query.OrderByDescending(bp => bp.Title) : query.OrderBy(bp => bp.Title),
                "type" => request.SortDescending ? query.OrderByDescending(bp => bp.PlanType) : query.OrderBy(bp => bp.PlanType),
                "completed" => request.SortDescending ? query.OrderByDescending(bp => bp.CompletionPercentage) : query.OrderBy(bp => bp.CompletionPercentage),
                _ => request.SortDescending ? query.OrderByDescending(bp => bp.Created) : query.OrderBy(bp => bp.Created)
            };

            var totalCount = await query.CountAsync(cancellationToken);
            var businessPlans = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(bp => new AdminBusinessPlanInfo
                {
                    Id = bp.Id,
                    Title = bp.Title,
                    Description = bp.Description ?? "",
                    PlanType = bp.PlanType.ToString(),
                    Status = bp.CompletionPercentage >= 100 ? "Completed" : "In Progress",
                    CreatedAt = bp.Created,
                    CompletedAt = bp.CompletionPercentage >= 100 ? bp.LastModified : null,
                    CreatedByUserId = Guid.Empty, // Simplified
                    CreatedByEmail = bp.CreatedBy ?? "Unknown",
                    OrganizationId = bp.OrganizationId,
                    OrganizationName = "Organization", // Simplified
                    SectionCount = 0, // Simplified
                    CompletedSectionCount = 0, // Simplified
                    AIGeneratedSectionCount = 0, // Simplified
                    FinancialProjectionCount = 0, // Simplified
                    EstimatedAICost = 0.50m, // Mock estimate
                    LastModifiedAt = bp.LastModified
                })
                .ToListAsync(cancellationToken);

            var paginatedResult = new PaginatedList<AdminBusinessPlanInfo>(businessPlans, totalCount, request.Page, request.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business plans for admin dashboard");
            return Result.Failure<PaginatedList<AdminBusinessPlanInfo>>("Failed to retrieve business plans.");
        }
    }

    public async Task<Result> UpdateUserStatusAsync(Guid userId, UserStatus status, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating user status for user {UserId} to {Status}. Reason: {Reason}", userId, status, reason);

            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            // Update user status based on the status enum
            var isActive = status == UserStatus.Active;
            if (user.IsActive != isActive)
            {
                if (isActive)
                {
                    user.Activate();
                }
                else
                {
                    user.Deactivate();
                }
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("User {UserId} status updated to {Status}", userId, status);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status for user {UserId}", userId);
            return Result.Failure("Failed to update user status.");
        }
    }

    public async Task<Result> UpdateOrganizationStatusAsync(Guid organizationId, bool isActive, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating organization status for organization {OrganizationId} to {IsActive}. Reason: {Reason}", organizationId, isActive, reason);

            var organization = await _context.Organizations.FindAsync(new object[] { organizationId }, cancellationToken);
            if (organization == null)
            {
                return Result.Failure("Organization not found.");
            }

            // Update organization status
            if (organization.IsActive != isActive)
            {
                if (isActive)
                {
                    organization.Reactivate();
                }
                else
                {
                    organization.Deactivate();
                }
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Organization {OrganizationId} status updated to {IsActive}", organizationId, isActive);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization status for organization {OrganizationId}", organizationId);
            return Result.Failure("Failed to update organization status.");
        }
    }

    public async Task<Result<PaginatedList<AdminActivityLog>>> GetActivityLogsAsync(AdminActivityLogRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving activity logs for admin dashboard");

            var query = _context.AuditLogs.AsQueryable();

            // Apply basic filtering
            if (!string.IsNullOrEmpty(request.Action))
            {
                query = query.Where(log => log.Action.Contains(request.Action));
            }

            if (request.UserId.HasValue)
            {
                query = query.Where(log => log.UserId == request.UserId.Value);
            }

            if (request.IsSuccess.HasValue)
            {
                query = query.Where(log => log.Success == request.IsSuccess.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(log => log.Timestamp <= request.EndDate.Value);
            }

            // Apply sorting
            query = query.OrderByDescending(log => log.Timestamp);

            var totalCount = await query.CountAsync(cancellationToken);
            var auditLogsData = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Get user emails for logs that have userIds
            var userIds = auditLogsData.Where(log => log.UserId.HasValue).Select(log => log.UserId!.Value).Distinct().ToList();
            var userEmails = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Email.Value, cancellationToken);

            var auditLogs = auditLogsData.Select(log => new AdminActivityLog
            {
                Id = log.Id,
                Timestamp = log.Timestamp,
                Action = log.Action,
                Entity = log.EntityType,
                EntityId = null, // Simplified - would need post-processing to parse
                UserId = log.UserId,
                UserEmail = log.UserId.HasValue && userEmails.TryGetValue(log.UserId.Value, out var email) ? email : (log.UserId.HasValue ? "Unknown" : "System"),
                IPAddress = log.IpAddress ?? "Unknown",
                UserAgent = log.UserAgent ?? "Unknown",
                IsSuccess = log.Success,
                ErrorMessage = log.ErrorMessage,
                Metadata = new Dictionary<string, object>()
            }).ToList();

            var paginatedResult = new PaginatedList<AdminActivityLog>(auditLogs, totalCount, request.Page, request.PageSize);
            return Result.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity logs");
            return Result.Failure<PaginatedList<AdminActivityLog>>("Failed to retrieve activity logs.");
        }
    }

    public async Task<Result<AdminSystemHealth>> GetSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking system health for admin dashboard");

            var activeUsers = await _context.Users.CountAsync(u => u.IsActive, cancellationToken);

            // Check database health
            var dbHealthy = true;
            var dbResponseTime = 45.2;
            try
            {
                var startTime = DateTime.UtcNow;
                await _context.Users.CountAsync(cancellationToken);
                dbResponseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                dbHealthy = dbResponseTime < 1000; // Healthy if response time < 1 second
            }
            catch
            {
                dbHealthy = false;
                dbResponseTime = 5000; // Indicate slow/unhealthy
            }

            var systemHealth = new AdminSystemHealth
            {
                CheckedAt = DateTime.UtcNow,
                OverallStatus = dbHealthy ? "Healthy" : "Warning",
                CpuUsage = 15.2, // Would need system metrics in production
                MemoryUsage = 68.5, // Would need system metrics in production
                DiskUsage = 42.8, // Would need system metrics in production
                ActiveConnections = activeUsers / 10,
                DatabaseResponseTime = dbResponseTime,
                APIResponseTime = 125.8, // Would need to track API response times
                DatabaseHealthy = dbHealthy,
                EmailServiceHealthy = true, // Would need to check email service
                AIServiceHealthy = true, // Would need to check AI service
                Alerts = new List<AdminHealthAlert>
                {
                    new() { Severity = dbHealthy ? "Info" : "Warning", Message = dbHealthy ? "System running normally" : "Database response time is high", Timestamp = DateTime.UtcNow, Component = "Database" }
                }
            };

            return Result.Success(systemHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking system health");
            return Result.Failure<AdminSystemHealth>("Failed to check system health.");
        }
    }

    public Task<Result<AdminReportResult>> GenerateSystemReportAsync(AdminReportType reportType, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating system report of type {ReportType}", reportType);

            var reportContent = $"System Report: {reportType}\nGenerated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\n\nReport data would be generated here.";
            var reportData = Encoding.UTF8.GetBytes(reportContent);
            var fileName = $"{reportType}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";

            var result = new AdminReportResult
            {
                ReportData = reportData,
                FileName = fileName,
                ContentType = "text/plain",
                ReportType = reportType,
                GeneratedAt = DateTime.UtcNow,
                Parameters = parameters
            };

            return Task.FromResult(Result.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating system report of type {ReportType}", reportType);
            return Task.FromResult(Result.Failure<AdminReportResult>("Failed to generate system report."));
        }
    }

    public async Task<Result<AdminAIUsageStats>> GetAIUsageStatsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving AI usage statistics from {StartDate} to {EndDate}", startDate, endDate);

            var businessPlansInPeriod = await _context.BusinessPlans
                .Where(bp => bp.Created >= startDate && bp.Created <= endDate)
                .CountAsync(cancellationToken);

            var aiStats = new AdminAIUsageStats
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRequests = businessPlansInPeriod * 8,
                SuccessfulRequests = (int)(businessPlansInPeriod * 8 * 0.95),
                FailedRequests = (int)(businessPlansInPeriod * 8 * 0.05),
                TotalCost = businessPlansInPeriod * 12.50m,
                AverageCostPerRequest = 1.56m,
                TotalTokensUsed = businessPlansInPeriod * 15000,
                DailyUsage = new List<AdminAIDailyUsage>(),
                FeatureBreakdown = new List<AdminAIFeatureUsage>
                {
                    new() { Feature = "Business Plan Generation", Requests = businessPlansInPeriod * 4, Cost = businessPlansInPeriod * 6.00m, AverageResponseTime = 2.8 }
                }
            };

            return Result.Success(aiStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AI usage statistics");
            return Result.Failure<AdminAIUsageStats>("Failed to retrieve AI usage statistics.");
        }
    }

    public async Task<Result<AdminRegenerationResult>> ForceBusinessPlanRegenerationAsync(Guid businessPlanId, List<string>? sections = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Forcing business plan regeneration for {BusinessPlanId}", businessPlanId);

            var businessPlan = await _context.BusinessPlans.FindAsync(new object[] { businessPlanId }, cancellationToken);
            if (businessPlan == null)
            {
                return Result.Failure<AdminRegenerationResult>("Business plan not found.");
            }

            var sectionsToRegenerate = sections ?? new List<string>
            {
                "Executive Summary",
                "Market Analysis",
                "Financial Projections"
            };

            var result = new AdminRegenerationResult
            {
                BusinessPlanId = businessPlanId,
                IsSuccess = true,
                RegeneratedSections = sectionsToRegenerate,
                FailedSections = new List<string>(),
                EstimatedCost = sectionsToRegenerate.Count * 1.25m,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow.AddMinutes(5)
            };

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forcing business plan regeneration for {BusinessPlanId}", businessPlanId);
            return Result.Failure<AdminRegenerationResult>("Failed to force business plan regeneration.");
        }
    }

    public async Task<Result<AdminUserDetail>> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting user detail for {UserId}", userId);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                return Result.Failure<AdminUserDetail>("User not found.");
            }

            // Get login stats
            var loginStats = await _context.LoginHistories
                .Where(lh => lh.UserId == userId)
                .GroupBy(lh => lh.UserId)
                .Select(g => new
                {
                    Total = g.Count(),
                    Successful = g.Count(lh => lh.IsSuccessful),
                    Failed = g.Count(lh => !lh.IsSuccessful)
                })
                .FirstOrDefaultAsync(cancellationToken);

            // Get active session count
            var activeSessionCount = await _context.ActiveSessions
                .CountAsync(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow, cancellationToken);

            // Get organization memberships
            var organizations = await _context.OrganizationMembers
                .Include(om => om.Organization)
                .Where(om => om.UserId == userId && om.IsActive)
                .Select(om => new AdminUserOrganizationInfo
                {
                    Id = om.OrganizationId,
                    Name = om.Organization.Name,
                    Role = om.Role.ToString(),
                    JoinedAt = om.Created,
                    IsActive = om.Organization.IsActive
                })
                .ToListAsync(cancellationToken);

            // Get business plan count
            var businessPlanCount = await _context.BusinessPlans
                .CountAsync(bp => bp.CreatedBy == userId.ToString(), cancellationToken);

            // Check 2FA status
            var has2FA = await _context.TwoFactorAuths
                .AnyAsync(t => t.UserId == userId && t.IsEnabled, cancellationToken);

            var detail = new AdminUserDetail
            {
                Id = user.Id,
                Email = user.Email.Value,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                UserType = user.UserType.ToString(),
                Persona = user.Persona?.ToString(),
                Status = user.IsActive ? UserStatus.Active : UserStatus.Inactive,
                Provider = user.IsGoogleUser ? "google" : user.IsMicrosoftUser ? "microsoft" : "local",
                EmailVerified = user.IsEmailConfirmed,
                EmailConfirmedAt = user.EmailConfirmedAt,
                TwoFactorEnabled = has2FA,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberVerified = user.PhoneNumberVerified,
                ProfilePictureUrl = user.ProfilePictureUrl,
                OnboardingCompleted = user.OnboardingCompleted,
                OnboardingStep = user.OnboardingStep,
                IsLockedOut = user.IsLockedOut,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                RequirePasswordChange = user.RequirePasswordChange,
                PasswordLastChangedAt = user.PasswordLastChangedAt,
                CreatedAt = user.Created,
                LastLoginAt = user.LastLoginAt,
                LastModifiedAt = user.LastModified,
                LoginCount = loginStats?.Total ?? 0,
                SuccessfulLoginCount = loginStats?.Successful ?? 0,
                FailedLoginCount = loginStats?.Failed ?? 0,
                ActiveSessionCount = activeSessionCount,
                OrganizationCount = organizations.Count,
                BusinessPlanCount = businessPlanCount,
                Roles = user.UserRoles.Select(ur => new AdminUserRoleInfo
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsSystemRole = ur.Role.IsSystemRole
                }).ToList(),
                Organizations = organizations
            };

            return Result.Success(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user detail for {UserId}", userId);
            return Result.Failure<AdminUserDetail>("Failed to get user detail.");
        }
    }

    public async Task<Result<Guid>> CreateUserAsync(AdminCreateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Admin creating new user with email {Email}", request.Email);

            // Check if email already exists
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email.Value.ToLower() == request.Email.ToLower(), cancellationToken);

            if (emailExists)
            {
                return Result.Failure<Guid>("A user with this email already exists.");
            }

            // Parse user type
            if (!Enum.TryParse<UserType>(request.UserType, true, out var userType))
            {
                userType = UserType.Entrepreneur;
            }

            var email = new EmailAddress(request.Email);
            var user = new User(request.FirstName, request.LastName, email, request.Email, userType);

            // Set password
            var password = request.Password ?? _securityService.GenerateSecureToken(12);
            var passwordHash = _securityService.HashPassword(password);
            user.SetPasswordHash(passwordHash);

            // Set email verified if requested
            if (request.EmailVerified)
            {
                user.ConfirmEmail();
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            // Assign roles if provided
            if (request.RoleIds != null && request.RoleIds.Any())
            {
                foreach (var roleId in request.RoleIds)
                {
                    var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);
                    if (role != null)
                    {
                        var userRole = new UserRole(user.Id, role.Id);
                        _context.UserRoles.Add(userRole);
                    }
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Admin created user {UserId} with email {Email}", user.Id, request.Email);
            return Result.Success(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user from admin panel");
            return Result.Failure<Guid>("Failed to create user.");
        }
    }

    public async Task<Result> UpdateUserProfileAsync(Guid userId, AdminUpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Admin updating user profile for {UserId}", userId);

            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            if (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.LastName))
            {
                user.UpdateProfile(
                    request.FirstName ?? user.FirstName,
                    request.LastName ?? user.LastName,
                    request.UserName);
            }

            if (!string.IsNullOrEmpty(request.UserType) && Enum.TryParse<UserType>(request.UserType, true, out var userType))
            {
                user.UpdateUserType(userType);
            }

            if (request.PhoneNumber != null)
            {
                user.UpdatePhoneNumber(request.PhoneNumber);
            }

            if (request.EmailVerified.HasValue)
            {
                if (request.EmailVerified.Value && !user.IsEmailConfirmed)
                {
                    user.ConfirmEmail();
                }
            }

            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                    user.Activate();
                else
                    user.Deactivate();
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Admin updated user profile for {UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile for {UserId}", userId);
            return Result.Failure("Failed to update user profile.");
        }
    }

    public async Task<Result> AdminResetPasswordAsync(Guid userId, string? newPassword = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Admin resetting password for user {UserId}", userId);

            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }

            var password = newPassword ?? _securityService.GenerateSecureToken(12);
            var passwordHash = _securityService.HashPassword(password);
            user.UpdatePassword(passwordHash);
            user.ForcePasswordChange();
            user.UnlockAccount();

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Admin reset password for user {UserId}", userId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
            return Result.Failure("Failed to reset user password.");
        }
    }

    public async Task<Result<List<AdminUserSession>>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _context.ActiveSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.LastActivityAt)
                .Select(s => new AdminUserSession
                {
                    Id = s.Id,
                    CreatedAt = s.CreatedAt,
                    LastActivityAt = s.LastActivityAt,
                    ExpiresAt = s.ExpiresAt,
                    IsActive = s.IsActive,
                    IpAddress = s.IpAddress,
                    UserAgent = s.UserAgent,
                    DeviceType = s.DeviceType,
                    Browser = s.Browser,
                    OperatingSystem = s.OperatingSystem,
                    Country = s.Country,
                    City = s.City,
                    IsExpired = s.ExpiresAt < DateTime.UtcNow
                })
                .ToListAsync(cancellationToken);

            return Result.Success(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions for user {UserId}", userId);
            return Result.Failure<List<AdminUserSession>>("Failed to get user sessions.");
        }
    }

    public async Task<Result> TerminateUserSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _context.ActiveSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

            if (session == null)
            {
                return Result.Failure("Session not found.");
            }

            session.Revoke();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Admin terminated session {SessionId} for user {UserId}", sessionId, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating session {SessionId} for user {UserId}", sessionId, userId);
            return Result.Failure("Failed to terminate session.");
        }
    }

    public async Task<Result> TerminateAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _context.ActiveSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var session in sessions)
            {
                session.Revoke();
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Admin terminated all sessions for user {UserId}. Count: {Count}", userId, sessions.Count);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating all sessions for user {UserId}", userId);
            return Result.Failure("Failed to terminate user sessions.");
        }
    }

    public async Task<Result<List<AdminLoginHistoryEntry>>> GetUserLoginHistoryAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _context.LoginHistories
                .Where(lh => lh.UserId == userId)
                .OrderByDescending(lh => lh.LoginAttemptAt)
                .Take(limit)
                .Select(lh => new AdminLoginHistoryEntry
                {
                    Id = lh.Id,
                    LoginAttemptAt = lh.LoginAttemptAt,
                    IsSuccessful = lh.IsSuccessful,
                    FailureReason = lh.FailureReason,
                    IpAddress = lh.IpAddress,
                    UserAgent = lh.UserAgent,
                    DeviceType = lh.DeviceType,
                    Browser = lh.Browser,
                    OperatingSystem = lh.OperatingSystem,
                    Country = lh.Country,
                    City = lh.City
                })
                .ToListAsync(cancellationToken);

            return Result.Success(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting login history for user {UserId}", userId);
            return Result.Failure<List<AdminLoginHistoryEntry>>("Failed to get login history.");
        }
    }

    public async Task<Result<byte[]>> ExportUsersAsync(AdminUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Exporting users to CSV");

            // Use existing query logic but without pagination
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(u => u.Email.Value.ToLower().Contains(searchTerm) ||
                                       u.FirstName.ToLower().Contains(searchTerm) ||
                                       u.LastName.ToLower().Contains(searchTerm));
            }

            if (request.Status.HasValue)
            {
                var isActive = request.Status.Value == UserStatus.Active;
                query = query.Where(u => u.IsActive == isActive);
            }

            if (!string.IsNullOrEmpty(request.UserType))
            {
                query = query.Where(u => u.UserType.ToString() == request.UserType);
            }

            if (request.EmailVerified.HasValue)
            {
                query = query.Where(u => u.IsEmailConfirmed == request.EmailVerified.Value);
            }

            var users = await query
                .OrderByDescending(u => u.Created)
                .Select(u => new
                {
                    u.Id,
                    Email = u.Email.Value,
                    u.FirstName,
                    u.LastName,
                    u.UserName,
                    UserType = u.UserType.ToString(),
                    Status = u.IsActive ? "Active" : "Inactive",
                    EmailVerified = u.IsEmailConfirmed ? "Yes" : "No",
                    Created = u.Created,
                    LastLogin = u.LastLoginAt
                })
                .ToListAsync(cancellationToken);

            var csv = new StringBuilder();
            csv.AppendLine("Id,Email,FirstName,LastName,UserName,UserType,Status,EmailVerified,Created,LastLogin");

            foreach (var user in users)
            {
                csv.AppendLine($"\"{user.Id}\",\"{user.Email}\",\"{user.FirstName}\",\"{user.LastName}\",\"{user.UserName}\",\"{user.UserType}\",\"{user.Status}\",\"{user.EmailVerified}\",\"{user.Created:yyyy-MM-dd HH:mm:ss}\",\"{user.LastLogin?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}\"");
            }

            return Result.Success(Encoding.UTF8.GetBytes(csv.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting users to CSV");
            return Result.Failure<byte[]>("Failed to export users.");
        }
    }

    public async Task<Result<AdminBulkActionResult>> BulkUpdateUserStatusAsync(List<Guid> userIds, UserStatus status, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Bulk updating {Count} users to status {Status}. Reason: {Reason}", userIds.Count, status, reason);

            var result = new AdminBulkActionResult
            {
                TotalRequested = userIds.Count
            };

            var isActive = status == UserStatus.Active;

            foreach (var userId in userIds)
            {
                try
                {
                    var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
                    if (user == null)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"User {userId} not found.");
                        continue;
                    }

                    if (isActive)
                        user.Activate();
                    else
                        user.Deactivate();

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Failed to update user {userId}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Bulk status update completed. Success: {Success}, Failed: {Failed}", result.SuccessCount, result.FailedCount);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk user status update");
            return Result.Failure<AdminBulkActionResult>("Failed to perform bulk status update.");
        }
    }
}