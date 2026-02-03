namespace MetaReport.Models;

/// <summary>
/// Aggregated trading report data for email generation.
/// </summary>
public class TradingReport
{
    /// <summary>
    /// Current account information.
    /// </summary>
    public AccountInfo Account { get; set; } = new();

    /// <summary>
    /// List of deals from the last 24 hours.
    /// </summary>
    public List<Deal> RecentDeals { get; set; } = new();

    /// <summary>
    /// Report generation timestamp.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Start of the reporting period.
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// End of the reporting period.
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Only trading deals (excludes balance/credit operations).
    /// </summary>
    public IEnumerable<Deal> TradingDeals => RecentDeals.Where(d => d.IsTradeDeal);

    /// <summary>
    /// Total profit/loss from trading deals in the period.
    /// </summary>
    public decimal TotalProfit => TradingDeals.Sum(d => d.NetProfit);

    /// <summary>
    /// Number of trading deals in the period.
    /// </summary>
    public int TradeCount => TradingDeals.Count();

    /// <summary>
    /// Number of winning trades (positive profit).
    /// </summary>
    public int WinningTrades => TradingDeals.Count(d => d.NetProfit > 0);

    /// <summary>
    /// Number of losing trades (negative profit).
    /// </summary>
    public int LosingTrades => TradingDeals.Count(d => d.NetProfit < 0);

    /// <summary>
    /// Win rate percentage (excluding break-even trades).
    /// </summary>
    public decimal WinRate
    {
        get
        {
            var decisiveTradeCount = WinningTrades + LosingTrades;
            return decisiveTradeCount > 0 ? (decimal)WinningTrades / decisiveTradeCount * 100 : 0;
        }
    }
}
