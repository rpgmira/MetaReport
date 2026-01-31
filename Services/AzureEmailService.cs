using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MetaReport.Models;
using MetaReport.Models.Options;
using System.Text;

namespace MetaReport.Services;

/// <summary>
/// Email service implementation using Azure Communication Services.
/// </summary>
public class AzureEmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly EmailOptions _options;
    private readonly ILogger<AzureEmailService> _logger;
    private readonly TimeZoneInfo _timeZone;

    public AzureEmailService(
        EmailClient emailClient,
        IOptions<EmailOptions> options,
        IConfiguration configuration,
        ILogger<AzureEmailService> logger)
    {
        _emailClient = emailClient;
        _options = options.Value;
        _logger = logger;
        
        // Read timezone from WEBSITE_TIME_ZONE configuration (defaults to UTC if not set)
        var timeZoneId = configuration["WEBSITE_TIME_ZONE"] ?? "UTC";
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        _logger.LogDebug("Using timezone: {TimeZone}", _timeZone.DisplayName);
    }

    /// <inheritdoc />
    public async Task<bool> SendReportAsync(TradingReport report, CancellationToken cancellationToken = default)
    {
        var recipients = _options.GetRecipients();
        
        if (recipients.Count == 0)
        {
            _logger.LogError("No recipients configured for email");
            return false;
        }
        
        _logger.LogInformation("Preparing email report for {RecipientCount} recipients: {Recipients}", 
            recipients.Count, string.Join(", ", recipients));

        var subject = $"📊 MetaReport - Daily Trading Summary ({report.GeneratedAt:yyyy-MM-dd})";
        var plainTextContent = BuildPlainTextContent(report);
        var htmlContent = BuildHtmlContent(report);

        try
        {
            // Build recipient list
            var toRecipients = recipients.Select(email => new EmailAddress(email)).ToList();

            var emailMessage = new EmailMessage(
                senderAddress: _options.FromAddress,
                content: new EmailContent(subject)
                {
                    PlainText = plainTextContent,
                    Html = htmlContent
                },
                recipients: new EmailRecipients(toRecipients));

            // Send email and wait for completion
            var operation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                emailMessage,
                cancellationToken);

            if (operation.HasCompleted && operation.Value.Status == EmailSendStatus.Succeeded)
            {
                _logger.LogInformation("Email sent successfully to {RecipientCount} recipients via Azure Communication Services", 
                    recipients.Count);
                return true;
            }
            else
            {
                _logger.LogError("Failed to send email. Status: {Status}", operation.Value.Status);
                return false;
            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Communication Services error: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email");
            throw;
        }
    }

    private DateTime ToLocalTime(DateTime utcTime) => TimeZoneInfo.ConvertTimeFromUtc(utcTime, _timeZone);
    
    private string TimeZoneDisplayName => _timeZone.DisplayName;
    
    // Get a short timezone name (e.g., "COT" for SA Pacific Standard Time)
    private string GetTimeZoneAbbreviation()
    {
        // Map common timezone IDs to their abbreviations
        return _timeZone.Id switch
        {
            "SA Pacific Standard Time" => "COT",  // Colombia Time
            "Eastern Standard Time" => "EST",
            "Pacific Standard Time" => "PST",
            "Central Standard Time" => "CST",
            "UTC" => "UTC",
            _ => _timeZone.StandardName.Length <= 5 ? _timeZone.StandardName : _timeZone.Id
        };
    }
    
    private string BuildPlainTextContent(TradingReport report)
    {
        var sb = new StringBuilder();
        var localGeneratedAt = ToLocalTime(report.GeneratedAt);
        var localPeriodStart = ToLocalTime(report.PeriodStart);
        var localPeriodEnd = ToLocalTime(report.PeriodEnd);
        var tzAbbreviation = GetTimeZoneAbbreviation();
        
        sb.AppendLine("=== MetaReport - Daily Trading Summary ===");
        sb.AppendLine();
        sb.AppendLine($"Report Generated: {localGeneratedAt:yyyy-MM-dd HH:mm:ss} ({tzAbbreviation})");
        sb.AppendLine($"Period: {localPeriodStart:yyyy-MM-dd HH:mm} - {localPeriodEnd:yyyy-MM-dd HH:mm} ({tzAbbreviation})");
        sb.AppendLine();
        sb.AppendLine("--- Account Summary ---");
        sb.AppendLine($"Account: {report.Account.Name} ({report.Account.Login})");
        sb.AppendLine($"Broker: {report.Account.Broker}");
        sb.AppendLine($"Server: {report.Account.Server}");
        sb.AppendLine($"Platform: {report.Account.Platform?.ToUpper()}");
        sb.AppendLine();
        sb.AppendLine($"Balance: {report.Account.Balance:N2} {report.Account.Currency}");
        sb.AppendLine($"Equity: {report.Account.Equity:N2} {report.Account.Currency}");
        sb.AppendLine($"Free Margin: {report.Account.FreeMargin:N2} {report.Account.Currency}");
        sb.AppendLine($"Leverage: 1:{report.Account.Leverage}");
        sb.AppendLine();
        sb.AppendLine("--- Trading Summary ---");
        sb.AppendLine($"Total Trades: {report.TradeCount}");
        sb.AppendLine($"Winning Trades: {report.WinningTrades}");
        sb.AppendLine($"Losing Trades: {report.LosingTrades}");
        sb.AppendLine($"Win Rate: {report.WinRate:N1}%");
        sb.AppendLine();
        sb.AppendLine($"Total Profit/Loss: {report.TotalProfit:N2} {report.Account.Currency}");
        sb.AppendLine();
        
        if (report.TradingDeals.Any())
        {
            sb.AppendLine($"--- All Deals ({report.TradeCount} total) ---");
            foreach (var deal in report.TradingDeals.OrderByDescending(d => d.Time))
            {
                var localDealTime = ToLocalTime(deal.Time);
                sb.AppendLine($"  {localDealTime:HH:mm:ss} | {deal.Symbol} | {deal.Type} | {deal.Volume} lots | P/L: {deal.NetProfit:N2}");
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine("This report was generated automatically by MetaReport.");
        
        return sb.ToString();
    }

    private string BuildHtmlContent(TradingReport report)
    {
        var profitColor = report.TotalProfit >= 0 ? "#28a745" : "#dc3545";
        var winRateColor = report.WinRate >= 50 ? "#28a745" : (report.WinRate >= 30 ? "#ffc107" : "#dc3545");
        var localGeneratedAt = ToLocalTime(report.GeneratedAt);
        var tzAbbreviation = GetTimeZoneAbbreviation();
        
        var dealsHtml = new StringBuilder();
        if (report.TradingDeals.Any())
        {
            dealsHtml.AppendLine("<table style=\"width: 100%; border-collapse: collapse; margin-top: 10px;\">");
            dealsHtml.AppendLine("<tr style=\"background: #f8f9fa;\"><th style=\"padding: 8px; text-align: left; border-bottom: 2px solid #dee2e6;\">Time</th><th style=\"padding: 8px; text-align: left; border-bottom: 2px solid #dee2e6;\">Symbol</th><th style=\"padding: 8px; text-align: left; border-bottom: 2px solid #dee2e6;\">Type</th><th style=\"padding: 8px; text-align: right; border-bottom: 2px solid #dee2e6;\">Volume</th><th style=\"padding: 8px; text-align: right; border-bottom: 2px solid #dee2e6;\">P/L</th></tr>");
            
            foreach (var deal in report.TradingDeals.OrderByDescending(d => d.Time))
            {
                var localDealTime = ToLocalTime(deal.Time);
                var dealProfitColor = deal.NetProfit >= 0 ? "#28a745" : "#dc3545";
                dealsHtml.AppendLine($"<tr><td style=\"padding: 8px; border-bottom: 1px solid #dee2e6;\">{localDealTime:HH:mm:ss}</td><td style=\"padding: 8px; border-bottom: 1px solid #dee2e6;\">{deal.Symbol}</td><td style=\"padding: 8px; border-bottom: 1px solid #dee2e6;\">{deal.Type}</td><td style=\"padding: 8px; text-align: right; border-bottom: 1px solid #dee2e6;\">{deal.Volume}</td><td style=\"padding: 8px; text-align: right; border-bottom: 1px solid #dee2e6; color: {dealProfitColor};\">{deal.NetProfit:N2}</td></tr>");
            }
            
            dealsHtml.AppendLine("</table>");
        }
        else
        {
            dealsHtml.AppendLine("<p style=\"color: #6c757d; text-align: center;\">No deals in this period</p>");
        }

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; border-radius: 10px 10px 0 0;"">
        <h1 style=""color: white; margin: 0; font-size: 24px;"">📊 MetaReport</h1>
        <p style=""color: rgba(255,255,255,0.9); margin: 10px 0 0 0;"">Daily Trading Summary</p>
    </div>
    
    <div style=""background: #f8f9fa; padding: 15px 20px; border-left: 1px solid #dee2e6; border-right: 1px solid #dee2e6;"">
        <p style=""margin: 0; color: #6c757d; font-size: 14px;"">
            📅 {localGeneratedAt:dddd, MMMM d, yyyy} at {localGeneratedAt:HH:mm} ({tzAbbreviation})
        </p>
    </div>
    
    <div style=""background: white; padding: 20px; border: 1px solid #dee2e6;"">
        <h2 style=""color: #333; border-bottom: 2px solid #667eea; padding-bottom: 10px; margin-top: 0;"">Account Summary</h2>
        
        <div style=""display: grid; gap: 10px;"">
            <div class=""account-item""><strong>Account:</strong> {report.Account.Name} ({report.Account.Login})</div>
            <div class=""account-item""><strong>Broker:</strong> {report.Account.Broker}</div>
            <div class=""account-item""><strong>Server:</strong> {report.Account.Server}</div>
            <div class=""account-item""><strong>Platform:</strong> {report.Account.Platform?.ToUpper()}</div>
        </div>
        
        <div style=""display: grid; grid-template-columns: repeat(2, 1fr); gap: 15px; margin-top: 20px;"">
            <div style=""background: #e3f2fd; padding: 15px; border-radius: 8px; text-align: center;"">
                <div style=""font-size: 24px; font-weight: bold; color: #1976d2;"">{report.Account.Balance:N2}</div>
                <div style=""color: #666; font-size: 12px;"">Balance ({report.Account.Currency})</div>
            </div>
            <div style=""background: #e8f5e9; padding: 15px; border-radius: 8px; text-align: center;"">
                <div style=""font-size: 24px; font-weight: bold; color: #388e3c;"">{report.Account.Equity:N2}</div>
                <div style=""color: #666; font-size: 12px;"">Equity ({report.Account.Currency})</div>
            </div>
        </div>
    </div>
    
    <div style=""background: white; padding: 20px; border: 1px solid #dee2e6; border-top: none;"">
        <h2 style=""color: #333; border-bottom: 2px solid #667eea; padding-bottom: 10px; margin-top: 0;"">Trading Performance</h2>
        
        <div style=""display: grid; grid-template-columns: repeat(3, 1fr); gap: 15px;"">
            <div style=""text-align: center; padding: 15px; background: #f8f9fa; border-radius: 8px;"">
                <div style=""font-size: 28px; font-weight: bold; color: {profitColor};"">{report.TotalProfit:N2}</div>
                <div style=""color: #666; font-size: 12px;"">Total P/L ({report.Account.Currency})</div>
            </div>
            <div style=""text-align: center; padding: 15px; background: #f8f9fa; border-radius: 8px;"">
                <div style=""font-size: 28px; font-weight: bold;"">{report.TradeCount}</div>
                <div style=""color: #666; font-size: 12px;"">Total Trades</div>
            </div>
            <div style=""text-align: center; padding: 15px; background: #f8f9fa; border-radius: 8px;"">
                <div style=""font-size: 28px; font-weight: bold; color: {winRateColor};"">{report.WinRate:N1}%</div>
                <div style=""color: #666; font-size: 12px;"">Win Rate</div>
            </div>
        </div>
        
        <div style=""display: grid; grid-template-columns: repeat(2, 1fr); gap: 15px; margin-top: 15px;"">
            <div style=""text-align: center; padding: 10px; background: #e8f5e9; border-radius: 8px;"">
                <div style=""font-size: 18px; font-weight: bold; color: #28a745;"">{report.WinningTrades} wins</div>
            </div>
            <div style=""text-align: center; padding: 10px; background: #ffebee; border-radius: 8px;"">
                <div style=""font-size: 18px; font-weight: bold; color: #dc3545;"">{report.LosingTrades} losses</div>
            </div>
        </div>
    </div>
    
    <div style=""background: white; padding: 20px; border: 1px solid #dee2e6; border-top: none;"">
        <h2 style=""color: #333; border-bottom: 2px solid #667eea; padding-bottom: 10px; margin-top: 0;"">Deals (Last 24 Hours)</h2>
        {dealsHtml}
    </div>
    
    <div style=""background: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-top: none; border-radius: 0 0 10px 10px; text-align: center;"">
        <p style=""margin: 0; color: #6c757d; font-size: 12px;"">
            This report was generated automatically by MetaReport<br>
            Powered by Azure Communication Services
        </p>
    </div>
</body>
</html>";
    }
}
