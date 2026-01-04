using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sqordia.Application.Common.Interfaces;
using Sqordia.Application.Common.Models;
using Sqordia.Application.Services;
using Sqordia.Domain.Entities;
using Error = Sqordia.Application.Common.Models.Error;

namespace Sqordia.Application.Services.Implementations;

public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CurrencyConversionService> _logger;
    private const string DefaultBaseCurrency = "USD";

    public CurrencyConversionService(
        IApplicationDbContext context,
        ILogger<CurrencyConversionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<decimal>> ConvertCurrencyAsync(
        decimal amount,
        string fromCurrencyCode,
        string toCurrencyCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (fromCurrencyCode == toCurrencyCode)
            {
                return Result.Success(amount);
            }

            var exchangeRateResult = await GetExchangeRateAsync(fromCurrencyCode, toCurrencyCode, cancellationToken);
            if (!exchangeRateResult.IsSuccess)
            {
                return Result.Failure<decimal>(exchangeRateResult.Error ?? Error.Failure("Currency.ConversionFailed", "Failed to get exchange rate"));
            }

            var convertedAmount = amount * exchangeRateResult.Value;
            
            _logger.LogInformation(
                "Converted {Amount} {FromCurrency} to {ConvertedAmount} {ToCurrency} (Rate: {Rate})",
                amount, fromCurrencyCode, convertedAmount, toCurrencyCode, exchangeRateResult.Value);

            return Result.Success(convertedAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting currency from {FromCurrency} to {ToCurrency}", 
                fromCurrencyCode, toCurrencyCode);
            return Result.Failure<decimal>($"Currency conversion failed: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> GetExchangeRateAsync(
        string fromCurrencyCode,
        string toCurrencyCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (fromCurrencyCode == toCurrencyCode)
            {
                return Result.Success(1m);
            }

            // Normalize currency codes
            fromCurrencyCode = fromCurrencyCode.ToUpperInvariant();
            toCurrencyCode = toCurrencyCode.ToUpperInvariant();

            // Try to get from database first
            var fromCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Code == fromCurrencyCode && c.IsActive, cancellationToken);

            var toCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Code == toCurrencyCode && c.IsActive, cancellationToken);

            if (fromCurrency == null || toCurrency == null)
            {
                // Fallback to hardcoded rates for common currencies
                return GetFallbackExchangeRate(fromCurrencyCode, toCurrencyCode);
            }

            // Check for direct exchange rate
            var exchangeRate = await _context.ExchangeRates
                .Include(er => er.FromCurrency)
                .Include(er => er.ToCurrency)
                .FirstOrDefaultAsync(er =>
                    er.FromCurrency.Code == fromCurrencyCode &&
                    er.ToCurrency.Code == toCurrencyCode &&
                    er.IsActive &&
                    er.EffectiveDate <= DateTime.UtcNow &&
                    (er.ExpiryDate == default || er.ExpiryDate >= DateTime.UtcNow),
                    cancellationToken);

            if (exchangeRate != null)
            {
                return Result.Success(exchangeRate.Rate);
            }

            // Try inverse rate
            var inverseRate = await _context.ExchangeRates
                .Include(er => er.FromCurrency)
                .Include(er => er.ToCurrency)
                .FirstOrDefaultAsync(er =>
                    er.FromCurrency.Code == toCurrencyCode &&
                    er.ToCurrency.Code == fromCurrencyCode &&
                    er.IsActive &&
                    er.EffectiveDate <= DateTime.UtcNow &&
                    (er.ExpiryDate == default || er.ExpiryDate >= DateTime.UtcNow),
                    cancellationToken);

            if (inverseRate != null)
            {
                return Result.Success(inverseRate.InverseRate);
            }

            // Calculate via base currency (USD)
            if (fromCurrencyCode != DefaultBaseCurrency && toCurrencyCode != DefaultBaseCurrency)
            {
                var fromToBase = await GetExchangeRateAsync(fromCurrencyCode, DefaultBaseCurrency, cancellationToken);
                var baseToTo = await GetExchangeRateAsync(DefaultBaseCurrency, toCurrencyCode, cancellationToken);

                if (fromToBase.IsSuccess && baseToTo.IsSuccess)
                {
                    return Result.Success(fromToBase.Value * baseToTo.Value);
                }
            }

            // Fallback to hardcoded rates
            return GetFallbackExchangeRate(fromCurrencyCode, toCurrencyCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exchange rate from {FromCurrency} to {ToCurrency}",
                fromCurrencyCode, toCurrencyCode);
            return Result.Failure<decimal>($"Failed to get exchange rate: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, decimal>>> ConvertToBaseCurrencyAsync(
        Dictionary<string, decimal> amounts,
        string baseCurrencyCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var convertedAmounts = new Dictionary<string, decimal>();

            foreach (var kvp in amounts)
            {
                var conversionResult = await ConvertCurrencyAsync(
                    kvp.Value,
                    kvp.Key,
                    baseCurrencyCode,
                    cancellationToken);

                if (conversionResult.IsSuccess)
                {
                    convertedAmounts[kvp.Key] = conversionResult.Value;
                }
                else
                {
                    _logger.LogWarning("Failed to convert {Amount} {Currency} to {BaseCurrency}: {Error}",
                        kvp.Value, kvp.Key, baseCurrencyCode, conversionResult.Error);
                }
            }

            return Result.Success(convertedAmounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting amounts to base currency {BaseCurrency}", baseCurrencyCode);
            return Result.Failure<Dictionary<string, decimal>>($"Currency conversion failed: {ex.Message}");
        }
    }

    public async Task<Result<int>> UpdateExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // In production, this would fetch rates from an external API (e.g., ExchangeRate-API, Fixer.io)
            // For now, we'll use hardcoded rates as a fallback
            
            _logger.LogInformation("Exchange rate update requested - using fallback rates");
            
            // This is a placeholder - in production, implement actual API integration
            // Example: await FetchRatesFromAPIAsync(cancellationToken);
            
            // Acknowledge cancellation token
            await Task.CompletedTask;
            
            return Result.Success(0); // Return 0 as we're using fallback rates
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating exchange rates");
            return Result.Failure<int>($"Failed to update exchange rates: {ex.Message}");
        }
    }

    private Result<decimal> GetFallbackExchangeRate(string fromCurrency, string toCurrency)
    {
        // Fallback exchange rates (as of common rates - should be updated regularly)
        // These are approximate rates and should be replaced with real-time data
        var rates = new Dictionary<string, decimal>
        {
            { "USD", 1.0m },
            { "EUR", 0.85m },
            { "GBP", 0.73m },
            { "CAD", 1.25m },
            { "AUD", 1.35m },
            { "JPY", 110.0m },
            { "CHF", 0.92m },
            { "CNY", 6.45m },
            { "INR", 74.0m },
            { "MXN", 20.0m }
        };

        if (!rates.ContainsKey(fromCurrency) || !rates.ContainsKey(toCurrency))
        {
            _logger.LogWarning("Exchange rate not available for {FromCurrency} to {ToCurrency}, using 1:1",
                fromCurrency, toCurrency);
            return Result.Success(1m); // Default to 1:1 if currency not found
        }

        // Convert via USD base
        var fromRate = rates[fromCurrency];
        var toRate = rates[toCurrency];
        var exchangeRate = toRate / fromRate;

        _logger.LogInformation("Using fallback exchange rate: {FromCurrency} to {ToCurrency} = {Rate}",
            fromCurrency, toCurrency, exchangeRate);

        return Result.Success(exchangeRate);
    }
}

