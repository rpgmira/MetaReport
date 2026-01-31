using System.Text.Json.Serialization;

namespace MetaReport.Models;

/// <summary>
/// Represents MT4/MT5 account information from MetaAPI.
/// </summary>
public class AccountInfo
{
    /// <summary>
    /// Account balance.
    /// </summary>
    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    /// <summary>
    /// Account equity (balance + floating profit/loss).
    /// </summary>
    [JsonPropertyName("equity")]
    public decimal Equity { get; set; }

    /// <summary>
    /// Used margin.
    /// </summary>
    [JsonPropertyName("margin")]
    public decimal Margin { get; set; }

    /// <summary>
    /// Free margin available for trading.
    /// </summary>
    [JsonPropertyName("freeMargin")]
    public decimal FreeMargin { get; set; }

    /// <summary>
    /// Account leverage.
    /// </summary>
    [JsonPropertyName("leverage")]
    public int Leverage { get; set; }

    /// <summary>
    /// Margin level percentage.
    /// </summary>
    [JsonPropertyName("marginLevel")]
    public decimal? MarginLevel { get; set; }

    /// <summary>
    /// Account currency (e.g., USD, EUR).
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Broker name.
    /// </summary>
    [JsonPropertyName("broker")]
    public string Broker { get; set; } = string.Empty;

    /// <summary>
    /// Trading server name.
    /// </summary>
    [JsonPropertyName("server")]
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Account holder name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Account login number.
    /// </summary>
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Platform type (mt4 or mt5).
    /// </summary>
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;
}
