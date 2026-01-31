using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MetaReport.Services;

namespace MetaReport.Functions;

/// <summary>
/// Timer-triggered function that sends daily trading reports.
/// Runs at a configurable time (default: 8 PM daily in configured timezone).
/// </summary>
public class DailyReportFunction
{
    private readonly IMetaApiService _metaApiService;
    private readonly IEmailService _emailService;
    private readonly ILogger<DailyReportFunction> _logger;

    public DailyReportFunction(
        IMetaApiService metaApiService,
        IEmailService emailService,
        ILogger<DailyReportFunction> logger)
    {
        _metaApiService = metaApiService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Timer trigger that runs according to the ScheduleCronExpression setting.
    /// Default: "0 0 20 * * *" (8:00 PM daily).
    /// Configure WEBSITE_TIME_ZONE to set the timezone (e.g., "SA Pacific Standard Time" for Bogota).
    /// </summary>
    [Function("DailyReport")]
    public async Task Run(
        [TimerTrigger("%ScheduleCronExpression%")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("DailyReport function triggered at {Time}", DateTime.UtcNow);

        if (timerInfo.IsPastDue)
        {
            _logger.LogWarning("Timer is running late! Past due execution detected.");
        }

        try
        {
            // Generate the trading report
            var report = await _metaApiService.GenerateReportAsync(cancellationToken);

            _logger.LogInformation(
                "Report generated: Balance={Balance}, Trades={TradeCount}, P/L={TotalProfit}",
                report.Account.Balance,
                report.TradeCount,
                report.TotalProfit);

            // Send the email
            var sent = await _emailService.SendReportAsync(report, cancellationToken);

            if (sent)
            {
                _logger.LogInformation("Daily report email sent successfully");
            }
            else
            {
                _logger.LogError("Failed to send daily report email");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing DailyReport function");
            throw;
        }

        _logger.LogInformation("Next scheduled run: {NextRun}", timerInfo.ScheduleStatus?.Next);
    }
}
