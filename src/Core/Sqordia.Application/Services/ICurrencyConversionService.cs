using Sqordia.Application.Common.Models;

namespace Sqordia.Application.Services;

/// <summary>
/// Service for currency conversion and exchange rate management
/// </summary>
public interface ICurrencyConversionService
{
    /// <summary>
    /// Converts an amount from one currency to another
    /// </summary>
    /// <param name="amount">Amount to convert</param>
    /// <param name="fromCurrencyCode">Source currency code (e.g., "USD")</param>
    /// <param name="toCurrencyCode">Target currency code (e.g., "EUR")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Converted amount</returns>
    Task<Result<decimal>> ConvertCurrencyAsync(
        decimal amount,
        string fromCurrencyCode,
        string toCurrencyCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current exchange rate between two currencies
    /// </summary>
    /// <param name="fromCurrencyCode">Source currency code</param>
    /// <param name="toCurrencyCode">Target currency code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Exchange rate</returns>
    Task<Result<decimal>> GetExchangeRateAsync(
        string fromCurrencyCode,
        string toCurrencyCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts all amounts in a list to a base currency
    /// </summary>
    /// <param name="amounts">Dictionary of currency code to amount</param>
    /// <param name="baseCurrencyCode">Target currency code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of converted amounts</returns>
    Task<Result<Dictionary<string, decimal>>> ConvertToBaseCurrencyAsync(
        Dictionary<string, decimal> amounts,
        string baseCurrencyCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates exchange rates from an external source
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rates updated</returns>
    Task<Result<int>> UpdateExchangeRatesAsync(CancellationToken cancellationToken = default);
}

