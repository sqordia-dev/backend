namespace Sqordia.Application.Financial.Queries;

public class ScenarioAnalysisDto
{
    public string Scenario { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal NetIncome { get; set; }
    public decimal CashFlow { get; set; }
    public decimal ROI { get; set; }
    public decimal NPV { get; set; }
    public decimal IRR { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public string Assumptions { get; set; } = string.Empty;
    public List<ScenarioVariableDto> Variables { get; set; } = new();
}

public class ScenarioVariableDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
}
