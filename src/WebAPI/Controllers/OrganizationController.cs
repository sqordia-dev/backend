using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sqordia.Application.Services;
using Sqordia.Contracts.Requests.Organization;

namespace WebAPI.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/organizations")]
[Authorize]
public class OrganizationController : BaseApiController
{
    private readonly IOrganizationService _organizationService;

    public OrganizationController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    /// <summary>
    /// Create a new organization
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
    {
        var result = await _organizationService.CreateOrganizationAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all organizations the current user belongs to
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserOrganizations()
    {
        var result = await _organizationService.GetUserOrganizationsAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Get organization by ID
    /// </summary>
    [HttpGet("{organizationId:guid}")]
    public async Task<IActionResult> GetOrganization(Guid organizationId)
    {
        var result = await _organizationService.GetOrganizationAsync(organizationId);
        return HandleResult(result);
    }

    /// <summary>
    /// Get organization with all members
    /// </summary>
    [HttpGet("{organizationId:guid}/detail")]
    public async Task<IActionResult> GetOrganizationDetail(Guid organizationId)
    {
        var result = await _organizationService.GetOrganizationDetailAsync(organizationId);
        return HandleResult(result);
    }

    /// <summary>
    /// Update organization details
    /// </summary>
    [HttpPut("{organizationId:guid}")]
    public async Task<IActionResult> UpdateOrganization(Guid organizationId, [FromBody] UpdateOrganizationRequest request)
    {
        var result = await _organizationService.UpdateOrganizationAsync(organizationId, request);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete organization (soft delete)
    /// </summary>
    [HttpDelete("{organizationId:guid}")]
    public async Task<IActionResult> DeleteOrganization(Guid organizationId)
    {
        var result = await _organizationService.DeleteOrganizationAsync(organizationId);
        return HandleResult(result);
    }

    /// <summary>
    /// Deactivate organization
    /// </summary>
    [HttpPost("{organizationId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateOrganization(Guid organizationId)
    {
        var result = await _organizationService.DeactivateOrganizationAsync(organizationId);
        return HandleResult(result);
    }

    /// <summary>
    /// Reactivate organization
    /// </summary>
    [HttpPost("{organizationId:guid}/reactivate")]
    public async Task<IActionResult> ReactivateOrganization(Guid organizationId)
    {
        var result = await _organizationService.ReactivateOrganizationAsync(organizationId);
        return HandleResult(result);
    }

    /// <summary>
    /// Update organization settings
    /// </summary>
    [HttpPut("{organizationId:guid}/settings")]
    public async Task<IActionResult> UpdateOrganizationSettings(Guid organizationId, [FromBody] UpdateOrganizationSettingsRequest request)
    {
        var result = await _organizationService.UpdateOrganizationSettingsAsync(organizationId, request);
        return HandleResult(result);
    }

    /// <summary>
    /// Get all members of an organization
    /// </summary>
    [HttpGet("{organizationId:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid organizationId)
    {
        var result = await _organizationService.GetMembersAsync(organizationId);
        return HandleResult(result);
    }

    /// <summary>
    /// Add a member to the organization
    /// </summary>
    [HttpPost("{organizationId:guid}/members")]
    public async Task<IActionResult> AddMember(Guid organizationId, [FromBody] AddOrganizationMemberRequest request)
    {
        var result = await _organizationService.AddMemberAsync(organizationId, request);
        return HandleResult(result);
    }

    /// <summary>
    /// Update member role
    /// </summary>
    [HttpPut("{organizationId:guid}/members/{memberId:guid}/role")]
    public async Task<IActionResult> UpdateMemberRole(Guid organizationId, Guid memberId, [FromBody] UpdateMemberRoleRequest request)
    {
        var result = await _organizationService.UpdateMemberRoleAsync(organizationId, memberId, request);
        return HandleResult(result);
    }

    /// <summary>
    /// Remove a member from the organization
    /// </summary>
    [HttpDelete("{organizationId:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid organizationId, Guid memberId)
    {
        var result = await _organizationService.RemoveMemberAsync(organizationId, memberId);
        return HandleResult(result);
    }
}

