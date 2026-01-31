using MetaReport.Models;

namespace MetaReport.Services;

/// <summary>
/// Service interface for interacting with MetaAPI.
/// </summary>
public interface IMetaApiService
{
    /// <summary>
    /// Gets current account information (balance, equity, margin, etc.).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Account information.</returns>
    Task<AccountInfo> GetAccountInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets deal history for a specified time period.
    /// </summary>
    /// <param name="startTime">Start of the period (inclusive).</param>
    /// <param name="endTime">End of the period (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of deals in the period.</returns>
    Task<List<Deal>> GetDealsAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets deals from the last 24 hours.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of deals from the last 24 hours.</returns>
    Task<List<Deal>> GetLast24HoursDealsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a complete trading report with account info and recent deals.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Complete trading report.</returns>
    Task<TradingReport> GenerateReportAsync(CancellationToken cancellationToken = default);
}
