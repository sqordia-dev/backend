using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for admin dashboard functionality and system management
/// </summary>
public interface IAdminDashboardService
{
    /// <summary>
    /// Get comprehensive system overview metrics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System overview statistics</returns>
    Task<Result<AdminSystemOverview>> GetSystemOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed user management data with pagination
    /// </summary>
    /// <param name="request">User management request with filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated user management data</returns>
    Task<Result<PaginatedList<AdminUserInfo>>> GetUsersAsync(AdminUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed organization management data
    /// </summary>
    /// <param name="request">Organization request with filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated organization data</returns>
    Task<Result<PaginatedList<AdminOrganizationInfo>>> GetOrganizationsAsync(AdminOrganizationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get business plan analytics and management data
    /// </summary>
    /// <param name="request">Business plan request with filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated business plan data</returns>
    Task<Result<PaginatedList<AdminBusinessPlanInfo>>> GetBusinessPlansAsync(AdminBusinessPlanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user status (activate, deactivate, suspend)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="status">New user status</param>
    /// <param name="reason">Reason for status change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    Task<Result> UpdateUserStatusAsync(Guid userId, UserStatus status, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update organization status
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="isActive">Active status</param>
    /// <param name="reason">Reason for change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    Task<Result> UpdateOrganizationStatusAsync(Guid organizationId, bool isActive, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get system activity logs with filtering
    /// </summary>
    /// <param name="request">Activity log request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated activity logs</returns>
    Task<Result<PaginatedList<AdminActivityLog>>> GetActivityLogsAsync(AdminActivityLogRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get system health metrics and performance data
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>System health information</returns>
    Task<Result<AdminSystemHealth>> GetSystemHealthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate comprehensive system reports
    /// </summary>
    /// <param name="reportType">Type of report to generate</param>
    /// <param name="parameters">Report parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated report data</returns>
    Task<Result<AdminReportResult>> GenerateSystemReportAsync(AdminReportType reportType, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get AI usage statistics and costs
    /// </summary>
    /// <param name="startDate">Start date for analysis</param>
    /// <param name="endDate">End date for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI usage analytics</returns>
    Task<Result<AdminAIUsageStats>> GetAIUsageStatsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Force business plan regeneration for specific user/organization
    /// </summary>
    /// <param name="businessPlanId">Business plan ID</param>
    /// <param name="sections">Sections to regenerate (null for all)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Regeneration result</returns>
    Task<Result<AdminRegenerationResult>> ForceBusinessPlanRegenerationAsync(Guid businessPlanId, List<string>? sections = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get comprehensive user detail for admin management
    /// </summary>
    Task<Result<AdminUserDetail>> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new user from admin panel
    /// </summary>
    Task<Result<Guid>> CreateUserAsync(AdminCreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user profile from admin panel
    /// </summary>
    Task<Result> UpdateUserProfileAsync(Guid userId, AdminUpdateUserProfileRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Force password reset for a user
    /// </summary>
    Task<Result> AdminResetPasswordAsync(Guid userId, string? newPassword = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active sessions for a user
    /// </summary>
    Task<Result<List<AdminUserSession>>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminate a specific user session
    /// </summary>
    Task<Result> TerminateUserSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminate all sessions for a user
    /// </summary>
    Task<Result> TerminateAllUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get login history for a user
    /// </summary>
    Task<Result<List<AdminLoginHistoryEntry>>> GetUserLoginHistoryAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export users to CSV
    /// </summary>
    Task<Result<byte[]>> ExportUsersAsync(AdminUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update user status
    /// </summary>
    Task<Result<AdminBulkActionResult>> BulkUpdateUserStatusAsync(List<Guid> userIds, UserStatus status, string reason, CancellationToken cancellationToken = default);
}

/// <summary>
/// System overview for admin dashboard
/// </summary>
public class AdminSystemOverview
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // User statistics
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }

    // Organization statistics
    public int TotalOrganizations { get; set; }
    public int ActiveOrganizations { get; set; }
    public int NewOrganizationsToday { get; set; }
    public int NewOrganizationsThisWeek { get; set; }

    // Business plan statistics
    public int TotalBusinessPlans { get; set; }
    public int CompletedBusinessPlans { get; set; }
    public int InProgressBusinessPlans { get; set; }
    public int BusinessPlansCreatedToday { get; set; }
    public int BusinessPlansCreatedThisWeek { get; set; }

    // AI usage statistics
    public int TotalAIRequests { get; set; }
    public int AIRequestsToday { get; set; }
    public decimal EstimatedAICostToday { get; set; }
    public decimal EstimatedAICostThisMonth { get; set; }

    // System performance
    public double AverageResponseTime { get; set; }
    public double SystemUptime { get; set; }
    public int ActiveSessions { get; set; }

    // Popular features
    public List<AdminFeatureUsage> PopularFeatures { get; set; } = new();

    // Recent activity summary
    public List<AdminRecentActivity> RecentActivities { get; set; } = new();
}

/// <summary>
/// Detailed user information for admin management
/// </summary>
public class AdminUserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Name => $"{FirstName} {LastName}".Trim();
    public string UserName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool EmailVerified { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int LoginCount { get; set; }
    public int OrganizationCount { get; set; }
    public int BusinessPlanCount { get; set; }
    public int AIRequestCount { get; set; }
    public decimal EstimatedAICost { get; set; }
    public string? LastActiveIP { get; set; }
    public string? LastActiveLocation { get; set; }
    public bool IsActive => Status == UserStatus.Active;
}

/// <summary>
/// Organization information for admin management
/// </summary>
public class AdminOrganizationInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OrganizationType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int BusinessPlanCount { get; set; }
    public int CompletedBusinessPlanCount { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

/// <summary>
/// Business plan information for admin management
/// </summary>
public class AdminBusinessPlanInfo
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByEmail { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public int SectionCount { get; set; }
    public int CompletedSectionCount { get; set; }
    public int AIGeneratedSectionCount { get; set; }
    public int FinancialProjectionCount { get; set; }
    public decimal EstimatedAICost { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}

/// <summary>
/// System activity log for admin monitoring
/// </summary>
public class AdminActivityLog
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// System health information
/// </summary>
public class AdminSystemHealth
{
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    public string OverallStatus { get; set; } = "Healthy"; // Healthy, Warning, Critical
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public int ActiveConnections { get; set; }
    public double DatabaseResponseTime { get; set; }
    public double APIResponseTime { get; set; }
    public bool DatabaseHealthy { get; set; }
    public bool EmailServiceHealthy { get; set; }
    public bool AIServiceHealthy { get; set; }
    public List<AdminHealthAlert> Alerts { get; set; } = new();
}

/// <summary>
/// Feature usage statistics
/// </summary>
public class AdminFeatureUsage
{
    public string FeatureName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public int UniqueUsers { get; set; }
    public DateTime LastUsed { get; set; }
}

/// <summary>
/// Recent activity summary
/// </summary>
public class AdminRecentActivity
{
    public string Activity { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? EntityName { get; set; }
}

/// <summary>
/// Health alert information
/// </summary>
public class AdminHealthAlert
{
    public string Severity { get; set; } = string.Empty; // Info, Warning, Critical
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Component { get; set; }
}

/// <summary>
/// AI usage statistics
/// </summary>
public class AdminAIUsageStats
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageCostPerRequest { get; set; }
    public int TotalTokensUsed { get; set; }
    public List<AdminAIDailyUsage> DailyUsage { get; set; } = new();
    public List<AdminAIFeatureUsage> FeatureBreakdown { get; set; } = new();
}

/// <summary>
/// Daily AI usage breakdown
/// </summary>
public class AdminAIDailyUsage
{
    public DateTime Date { get; set; }
    public int Requests { get; set; }
    public decimal Cost { get; set; }
    public int Tokens { get; set; }
}

/// <summary>
/// AI feature usage breakdown
/// </summary>
public class AdminAIFeatureUsage
{
    public string Feature { get; set; } = string.Empty;
    public int Requests { get; set; }
    public decimal Cost { get; set; }
    public double AverageResponseTime { get; set; }
}

/// <summary>
/// Report generation result
/// </summary>
public class AdminReportResult
{
    public byte[] ReportData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public AdminReportType ReportType { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Business plan regeneration result
/// </summary>
public class AdminRegenerationResult
{
    public Guid BusinessPlanId { get; set; }
    public bool IsSuccess { get; set; }
    public List<string> RegeneratedSections { get; set; } = new();
    public List<string> FailedSections { get; set; } = new();
    public decimal EstimatedCost { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request models for admin operations
/// </summary>
public class AdminUserRequest : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public UserStatus? Status { get; set; }
    public string? UserType { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? LastLoginAfter { get; set; }
    public bool? EmailVerified { get; set; }
    public bool? TwoFactorEnabled { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class AdminOrganizationRequest : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public string? OrganizationType { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class AdminBusinessPlanRequest : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public string? PlanType { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? UserId { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class AdminActivityLogRequest : PaginationRequest
{
    public string? Action { get; set; }
    public string? Entity { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsSuccess { get; set; }
    public string? IPAddress { get; set; }
    public string SortBy { get; set; } = "Timestamp";
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// User status enumeration
/// </summary>
public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    Banned,
    PendingVerification
}

/// <summary>
/// Admin report types
/// </summary>
public enum AdminReportType
{
    UserActivity,
    OrganizationSummary,
    BusinessPlanAnalytics,
    AIUsageReport,
    SystemPerformance,
    FinancialOverview,
    SecurityAudit
}

/// <summary>
/// Base pagination request
/// </summary>
public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>
/// Comprehensive user detail for admin management
/// </summary>
public class AdminUserDetail
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public string? Persona { get; set; }
    public UserStatus Status { get; set; }
    public string Provider { get; set; } = "local";
    public bool EmailVerified { get; set; }
    public DateTime? EmailConfirmedAt { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberVerified { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool OnboardingCompleted { get; set; }
    public int? OnboardingStep { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public bool RequirePasswordChange { get; set; }
    public DateTime? PasswordLastChangedAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    // Statistics
    public int LoginCount { get; set; }
    public int SuccessfulLoginCount { get; set; }
    public int FailedLoginCount { get; set; }
    public int ActiveSessionCount { get; set; }
    public int OrganizationCount { get; set; }
    public int BusinessPlanCount { get; set; }

    // Roles
    public List<AdminUserRoleInfo> Roles { get; set; } = new();

    // Organizations
    public List<AdminUserOrganizationInfo> Organizations { get; set; } = new();
}

/// <summary>
/// Role info for user detail
/// </summary>
public class AdminUserRoleInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
}

/// <summary>
/// Organization info for user detail
/// </summary>
public class AdminUserOrganizationInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request to create user from admin panel
/// </summary>
public class AdminCreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string UserType { get; set; } = "Entrepreneur";
    public bool SendWelcomeEmail { get; set; } = true;
    public bool EmailVerified { get; set; }
    public List<Guid>? RoleIds { get; set; }
}

/// <summary>
/// Request to update user profile from admin panel
/// </summary>
public class AdminUpdateUserProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public string? UserType { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? EmailVerified { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Active session info for admin
/// </summary>
public class AdminUserSession
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public bool IsExpired { get; set; }
}

/// <summary>
/// Login history entry for admin
/// </summary>
public class AdminLoginHistoryEntry
{
    public Guid Id { get; set; }
    public DateTime LoginAttemptAt { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}

/// <summary>
/// Bulk action result
/// </summary>
public class AdminBulkActionResult
{
    public int TotalRequested { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}