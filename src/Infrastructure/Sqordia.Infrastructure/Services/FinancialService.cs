using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Financial.Commands;
using Sqordia.Application.Financial.Queries;
using Sqordia.Application.Financial.Services;
using Sqordia.Contracts.Requests.Financial;
using Sqordia.Domain.Enums;

namespace Sqordia.Infrastructure.Services;

public class FinancialService : IFinancialService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FinancialService> _logger;
    private readonly IFormulaEngine _formulaEngine;

    public FinancialService(
        IApplicationDbContext context,
        ILogger<FinancialService> logger,
        IFormulaEngine formulaEngine)
    {
        _context = context;
        _logger = logger;
        _formulaEngine = formulaEngine;
    }

    // Financial Projection CRUD Operations
    public async Task<Result<FinancialProjectionDto>> CreateFinancialProjectionAsync(CreateFinancialProjectionCommand command)
    {
        try
        {
            _logger.LogInformation("Creating financial projection for business plan {BusinessPlanId}", command.BusinessPlanId);

            var projection = new Domain.Entities.FinancialProjectionItem
            {
                BusinessPlanId = command.BusinessPlanId,
                Name = command.Name,
                Description = command.Description,
                ProjectionType = command.ProjectionType,
                Scenario = command.Scenario.ToString(),
                Year = command.Year,
                Month = command.Month,
                Amount = command.Amount,
                CurrencyCode = command.CurrencyCode,
                BaseAmount = command.Amount,
                Category = command.Category.ToString(),
                SubCategory = command.SubCategory,
                IsRecurring = command.IsRecurring,
                Frequency = command.Frequency,
                GrowthRate = command.GrowthRate,
                Assumptions = command.Assumptions,
                Notes = command.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FinancialProjectionItems.Add(projection);
            await _context.SaveChangesAsync();

            return Result<FinancialProjectionDto>.Success(MapToDto(projection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating financial projection");
            return Result.Failure<FinancialProjectionDto>(
                Error.InternalServerError("Financial.CreateFailed", "Failed to create financial projection"));
        }
    }

    public async Task<Result<FinancialProjectionDto>> GetFinancialProjectionByIdAsync(Guid id)
    {
        try
        {
            var projection = await _context.FinancialProjectionItems
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projection == null)
            {
                return Result.Failure<FinancialProjectionDto>(
                    Error.NotFound("Financial.NotFound", $"Financial projection with ID {id} not found"));
            }

            return Result<FinancialProjectionDto>.Success(MapToDto(projection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial projection {Id}", id);
            return Result.Failure<FinancialProjectionDto>(
                Error.InternalServerError("Financial.GetFailed", "Failed to retrieve financial projection"));
        }
    }

    public async Task<Result<List<FinancialProjectionDto>>> GetFinancialProjectionsByBusinessPlanAsync(Guid businessPlanId)
    {
        try
        {
            var projections = await _context.FinancialProjectionItems
                .Where(p => p.BusinessPlanId == businessPlanId)
                .OrderBy(p => p.Year)
                .ThenBy(p => p.Month)
                .ToListAsync();

            return Result<List<FinancialProjectionDto>>.Success(projections.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial projections for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<List<FinancialProjectionDto>>(
                Error.InternalServerError("Financial.GetFailed", "Failed to retrieve financial projections"));
        }
    }

    public async Task<Result<List<FinancialProjectionDto>>> GetFinancialProjectionsByScenarioAsync(Guid businessPlanId, ScenarioType scenario)
    {
        try
        {
            var scenarioName = scenario.ToString();
            var projections = await _context.FinancialProjectionItems
                .Where(p => p.BusinessPlanId == businessPlanId && p.Scenario == scenarioName)
                .OrderBy(p => p.Year)
                .ThenBy(p => p.Month)
                .ToListAsync();

            return Result<List<FinancialProjectionDto>>.Success(projections.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial projections for scenario {Scenario}", scenario);
            return Result.Failure<List<FinancialProjectionDto>>(
                Error.InternalServerError("Financial.GetFailed", "Failed to retrieve financial projections"));
        }
    }

    public async Task<Result<FinancialProjectionDto>> UpdateFinancialProjectionAsync(UpdateFinancialProjectionCommand command)
    {
        try
        {
            var projection = await _context.FinancialProjectionItems
                .FirstOrDefaultAsync(p => p.Id == command.Id);

            if (projection == null)
            {
                return Result.Failure<FinancialProjectionDto>(
                    Error.NotFound("Financial.NotFound", $"Financial projection with ID {command.Id} not found"));
            }

            projection.Name = command.Name;
            projection.Description = command.Description;
            projection.ProjectionType = command.ProjectionType;
            projection.Scenario = command.Scenario.ToString();
            projection.Year = command.Year;
            projection.Month = command.Month;
            projection.Amount = command.Amount;
            projection.CurrencyCode = command.CurrencyCode;
            projection.BaseAmount = command.Amount;
            projection.Category = command.Category.ToString();
            projection.SubCategory = command.SubCategory;
            projection.IsRecurring = command.IsRecurring;
            projection.Frequency = command.Frequency;
            projection.GrowthRate = command.GrowthRate;
            projection.Assumptions = command.Assumptions;
            projection.Notes = command.Notes;
            projection.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<FinancialProjectionDto>.Success(MapToDto(projection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating financial projection {Id}", command.Id);
            return Result.Failure<FinancialProjectionDto>(
                Error.InternalServerError("Financial.UpdateFailed", "Failed to update financial projection"));
        }
    }

    public async Task<Result<bool>> DeleteFinancialProjectionAsync(Guid id)
    {
        try
        {
            var projection = await _context.FinancialProjectionItems
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projection == null)
            {
                return Result.Failure<bool>(
                    Error.NotFound("Financial.NotFound", $"Financial projection with ID {id} not found"));
            }

            _context.FinancialProjectionItems.Remove(projection);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting financial projection {Id}", id);
            return Result.Failure<bool>(
                Error.InternalServerError("Financial.DeleteFailed", "Failed to delete financial projection"));
        }
    }

    private static FinancialProjectionDto MapToDto(Domain.Entities.FinancialProjectionItem item)
    {
        return new FinancialProjectionDto
        {
            Id = item.Id,
            BusinessPlanId = item.BusinessPlanId,
            Name = item.Name,
            Description = item.Description,
            ProjectionType = item.ProjectionType,
            Scenario = item.Scenario,
            Year = item.Year,
            Month = item.Month,
            Amount = item.Amount,
            CurrencyCode = item.CurrencyCode,
            BaseAmount = item.BaseAmount,
            Category = item.Category,
            SubCategory = item.SubCategory,
            IsRecurring = item.IsRecurring,
            Frequency = item.Frequency,
            GrowthRate = item.GrowthRate,
            Assumptions = item.Assumptions,
            Notes = item.Notes,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }

    // Currency Operations
    public async Task<Result<CurrencyDto>> GetCurrencyAsync(string currencyCode)
    {
        try
        {
            var currency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Code == currencyCode);

            if (currency == null)
            {
                // Return default currency info for common currencies
                var defaultCurrency = GetDefaultCurrency(currencyCode);
                if (defaultCurrency != null)
                {
                    return Result<CurrencyDto>.Success(defaultCurrency);
                }

                return Result.Failure<CurrencyDto>(
                    Error.NotFound("Currency.NotFound", $"Currency with code {currencyCode} not found"));
            }

            return Result<CurrencyDto>.Success(new CurrencyDto
            {
                Id = currency.Id,
                Code = currency.Code,
                Name = currency.Name,
                Symbol = currency.Symbol,
                Country = currency.Country,
                Region = currency.Region,
                IsActive = currency.IsActive,
                DecimalPlaces = currency.DecimalPlaces,
                ExchangeRate = currency.ExchangeRate,
                LastUpdated = currency.LastUpdated,
                Source = currency.Source
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving currency {CurrencyCode}", currencyCode);
            return Result.Failure<CurrencyDto>(
                Error.InternalServerError("Currency.GetFailed", "Failed to retrieve currency"));
        }
    }

    public async Task<Result<List<CurrencyDto>>> GetAllCurrenciesAsync()
    {
        try
        {
            var currencies = await _context.Currencies
                .Where(c => c.IsActive)
                .OrderBy(c => c.Code)
                .ToListAsync();

            if (!currencies.Any())
            {
                // Return default currencies if none in database
                return Result<List<CurrencyDto>>.Success(GetDefaultCurrencies());
            }

            var dtos = currencies.Select(c => new CurrencyDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Symbol = c.Symbol,
                Country = c.Country,
                Region = c.Region,
                IsActive = c.IsActive,
                DecimalPlaces = c.DecimalPlaces,
                ExchangeRate = c.ExchangeRate,
                LastUpdated = c.LastUpdated,
                Source = c.Source
            }).ToList();

            return Result<List<CurrencyDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all currencies");
            return Result.Failure<List<CurrencyDto>>(
                Error.InternalServerError("Currency.GetAllFailed", "Failed to retrieve currencies"));
        }
    }

    public async Task<Result<decimal>> ConvertCurrencyAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        try
        {
            if (fromCurrency == toCurrency)
            {
                return Result<decimal>.Success(amount);
            }

            var rateResult = await GetExchangeRateAsync(fromCurrency, toCurrency);
            if (!rateResult.IsSuccess)
            {
                return Result.Failure<decimal>(rateResult.Error!);
            }

            var convertedAmount = amount * rateResult.Value!.Rate;
            return Result<decimal>.Success(Math.Round(convertedAmount, 2));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting {Amount} from {FromCurrency} to {ToCurrency}", amount, fromCurrency, toCurrency);
            return Result.Failure<decimal>(
                Error.InternalServerError("Currency.ConversionFailed", "Failed to convert currency"));
        }
    }

    public async Task<Result<ExchangeRateDto>> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        try
        {
            if (fromCurrency == toCurrency)
            {
                return Result<ExchangeRateDto>.Success(new ExchangeRateDto
                {
                    Id = Guid.Empty,
                    FromCurrency = fromCurrency,
                    ToCurrency = toCurrency,
                    Rate = 1.0m,
                    InverseRate = 1.0m,
                    EffectiveDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(1),
                    Source = "System",
                    IsActive = true,
                    Provider = "Internal",
                    Spread = 0
                });
            }

            var exchangeRate = await _context.ExchangeRates
                .Where(r => r.FromCurrency.Code == fromCurrency && r.ToCurrency.Code == toCurrency && r.IsActive)
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefaultAsync();

            if (exchangeRate == null)
            {
                // Try inverse rate
                var inverseRate = await _context.ExchangeRates
                    .Where(r => r.FromCurrency.Code == toCurrency && r.ToCurrency.Code == fromCurrency && r.IsActive)
                    .OrderByDescending(r => r.EffectiveDate)
                    .FirstOrDefaultAsync();

                if (inverseRate != null)
                {
                    return Result<ExchangeRateDto>.Success(new ExchangeRateDto
                    {
                        Id = inverseRate.Id,
                        FromCurrency = fromCurrency,
                        ToCurrency = toCurrency,
                        Rate = 1 / inverseRate.Rate,
                        InverseRate = inverseRate.Rate,
                        EffectiveDate = inverseRate.EffectiveDate,
                        ExpiryDate = inverseRate.ExpiryDate,
                        Source = inverseRate.Source,
                        IsActive = inverseRate.IsActive,
                        Provider = inverseRate.Provider,
                        Spread = inverseRate.Spread
                    });
                }

                // Return default rate based on common currency pairs
                var defaultRate = GetDefaultExchangeRate(fromCurrency, toCurrency);
                if (defaultRate != null)
                {
                    return Result<ExchangeRateDto>.Success(defaultRate);
                }

                return Result.Failure<ExchangeRateDto>(
                    Error.NotFound("ExchangeRate.NotFound", $"Exchange rate from {fromCurrency} to {toCurrency} not found"));
            }

            return Result<ExchangeRateDto>.Success(new ExchangeRateDto
            {
                Id = exchangeRate.Id,
                FromCurrency = exchangeRate.FromCurrency?.Code ?? fromCurrency,
                ToCurrency = exchangeRate.ToCurrency?.Code ?? toCurrency,
                Rate = exchangeRate.Rate,
                InverseRate = 1 / exchangeRate.Rate,
                EffectiveDate = exchangeRate.EffectiveDate,
                ExpiryDate = exchangeRate.ExpiryDate,
                Source = exchangeRate.Source,
                IsActive = exchangeRate.IsActive,
                Provider = exchangeRate.Provider,
                Spread = exchangeRate.Spread
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exchange rate from {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            return Result.Failure<ExchangeRateDto>(
                Error.InternalServerError("ExchangeRate.GetFailed", "Failed to retrieve exchange rate"));
        }
    }

    private static CurrencyDto? GetDefaultCurrency(string code)
    {
        var defaults = new Dictionary<string, CurrencyDto>(StringComparer.OrdinalIgnoreCase)
        {
            ["CAD"] = new() { Code = "CAD", Name = "Canadian Dollar", Symbol = "$", Country = "Canada", DecimalPlaces = 2, ExchangeRate = 1.0m, IsActive = true },
            ["USD"] = new() { Code = "USD", Name = "US Dollar", Symbol = "$", Country = "United States", DecimalPlaces = 2, ExchangeRate = 0.74m, IsActive = true },
            ["EUR"] = new() { Code = "EUR", Name = "Euro", Symbol = "€", Country = "European Union", DecimalPlaces = 2, ExchangeRate = 0.68m, IsActive = true },
            ["GBP"] = new() { Code = "GBP", Name = "British Pound", Symbol = "£", Country = "United Kingdom", DecimalPlaces = 2, ExchangeRate = 0.58m, IsActive = true }
        };

        return defaults.TryGetValue(code, out var currency) ? currency : null;
    }

    private static List<CurrencyDto> GetDefaultCurrencies()
    {
        return new List<CurrencyDto>
        {
            new() { Code = "CAD", Name = "Canadian Dollar", Symbol = "$", Country = "Canada", DecimalPlaces = 2, ExchangeRate = 1.0m, IsActive = true },
            new() { Code = "USD", Name = "US Dollar", Symbol = "$", Country = "United States", DecimalPlaces = 2, ExchangeRate = 0.74m, IsActive = true },
            new() { Code = "EUR", Name = "Euro", Symbol = "€", Country = "European Union", DecimalPlaces = 2, ExchangeRate = 0.68m, IsActive = true },
            new() { Code = "GBP", Name = "British Pound", Symbol = "£", Country = "United Kingdom", DecimalPlaces = 2, ExchangeRate = 0.58m, IsActive = true }
        };
    }

    private static ExchangeRateDto? GetDefaultExchangeRate(string from, string to)
    {
        // Default rates relative to CAD (approximate)
        var ratesToCad = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["CAD"] = 1.0m,
            ["USD"] = 1.35m,
            ["EUR"] = 1.47m,
            ["GBP"] = 1.72m
        };

        if (ratesToCad.TryGetValue(from, out var fromRate) && ratesToCad.TryGetValue(to, out var toRate))
        {
            var rate = fromRate / toRate;
            return new ExchangeRateDto
            {
                FromCurrency = from,
                ToCurrency = to,
                Rate = rate,
                InverseRate = 1 / rate,
                EffectiveDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                Source = "Default",
                IsActive = true,
                Provider = "System",
                Spread = 0
            };
        }

        return null;
    }

    // Tax Operations
    public async Task<Result<TaxCalculationDto>> CalculateTaxAsync(TaxCalculationRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating tax for {TaxType} in {Country}/{Region}", request.TaxType, request.Country, request.Region);

            // Get applicable tax rules
            var taxRulesResult = await GetTaxRulesAsync(request.Country, request.Region);
            if (!taxRulesResult.IsSuccess)
            {
                return Result.Failure<TaxCalculationDto>(taxRulesResult.Error!);
            }

            var applicableRule = taxRulesResult.Value!
                .FirstOrDefault(r => r.TaxType.Equals(request.TaxType, StringComparison.OrdinalIgnoreCase) && r.IsActive);

            decimal taxRate;
            string taxName;
            string calculationMethod;

            if (applicableRule != null)
            {
                taxRate = applicableRule.Rate;
                taxName = applicableRule.Name;
                calculationMethod = applicableRule.CalculationMethod;
            }
            else
            {
                // Use default tax rates
                var defaultRate = GetDefaultTaxRate(request.Country, request.Region, request.TaxType);
                taxRate = defaultRate.Rate;
                taxName = defaultRate.Name;
                calculationMethod = "Percentage";
            }

            var taxAmount = request.TaxableAmount * (taxRate / 100);

            var calculation = new TaxCalculationDto
            {
                Id = Guid.NewGuid(),
                FinancialProjectionId = request.FinancialProjectionId,
                TaxRuleId = applicableRule?.Id ?? Guid.Empty,
                TaxName = taxName,
                TaxType = request.TaxType,
                TaxableAmount = request.TaxableAmount,
                TaxRate = taxRate,
                TaxAmount = Math.Round(taxAmount, 2),
                CurrencyCode = request.CurrencyCode,
                CalculationMethod = calculationMethod,
                Country = request.Country,
                Region = request.Region,
                TaxPeriod = request.TaxPeriod,
                IsPaid = false,
                Notes = request.Notes
            };

            return Result<TaxCalculationDto>.Success(calculation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating tax");
            return Result.Failure<TaxCalculationDto>(
                Error.InternalServerError("Tax.CalculationFailed", "Failed to calculate tax"));
        }
    }

    public async Task<Result<List<TaxCalculationDto>>> CalculateTaxesForProjectionAsync(Guid projectionId)
    {
        try
        {
            var projection = await _context.FinancialProjectionItems
                .FirstOrDefaultAsync(p => p.Id == projectionId);

            if (projection == null)
            {
                return Result.Failure<List<TaxCalculationDto>>(
                    Error.NotFound("Financial.NotFound", $"Financial projection with ID {projectionId} not found"));
            }

            var calculations = new List<TaxCalculationDto>();

            // Calculate federal and provincial taxes for Canadian business
            var federalTax = await CalculateTaxAsync(new TaxCalculationRequest
            {
                FinancialProjectionId = projectionId,
                Country = "Canada",
                Region = "Federal",
                TaxType = "Corporate",
                TaxableAmount = projection.Amount,
                CurrencyCode = projection.CurrencyCode,
                TaxPeriod = new DateTime(projection.Year, projection.Month > 0 ? projection.Month : 1, 1)
            });

            if (federalTax.IsSuccess)
            {
                calculations.Add(federalTax.Value!);
            }

            // Add provincial tax (default to Ontario)
            var provincialTax = await CalculateTaxAsync(new TaxCalculationRequest
            {
                FinancialProjectionId = projectionId,
                Country = "Canada",
                Region = "Ontario",
                TaxType = "Provincial",
                TaxableAmount = projection.Amount,
                CurrencyCode = projection.CurrencyCode,
                TaxPeriod = new DateTime(projection.Year, projection.Month > 0 ? projection.Month : 1, 1)
            });

            if (provincialTax.IsSuccess)
            {
                calculations.Add(provincialTax.Value!);
            }

            return Result<List<TaxCalculationDto>>.Success(calculations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating taxes for projection {ProjectionId}", projectionId);
            return Result.Failure<List<TaxCalculationDto>>(
                Error.InternalServerError("Tax.CalculationFailed", "Failed to calculate taxes for projection"));
        }
    }

    public async Task<Result<List<TaxRuleDto>>> GetTaxRulesAsync(string country, string region)
    {
        try
        {
            var rules = await _context.TaxRules
                .Where(r => r.Country == country && (string.IsNullOrEmpty(region) || r.Region == region) && r.IsActive)
                .ToListAsync();

            if (!rules.Any())
            {
                // Return default tax rules for Canada
                return Result<List<TaxRuleDto>>.Success(GetDefaultTaxRules(country, region));
            }

            var dtos = rules.Select(r => new TaxRuleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Country = r.Country,
                Region = r.Region,
                TaxType = r.TaxType,
                Rate = r.Rate,
                MinAmount = r.MinAmount,
                MaxAmount = r.MaxAmount,
                IsPercentage = r.IsPercentage,
                CalculationMethod = r.CalculationMethod,
                ApplicableTo = r.ApplicableTo,
                IsActive = r.IsActive,
                EffectiveDate = r.EffectiveDate,
                ExpiryDate = r.ExpiryDate,
                CurrencyCode = r.CurrencyCode,
                LegalReference = r.LegalReference,
                Notes = r.Notes
            }).ToList();

            return Result<List<TaxRuleDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tax rules for {Country}/{Region}", country, region);
            return Result.Failure<List<TaxRuleDto>>(
                Error.InternalServerError("Tax.GetRulesFailed", "Failed to retrieve tax rules"));
        }
    }

    private static (decimal Rate, string Name) GetDefaultTaxRate(string country, string region, string taxType)
    {
        // Canadian tax rates (2024)
        if (country.Equals("Canada", StringComparison.OrdinalIgnoreCase))
        {
            if (taxType.Equals("Corporate", StringComparison.OrdinalIgnoreCase) || taxType.Equals("Federal", StringComparison.OrdinalIgnoreCase))
            {
                return (15.0m, "Federal Corporate Tax");
            }

            if (taxType.Equals("Provincial", StringComparison.OrdinalIgnoreCase))
            {
                return region?.ToUpperInvariant() switch
                {
                    "ONTARIO" => (11.5m, "Ontario Corporate Tax"),
                    "QUEBEC" => (11.5m, "Quebec Corporate Tax"),
                    "BRITISH COLUMBIA" => (12.0m, "BC Corporate Tax"),
                    "ALBERTA" => (8.0m, "Alberta Corporate Tax"),
                    _ => (12.0m, "Provincial Corporate Tax")
                };
            }

            if (taxType.Equals("GST", StringComparison.OrdinalIgnoreCase))
            {
                return (5.0m, "GST");
            }

            if (taxType.Equals("HST", StringComparison.OrdinalIgnoreCase))
            {
                return region?.ToUpperInvariant() switch
                {
                    "ONTARIO" => (13.0m, "Ontario HST"),
                    "NOVA SCOTIA" => (15.0m, "Nova Scotia HST"),
                    "NEW BRUNSWICK" => (15.0m, "New Brunswick HST"),
                    _ => (13.0m, "HST")
                };
            }
        }

        return (20.0m, "Default Tax");
    }

    private static List<TaxRuleDto> GetDefaultTaxRules(string country, string region)
    {
        if (!country.Equals("Canada", StringComparison.OrdinalIgnoreCase))
        {
            return new List<TaxRuleDto>();
        }

        var rules = new List<TaxRuleDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Federal Corporate Tax",
                Description = "Canadian federal corporate income tax",
                Country = "Canada",
                Region = "Federal",
                TaxType = "Corporate",
                Rate = 15.0m,
                IsPercentage = true,
                CalculationMethod = "Percentage",
                ApplicableTo = "Corporate Income",
                IsActive = true,
                EffectiveDate = new DateTime(2024, 1, 1),
                CurrencyCode = "CAD"
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "GST",
                Description = "Goods and Services Tax",
                Country = "Canada",
                Region = "Federal",
                TaxType = "GST",
                Rate = 5.0m,
                IsPercentage = true,
                CalculationMethod = "Percentage",
                ApplicableTo = "Sales",
                IsActive = true,
                EffectiveDate = new DateTime(2024, 1, 1),
                CurrencyCode = "CAD"
            }
        };

        // Add provincial tax based on region
        var provincialRate = region?.ToUpperInvariant() switch
        {
            "ONTARIO" => 11.5m,
            "QUEBEC" => 11.5m,
            "BRITISH COLUMBIA" => 12.0m,
            "ALBERTA" => 8.0m,
            _ => 12.0m
        };

        rules.Add(new TaxRuleDto
        {
            Id = Guid.NewGuid(),
            Name = $"{region ?? "Provincial"} Corporate Tax",
            Description = "Provincial corporate income tax",
            Country = "Canada",
            Region = region ?? "Ontario",
            TaxType = "Provincial",
            Rate = provincialRate,
            IsPercentage = true,
            CalculationMethod = "Percentage",
            ApplicableTo = "Corporate Income",
            IsActive = true,
            EffectiveDate = new DateTime(2024, 1, 1),
            CurrencyCode = "CAD"
        });

        return rules;
    }

    // KPI Operations
    public async Task<Result<List<FinancialKPIDto>>> CalculateKPIsAsync(Guid businessPlanId)
    {
        try
        {
            _logger.LogInformation("Calculating KPIs for business plan {BusinessPlanId}", businessPlanId);

            var projections = await _context.BusinessPlanFinancialProjections
                .Where(p => p.BusinessPlanId == businessPlanId)
                .ToListAsync();

            var projectionItems = await _context.FinancialProjectionItems
                .Where(p => p.BusinessPlanId == businessPlanId)
                .ToListAsync();

            var kpis = new List<FinancialKPIDto>();
            var currentYear = DateTime.UtcNow.Year;
            var currentMonth = DateTime.UtcNow.Month;

            // Calculate Revenue KPIs
            var totalRevenue = projections.Sum(p => p.Revenue ?? 0) +
                              projectionItems.Where(i => i.Category.Contains("Revenue", StringComparison.OrdinalIgnoreCase)).Sum(i => i.Amount);
            kpis.Add(CreateKpi(businessPlanId, "Total Revenue", "Revenue", "Currency", totalRevenue, "CAD", currentYear, currentMonth));

            // Calculate Expense KPIs
            var totalExpenses = projections.Sum(p =>
                (p.CostOfGoodsSold ?? 0) + (p.OperatingExpenses ?? 0) + (p.MarketingExpenses ?? 0) +
                (p.RAndDExpenses ?? 0) + (p.AdministrativeExpenses ?? 0) + (p.OtherExpenses ?? 0));
            kpis.Add(CreateKpi(businessPlanId, "Total Expenses", "Expenses", "Currency", totalExpenses, "CAD", currentYear, currentMonth));

            // Gross Profit Margin
            var totalCogs = projections.Sum(p => p.CostOfGoodsSold ?? 0);
            var grossProfit = totalRevenue - totalCogs;
            var grossMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;
            kpis.Add(CreateKpi(businessPlanId, "Gross Profit Margin", "Profitability", "Percentage", grossMargin, "%", currentYear, currentMonth));

            // Net Profit Margin
            var netIncome = projections.Sum(p => p.NetIncome ?? 0);
            var netMargin = totalRevenue > 0 ? (netIncome / totalRevenue) * 100 : 0;
            kpis.Add(CreateKpi(businessPlanId, "Net Profit Margin", "Profitability", "Percentage", netMargin, "%", currentYear, currentMonth));

            // Operating Margin
            var operatingIncome = grossProfit - (totalExpenses - totalCogs);
            var operatingMargin = totalRevenue > 0 ? (operatingIncome / totalRevenue) * 100 : 0;
            kpis.Add(CreateKpi(businessPlanId, "Operating Margin", "Profitability", "Percentage", operatingMargin, "%", currentYear, currentMonth));

            // EBITDA
            var ebitda = projections.Sum(p => p.EBITDA ?? 0);
            kpis.Add(CreateKpi(businessPlanId, "EBITDA", "Profitability", "Currency", ebitda, "CAD", currentYear, currentMonth));

            // Cash Position
            var cashBalance = projections.LastOrDefault()?.CashBalance ?? 0;
            kpis.Add(CreateKpi(businessPlanId, "Cash Position", "Liquidity", "Currency", cashBalance, "CAD", currentYear, currentMonth));

            // Revenue Growth Rate
            var yearlyRevenues = projections
                .GroupBy(p => p.Year)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(p => p.Revenue ?? 0))
                .ToList();

            decimal revenueGrowth = 0;
            if (yearlyRevenues.Count >= 2 && yearlyRevenues[^2] > 0)
            {
                revenueGrowth = ((yearlyRevenues[^1] - yearlyRevenues[^2]) / yearlyRevenues[^2]) * 100;
            }
            kpis.Add(CreateKpi(businessPlanId, "Revenue Growth Rate", "Growth", "Percentage", revenueGrowth, "%", currentYear, currentMonth));

            // Burn Rate (for startups)
            var monthlyExpenses = totalExpenses / Math.Max(projections.Count, 1);
            kpis.Add(CreateKpi(businessPlanId, "Monthly Burn Rate", "Cash Flow", "Currency", monthlyExpenses, "CAD", currentYear, currentMonth));

            // Runway (months of cash remaining)
            var runway = monthlyExpenses > 0 ? cashBalance / monthlyExpenses : 0;
            kpis.Add(CreateKpi(businessPlanId, "Cash Runway", "Cash Flow", "Months", runway, "months", currentYear, currentMonth));

            return Result<List<FinancialKPIDto>>.Success(kpis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating KPIs for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<List<FinancialKPIDto>>(
                Error.InternalServerError("KPI.CalculationFailed", "Failed to calculate KPIs"));
        }
    }

    public async Task<Result<FinancialKPIDto>> GetKPIByNameAsync(Guid businessPlanId, string kpiName)
    {
        try
        {
            var kpisResult = await CalculateKPIsAsync(businessPlanId);
            if (!kpisResult.IsSuccess)
            {
                return Result.Failure<FinancialKPIDto>(kpisResult.Error!);
            }

            var kpi = kpisResult.Value!.FirstOrDefault(k => k.Name.Equals(kpiName, StringComparison.OrdinalIgnoreCase));
            if (kpi == null)
            {
                return Result.Failure<FinancialKPIDto>(
                    Error.NotFound("KPI.NotFound", $"KPI with name '{kpiName}' not found"));
            }

            return Result<FinancialKPIDto>.Success(kpi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KPI {KpiName} for business plan {BusinessPlanId}", kpiName, businessPlanId);
            return Result.Failure<FinancialKPIDto>(
                Error.InternalServerError("KPI.GetFailed", "Failed to retrieve KPI"));
        }
    }

    public async Task<Result<List<FinancialKPIDto>>> GetKPIsByCategoryAsync(Guid businessPlanId, string category)
    {
        try
        {
            var kpisResult = await CalculateKPIsAsync(businessPlanId);
            if (!kpisResult.IsSuccess)
            {
                return Result.Failure<List<FinancialKPIDto>>(kpisResult.Error!);
            }

            var kpis = kpisResult.Value!
                .Where(k => k.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Result<List<FinancialKPIDto>>.Success(kpis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KPIs by category {Category} for business plan {BusinessPlanId}", category, businessPlanId);
            return Result.Failure<List<FinancialKPIDto>>(
                Error.InternalServerError("KPI.GetFailed", "Failed to retrieve KPIs by category"));
        }
    }

    private static FinancialKPIDto CreateKpi(Guid businessPlanId, string name, string category, string metricType, decimal value, string unit, int year, int month)
    {
        var trend = value > 0 ? "Positive" : value < 0 ? "Negative" : "Neutral";
        var status = metricType == "Percentage"
            ? (value >= 20 ? "Excellent" : value >= 10 ? "Good" : value >= 0 ? "Fair" : "Poor")
            : "Calculated";

        return new FinancialKPIDto
        {
            Id = Guid.NewGuid(),
            BusinessPlanId = businessPlanId,
            Name = name,
            Description = $"Calculated {name} for the business plan",
            Category = category,
            MetricType = metricType,
            Value = Math.Round(value, 2),
            Unit = unit,
            CurrencyCode = unit == "CAD" ? "CAD" : "",
            Year = year,
            Month = month,
            TargetValue = 0,
            PreviousValue = 0,
            ChangePercentage = 0,
            Trend = trend,
            Status = status
        };
    }

    // Investment Analysis Operations
    public async Task<Result<InvestmentAnalysisDto>> CreateInvestmentAnalysisAsync(CreateInvestmentAnalysisCommand command)
    {
        try
        {
            _logger.LogInformation("Creating investment analysis for business plan {BusinessPlanId}", command.BusinessPlanId);

            // Calculate NPV, IRR, and ROI based on the provided data
            var npv = CalculateNPV(command.InitialInvestment, command.ExpectedReturn, command.DiscountRate, command.AnalysisPeriod);
            var irr = CalculateIRR(command.InitialInvestment, command.ExpectedReturn, command.AnalysisPeriod);
            var roi = command.InitialInvestment > 0
                ? ((command.ExpectedReturn - command.InitialInvestment) / command.InitialInvestment) * 100
                : 0;
            var paybackPeriod = command.ExpectedReturn > 0
                ? (command.InitialInvestment / (command.ExpectedReturn / command.AnalysisPeriod))
                : 0;

            var analysis = new InvestmentAnalysisDto
            {
                Id = Guid.NewGuid(),
                BusinessPlanId = command.BusinessPlanId,
                AnalysisType = command.AnalysisType,
                Name = command.Name,
                Description = command.Description,
                InitialInvestment = command.InitialInvestment,
                ExpectedReturn = command.ExpectedReturn,
                NetPresentValue = Math.Round(npv, 2),
                InternalRateOfReturn = Math.Round(irr, 2),
                PaybackPeriod = Math.Round(paybackPeriod, 2),
                ReturnOnInvestment = Math.Round(roi, 2),
                CurrencyCode = command.CurrencyCode,
                DiscountRate = command.DiscountRate,
                AnalysisPeriod = command.AnalysisPeriod,
                RiskLevel = command.RiskLevel,
                InvestmentType = command.InvestmentType,
                InvestorType = command.InvestorType,
                Valuation = command.Valuation,
                EquityOffering = command.EquityOffering,
                FundingRequired = command.FundingRequired,
                FundingStage = command.FundingStage,
                Assumptions = command.Assumptions,
                Notes = command.Notes
            };

            return await Task.FromResult(Result<InvestmentAnalysisDto>.Success(analysis));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating investment analysis");
            return Result.Failure<InvestmentAnalysisDto>(
                Error.InternalServerError("Investment.CreateFailed", "Failed to create investment analysis"));
        }
    }

    public async Task<Result<InvestmentAnalysisDto>> CalculateROIAsync(Guid businessPlanId, decimal investmentAmount)
    {
        try
        {
            _logger.LogInformation("Calculating ROI for business plan {BusinessPlanId} with investment {Investment}", businessPlanId, investmentAmount);

            var projections = await _context.BusinessPlanFinancialProjections
                .Where(p => p.BusinessPlanId == businessPlanId)
                .OrderBy(p => p.Year)
                .ThenBy(p => p.Month ?? 0)
                .ToListAsync();

            if (!projections.Any())
            {
                return Result.Failure<InvestmentAnalysisDto>(
                    Error.NotFound("Financial.NoProjections", "No financial projections found for this business plan"));
            }

            var totalNetIncome = projections.Sum(p => p.NetIncome ?? 0);
            var analysisPeriod = projections.Select(p => p.Year).Distinct().Count();
            var averageAnnualReturn = analysisPeriod > 0 ? totalNetIncome / analysisPeriod : 0;

            var roi = investmentAmount > 0 ? ((totalNetIncome - investmentAmount) / investmentAmount) * 100 : 0;
            var paybackPeriod = averageAnnualReturn > 0 ? investmentAmount / averageAnnualReturn : 0;

            var analysis = new InvestmentAnalysisDto
            {
                Id = Guid.NewGuid(),
                BusinessPlanId = businessPlanId,
                AnalysisType = "ROI Analysis",
                Name = "Return on Investment Analysis",
                Description = $"ROI calculation based on {investmentAmount:C} investment",
                InitialInvestment = investmentAmount,
                ExpectedReturn = totalNetIncome,
                ReturnOnInvestment = Math.Round(roi, 2),
                PaybackPeriod = Math.Round(paybackPeriod, 2),
                CurrencyCode = "CAD",
                AnalysisPeriod = analysisPeriod,
                RiskLevel = roi > 20 ? "Low" : roi > 10 ? "Medium" : "High"
            };

            return Result<InvestmentAnalysisDto>.Success(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating ROI for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<InvestmentAnalysisDto>(
                Error.InternalServerError("Investment.ROIFailed", "Failed to calculate ROI"));
        }
    }

    public async Task<Result<InvestmentAnalysisDto>> CalculateNPVAsync(Guid businessPlanId, decimal discountRate)
    {
        try
        {
            _logger.LogInformation("Calculating NPV for business plan {BusinessPlanId} with discount rate {DiscountRate}%", businessPlanId, discountRate);

            var projections = await _context.BusinessPlanFinancialProjections
                .Where(p => p.BusinessPlanId == businessPlanId)
                .OrderBy(p => p.Year)
                .ThenBy(p => p.Month ?? 0)
                .ToListAsync();

            if (!projections.Any())
            {
                return Result.Failure<InvestmentAnalysisDto>(
                    Error.NotFound("Financial.NoProjections", "No financial projections found for this business plan"));
            }

            // Group cash flows by year
            var yearlyCashFlows = projections
                .GroupBy(p => p.Year)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(p => p.CashFlow ?? p.NetIncome ?? 0))
                .ToList();

            // Calculate NPV
            var initialInvestment = Math.Abs(yearlyCashFlows.FirstOrDefault());
            var npv = 0m;
            var rate = discountRate / 100;

            for (int i = 0; i < yearlyCashFlows.Count; i++)
            {
                npv += yearlyCashFlows[i] / (decimal)Math.Pow((double)(1 + rate), i + 1);
            }

            var totalReturn = yearlyCashFlows.Sum();
            var irr = CalculateIRR(initialInvestment, totalReturn, yearlyCashFlows.Count);

            var analysis = new InvestmentAnalysisDto
            {
                Id = Guid.NewGuid(),
                BusinessPlanId = businessPlanId,
                AnalysisType = "NPV Analysis",
                Name = "Net Present Value Analysis",
                Description = $"NPV calculation with {discountRate}% discount rate",
                InitialInvestment = initialInvestment,
                ExpectedReturn = totalReturn,
                NetPresentValue = Math.Round(npv, 2),
                InternalRateOfReturn = Math.Round(irr, 2),
                DiscountRate = discountRate,
                CurrencyCode = "CAD",
                AnalysisPeriod = yearlyCashFlows.Count,
                RiskLevel = npv > 0 ? "Acceptable" : "High"
            };

            return Result<InvestmentAnalysisDto>.Success(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating NPV for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<InvestmentAnalysisDto>(
                Error.InternalServerError("Investment.NPVFailed", "Failed to calculate NPV"));
        }
    }

    public async Task<Result<InvestmentAnalysisDto>> CalculateIRRAsync(Guid businessPlanId)
    {
        try
        {
            _logger.LogInformation("Calculating IRR for business plan {BusinessPlanId}", businessPlanId);

            var projections = await _context.BusinessPlanFinancialProjections
                .Where(p => p.BusinessPlanId == businessPlanId)
                .OrderBy(p => p.Year)
                .ThenBy(p => p.Month ?? 0)
                .ToListAsync();

            if (!projections.Any())
            {
                return Result.Failure<InvestmentAnalysisDto>(
                    Error.NotFound("Financial.NoProjections", "No financial projections found for this business plan"));
            }

            var yearlyCashFlows = projections
                .GroupBy(p => p.Year)
                .OrderBy(g => g.Key)
                .Select(g => g.Sum(p => p.CashFlow ?? p.NetIncome ?? 0))
                .ToList();

            var initialInvestment = Math.Abs(yearlyCashFlows.FirstOrDefault());
            var totalReturn = yearlyCashFlows.Sum();
            var irr = CalculateIRR(initialInvestment, totalReturn, yearlyCashFlows.Count);

            // Calculate NPV at a standard 10% discount rate for comparison
            var npvAt10 = CalculateNPV(initialInvestment, totalReturn, 10, yearlyCashFlows.Count);

            var analysis = new InvestmentAnalysisDto
            {
                Id = Guid.NewGuid(),
                BusinessPlanId = businessPlanId,
                AnalysisType = "IRR Analysis",
                Name = "Internal Rate of Return Analysis",
                Description = "IRR calculation based on projected cash flows",
                InitialInvestment = initialInvestment,
                ExpectedReturn = totalReturn,
                InternalRateOfReturn = Math.Round(irr, 2),
                NetPresentValue = Math.Round(npvAt10, 2),
                DiscountRate = 10, // Standard comparison rate
                CurrencyCode = "CAD",
                AnalysisPeriod = yearlyCashFlows.Count,
                RiskLevel = irr > 15 ? "Low" : irr > 8 ? "Medium" : "High"
            };

            return Result<InvestmentAnalysisDto>.Success(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating IRR for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<InvestmentAnalysisDto>(
                Error.InternalServerError("Investment.IRRFailed", "Failed to calculate IRR"));
        }
    }

    private static decimal CalculateNPV(decimal initialInvestment, decimal totalReturn, decimal discountRate, int periods)
    {
        if (periods <= 0) return 0;

        var rate = discountRate / 100;
        var annualCashFlow = totalReturn / periods;
        var npv = -initialInvestment;

        for (int i = 1; i <= periods; i++)
        {
            npv += annualCashFlow / (decimal)Math.Pow((double)(1 + rate), i);
        }

        return npv;
    }

    private static decimal CalculateIRR(decimal initialInvestment, decimal totalReturn, int periods)
    {
        if (initialInvestment <= 0 || periods <= 0) return 0;

        // Simple IRR approximation using the average annual return method
        var averageAnnualReturn = totalReturn / periods;
        var irr = initialInvestment > 0
            ? (decimal)Math.Pow((double)(totalReturn / initialInvestment), 1.0 / periods) - 1
            : 0;

        return irr * 100; // Return as percentage
    }

    // General Financial Report
    public async Task<Result<FinancialReportDto>> GenerateFinancialReportAsync(Guid businessPlanId, string reportType)
    {
        try
        {
            _logger.LogInformation("Generating {ReportType} report for business plan {BusinessPlanId}", reportType, businessPlanId);

            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId);

            if (businessPlan == null)
            {
                return Result.Failure<FinancialReportDto>(
                    Error.NotFound("Financial.BusinessPlanNotFound", $"Business plan with ID {businessPlanId} not found"));
            }

            // Generate specific report based on type
            string content;
            string title;

            switch (reportType.ToLowerInvariant())
            {
                case "summary":
                case "executive":
                    var kpisResult = await CalculateKPIsAsync(businessPlanId);
                    var kpis = kpisResult.IsSuccess ? kpisResult.Value! : new List<FinancialKPIDto>();
                    content = GenerateExecutiveSummaryContent(kpis);
                    title = "Executive Financial Summary";
                    break;

                case "cashflow":
                case "cash-flow":
                    var cashFlowResult = await GenerateCashFlowReportAsync(businessPlanId);
                    content = cashFlowResult.IsSuccess
                        ? System.Text.Json.JsonSerializer.Serialize(cashFlowResult.Value)
                        : "{}";
                    title = "Cash Flow Report";
                    break;

                case "profitloss":
                case "profit-loss":
                case "p&l":
                    var plResult = await GenerateProfitLossReportAsync(businessPlanId);
                    content = plResult.IsSuccess
                        ? System.Text.Json.JsonSerializer.Serialize(plResult.Value)
                        : "{}";
                    title = "Profit & Loss Report";
                    break;

                case "balancesheet":
                case "balance-sheet":
                    var bsResult = await GenerateBalanceSheetReportAsync(businessPlanId);
                    content = bsResult.IsSuccess
                        ? System.Text.Json.JsonSerializer.Serialize(bsResult.Value)
                        : "{}";
                    title = "Balance Sheet Report";
                    break;

                case "comprehensive":
                case "full":
                default:
                    content = await GenerateComprehensiveReportContent(businessPlanId);
                    title = "Comprehensive Financial Report";
                    break;
            }

            var report = new FinancialReportDto
            {
                Id = Guid.NewGuid(),
                BusinessPlanId = businessPlanId,
                ReportType = reportType,
                Title = title,
                Description = $"Financial report generated for business plan",
                GeneratedAt = DateTime.UtcNow,
                Currency = "CAD",
                Period = DateTime.UtcNow.Year.ToString(),
                Status = "Generated",
                Content = content
            };

            return Result<FinancialReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial report for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<FinancialReportDto>(
                Error.InternalServerError("Financial.ReportGenerationFailed", "Failed to generate financial report"));
        }
    }

    private static string GenerateExecutiveSummaryContent(List<FinancialKPIDto> kpis)
    {
        var summary = new
        {
            GeneratedAt = DateTime.UtcNow,
            KPIs = kpis.Select(k => new { k.Name, k.Value, k.Unit, k.Category, k.Status }),
            Highlights = kpis.Where(k => k.Status == "Excellent" || k.Status == "Good").Select(k => k.Name).ToList(),
            Concerns = kpis.Where(k => k.Status == "Poor").Select(k => k.Name).ToList()
        };

        return System.Text.Json.JsonSerializer.Serialize(summary);
    }

    private async Task<string> GenerateComprehensiveReportContent(Guid businessPlanId)
    {
        var kpisResult = await CalculateKPIsAsync(businessPlanId);
        var cashFlowResult = await GenerateCashFlowReportAsync(businessPlanId);
        var plResult = await GenerateProfitLossReportAsync(businessPlanId);
        var bsResult = await GenerateBalanceSheetReportAsync(businessPlanId);

        var comprehensive = new
        {
            GeneratedAt = DateTime.UtcNow,
            KPIs = kpisResult.IsSuccess ? kpisResult.Value : null,
            CashFlow = cashFlowResult.IsSuccess ? cashFlowResult.Value : null,
            ProfitLoss = plResult.IsSuccess ? plResult.Value : null,
            BalanceSheet = bsResult.IsSuccess ? bsResult.Value : null
        };

        return System.Text.Json.JsonSerializer.Serialize(comprehensive);
    }

    public async Task<Result<CashFlowReportDto>> GenerateCashFlowReportAsync(Guid businessPlanId)
    {
        try
        {
            _logger.LogInformation("Generating cash flow report for business plan {BusinessPlanId}", businessPlanId);

            // Verify business plan exists
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId);

            if (businessPlan == null)
            {
                return Result.Failure<CashFlowReportDto>(
                    Error.NotFound("Financial.BusinessPlanNotFound", $"Business plan with ID {businessPlanId} not found"));
            }

            // Get financial projections ordered by period
            var projections = await _context.BusinessPlanFinancialProjections
                .Where(fp => fp.BusinessPlanId == businessPlanId)
                .OrderBy(fp => fp.Year)
                .ThenBy(fp => fp.Month ?? 0)
                .ToListAsync();

            // Get financial projection items
            var projectionItems = await _context.FinancialProjectionItems
                .Where(fpi => fpi.BusinessPlanId == businessPlanId)
                .ToListAsync();

            // Determine the most recent period
            var latestProjection = projections.LastOrDefault();
            var currentYear = latestProjection?.Year ?? DateTime.UtcNow.Year;
            var currentMonth = latestProjection?.Month ?? DateTime.UtcNow.Month;

            var cashFlowItems = new List<CashFlowItemDto>();

            // Calculate cash inflows from revenue
            decimal totalInflows = 0;
            foreach (var projection in projections)
            {
                if (projection.Revenue.HasValue && projection.Revenue.Value > 0)
                {
                    cashFlowItems.Add(new CashFlowItemDto
                    {
                        Category = "Operating Activities",
                        Description = "Revenue",
                        Amount = projection.Revenue.Value,
                        Type = "Inflow",
                        Month = projection.Month ?? 0,
                        Year = projection.Year
                    });
                    totalInflows += projection.Revenue.Value;
                }
            }

            // Add revenue items from projection items
            var revenueItems = projectionItems
                .Where(i => i.Category.Contains("Revenue", StringComparison.OrdinalIgnoreCase) ||
                           i.ProjectionType.Contains("Revenue", StringComparison.OrdinalIgnoreCase));
            foreach (var item in revenueItems)
            {
                cashFlowItems.Add(new CashFlowItemDto
                {
                    Category = "Operating Activities",
                    Description = item.Name,
                    Amount = item.Amount,
                    Type = "Inflow",
                    Month = item.Month,
                    Year = item.Year
                });
                totalInflows += item.Amount;
            }

            // Calculate cash outflows from expenses
            decimal totalOutflows = 0;
            foreach (var projection in projections)
            {
                // COGS
                if (projection.CostOfGoodsSold.HasValue && projection.CostOfGoodsSold.Value > 0)
                {
                    cashFlowItems.Add(new CashFlowItemDto
                    {
                        Category = "Operating Activities",
                        Description = "Cost of Goods Sold",
                        Amount = projection.CostOfGoodsSold.Value,
                        Type = "Outflow",
                        Month = projection.Month ?? 0,
                        Year = projection.Year
                    });
                    totalOutflows += projection.CostOfGoodsSold.Value;
                }

                // Operating Expenses
                if (projection.OperatingExpenses.HasValue && projection.OperatingExpenses.Value > 0)
                {
                    cashFlowItems.Add(new CashFlowItemDto
                    {
                        Category = "Operating Activities",
                        Description = "Operating Expenses",
                        Amount = projection.OperatingExpenses.Value,
                        Type = "Outflow",
                        Month = projection.Month ?? 0,
                        Year = projection.Year
                    });
                    totalOutflows += projection.OperatingExpenses.Value;
                }

                // Marketing Expenses
                if (projection.MarketingExpenses.HasValue && projection.MarketingExpenses.Value > 0)
                {
                    cashFlowItems.Add(new CashFlowItemDto
                    {
                        Category = "Operating Activities",
                        Description = "Marketing Expenses",
                        Amount = projection.MarketingExpenses.Value,
                        Type = "Outflow",
                        Month = projection.Month ?? 0,
                        Year = projection.Year
                    });
                    totalOutflows += projection.MarketingExpenses.Value;
                }

                // R&D Expenses
                if (projection.RAndDExpenses.HasValue && projection.RAndDExpenses.Value > 0)
                {
                    cashFlowItems.Add(new CashFlowItemDto
                    {
                        Category = "Operating Activities",
                        Description = "R&D Expenses",
                        Amount = projection.RAndDExpenses.Value,
                        Type = "Outflow",
                        Month = projection.Month ?? 0,
                        Year = projection.Year
                    });
                    totalOutflows += projection.RAndDExpenses.Value;
                }

                // Administrative Expenses
                if (projection.AdministrativeExpenses.HasValue && projection.AdministrativeExpenses.Value > 0)
                {
                    cashFlowItems.Add(new CashFlowItemDto
                    {
                        Category = "Operating Activities",
                        Description = "Administrative Expenses",
                        Amount = projection.AdministrativeExpenses.Value,
                        Type = "Outflow",
                        Month = projection.Month ?? 0,
                        Year = projection.Year
                    });
                    totalOutflows += projection.AdministrativeExpenses.Value;
                }
            }

            // Add expense items from projection items
            var expenseItems = projectionItems
                .Where(i => i.Category.Contains("Expense", StringComparison.OrdinalIgnoreCase) ||
                           i.ProjectionType.Contains("Expense", StringComparison.OrdinalIgnoreCase) ||
                           i.Category.Contains("Cost", StringComparison.OrdinalIgnoreCase));
            foreach (var item in expenseItems)
            {
                cashFlowItems.Add(new CashFlowItemDto
                {
                    Category = "Operating Activities",
                    Description = item.Name,
                    Amount = item.Amount,
                    Type = "Outflow",
                    Month = item.Month,
                    Year = item.Year
                });
                totalOutflows += item.Amount;
            }

            // Calculate opening and closing balances
            var firstProjection = projections.FirstOrDefault();
            var openingBalance = firstProjection?.CashBalance ?? 0;
            var netCashFlow = totalInflows - totalOutflows;
            var closingBalance = latestProjection?.CashBalance ?? (openingBalance + netCashFlow);

            var report = new CashFlowReportDto
            {
                BusinessPlanId = businessPlanId,
                Period = latestProjection?.Month.HasValue == true
                    ? $"{currentYear}-{currentMonth:D2}"
                    : $"{currentYear}",
                Currency = "CAD",
                OpeningBalance = openingBalance,
                CashInflows = totalInflows,
                CashOutflows = totalOutflows,
                NetCashFlow = netCashFlow,
                ClosingBalance = closingBalance,
                Items = cashFlowItems,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully generated cash flow report for business plan {BusinessPlanId}", businessPlanId);
            return Result<CashFlowReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cash flow report for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<CashFlowReportDto>(
                Error.InternalServerError("Financial.CashFlowGenerationFailed", "Failed to generate cash flow report"));
        }
    }

    public async Task<Result<ProfitLossReportDto>> GenerateProfitLossReportAsync(Guid businessPlanId)
    {
        try
        {
            _logger.LogInformation("Generating profit & loss report for business plan {BusinessPlanId}", businessPlanId);

            // Verify business plan exists
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId);

            if (businessPlan == null)
            {
                return Result.Failure<ProfitLossReportDto>(
                    Error.NotFound("Financial.BusinessPlanNotFound", $"Business plan with ID {businessPlanId} not found"));
            }

            // Get financial projections
            var projections = await _context.BusinessPlanFinancialProjections
                .Where(fp => fp.BusinessPlanId == businessPlanId)
                .OrderByDescending(fp => fp.Year)
                .ThenByDescending(fp => fp.Month ?? 0)
                .ToListAsync();

            // Get financial projection items
            var projectionItems = await _context.FinancialProjectionItems
                .Where(fpi => fpi.BusinessPlanId == businessPlanId)
                .ToListAsync();

            // Determine the most recent period
            var latestProjection = projections.FirstOrDefault();
            var currentYear = latestProjection?.Year ?? DateTime.UtcNow.Year;
            var currentMonth = latestProjection?.Month ?? DateTime.UtcNow.Month;

            var profitLossItems = new List<ProfitLossItemDto>();

            // Aggregate totals from projections
            decimal totalRevenue = projections.Sum(p => p.Revenue ?? 0);
            decimal totalCOGS = projections.Sum(p => p.CostOfGoodsSold ?? 0);
            decimal totalOperatingExpenses = projections.Sum(p =>
                (p.OperatingExpenses ?? 0) +
                (p.MarketingExpenses ?? 0) +
                (p.RAndDExpenses ?? 0) +
                (p.AdministrativeExpenses ?? 0) +
                (p.OtherExpenses ?? 0));

            // Add revenue items
            foreach (var projection in projections.Where(p => p.Revenue.HasValue && p.Revenue.Value > 0))
            {
                profitLossItems.Add(new ProfitLossItemDto
                {
                    Category = "Revenue",
                    Description = "Sales Revenue",
                    Amount = projection.Revenue!.Value,
                    Type = "Revenue",
                    Month = projection.Month ?? 0,
                    Year = projection.Year
                });
            }

            // Add revenue from projection items
            var revenueItems = projectionItems
                .Where(i => i.Category.Contains("Revenue", StringComparison.OrdinalIgnoreCase) ||
                           i.ProjectionType.Contains("Revenue", StringComparison.OrdinalIgnoreCase));
            foreach (var item in revenueItems)
            {
                profitLossItems.Add(new ProfitLossItemDto
                {
                    Category = "Revenue",
                    Description = item.Name,
                    Amount = item.Amount,
                    Type = "Revenue",
                    Month = item.Month,
                    Year = item.Year
                });
                totalRevenue += item.Amount;
            }

            // Add COGS items
            foreach (var projection in projections.Where(p => p.CostOfGoodsSold.HasValue && p.CostOfGoodsSold.Value > 0))
            {
                profitLossItems.Add(new ProfitLossItemDto
                {
                    Category = "Cost of Goods Sold",
                    Description = "Direct Costs",
                    Amount = projection.CostOfGoodsSold!.Value,
                    Type = "Expense",
                    Month = projection.Month ?? 0,
                    Year = projection.Year
                });
            }

            // Add COGS from projection items
            var cogsItems = projectionItems
                .Where(i => i.Category.Contains("COGS", StringComparison.OrdinalIgnoreCase) ||
                           i.Category.Contains("Cost of Goods", StringComparison.OrdinalIgnoreCase));
            foreach (var item in cogsItems)
            {
                profitLossItems.Add(new ProfitLossItemDto
                {
                    Category = "Cost of Goods Sold",
                    Description = item.Name,
                    Amount = item.Amount,
                    Type = "Expense",
                    Month = item.Month,
                    Year = item.Year
                });
                totalCOGS += item.Amount;
            }

            // Add operating expense items
            foreach (var projection in projections)
            {
                if (projection.OperatingExpenses.HasValue && projection.OperatingExpenses.Value > 0)
                {
                    profitLossItems.Add(new ProfitLossItemDto
                    {
                        Category = "Operating Expenses",
                        Description = "General Operating Expenses",
                        Amount = projection.OperatingExpenses.Value,
                        Type = "Expense",
                        Month = projection.Month ?? 0,
                        Year = projection.Year
                    });
                }

                if (projection.MarketingExpenses.HasValue && projection.MarketingExpenses.Value > 0)
                {
                    profitLossItems.Add(new ProfitLossItemDto
                    {
                        Category = "Operating Expenses",
                        Description = "Marketing & Sales",
                        Amount = projection.MarketingExpenses.Value,
                        Type = "Expense",
                        Month = projection.Month ?? 0,
                        Year = projection.Year
                    });
                }

                if (projection.RAndDExpenses.HasValue && projection.RAndDExpenses.Value > 0)
                {
                    profitLossItems.Add(new ProfitLossItemDto
                    {
                        Category = "Operating Expenses",
                        Description = "Research & Development",
                        Amount = projection.RAndDExpenses.Value,
                        Type = "Expense",
                        Month = projection.Month ?? 0,
                        Year = projection.Year
                    });
                }

                if (projection.AdministrativeExpenses.HasValue && projection.AdministrativeExpenses.Value > 0)
                {
                    profitLossItems.Add(new ProfitLossItemDto
                    {
                        Category = "Operating Expenses",
                        Description = "Administrative Expenses",
                        Amount = projection.AdministrativeExpenses.Value,
                        Type = "Expense",
                        Month = projection.Month ?? 0,
                        Year = projection.Year
                    });
                }

                if (projection.OtherExpenses.HasValue && projection.OtherExpenses.Value > 0)
                {
                    profitLossItems.Add(new ProfitLossItemDto
                    {
                        Category = "Operating Expenses",
                        Description = "Other Expenses",
                        Amount = projection.OtherExpenses.Value,
                        Type = "Expense",
                        Month = projection.Month ?? 0,
                        Year = projection.Year
                    });
                }
            }

            // Add expense items from projection items
            var expenseItems = projectionItems
                .Where(i => (i.Category.Contains("Expense", StringComparison.OrdinalIgnoreCase) ||
                            i.ProjectionType.Contains("Expense", StringComparison.OrdinalIgnoreCase)) &&
                           !i.Category.Contains("COGS", StringComparison.OrdinalIgnoreCase) &&
                           !i.Category.Contains("Cost of Goods", StringComparison.OrdinalIgnoreCase));
            foreach (var item in expenseItems)
            {
                profitLossItems.Add(new ProfitLossItemDto
                {
                    Category = "Operating Expenses",
                    Description = item.Name,
                    Amount = item.Amount,
                    Type = "Expense",
                    Month = item.Month,
                    Year = item.Year
                });
                totalOperatingExpenses += item.Amount;
            }

            // Calculate P&L metrics
            var grossProfit = totalRevenue - totalCOGS;
            var operatingIncome = grossProfit - totalOperatingExpenses;

            // Interest and taxes (estimate if not available)
            decimal interestExpense = 0;
            decimal taxExpense = 0;

            // Check for interest/tax items in projection items
            var interestItems = projectionItems
                .Where(i => i.Category.Contains("Interest", StringComparison.OrdinalIgnoreCase));
            interestExpense = interestItems.Sum(i => i.Amount);

            var taxItems = projectionItems
                .Where(i => i.Category.Contains("Tax", StringComparison.OrdinalIgnoreCase));
            taxExpense = taxItems.Sum(i => i.Amount);

            // If no tax data, estimate at 20% of operating income (if positive)
            if (taxExpense == 0 && operatingIncome > 0)
            {
                taxExpense = operatingIncome * 0.20m;
            }

            var netIncome = operatingIncome - interestExpense - taxExpense;

            var report = new ProfitLossReportDto
            {
                BusinessPlanId = businessPlanId,
                Period = latestProjection?.Month.HasValue == true
                    ? $"{currentYear}-{currentMonth:D2}"
                    : $"{currentYear}",
                Currency = "CAD",
                Revenue = totalRevenue,
                CostOfGoodsSold = totalCOGS,
                GrossProfit = grossProfit,
                OperatingExpenses = totalOperatingExpenses,
                OperatingIncome = operatingIncome,
                InterestExpense = interestExpense,
                TaxExpense = taxExpense,
                NetIncome = netIncome,
                Items = profitLossItems,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully generated profit & loss report for business plan {BusinessPlanId}", businessPlanId);
            return Result<ProfitLossReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating profit & loss report for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<ProfitLossReportDto>(
                Error.InternalServerError("Financial.ProfitLossGenerationFailed", "Failed to generate profit & loss report"));
        }
    }

    public async Task<Result<BalanceSheetReportDto>> GenerateBalanceSheetReportAsync(Guid businessPlanId)
    {
        try
        {
            _logger.LogInformation("Generating balance sheet report for business plan {BusinessPlanId}", businessPlanId);

            // Verify business plan exists
            var businessPlan = await _context.BusinessPlans
                .FirstOrDefaultAsync(bp => bp.Id == businessPlanId);

            if (businessPlan == null)
            {
                return Result.Failure<BalanceSheetReportDto>(
                    Error.NotFound("Financial.BusinessPlanNotFound", $"Business plan with ID {businessPlanId} not found"));
            }

            // Get financial projections for the business plan
            var projections = await _context.BusinessPlanFinancialProjections
                .Where(fp => fp.BusinessPlanId == businessPlanId)
                .OrderByDescending(fp => fp.Year)
                .ThenByDescending(fp => fp.Month ?? 0)
                .ToListAsync();

            // Get financial projection items for more detailed breakdown
            var projectionItems = await _context.FinancialProjectionItems
                .Where(fpi => fpi.BusinessPlanId == businessPlanId)
                .ToListAsync();

            // Determine the most recent period
            var latestProjection = projections.FirstOrDefault();
            var currentYear = latestProjection?.Year ?? DateTime.UtcNow.Year;
            var currentMonth = latestProjection?.Month ?? DateTime.UtcNow.Month;

            // Build balance sheet from available data
            var assets = new List<BalanceSheetItemDto>();
            var liabilities = new List<BalanceSheetItemDto>();
            var equity = new List<BalanceSheetItemDto>();

            // Calculate assets from projections
            if (latestProjection != null)
            {
                // Cash and cash equivalents
                if (latestProjection.CashBalance.HasValue)
                {
                    assets.Add(new BalanceSheetItemDto
                    {
                        Category = "Current Assets",
                        Description = "Cash and Cash Equivalents",
                        Amount = latestProjection.CashBalance.Value,
                        Type = "Asset",
                        Month = currentMonth,
                        Year = currentYear
                    });
                }
            }

            // Add items from FinancialProjectionItems categorized as assets
            var assetItems = projectionItems
                .Where(i => i.Category.Contains("Asset", StringComparison.OrdinalIgnoreCase) ||
                           i.ProjectionType.Contains("Asset", StringComparison.OrdinalIgnoreCase))
                .GroupBy(i => i.SubCategory)
                .Select(g => new BalanceSheetItemDto
                {
                    Category = g.First().Category,
                    Description = string.IsNullOrEmpty(g.Key) ? g.First().Name : g.Key,
                    Amount = g.Sum(i => i.Amount),
                    Type = "Asset",
                    Month = g.Max(i => i.Month),
                    Year = g.Max(i => i.Year)
                });
            assets.AddRange(assetItems);

            // Add items from FinancialProjectionItems categorized as liabilities
            var liabilityItems = projectionItems
                .Where(i => i.Category.Contains("Liabilit", StringComparison.OrdinalIgnoreCase) ||
                           i.ProjectionType.Contains("Liabilit", StringComparison.OrdinalIgnoreCase) ||
                           i.Category.Contains("Debt", StringComparison.OrdinalIgnoreCase) ||
                           i.Category.Contains("Payable", StringComparison.OrdinalIgnoreCase))
                .GroupBy(i => i.SubCategory)
                .Select(g => new BalanceSheetItemDto
                {
                    Category = g.First().Category,
                    Description = string.IsNullOrEmpty(g.Key) ? g.First().Name : g.Key,
                    Amount = g.Sum(i => i.Amount),
                    Type = "Liability",
                    Month = g.Max(i => i.Month),
                    Year = g.Max(i => i.Year)
                });
            liabilities.AddRange(liabilityItems);

            // Calculate retained earnings from accumulated net income
            var totalNetIncome = projections
                .Where(p => p.NetIncome.HasValue)
                .Sum(p => p.NetIncome ?? 0);

            if (totalNetIncome != 0)
            {
                equity.Add(new BalanceSheetItemDto
                {
                    Category = "Retained Earnings",
                    Description = "Accumulated Net Income",
                    Amount = totalNetIncome,
                    Type = "Equity",
                    Month = currentMonth,
                    Year = currentYear
                });
            }

            // Add items from FinancialProjectionItems categorized as equity
            var equityItems = projectionItems
                .Where(i => i.Category.Contains("Equity", StringComparison.OrdinalIgnoreCase) ||
                           i.ProjectionType.Contains("Equity", StringComparison.OrdinalIgnoreCase) ||
                           i.Category.Contains("Capital", StringComparison.OrdinalIgnoreCase))
                .GroupBy(i => i.SubCategory)
                .Select(g => new BalanceSheetItemDto
                {
                    Category = g.First().Category,
                    Description = string.IsNullOrEmpty(g.Key) ? g.First().Name : g.Key,
                    Amount = g.Sum(i => i.Amount),
                    Type = "Equity",
                    Month = g.Max(i => i.Month),
                    Year = g.Max(i => i.Year)
                });
            equity.AddRange(equityItems);

            // Calculate totals
            var totalAssets = assets.Sum(a => a.Amount);
            var totalLiabilities = liabilities.Sum(l => l.Amount);
            var totalEquity = equity.Sum(e => e.Amount);

            // If balance sheet doesn't balance, add a balancing equity item
            var difference = totalAssets - totalLiabilities - totalEquity;
            if (difference != 0 && totalAssets > 0)
            {
                equity.Add(new BalanceSheetItemDto
                {
                    Category = "Owner's Equity",
                    Description = "Balancing Equity",
                    Amount = difference,
                    Type = "Equity",
                    Month = currentMonth,
                    Year = currentYear
                });
                totalEquity += difference;
            }

            var report = new BalanceSheetReportDto
            {
                BusinessPlanId = businessPlanId,
                Period = latestProjection?.Month.HasValue == true
                    ? $"{currentYear}-{currentMonth:D2}"
                    : $"{currentYear}",
                Currency = "CAD",
                TotalAssets = totalAssets,
                TotalLiabilities = totalLiabilities,
                TotalEquity = totalEquity,
                Assets = assets,
                Liabilities = liabilities,
                Equity = equity,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully generated balance sheet report for business plan {BusinessPlanId}", businessPlanId);
            return Result<BalanceSheetReportDto>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating balance sheet report for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<BalanceSheetReportDto>(
                Error.InternalServerError("Financial.BalanceSheetGenerationFailed", "Failed to generate balance sheet report"));
        }
    }

    // Scenario, Sensitivity, and Break-Even Analysis
    public async Task<Result<List<ScenarioAnalysisDto>>> PerformScenarioAnalysisAsync(Guid businessPlanId)
    {
        try
        {
            _logger.LogInformation("Performing scenario analysis for business plan {BusinessPlanId}", businessPlanId);

            var projections = await _context.BusinessPlanFinancialProjections
                .Where(p => p.BusinessPlanId == businessPlanId)
                .ToListAsync();

            if (!projections.Any())
            {
                return Result.Failure<List<ScenarioAnalysisDto>>(
                    Error.NotFound("Financial.NoProjections", "No financial projections found for this business plan"));
            }

            // Calculate base case metrics
            var baseRevenue = projections.Sum(p => p.Revenue ?? 0);
            var baseExpenses = projections.Sum(p =>
                (p.CostOfGoodsSold ?? 0) + (p.OperatingExpenses ?? 0) + (p.MarketingExpenses ?? 0) +
                (p.RAndDExpenses ?? 0) + (p.AdministrativeExpenses ?? 0) + (p.OtherExpenses ?? 0));
            var baseNetIncome = projections.Sum(p => p.NetIncome ?? 0);
            var baseCashFlow = projections.Sum(p => p.CashFlow ?? 0);

            var scenarios = new List<ScenarioAnalysisDto>
            {
                // Optimistic Scenario (+20% revenue, -10% expenses)
                CreateScenario("Optimistic", "Best case with increased revenue and reduced costs",
                    baseRevenue * 1.20m, baseExpenses * 0.90m, "Low",
                    new List<ScenarioVariableDto>
                    {
                        new() { Name = "Revenue Growth", Value = 20, Unit = "%", Impact = "Positive" },
                        new() { Name = "Cost Reduction", Value = 10, Unit = "%", Impact = "Positive" }
                    }),

                // Realistic Scenario (base case)
                CreateScenario("Realistic", "Base case based on current projections",
                    baseRevenue, baseExpenses, "Medium",
                    new List<ScenarioVariableDto>
                    {
                        new() { Name = "Revenue Growth", Value = 0, Unit = "%", Impact = "Neutral" },
                        new() { Name = "Cost Change", Value = 0, Unit = "%", Impact = "Neutral" }
                    }),

                // Pessimistic Scenario (-20% revenue, +15% expenses)
                CreateScenario("Pessimistic", "Worst case with reduced revenue and increased costs",
                    baseRevenue * 0.80m, baseExpenses * 1.15m, "High",
                    new List<ScenarioVariableDto>
                    {
                        new() { Name = "Revenue Decline", Value = -20, Unit = "%", Impact = "Negative" },
                        new() { Name = "Cost Increase", Value = 15, Unit = "%", Impact = "Negative" }
                    })
            };

            return Result<List<ScenarioAnalysisDto>>.Success(scenarios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing scenario analysis for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<List<ScenarioAnalysisDto>>(
                Error.InternalServerError("Analysis.ScenarioFailed", "Failed to perform scenario analysis"));
        }
    }

    private static ScenarioAnalysisDto CreateScenario(string name, string description, decimal revenue, decimal expenses, string riskLevel, List<ScenarioVariableDto> variables)
    {
        var netIncome = revenue - expenses;
        var cashFlow = netIncome * 0.85m; // Approximate cash flow
        var roi = expenses > 0 ? ((netIncome / expenses) * 100) : 0;
        var npv = CalculateNPV(expenses, netIncome * 5, 10, 5); // 5-year projection
        var irr = CalculateIRR(expenses, netIncome * 5, 5);

        return new ScenarioAnalysisDto
        {
            Scenario = name,
            Description = description,
            Revenue = Math.Round(revenue, 2),
            Expenses = Math.Round(expenses, 2),
            NetIncome = Math.Round(netIncome, 2),
            CashFlow = Math.Round(cashFlow, 2),
            ROI = Math.Round(roi, 2),
            NPV = Math.Round(npv, 2),
            IRR = Math.Round(irr, 2),
            Currency = "CAD",
            RiskLevel = riskLevel,
            Assumptions = $"Based on {name.ToLower()} market conditions",
            Variables = variables
        };
    }

    public async Task<Result<SensitivityAnalysisDto>> PerformSensitivityAnalysisAsync(Guid businessPlanId, string variable)
    {
        try
        {
            _logger.LogInformation("Performing sensitivity analysis for {Variable} on business plan {BusinessPlanId}", variable, businessPlanId);

            var projections = await _context.BusinessPlanFinancialProjections
                .Where(p => p.BusinessPlanId == businessPlanId)
                .ToListAsync();

            if (!projections.Any())
            {
                return Result.Failure<SensitivityAnalysisDto>(
                    Error.NotFound("Financial.NoProjections", "No financial projections found for this business plan"));
            }

            var baseRevenue = projections.Sum(p => p.Revenue ?? 0);
            var baseExpenses = projections.Sum(p =>
                (p.CostOfGoodsSold ?? 0) + (p.OperatingExpenses ?? 0) + (p.MarketingExpenses ?? 0) +
                (p.RAndDExpenses ?? 0) + (p.AdministrativeExpenses ?? 0) + (p.OtherExpenses ?? 0));
            var baseNetIncome = baseRevenue - baseExpenses;

            decimal baseValue;
            string unit;
            string description;

            switch (variable.ToLowerInvariant())
            {
                case "revenue":
                    baseValue = baseRevenue;
                    unit = "CAD";
                    description = "Impact of revenue changes on financial metrics";
                    break;
                case "expenses":
                case "costs":
                    baseValue = baseExpenses;
                    unit = "CAD";
                    description = "Impact of expense changes on financial metrics";
                    break;
                case "price":
                case "pricing":
                    baseValue = 100; // Assume base price index
                    unit = "%";
                    description = "Impact of pricing changes on financial metrics";
                    break;
                default:
                    baseValue = baseRevenue;
                    unit = "CAD";
                    description = $"Sensitivity analysis for {variable}";
                    break;
            }

            // Generate sensitivity points (-30% to +30%)
            var points = new List<SensitivityPointDto>();
            for (decimal change = -30; change <= 30; change += 10)
            {
                var adjustedValue = baseValue * (1 + change / 100);
                var adjustedNetIncome = variable.ToLowerInvariant() == "revenue" || variable.ToLowerInvariant() == "price"
                    ? adjustedValue - baseExpenses
                    : baseRevenue - adjustedValue;

                var roi = baseExpenses > 0 ? (adjustedNetIncome / baseExpenses) * 100 : 0;
                var npv = CalculateNPV(baseExpenses, adjustedNetIncome * 5, 10, 5);
                var irr = CalculateIRR(baseExpenses, adjustedNetIncome * 5, 5);
                var payback = adjustedNetIncome > 0 ? baseExpenses / adjustedNetIncome : 999;

                points.Add(new SensitivityPointDto
                {
                    VariableValue = Math.Round(adjustedValue, 2),
                    NPV = Math.Round(npv, 2),
                    IRR = Math.Round(irr, 2),
                    ROI = Math.Round(roi, 2),
                    PaybackPeriod = Math.Round(Math.Min(payback, 99), 2)
                });
            }

            var analysis = new SensitivityAnalysisDto
            {
                Variable = variable,
                Description = description,
                BaseValue = Math.Round(baseValue, 2),
                MinValue = Math.Round(baseValue * 0.70m, 2),
                MaxValue = Math.Round(baseValue * 1.30m, 2),
                Step = Math.Round(baseValue * 0.10m, 2),
                Points = points,
                Currency = "CAD",
                Unit = unit,
                Impact = "Variable"
            };

            return Result<SensitivityAnalysisDto>.Success(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing sensitivity analysis for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<SensitivityAnalysisDto>(
                Error.InternalServerError("Analysis.SensitivityFailed", "Failed to perform sensitivity analysis"));
        }
    }

    public async Task<Result<BreakEvenAnalysisDto>> CalculateBreakEvenAsync(Guid businessPlanId)
    {
        try
        {
            _logger.LogInformation("Calculating break-even for business plan {BusinessPlanId}", businessPlanId);

            var projections = await _context.BusinessPlanFinancialProjections
                .Where(p => p.BusinessPlanId == businessPlanId)
                .OrderBy(p => p.Year)
                .ThenBy(p => p.Month ?? 0)
                .ToListAsync();

            var projectionItems = await _context.FinancialProjectionItems
                .Where(p => p.BusinessPlanId == businessPlanId)
                .ToListAsync();

            if (!projections.Any() && !projectionItems.Any())
            {
                return Result.Failure<BreakEvenAnalysisDto>(
                    Error.NotFound("Financial.NoProjections", "No financial projections found for this business plan"));
            }

            // Calculate fixed and variable costs
            var fixedCosts = projections.Sum(p =>
                (p.AdministrativeExpenses ?? 0) + (p.RAndDExpenses ?? 0));

            var variableCosts = projections.Sum(p =>
                (p.CostOfGoodsSold ?? 0) + (p.MarketingExpenses ?? 0));

            var totalRevenue = projections.Sum(p => p.Revenue ?? 0);
            var totalUnits = projections.Sum(p => p.UnitsSold ?? 0);

            // If no units data, estimate based on revenue
            if (totalUnits == 0)
            {
                totalUnits = totalRevenue > 0 ? (int)(totalRevenue / 100) : 1000; // Assume $100 average price
            }

            var sellingPrice = totalUnits > 0 ? totalRevenue / totalUnits : 100;
            var variableCostPerUnit = totalUnits > 0 ? variableCosts / totalUnits : 50;
            var contributionMargin = sellingPrice - variableCostPerUnit;
            var contributionMarginRatio = sellingPrice > 0 ? contributionMargin / sellingPrice : 0;

            var breakEvenUnits = contributionMargin > 0 ? fixedCosts / contributionMargin : 0;
            var breakEvenRevenue = breakEvenUnits * sellingPrice;

            // Generate break-even points chart data
            var points = new List<BreakEvenPointDto>();
            var maxUnits = breakEvenUnits * 2;
            var step = maxUnits / 10;

            for (decimal units = 0; units <= maxUnits; units += step)
            {
                var revenue = units * sellingPrice;
                var totalCosts = fixedCosts + (units * variableCostPerUnit);
                var profit = revenue - totalCosts;

                points.Add(new BreakEvenPointDto
                {
                    Units = Math.Round(units, 0),
                    Revenue = Math.Round(revenue, 2),
                    TotalCosts = Math.Round(totalCosts, 2),
                    Profit = Math.Round(profit, 2),
                    Year = DateTime.UtcNow.Year,
                    Month = DateTime.UtcNow.Month
                });
            }

            var analysis = new BreakEvenAnalysisDto
            {
                BusinessPlanId = businessPlanId,
                FixedCosts = Math.Round(fixedCosts, 2),
                VariableCostsPerUnit = Math.Round(variableCostPerUnit, 2),
                SellingPricePerUnit = Math.Round(sellingPrice, 2),
                BreakEvenUnits = Math.Round(breakEvenUnits, 0),
                BreakEvenRevenue = Math.Round(breakEvenRevenue, 2),
                ContributionMargin = Math.Round(contributionMargin, 2),
                ContributionMarginRatio = Math.Round(contributionMarginRatio * 100, 2),
                Currency = "CAD",
                Timeframe = "Annual",
                Points = points,
                Notes = $"Break-even point: {breakEvenUnits:N0} units or {breakEvenRevenue:C} in revenue"
            };

            return Result<BreakEvenAnalysisDto>.Success(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating break-even for business plan {BusinessPlanId}", businessPlanId);
            return Result.Failure<BreakEvenAnalysisDto>(
                Error.InternalServerError("Analysis.BreakEvenFailed", "Failed to calculate break-even"));
        }
    }

    public async Task<Result<ConsultantFinancialProjectionDto>> CalculateConsultantFinancialsAsync(CalculateConsultantFinancialsRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating consultant financials for {City}, {Province}", request.City, request.Province);

            // Get location overhead estimate
            var overheadResult = await GetLocationOverheadEstimateAsync(request.City, request.Province);
            if (!overheadResult.IsSuccess)
            {
                return Result.Failure<ConsultantFinancialProjectionDto>(overheadResult.Error!);
            }

            var overhead = overheadResult.Value!;

            // Calculate billable hours per month (assuming 160 working hours/month)
            const decimal workingHoursPerMonth = 160m;
            var billableHoursPerMonth = (request.UtilizationPercent / 100m) * workingHoursPerMonth;

            // Calculate revenue
            var monthlyRevenue = billableHoursPerMonth * request.HourlyRate;
            var yearlyRevenue = monthlyRevenue * 12m;

            // Calculate expenses
            var monthlyOverhead = monthlyRevenue * (overhead.OverheadRate / 100m);
            var monthlyClientAcquisition = request.ClientAcquisitionCost / 12m; // Average per month
            var monthlyInsurance = overhead.InsuranceRate;
            var monthlySoftware = 150m; // Fixed estimate
            var monthlyTaxes = monthlyRevenue * (overhead.TaxRate / 100m);
            var monthlyOffice = overhead.OfficeCost;

            var monthlyExpenses = monthlyOverhead + monthlyClientAcquisition + monthlyInsurance +
                                 monthlySoftware + monthlyTaxes + monthlyOffice;
            var yearlyExpenses = monthlyExpenses * 12m;
            var netIncome = yearlyRevenue - yearlyExpenses;

            var projection = new ConsultantFinancialProjectionDto
            {
                MonthlyRevenue = monthlyRevenue,
                YearlyRevenue = yearlyRevenue,
                MonthlyExpenses = monthlyExpenses,
                YearlyExpenses = yearlyExpenses,
                NetIncome = netIncome,
                Breakdown = new ConsultantFinancialBreakdownDto
                {
                    Revenue = new RevenueBreakdownDto
                    {
                        BillableHours = billableHoursPerMonth,
                        HourlyRate = request.HourlyRate,
                        UtilizationPercent = request.UtilizationPercent
                    },
                    Expenses = new ExpenseBreakdownDto
                    {
                        Overhead = monthlyOverhead,
                        ClientAcquisition = monthlyClientAcquisition,
                        Insurance = monthlyInsurance,
                        Software = monthlySoftware,
                        Taxes = monthlyTaxes,
                        Office = monthlyOffice
                    }
                }
            };

            return Result<ConsultantFinancialProjectionDto>.Success(projection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating consultant financials");
            return Result.Failure<ConsultantFinancialProjectionDto>(
                Error.InternalServerError("Financial.CalculationFailed", "Failed to calculate consultant financials"));
        }
    }

    public async Task<Result<LocationOverheadEstimateDto>> GetLocationOverheadEstimateAsync(string city, string province)
    {
        try
        {
            _logger.LogInformation("Getting location overhead estimate for {City}, {Province}", city, province);

            // Query LocationOverheadRates table - try by province name first, then by code
            var rate = await _context.LocationOverheadRates
                .Where(r => r.IsActive &&
                           (r.Province.ToLower() == province.ToLower() ||
                            r.ProvinceCode.ToLower() == province.ToLower()))
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefaultAsync();

            if (rate != null)
            {
                return Result<LocationOverheadEstimateDto>.Success(new LocationOverheadEstimateDto
                {
                    City = city,
                    Province = rate.Province,
                    OverheadRate = rate.OverheadRate,
                    InsuranceRate = rate.InsuranceRate,
                    TaxRate = rate.TaxRate,
                    OfficeCost = rate.OfficeCost,
                    Currency = rate.Currency
                });
            }

            // Fall back to hardcoded defaults if not found in database
            _logger.LogWarning("No overhead rate found for province {Province}, using defaults", province);
            var defaults = GetDefaultOverheadRates(province);

            return Result<LocationOverheadEstimateDto>.Success(new LocationOverheadEstimateDto
            {
                City = city,
                Province = province,
                OverheadRate = defaults.OverheadRate,
                InsuranceRate = defaults.InsuranceRate,
                TaxRate = defaults.TaxRate,
                OfficeCost = defaults.OfficeCost,
                Currency = "CAD"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location overhead estimate");
            return Result.Failure<LocationOverheadEstimateDto>(
                Error.InternalServerError("Financial.OverheadEstimateFailed", "Failed to get location overhead estimate"));
        }
    }

    public async Task<Result<object>> UpdateFinancialCellAsync(Guid planId, UpdateFinancialCellRequest request)
    {
        try
        {
            _logger.LogInformation("Updating financial cell {RowId}/{CellId} for plan {PlanId}", request.RowId, request.CellId, planId);

            // Get or create the cell
            var existingCell = await _context.FinancialCells
                .FirstOrDefaultAsync(c => c.BusinessPlanId == planId &&
                                         c.RowId == request.RowId &&
                                         c.ColumnId == request.CellId);

            var isNewCell = existingCell == null;
            var cell = existingCell ?? new Domain.Entities.FinancialCell
            {
                BusinessPlanId = planId,
                RowId = request.RowId,
                ColumnId = request.CellId,
                SheetName = request.SheetName ?? "Main",
                CellType = request.CellType ?? "number",
                CreatedAt = DateTime.UtcNow
            };

            if (isNewCell)
            {
                _context.FinancialCells.Add(cell);
            }

            // Check if locked
            if (cell.IsLocked)
            {
                return Result.Failure<object>(
                    Error.Validation("Financial.CellLocked", "This cell is locked and cannot be edited"));
            }

            // Update cell with formula or value
            if (!string.IsNullOrWhiteSpace(request.Formula))
            {
                // Validate formula syntax
                var validation = _formulaEngine.ValidateFormula(request.Formula);
                if (!validation.IsValid)
                {
                    return Result.Failure<object>(
                        Error.Validation("Financial.InvalidFormula", validation.ErrorMessage ?? "Invalid formula syntax"));
                }

                // Check for circular dependencies
                var allCells = await _context.FinancialCells
                    .Where(c => c.BusinessPlanId == planId)
                    .ToListAsync();

                if (_formulaEngine.WouldCreateCircularDependency(cell.GetCellReference(), request.Formula, allCells))
                {
                    return Result.Failure<object>(
                        Error.Validation("Financial.CircularDependency", "This formula would create a circular dependency"));
                }

                cell.Formula = request.Formula;
                cell.IsCalculated = true;
            }
            else
            {
                cell.Formula = null;
                cell.IsCalculated = false;
            }

            cell.Value = request.Value;
            cell.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Recalculate dependent cells
            var allCellsAfterUpdate = await _context.FinancialCells
                .Where(c => c.BusinessPlanId == planId)
                .ToListAsync();

            var updatedValues = _formulaEngine.RecalculateDependents(cell, cell.Value, allCellsAfterUpdate);

            // Update dependent cells in database
            var updatedCells = new List<object>();
            foreach (var kvp in updatedValues)
            {
                var depCell = allCellsAfterUpdate.FirstOrDefault(c => c.GetCellReference() == kvp.Key);
                if (depCell != null && depCell.Id != cell.Id)
                {
                    depCell.Value = kvp.Value;
                    depCell.UpdatedAt = DateTime.UtcNow;
                    updatedCells.Add(new
                    {
                        rowId = depCell.RowId,
                        cellId = depCell.ColumnId,
                        value = kvp.Value
                    });
                }
            }

            if (updatedCells.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Updated cell and {Count} dependent cells for plan {PlanId}",
                updatedCells.Count, planId);

            var result = new
            {
                planId,
                rowId = request.RowId,
                cellId = request.CellId,
                value = cell.Value,
                formula = cell.Formula,
                isCalculated = cell.IsCalculated,
                updatedAt = cell.UpdatedAt,
                updatedCells
            };

            return Result.Success<object>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating financial cell");
            return Result.Failure<object>(
                Error.InternalServerError("Financial.CellUpdateFailed", "Failed to update financial cell"));
        }
    }

    private (decimal OverheadRate, decimal InsuranceRate, decimal TaxRate, decimal OfficeCost) GetDefaultOverheadRates(string province)
    {
        // Default rates by province (simplified)
        return province.ToUpperInvariant() switch
        {
            "QUEBEC" => (15m, 200m, 25m, 500m), // 15% overhead, $200 insurance, 25% tax, $500 office
            "ONTARIO" => (12m, 180m, 20m, 600m),
            "BRITISH COLUMBIA" => (13m, 190m, 22m, 700m),
            "ALBERTA" => (10m, 150m, 15m, 500m),
            _ => (12m, 175m, 20m, 550m) // Default for other provinces
        };
    }
}

