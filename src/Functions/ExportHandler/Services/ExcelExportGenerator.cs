using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Logging;
using Sqordia.Functions.ExportHandler.Models;

namespace Sqordia.Functions.ExportHandler.Services;

/// <summary>
/// Excel export generator using OpenXML
/// </summary>
public class ExcelExportGenerator : IExportGenerator
{
    private readonly ILogger<ExcelExportGenerator> _logger;

    public string ExportType => "excel";
    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string FileExtension => ".xlsx";

    public ExcelExportGenerator(ILogger<ExcelExportGenerator> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> GenerateAsync(BusinessPlanExportData data, string language, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Excel document for business plan {PlanId}", data.Id);

        using var memoryStream = new MemoryStream();
        using (var spreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = spreadsheetDocument.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());

            // Create Summary sheet
            var summarySheetPart = workbookPart.AddNewPart<WorksheetPart>();
            summarySheetPart.Worksheet = new Worksheet(new SheetData());
            var summarySheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(summarySheetPart),
                SheetId = 1,
                Name = "Summary"
            };
            sheets.Append(summarySheet);
            PopulateSummarySheet(summarySheetPart.Worksheet.GetFirstChild<SheetData>()!, data);

            // Create Content sheet
            var contentSheetPart = workbookPart.AddNewPart<WorksheetPart>();
            contentSheetPart.Worksheet = new Worksheet(new SheetData());
            var contentSheet = new Sheet
            {
                Id = workbookPart.GetIdOfPart(contentSheetPart),
                SheetId = 2,
                Name = "Content"
            };
            sheets.Append(contentSheet);
            PopulateContentSheet(contentSheetPart.Worksheet.GetFirstChild<SheetData>()!, data);

            workbookPart.Workbook.Save();
        }

        var bytes = memoryStream.ToArray();
        _logger.LogInformation("Excel document generated successfully, size: {Size} bytes", bytes.Length);

        return Task.FromResult(bytes);
    }

    private void PopulateSummarySheet(SheetData sheetData, BusinessPlanExportData data)
    {
        uint rowIndex = 1;

        // Title
        AddRow(sheetData, rowIndex++, "Business Plan Summary");
        AddRow(sheetData, rowIndex++, "");

        // Document info
        AddRow(sheetData, rowIndex++, "Property", "Value");
        AddRow(sheetData, rowIndex++, "Title", data.Title);
        AddRow(sheetData, rowIndex++, "Organization", data.OrganizationName);
        AddRow(sheetData, rowIndex++, "Plan Type", data.PlanType);
        AddRow(sheetData, rowIndex++, "Status", data.Status);
        AddRow(sheetData, rowIndex++, "Version", data.Version.ToString());
        AddRow(sheetData, rowIndex++, "Created", data.CreatedAt.ToString("yyyy-MM-dd"));
        if (data.FinalizedAt.HasValue)
        {
            AddRow(sheetData, rowIndex++, "Finalized", data.FinalizedAt.Value.ToString("yyyy-MM-dd"));
        }
        if (!string.IsNullOrWhiteSpace(data.Description))
        {
            AddRow(sheetData, rowIndex++, "Description", data.Description);
        }

        AddRow(sheetData, rowIndex++, "");
        AddRow(sheetData, rowIndex++, "Sections Overview");
        AddRow(sheetData, rowIndex++, "Section", "Has Content");

        foreach (var (title, _) in data.GetSections())
        {
            AddRow(sheetData, rowIndex++, title, "Yes");
        }
    }

    private void PopulateContentSheet(SheetData sheetData, BusinessPlanExportData data)
    {
        uint rowIndex = 1;

        // Header
        AddRow(sheetData, rowIndex++, "Section", "Content");
        AddRow(sheetData, rowIndex++, "");

        // Content sections
        foreach (var (title, content) in data.GetSections())
        {
            AddRow(sheetData, rowIndex++, title, content);
            AddRow(sheetData, rowIndex++, ""); // Spacing row
        }
    }

    private void AddRow(SheetData sheetData, uint rowIndex, params string[] values)
    {
        var row = new Row { RowIndex = rowIndex };

        for (int i = 0; i < values.Length; i++)
        {
            var cell = new Cell
            {
                CellReference = GetCellReference(i, rowIndex),
                DataType = CellValues.String,
                CellValue = new CellValue(values[i])
            };
            row.Append(cell);
        }

        sheetData.Append(row);
    }

    private string GetCellReference(int columnIndex, uint rowIndex)
    {
        string columnLetter = "";
        int column = columnIndex;

        while (column >= 0)
        {
            columnLetter = (char)('A' + column % 26) + columnLetter;
            column = column / 26 - 1;
        }

        return columnLetter + rowIndex;
    }
}
