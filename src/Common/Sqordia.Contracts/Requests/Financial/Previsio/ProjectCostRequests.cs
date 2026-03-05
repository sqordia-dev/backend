namespace Sqordia.Contracts.Requests.Financial.Previsio;

public class UpdateProjectCostSettingsRequest
{
    public int WorkingCapitalMonthsCOGS { get; set; }
    public int WorkingCapitalMonthsPayroll { get; set; }
    public int WorkingCapitalMonthsSalesExpenses { get; set; }
    public int WorkingCapitalMonthsAdminExpenses { get; set; }
    public int CapexInclusionMonths { get; set; }
}
