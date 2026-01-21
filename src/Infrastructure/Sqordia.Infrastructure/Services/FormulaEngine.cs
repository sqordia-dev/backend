using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Financial.Services;
using Sqordia.Domain.Entities;

namespace Sqordia.Infrastructure.Services;

/// <summary>
/// Spreadsheet-like formula engine that handles cell calculations with support for
/// common functions like SUM, AVG, MIN, MAX, IF, and basic arithmetic operations.
/// </summary>
public partial class FormulaEngine : IFormulaEngine
{
    private readonly ILogger<FormulaEngine> _logger;

    // Regex patterns for parsing formulas
    [GeneratedRegex(@"([A-Za-z_][A-Za-z0-9_]*!)?([A-Za-z_][A-Za-z0-9_]*(?:_[A-Za-z0-9_]+)*)")]
    private static partial Regex CellReferenceRegex();

    [GeneratedRegex(@"([A-Za-z_][A-Za-z0-9_]*!)?([A-Za-z_][A-Za-z0-9_]*):([A-Za-z_][A-Za-z0-9_]*)")]
    private static partial Regex RangeReferenceRegex();

    [GeneratedRegex(@"(SUM|AVG|AVERAGE|MIN|MAX|COUNT|ABS|ROUND)\s*\(([^)]+)\)", RegexOptions.IgnoreCase)]
    private static partial Regex FunctionRegex();

    [GeneratedRegex(@"IF\s*\(\s*([^,]+)\s*,\s*([^,]+)\s*,\s*([^)]+)\s*\)", RegexOptions.IgnoreCase)]
    private static partial Regex IfFunctionRegex();

    public FormulaEngine(ILogger<FormulaEngine> logger)
    {
        _logger = logger;
    }

    public decimal Evaluate(string formula, Dictionary<string, decimal> cellValues)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return 0m;

        // Remove leading '=' if present
        var expression = formula.TrimStart('=').Trim();

