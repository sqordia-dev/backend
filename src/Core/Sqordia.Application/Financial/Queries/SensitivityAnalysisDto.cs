namespace Sqordia.Application.Financial.Queries;

public class SensitivityAnalysisDto
{
    public string Variable { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BaseValue { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public decimal Step { get; set; }
    public List<SensitivityPointDto> Points { get; set; } = new();
    public string Currency { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
}

public class SensitivityPointDto
{
    public decimal VariableValue { get; set; }
    public decimal NPV { get; set; }
    public decimal IRR { get; set; }
    public decimal ROI { get; set; }
    public decimal PaybackPeriod { get; set; }
}
