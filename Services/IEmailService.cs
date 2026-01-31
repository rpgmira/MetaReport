using MetaReport.Models;

namespace MetaReport.Services;

/// <summary>
/// Service interface for sending email reports.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a trading report email.
    /// </summary>
    /// <param name="report">The trading report to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if email was sent successfully.</returns>
    Task<bool> SendReportAsync(TradingReport report, CancellationToken cancellationToken = default);
}
