using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.AI;

public class AnalyzeSectionRequest
{
    [Required]
    public string SectionName { get; set; } = null!;

    public string? Persona { get; set; }

    public LocationInfo? Location { get; set; }

    public string Language { get; set; } = "fr";
}

public class LocationInfo
{
    public string City { get; set; } = null!;
    public string Province { get; set; } = null!;
}