        try
        {
            // Process functions first (SUM, AVG, etc.)
            expression = ProcessFunctions(expression, cellValues);

            // Process IF functions
            expression = ProcessIfFunctions(expression, cellValues);

            // Replace cell references with values
            expression = ReplaceCellReferences(expression, cellValues);

            // Evaluate the arithmetic expression
            return EvaluateArithmetic(expression);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating formula: {Formula}", formula);
            throw new InvalidOperationException($"Failed to evaluate formula: {formula}", ex);
        }
    }

    public List<string> ParseDependencies(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return new List<string>();

        var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var expression = formula.TrimStart('=');

        // Find range references first (e.g., A1:A5)
        var rangeMatches = RangeReferenceRegex().Matches(expression);
        foreach (Match match in rangeMatches)
        {
            var range = match.Value;
            var expandedCells = ExpandRange(range);
            foreach (var cell in expandedCells)
            {
                dependencies.Add(cell);
            }
        }

        // Remove ranges from expression to avoid double-counting
        var expressionWithoutRanges = RangeReferenceRegex().Replace(expression, "");

        // Find individual cell references
        var cellMatches = CellReferenceRegex().Matches(expressionWithoutRanges);
        foreach (Match match in cellMatches)
        {
            var cellRef = match.Value;
            // Exclude function names
            if (!IsFunctionName(cellRef))
            {
                dependencies.Add(cellRef);
            }
        }

        return dependencies.ToList();
    }

    public List<string> ExpandRange(string range)
    {
        var cells = new List<string>();

        // Simple range expansion for named cells (row1:row5 style)
        // For now, we'll handle this as a list of cells between two indices
        var match = RangeReferenceRegex().Match(range);
        if (!match.Success)
        {
            return cells;
        }

        var sheetPrefix = match.Groups[1].Value;
        var startCell = match.Groups[2].Value;
        var endCell = match.Groups[3].Value;

        // Extract the common prefix and numeric suffix
        var startIndex = ExtractNumericSuffix(startCell);
        var endIndex = ExtractNumericSuffix(endCell);

        if (startIndex.HasValue && endIndex.HasValue)
        {
            var prefix = startCell.Substring(0, startCell.Length - startIndex.Value.ToString().Length);
            for (int i = startIndex.Value; i <= endIndex.Value; i++)
            {
                cells.Add($"{sheetPrefix}{prefix}{i}");
            }
        }
        else
        {
            // If we can't expand, treat the range as the start cell only
            cells.Add($"{sheetPrefix}{startCell}");
        }

        return cells;
    }

    public (bool IsValid, string? ErrorMessage) ValidateFormula(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return (true, null);

        var expression = formula.TrimStart('=').Trim();

        if (string.IsNullOrEmpty(expression))
            return (false, "Formula cannot be empty");

        // Check for balanced parentheses
        var parenCount = 0;
        foreach (var c in expression)
        {
            if (c == '(') parenCount++;
            if (c == ')') parenCount--;
            if (parenCount < 0)
                return (false, "Unmatched closing parenthesis");
        }
        if (parenCount != 0)
            return (false, "Unmatched opening parenthesis");

        // Check for invalid characters
        if (Regex.IsMatch(expression, @"[^A-Za-z0-9_!:+\-*/().,%\s<>=]"))
            return (false, "Formula contains invalid characters");

        return (true, null);
    }

    public bool WouldCreateCircularDependency(string targetCell, string formula, IEnumerable<FinancialCell> existingCells)
    {
        var dependencies = ParseDependencies(formula);

        // Direct self-reference
        if (dependencies.Contains(targetCell, StringComparer.OrdinalIgnoreCase))
            return true;

        // Build dependency graph
        var dependencyGraph = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        // Add existing dependencies
        foreach (var cell in existingCells.Where(c => !string.IsNullOrEmpty(c.Formula)))
        {
            var cellRef = cell.GetCellReference();
            var deps = ParseDependencies(cell.Formula!);
            dependencyGraph[cellRef] = new HashSet<string>(deps, StringComparer.OrdinalIgnoreCase);
        }

        // Add new formula dependencies
        dependencyGraph[targetCell] = new HashSet<string>(dependencies, StringComparer.OrdinalIgnoreCase);

        // Check for cycles using DFS
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var recursionStack = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return HasCycle(targetCell, dependencyGraph, visited, recursionStack);
    }

    public List<FinancialCell> GetCellsToRecalculate(string changedCellRef, IEnumerable<FinancialCell> allCells)
    {
        var cellsList = allCells.ToList();
        var dependents = new List<FinancialCell>();

        // Build reverse dependency graph (who depends on whom)
        var reverseDeps = new Dictionary<string, List<FinancialCell>>(StringComparer.OrdinalIgnoreCase);

        foreach (var cell in cellsList.Where(c => !string.IsNullOrEmpty(c.Formula)))
        {
            var deps = ParseDependencies(cell.Formula!);
            foreach (var dep in deps)
            {
                if (!reverseDeps.ContainsKey(dep))
                    reverseDeps[dep] = new List<FinancialCell>();
                reverseDeps[dep].Add(cell);
            }
        }

        // BFS to find all cells that need recalculation
        var queue = new Queue<string>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        queue.Enqueue(changedCellRef);
        visited.Add(changedCellRef);

        while (queue.Count > 0)
        {
            var currentRef = queue.Dequeue();
            if (reverseDeps.TryGetValue(currentRef, out var deps))
            {
                foreach (var depCell in deps)
                {
                    var depRef = depCell.GetCellReference();
                    if (!visited.Contains(depRef))
                    {
                        visited.Add(depRef);
                        dependents.Add(depCell);
                        queue.Enqueue(depRef);
                    }
                }
            }
        }

        // Topological sort to ensure correct calculation order
        return TopologicalSort(dependents);
    }

    public Dictionary<string, decimal> RecalculateDependents(FinancialCell changedCell, decimal newValue, IEnumerable<FinancialCell> allCells)
    {
        var results = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var cellsList = allCells.ToList();

        // Build value dictionary
        var cellValues = cellsList.ToDictionary(
            c => c.GetCellReference(),
            c => c.Value,
            StringComparer.OrdinalIgnoreCase);

        // Update the changed cell value
        var changedRef = changedCell.GetCellReference();
        cellValues[changedRef] = newValue;
        results[changedRef] = newValue;

        // Get cells to recalculate in order
        var cellsToRecalc = GetCellsToRecalculate(changedRef, cellsList);

        // Recalculate each cell
        foreach (var cell in cellsToRecalc)
        {
            if (!string.IsNullOrEmpty(cell.Formula))
            {
                var calculatedValue = Evaluate(cell.Formula, cellValues);
                var cellRef = cell.GetCellReference();
                cellValues[cellRef] = calculatedValue;
                results[cellRef] = calculatedValue;
            }
        }

        return results;
    }

    #region Private Helper Methods

    private string ProcessFunctions(string expression, Dictionary<string, decimal> cellValues)
    {
        var result = expression;
        var match = FunctionRegex().Match(result);

        while (match.Success)
        {
            var functionName = match.Groups[1].Value.ToUpperInvariant();
            var args = match.Groups[2].Value;
            var values = GetValuesFromArgs(args, cellValues);

            decimal calculatedValue = functionName switch
            {
                "SUM" => values.Sum(),
                "AVG" or "AVERAGE" => values.Any() ? values.Average() : 0m,
                "MIN" => values.Any() ? values.Min() : 0m,
                "MAX" => values.Any() ? values.Max() : 0m,
                "COUNT" => values.Count,
                "ABS" => values.Any() ? Math.Abs(values.First()) : 0m,
                "ROUND" => values.Count >= 2 ? Math.Round(values[0], (int)values[1]) : (values.Any() ? Math.Round(values[0]) : 0m),
                _ => 0m
            };

            result = result.Substring(0, match.Index) +
                     calculatedValue.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                     result.Substring(match.Index + match.Length);

            match = FunctionRegex().Match(result);
        }

        return result;
    }

    private string ProcessIfFunctions(string expression, Dictionary<string, decimal> cellValues)
    {
        var result = expression;
        var match = IfFunctionRegex().Match(result);

        while (match.Success)
        {
            var condition = match.Groups[1].Value.Trim();
            var trueValue = match.Groups[2].Value.Trim();
            var falseValue = match.Groups[3].Value.Trim();

            var conditionResult = EvaluateCondition(condition, cellValues);
            var selectedValue = conditionResult ? trueValue : falseValue;

            result = result.Substring(0, match.Index) +
                     selectedValue +
                     result.Substring(match.Index + match.Length);

            match = IfFunctionRegex().Match(result);
        }

        return result;
    }

    private bool EvaluateCondition(string condition, Dictionary<string, decimal> cellValues)
    {
        // Replace cell references first
        var processedCondition = ReplaceCellReferences(condition, cellValues);

        // Parse comparison operators
        var comparisonMatch = Regex.Match(processedCondition, @"([^<>=!]+)(<=|>=|<>|!=|<|>|=)(.+)");
        if (!comparisonMatch.Success)
            return false;

        var left = EvaluateArithmetic(comparisonMatch.Groups[1].Value.Trim());
        var op = comparisonMatch.Groups[2].Value;
        var right = EvaluateArithmetic(comparisonMatch.Groups[3].Value.Trim());

        return op switch
        {
            "<" => left < right,
            ">" => left > right,
            "<=" => left <= right,
            ">=" => left >= right,
            "=" => left == right,
            "<>" or "!=" => left != right,
            _ => false
        };
    }

    private List<decimal> GetValuesFromArgs(string args, Dictionary<string, decimal> cellValues)
    {
        var values = new List<decimal>();

        // Split by comma, but respect nested functions
        var parts = SplitArguments(args);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();

            // Check if it's a range
            if (RangeReferenceRegex().IsMatch(trimmed))
            {
                var expandedCells = ExpandRange(trimmed);
                foreach (var cellRef in expandedCells)
                {
                    if (cellValues.TryGetValue(cellRef, out var value))
                        values.Add(value);
                }
            }
            // Check if it's a cell reference
            else if (cellValues.TryGetValue(trimmed, out var value))
            {
                values.Add(value);
            }
            // Try to parse as number
            else if (decimal.TryParse(trimmed, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var numValue))
            {
                values.Add(numValue);
            }
        }

        return values;
    }

    private string ReplaceCellReferences(string expression, Dictionary<string, decimal> cellValues)
    {
        var result = expression;

        foreach (var kvp in cellValues.OrderByDescending(k => k.Key.Length))
        {
            result = Regex.Replace(result, Regex.Escape(kvp.Key),
                kvp.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                RegexOptions.IgnoreCase);
        }

        return result;
    }

    private decimal EvaluateArithmetic(string expression)
    {
        // Simple arithmetic evaluator using System.Data.DataTable.Compute
        // This handles +, -, *, /, parentheses
        try
        {
            var sanitized = expression.Replace(" ", "");
            var table = new System.Data.DataTable();
            var result = table.Compute(sanitized, "");
            return Convert.ToDecimal(result, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return 0m;
        }
    }

    private static bool IsFunctionName(string text)
    {
        var functions = new[] { "SUM", "AVG", "AVERAGE", "MIN", "MAX", "COUNT", "ABS", "ROUND", "IF" };
        return functions.Contains(text.ToUpperInvariant());
    }

    private static int? ExtractNumericSuffix(string text)
    {
        var match = Regex.Match(text, @"(\d+)$");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var num))
            return num;
        return null;
    }

    private static List<string> SplitArguments(string args)
    {
        var result = new List<string>();
        var depth = 0;
        var current = "";

        foreach (var c in args)
        {
            if (c == '(') depth++;
            else if (c == ')') depth--;
            else if (c == ',' && depth == 0)
            {
                result.Add(current);
                current = "";
                continue;
            }
            current += c;
        }

        if (!string.IsNullOrEmpty(current))
            result.Add(current);

        return result;
    }

    private static bool HasCycle(string node, Dictionary<string, HashSet<string>> graph,
        HashSet<string> visited, HashSet<string> recursionStack)
    {
        if (recursionStack.Contains(node))
            return true;

        if (visited.Contains(node))
            return false;

        visited.Add(node);
        recursionStack.Add(node);

        if (graph.TryGetValue(node, out var neighbors))
        {
            foreach (var neighbor in neighbors)
            {
                if (HasCycle(neighbor, graph, visited, recursionStack))
                    return true;
            }
        }

        recursionStack.Remove(node);
        return false;
    }

    private List<FinancialCell> TopologicalSort(List<FinancialCell> cells)
    {
        if (cells.Count == 0)
            return cells;

        // Build dependency graph
        var graph = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var inDegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var cellMap = new Dictionary<string, FinancialCell>(StringComparer.OrdinalIgnoreCase);

        foreach (var cell in cells)
        {
            var cellRef = cell.GetCellReference();
            cellMap[cellRef] = cell;
            graph[cellRef] = new List<string>();
            inDegree[cellRef] = 0;
        }

        // Build edges
        foreach (var cell in cells)
        {
            if (!string.IsNullOrEmpty(cell.Formula))
            {
                var cellRef = cell.GetCellReference();
                var deps = ParseDependencies(cell.Formula);
                foreach (var dep in deps)
                {
                    if (graph.ContainsKey(dep))
                    {
                        graph[dep].Add(cellRef);
                        inDegree[cellRef]++;
                    }
                }
            }
        }

        // Kahn's algorithm
        var queue = new Queue<string>();
        foreach (var kvp in inDegree.Where(kvp => kvp.Value == 0))
        {
            queue.Enqueue(kvp.Key);
        }

        var sorted = new List<FinancialCell>();
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            sorted.Add(cellMap[current]);

            foreach (var neighbor in graph[current])
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                    queue.Enqueue(neighbor);
            }
        }

        return sorted;
    }

    #endregion
}
