using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using MetaReport.Services;

namespace MetaReport.Functions;

/// <summary>
/// HTTP-triggered function for on-demand trading reports.
/// Call GET /api/report to trigger an immediate report.
/// </summary>
public class ManualReportFunction
{
    private readonly IMetaApiService _metaApiService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ManualReportFunction> _logger;

    public ManualReportFunction(
        IMetaApiService metaApiService,
        IEmailService emailService,
        ILogger<ManualReportFunction> logger)
    {
        _metaApiService = metaApiService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// HTTP GET endpoint to trigger an immediate trading report.
    /// Requires function-level authentication key.
    /// </summary>
    /// <param name="req">HTTP request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTTP response with result status.</returns>
    [Function("ManualReport")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "report")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("ManualReport function triggered via HTTP at {Time}", DateTime.UtcNow);

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
                _logger.LogInformation("Manual report email sent successfully");

                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new
                {
                    success = true,
                    message = "Trading report sent successfully",
                    summary = new
                    {
                        balance = report.Account.Balance,
                        equity = report.Account.Equity,
                        currency = report.Account.Currency,
                        tradeCount = report.TradeCount,
                        totalProfit = report.TotalProfit,
                        winRate = report.WinRate,
                        generatedAt = report.GeneratedAt
                    }
                }, cancellationToken);
                return successResponse;
            }
            else
            {
                _logger.LogError("Failed to send manual report email");

                var failResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await failResponse.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Failed to send email. Check logs for details."
                }, cancellationToken);
                return failResponse;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing ManualReport function");

            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new
            {
                success = false,
                message = "An error occurred while generating the report",
                error = ex.Message
            }, cancellationToken);
            return errorResponse;
        }
    }
}
