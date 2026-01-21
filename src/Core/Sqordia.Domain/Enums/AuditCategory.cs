namespace Sqordia.Domain.Enums;

/// <summary>
/// Category for Socratic Coach audit sections
/// </summary>
public enum AuditCategory
{
    /// <summary>
    /// Financial audits - revenue, costs, projections, cash flow
    /// </summary>
    Financial = 0,

    /// <summary>
    /// Strategic audits - market positioning, competitive advantage, growth strategy
    /// </summary>
    Strategic = 1,

    /// <summary>
    /// Legal audits - business structure, contracts, intellectual property
    /// </summary>
    Legal = 2,

    /// <summary>
    /// Compliance audits - regulatory requirements, Quebec Bill 96, industry standards
    /// </summary>
    Compliance = 3
}
