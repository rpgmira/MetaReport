namespace MetaReport.Models.Options;

/// <summary>
/// Configuration options for MetaAPI integration.
/// </summary>
public class MetaApiOptions
{
    /// <summary>
    /// Configuration section name in appsettings/local.settings.json.
    /// </summary>
    public const string SectionName = "MetaApi";

    /// <summary>
    /// MetaAPI authentication token.
    /// Obtain from https://app.metaapi.cloud/token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The MetaAPI account ID (not the MT4 login, but the MetaAPI provisioned account ID).
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for MetaAPI REST endpoints.
    /// Default is the New York region endpoint.
    /// </summary>
    public string BaseUrl { get; set; } = "https://mt-client-api-v1.new-york.agiliumtrade.ai";
}
