namespace Sqordia.Application.Financial.Engine;

/// <summary>
/// Bilingual labels for financial statement line items.
/// </summary>
public static class StatementLabels
{
    public static Labels Get(string language) =>
        language?.ToLowerInvariant() == "en" ? English : French;

    public sealed record Labels
    {
        // P&L
        public string GrossProfit { get; init; } = "";
        public string Depreciation { get; init; } = "";
        public string TotalOperatingExpenses { get; init; } = "";
        public string EBIT { get; init; } = "";
        public string InterestExpense { get; init; } = "";
        public string NetIncome { get; init; } = "";

        // Cash Flow
        public string SalesCollected { get; init; } = "";
        public string FinancingReceived { get; init; } = "";
        public string COGSCash { get; init; } = "";
        public string PayrollCash { get; init; } = "";
        public string SalesExpensesCash { get; init; } = "";
        public string AdminExpensesCash { get; init; } = "";
        public string CapexCash { get; init; } = "";
        public string LoanRepayment { get; init; } = "";
        public string Interest { get; init; } = "";
        public string NetCashFlow { get; init; } = "";
        public string CumulativeCash { get; init; } = "";

        // Balance Sheet
        public string Cash { get; init; } = "";
        public string AccountsReceivable { get; init; } = "";
        public string Inventory { get; init; } = "";
        public string FixedAssetsNet { get; init; } = "";
        public string TotalAssets { get; init; } = "";
        public string LoansPayable { get; init; } = "";
        public string TotalLiabilities { get; init; } = "";
        public string InvestedCapital { get; init; } = "";
        public string RetainedEarnings { get; init; } = "";
        public string TotalEquity { get; init; } = "";
        public string TotalLiabilitiesAndEquity { get; init; } = "";
    }

    public static readonly Labels French = new()
    {
        GrossProfit = "Bénéfice brut",
        Depreciation = "Amortissement",
        TotalOperatingExpenses = "Total des charges",
        EBIT = "BAII",
        InterestExpense = "Intérêts",
        NetIncome = "Bénéfice net",

        SalesCollected = "Ventes encaissées",
        FinancingReceived = "Financement reçu",
        COGSCash = "Coût des marchandises",
        PayrollCash = "Masse salariale",
        SalesExpensesCash = "Frais de vente",
        AdminExpensesCash = "Frais admin.",
        CapexCash = "Immobilisations",
        LoanRepayment = "Remboursement prêts",
        Interest = "Intérêts",
        NetCashFlow = "Flux net",
        CumulativeCash = "Encaisse cumulative",

        Cash = "Encaisse",
        AccountsReceivable = "Comptes clients",
        Inventory = "Stocks",
        FixedAssetsNet = "Immobilisations nettes",
        TotalAssets = "Total actif",
        LoansPayable = "Emprunts à payer",
        TotalLiabilities = "Total passif",
        InvestedCapital = "Capital investi",
        RetainedEarnings = "Bénéfices non répartis",
        TotalEquity = "Total capitaux propres",
        TotalLiabilitiesAndEquity = "Total passif et capitaux propres"
    };

    public static readonly Labels English = new()
    {
        GrossProfit = "Gross Profit",
        Depreciation = "Depreciation",
        TotalOperatingExpenses = "Total Operating Expenses",
        EBIT = "EBIT",
        InterestExpense = "Interest Expense",
        NetIncome = "Net Income",

        SalesCollected = "Sales Collected",
        FinancingReceived = "Financing Received",
        COGSCash = "Cost of Goods Sold",
        PayrollCash = "Payroll",
        SalesExpensesCash = "Sales Expenses",
        AdminExpensesCash = "Admin Expenses",
        CapexCash = "Capital Expenditures",
        LoanRepayment = "Loan Repayment",
        Interest = "Interest",
        NetCashFlow = "Net Cash Flow",
        CumulativeCash = "Cumulative Cash",

        Cash = "Cash",
        AccountsReceivable = "Accounts Receivable",
        Inventory = "Inventory",
        FixedAssetsNet = "Net Fixed Assets",
        TotalAssets = "Total Assets",
        LoansPayable = "Loans Payable",
        TotalLiabilities = "Total Liabilities",
        InvestedCapital = "Invested Capital",
        RetainedEarnings = "Retained Earnings",
        TotalEquity = "Total Equity",
        TotalLiabilitiesAndEquity = "Total Liabilities & Equity"
    };
}
