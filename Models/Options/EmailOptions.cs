namespace MetaReport.Models.Options;

/// <summary>
/// Configuration options for email sending via SendGrid.
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Configuration section name in appsettings/local.settings.json.
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// SendGrid API key.
    /// Obtain from SendGrid dashboard or Azure Marketplace.
    /// </summary>
    public string SendGridApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Sender email address (must be verified in SendGrid).
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Sender display name.
    /// </summary>
    public string FromName { get; set; } = "MetaReport";

    /// <summary>
    /// Recipient email address for reports.
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;

    /// <summary>
    /// Recipient display name.
    /// </summary>
    public string ToName { get; set; } = string.Empty;
}
