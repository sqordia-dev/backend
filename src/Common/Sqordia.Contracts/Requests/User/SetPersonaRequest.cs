using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.User;

public class SetPersonaRequest
{
    [Required]
    public string Persona { get; set; } = null!;
}
