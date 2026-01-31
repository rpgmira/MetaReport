namespace MetaReport.Models.Options;

/// <summary>
/// Configuration options for email sending.
/// Supports both Azure Communication Services and SendGrid (legacy).
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Configuration section name in appsettings/local.settings.json.
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// Azure Communication Services connection string.
    /// </summary>
    public string AzureConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// SendGrid API key (legacy - for backward compatibility).
    /// </summary>
    public string SendGridApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Sender email address.
    /// For Azure: use the Azure-managed domain address (DoNotReply@{domain}.azurecomm.net)
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Sender display name.
    /// </summary>
    public string FromName { get; set; } = "MetaReport";

    /// <summary>
    /// Recipient email addresses for reports (comma-separated).
    /// </summary>
    public string ToAddresses { get; set; } = string.Empty;

    /// <summary>
    /// Legacy single recipient (for backward compatibility).
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// Recipient display name.
    /// </summary>
    public string ToName { get; set; } = string.Empty;

    /// <summary>
    /// Gets all recipient email addresses as a list.
    /// </summary>
    public List<string> GetRecipients()
    {
        var recipients = new List<string>();
        
        // Add addresses from ToAddresses (comma-separated)
        if (!string.IsNullOrWhiteSpace(ToAddresses))
        {
            recipients.AddRange(ToAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }
        
        // Add legacy ToAddress if not already included
        if (!string.IsNullOrWhiteSpace(ToAddress) && !recipients.Contains(ToAddress, StringComparer.OrdinalIgnoreCase))
        {
            recipients.Add(ToAddress);
        }
        
        return recipients;
    }
}
