namespace Sqordia.Application.Financial.Queries;

public class LocationOverheadEstimateDto
{
    public string City { get; set; } = null!;
    public string Province { get; set; } = null!;
    public decimal OverheadRate { get; set; }
    public decimal InsuranceRate { get; set; }
    public decimal TaxRate { get; set; }
    public decimal OfficeCost { get; set; }
    public string Currency { get; set; } = "CAD";
}
