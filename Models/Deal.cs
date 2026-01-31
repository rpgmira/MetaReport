using System.Text.Json.Serialization;

namespace MetaReport.Models;

/// <summary>
/// Represents a completed trade/deal from MetaAPI history.
/// </summary>
public class Deal
{
    /// <summary>
    /// Deal unique identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Order identifier that triggered the deal.
    /// </summary>
    [JsonPropertyName("orderId")]
    public string? OrderId { get; set; }

    /// <summary>
    /// Position identifier the deal belongs to.
    /// </summary>
    [JsonPropertyName("positionId")]
    public string? PositionId { get; set; }

    /// <summary>
    /// Trading symbol (e.g., EURUSD, GBPJPY).
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Deal type (e.g., DEAL_TYPE_BUY, DEAL_TYPE_SELL, DEAL_TYPE_BALANCE).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Entry type (DEAL_ENTRY_IN, DEAL_ENTRY_OUT, DEAL_ENTRY_INOUT).
    /// </summary>
    [JsonPropertyName("entryType")]
    public string? EntryType { get; set; }

    /// <summary>
    /// Deal execution price.
    /// </summary>
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Trade volume in lots.
    /// </summary>
    [JsonPropertyName("volume")]
    public decimal Volume { get; set; }

    /// <summary>
    /// Profit/loss from the deal in account currency.
    /// </summary>
    [JsonPropertyName("profit")]
    public decimal Profit { get; set; }

    /// <summary>
    /// Swap charges.
    /// </summary>
    [JsonPropertyName("swap")]
    public decimal Swap { get; set; }

    /// <summary>
    /// Commission charges.
    /// </summary>
    [JsonPropertyName("commission")]
    public decimal Commission { get; set; }

    /// <summary>
    /// Deal execution time (ISO 8601 format).
    /// </summary>
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    /// <summary>
    /// Broker-reported time as string.
    /// </summary>
    [JsonPropertyName("brokerTime")]
    public string? BrokerTime { get; set; }

    /// <summary>
    /// Deal comment.
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// Calculated net profit (profit + swap + commission).
    /// </summary>
    [JsonIgnore]
    public decimal NetProfit => Profit + Swap + Commission;

    /// <summary>
    /// Indicates if this is a trading deal (buy/sell) vs balance/credit operation.
    /// </summary>
    [JsonIgnore]
    public bool IsTradeDeal => Type?.Contains("BUY", StringComparison.OrdinalIgnoreCase) == true ||
                                Type?.Contains("SELL", StringComparison.OrdinalIgnoreCase) == true;
}
