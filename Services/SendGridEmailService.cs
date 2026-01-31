using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using MetaReport.Models;
using MetaReport.Models.Options;

namespace MetaReport.Services;

/// <summary>
/// Email service implementation using SendGrid.
/// </summary>
public class SendGridEmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly EmailOptions _options;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(
        ISendGridClient sendGridClient,
        IOptions<EmailOptions> options,
        ILogger<SendGridEmailService> logger)
    {
        _sendGridClient = sendGridClient;
        _options = options.Value;
        _logger = logger;
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

        var from = new EmailAddress(_options.FromAddress, _options.FromName);
        var tos = recipients.Select(email => new EmailAddress(email)).ToList();
        var subject = $"📊 MetaReport - Daily Trading Summary ({report.GeneratedAt:yyyy-MM-dd})";
        
        var plainTextContent = BuildPlainTextContent(report);
        var htmlContent = BuildHtmlContent(report);

        var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, plainTextContent, htmlContent);

        try
        {
            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK || 
                response.StatusCode == HttpStatusCode.Accepted ||
                response.StatusCode == HttpStatusCode.Created)
            {
                _logger.LogInformation("Email sent successfully to {RecipientCount} recipients", recipients.Count);
                return true;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send email. Status: {StatusCode}, Body: {Body}", 
                    response.StatusCode, body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email");
            throw;
        }
    }

    private static string BuildPlainTextContent(TradingReport report)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("=== MetaReport - Daily Trading Summary ===");
        sb.AppendLine();
        sb.AppendLine($"Report Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"Period: {report.PeriodStart:yyyy-MM-dd HH:mm} - {report.PeriodEnd:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine();
        sb.AppendLine("--- Account Summary ---");
        sb.AppendLine($"Account: {report.Account.Name} ({report.Account.Login})");
        sb.AppendLine($"Broker: {report.Account.Broker}");
        sb.AppendLine($"Balance: {report.Account.Balance:N2} {report.Account.Currency}");
        sb.AppendLine($"Equity: {report.Account.Equity:N2} {report.Account.Currency}");
        sb.AppendLine($"Free Margin: {report.Account.FreeMargin:N2} {report.Account.Currency}");
        sb.AppendLine($"Leverage: 1:{report.Account.Leverage}");
        sb.AppendLine();
        sb.AppendLine("--- Last 24 Hours Performance ---");
        sb.AppendLine($"Total Trades: {report.TradeCount}");
        sb.AppendLine($"Winning: {report.WinningTrades} | Losing: {report.LosingTrades}");
        sb.AppendLine($"Win Rate: {report.WinRate:N1}%");
        sb.AppendLine($"Total P/L: {report.TotalProfit:N2} {report.Account.Currency}");
        sb.AppendLine();

        if (report.TradingDeals.Any())
        {
            sb.AppendLine("--- Trade Details ---");
            foreach (var deal in report.TradingDeals.OrderByDescending(d => d.Time))
            {
                var emoji = deal.NetProfit >= 0 ? "✅" : "❌";
                sb.AppendLine($"{emoji} {deal.Time:HH:mm} | {deal.Symbol} | {deal.Type} | {deal.Volume} lots | P/L: {deal.NetProfit:N2}");
            }
        }
        else
        {
            sb.AppendLine("No trades executed in the last 24 hours.");
        }

        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine("Sent by MetaReport - https://github.com/your-username/MetaReport");

        return sb.ToString();
    }

    private static string BuildHtmlContent(TradingReport report)
    {
        var profitColor = report.TotalProfit >= 0 ? "#22c55e" : "#ef4444";
        var winRateColor = report.WinRate >= 50 ? "#22c55e" : "#ef4444";

        var sb = new StringBuilder();
        
        sb.AppendLine(@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>MetaReport - Daily Trading Summary</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #1f2937; margin: 0; padding: 0; background-color: #f3f4f6; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .card { background: white; border-radius: 12px; padding: 24px; margin-bottom: 16px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
        .header { text-align: center; margin-bottom: 24px; }
        .header h1 { color: #1f2937; margin: 0; font-size: 24px; }
        .header p { color: #6b7280; margin: 8px 0 0; font-size: 14px; }
        .stats-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; margin-bottom: 24px; }
        .stat-box { background: #f9fafb; border-radius: 8px; padding: 16px; text-align: center; }
        .stat-label { color: #6b7280; font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px; }
        .stat-value { color: #1f2937; font-size: 24px; font-weight: 600; margin-top: 4px; }
        .stat-currency { font-size: 14px; color: #6b7280; font-weight: normal; }
        .section-title { font-size: 16px; font-weight: 600; color: #1f2937; margin-bottom: 12px; border-bottom: 2px solid #e5e7eb; padding-bottom: 8px; }
        .account-info { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
        .account-item { font-size: 14px; }
        .account-item strong { color: #6b7280; }
        table { width: 100%; border-collapse: collapse; font-size: 13px; }
        th { background: #f9fafb; padding: 10px 8px; text-align: left; font-weight: 600; color: #6b7280; border-bottom: 2px solid #e5e7eb; }
        td { padding: 10px 8px; border-bottom: 1px solid #e5e7eb; }
        .profit { color: #22c55e; font-weight: 600; }
        .loss { color: #ef4444; font-weight: 600; }
        .no-trades { text-align: center; color: #6b7280; padding: 24px; }
        .footer { text-align: center; font-size: 12px; color: #9ca3af; margin-top: 24px; }
        .footer a { color: #6b7280; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""card"">
            <div class=""header"">
                <h1>📊 MetaReport</h1>
                <p>Daily Trading Summary</p>
            </div>");

        // Stats Grid
        sb.AppendLine($@"
            <div class=""stats-grid"">
                <div class=""stat-box"">
                    <div class=""stat-label"">Balance</div>
                    <div class=""stat-value"">{report.Account.Balance:N2} <span class=""stat-currency"">{report.Account.Currency}</span></div>
                </div>
                <div class=""stat-box"">
                    <div class=""stat-label"">Equity</div>
                    <div class=""stat-value"">{report.Account.Equity:N2} <span class=""stat-currency"">{report.Account.Currency}</span></div>
                </div>
                <div class=""stat-box"">
                    <div class=""stat-label"">24h P/L</div>
                    <div class=""stat-value"" style=""color: {profitColor}"">{(report.TotalProfit >= 0 ? "+" : "")}{report.TotalProfit:N2}</div>
                </div>
                <div class=""stat-box"">
                    <div class=""stat-label"">Win Rate</div>
                    <div class=""stat-value"" style=""color: {winRateColor}"">{report.WinRate:N1}%</div>
                </div>
            </div>");

        // Account Info Section
        sb.AppendLine($@"
            <div class=""section-title"">Account Information</div>
            <div class=""account-info"">
                <div class=""account-item""><strong>Account:</strong> {report.Account.Name}</div>
                <div class=""account-item""><strong>Login:</strong> {report.Account.Login}</div>
                <div class=""account-item""><strong>Broker:</strong> {report.Account.Broker}</div>
                <div class=""account-item""><strong>Leverage:</strong> 1:{report.Account.Leverage}</div>
                <div class=""account-item""><strong>Free Margin:</strong> {report.Account.FreeMargin:N2} {report.Account.Currency}</div>
                <div class=""account-item""><strong>Platform:</strong> {report.Account.Platform.ToUpperInvariant()}</div>
            </div>
        </div>");

        // Trades Section
        sb.AppendLine(@"
        <div class=""card"">
            <div class=""section-title"">Last 24 Hours Trades</div>");

        var tradingDeals = report.TradingDeals.OrderByDescending(d => d.Time).ToList();
        
        if (tradingDeals.Any())
        {
            sb.AppendLine(@"
            <table>
                <thead>
                    <tr>
                        <th>Time (UTC)</th>
                        <th>Symbol</th>
                        <th>Type</th>
                        <th>Volume</th>
                        <th>P/L</th>
                    </tr>
                </thead>
                <tbody>");

            foreach (var deal in tradingDeals)
            {
                var plClass = deal.NetProfit >= 0 ? "profit" : "loss";
                var plSign = deal.NetProfit >= 0 ? "+" : "";
                var typeDisplay = deal.Type.Replace("DEAL_TYPE_", "");
                
                sb.AppendLine($@"
                    <tr>
                        <td>{deal.Time:HH:mm:ss}</td>
                        <td><strong>{deal.Symbol}</strong></td>
                        <td>{typeDisplay}</td>
                        <td>{deal.Volume}</td>
                        <td class=""{plClass}"">{plSign}{deal.NetProfit:N2}</td>
                    </tr>");
            }

            sb.AppendLine(@"
                </tbody>
            </table>");

            // Summary row
            sb.AppendLine($@"
            <div style=""margin-top: 16px; padding: 12px; background: #f9fafb; border-radius: 8px; display: flex; justify-content: space-between; font-size: 14px;"">
                <span><strong>Total Trades:</strong> {report.TradeCount} ({report.WinningTrades}W / {report.LosingTrades}L)</span>
                <span><strong>Net P/L:</strong> <span style=""color: {profitColor}"">{(report.TotalProfit >= 0 ? "+" : "")}{report.TotalProfit:N2} {report.Account.Currency}</span></span>
            </div>");
        }
        else
        {
            sb.AppendLine(@"
            <div class=""no-trades"">
                <p>📭 No trades executed in the last 24 hours</p>
            </div>");
        }

        sb.AppendLine(@"
        </div>");

        // Footer
        sb.AppendLine($@"
        <div class=""footer"">
            <p>Report generated at {report.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC</p>
            <p>Period: {report.PeriodStart:MMM dd, HH:mm} - {report.PeriodEnd:MMM dd, HH:mm} UTC</p>
            <p><a href=""https://github.com/your-username/MetaReport"">MetaReport</a> - Open Source Trading Reports</p>
        </div>
    </div>
</body>
</html>");

        return sb.ToString();
    }
}
