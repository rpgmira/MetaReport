using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MetaReport.Models;
using MetaReport.Models.Options;

namespace MetaReport.Services;

/// <summary>
/// Email service implementation using Azure Communication Services.
/// </summary>
public class AzureEmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly EmailOptions _options;
    private readonly IReportFormatter _formatter;
    private readonly ILogger<AzureEmailService> _logger;

    public AzureEmailService(
        EmailClient emailClient,
        IOptions<EmailOptions> options,
        IReportFormatter formatter,
        ILogger<AzureEmailService> logger)
    {
        _emailClient = emailClient;
        _options = options.Value;
        _formatter = formatter;
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

        var localGeneratedAt = _formatter.ToLocalTime(report.GeneratedAt);
        var subject = $"📊 MetaReport - Daily Trading Summary ({localGeneratedAt:yyyy-MM-dd})";
        var plainTextContent = _formatter.BuildPlainTextContent(report);
        var htmlContent = _formatter.BuildHtmlContent(report);

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
}
