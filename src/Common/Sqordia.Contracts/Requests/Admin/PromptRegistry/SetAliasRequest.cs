using System.ComponentModel.DataAnnotations;
using Sqordia.Domain.Enums;

namespace Sqordia.Contracts.Requests.Admin.PromptRegistry;

/// <summary>
/// Request to set a deployment alias for a prompt template
/// </summary>
public class SetAliasRequest
{
    /// <summary>
    /// The deployment alias to set (Production, Staging, Development, Experimental)
    /// Set to null to remove the alias
    /// </summary>
    public PromptAlias? Alias { get; set; }
}
