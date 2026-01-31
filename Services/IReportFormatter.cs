using MetaReport.Models;

namespace MetaReport.Services;

/// <summary>
/// Formats trading reports into email content.
/// </summary>
public interface IReportFormatter
{
    /// <summary>
    /// Builds plain text email content from a trading report.
    /// </summary>
    /// <param name="report">The trading report to format.</param>
    /// <returns>Plain text formatted content.</returns>
    string BuildPlainTextContent(TradingReport report);

    /// <summary>
    /// Builds HTML email content from a trading report.
    /// </summary>
    /// <param name="report">The trading report to format.</param>
    /// <returns>HTML formatted content.</returns>
    string BuildHtmlContent(TradingReport report);

    /// <summary>
    /// Gets the timezone abbreviation for display (e.g., "COT", "EST").
    /// </summary>
    /// <returns>Short timezone abbreviation.</returns>
    string GetTimeZoneAbbreviation();

    /// <summary>
    /// Converts a UTC time to the configured local timezone.
    /// </summary>
    /// <param name="utcTime">The UTC time to convert.</param>
    /// <returns>The time in the configured local timezone.</returns>
    DateTime ToLocalTime(DateTime utcTime);
}
