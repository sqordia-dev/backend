using System.ComponentModel.DataAnnotations;

namespace Sqordia.Contracts.Requests.Financial;

public class CalculateConsultantFinancialsRequest
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Hourly rate must be positive")]
    public decimal HourlyRate { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Utilization percentage must be between 0 and 100")]
    public decimal UtilizationPercent { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Client acquisition cost must be positive")]
    public decimal ClientAcquisitionCost { get; set; }

    [Required]
    public string City { get; set; } = null!;

    [Required]
    public string Province { get; set; } = null!;
}
