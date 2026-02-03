using FluentAssertions;
using MetaReport.Models;

namespace MetaReport.Tests.Models;

public class TradingReportTests
{
    private static AccountInfo CreateTestAccount() => new()
    {
        Name = "Test Account",
        Login = 12345,
        Balance = 10000.00m,
        Equity = 10500.00m,
        Currency = "USD",
        Broker = "Test Broker",
        Server = "Test-Server",
        Platform = "mt5"
    };

    private static Deal CreateDeal(string type, decimal profit, decimal swap = 0, decimal commission = 0) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Type = type,
        Profit = profit,
        Swap = swap,
        Commission = commission,
        Time = DateTime.UtcNow,
        Symbol = "EURUSD",
        Volume = 0.1m
    };

    [Fact]
    public void TradingDeals_ShouldOnlyReturnTradingDeals()
    {
        // Arrange
        var report = new TradingReport
        {
            Account = CreateTestAccount(),
            RecentDeals = new List<Deal>
            {
                CreateDeal("DEAL_TYPE_BUY", 50),
                CreateDeal("DEAL_TYPE_SELL", -20),
                CreateDeal("DEAL_TYPE_BALANCE", 1000),
                CreateDeal("DEAL_TYPE_CREDIT", 500),
                CreateDeal("DEAL_TYPE_BUY", 30)
            }
        };

        // Act
        var tradingDeals = report.TradingDeals;

        // Assert
        tradingDeals.Should().HaveCount(3);
        tradingDeals.Should().OnlyContain(d => d.IsTradeDeal);
    }

    [Fact]
    public void TotalProfit_ShouldSumNetProfitOfTradingDealsOnly()
    {
        // Arrange
        var report = new TradingReport
        {
            Account = CreateTestAccount(),
            RecentDeals = new List<Deal>
            {
                CreateDeal("DEAL_TYPE_BUY", 100, -2, -1),    // NetProfit = 97
                CreateDeal("DEAL_TYPE_SELL", -50, -1, -0.5m), // NetProfit = -51.5
                CreateDeal("DEAL_TYPE_BALANCE", 1000, 0, 0), // Should be excluded
            }
        };

        // Act
        var totalProfit = report.TotalProfit;

        // Assert
        totalProfit.Should().Be(45.5m); // 97 + (-51.5)
    }

    [Fact]
    public void TradeCount_ShouldCountOnlyTradingDeals()
    {
        // Arrange
        var report = new TradingReport
        {
            Account = CreateTestAccount(),
            RecentDeals = new List<Deal>
            {
                CreateDeal("DEAL_TYPE_BUY", 50),
                CreateDeal("DEAL_TYPE_SELL", -20),
                CreateDeal("DEAL_TYPE_BALANCE", 1000),
                CreateDeal("DEAL_TYPE_BUY", 30)
            }
        };

        // Act
        var tradeCount = report.TradeCount;

        // Assert
        tradeCount.Should().Be(3);
    }

    [Fact]
    public void WinningTrades_ShouldCountDealsWithPositiveNetProfit()
    {
        // Arrange
        var report = new TradingReport
        {
            Account = CreateTestAccount(),
            RecentDeals = new List<Deal>
            {
                CreateDeal("DEAL_TYPE_BUY", 50),      // Winning
                CreateDeal("DEAL_TYPE_SELL", -20),    // Losing
                CreateDeal("DEAL_TYPE_BUY", 0),       // Break-even (not winning)
                CreateDeal("DEAL_TYPE_SELL", 30)      // Winning
            }
        };

        // Act
        var winningTrades = report.WinningTrades;

        // Assert
        winningTrades.Should().Be(2);
    }

    [Fact]
    public void LosingTrades_ShouldCountDealsWithNegativeNetProfit()
    {
        // Arrange
        var report = new TradingReport
        {
            Account = CreateTestAccount(),
            RecentDeals = new List<Deal>
            {
                CreateDeal("DEAL_TYPE_BUY", 50),      // Winning
                CreateDeal("DEAL_TYPE_SELL", -20),    // Losing
                CreateDeal("DEAL_TYPE_BUY", 0),       // Break-even
                CreateDeal("DEAL_TYPE_SELL", -30)     // Losing
            }
        };

        // Act
        var losingTrades = report.LosingTrades;

        // Assert
        losingTrades.Should().Be(2);
    }

    [Fact]
    public void WinRate_ShouldCalculatePercentageCorrectly()
    {
        // Arrange
        var report = new TradingReport
        {
            Account = CreateTestAccount(),
            RecentDeals = new List<Deal>
            {
                CreateDeal("DEAL_TYPE_BUY", 50),
                CreateDeal("DEAL_TYPE_SELL", 30),
                CreateDeal("DEAL_TYPE_BUY", -20),
                CreateDeal("DEAL_TYPE_SELL", 10)
            }
        };

        // Act (3 winning out of 4 = 75%)
        var winRate = report.WinRate;

        // Assert
        winRate.Should().Be(75m);
    }

    [Fact]
    public void WinRate_WithNoTrades_ShouldReturnZero()
    {
        // Arrange
        var report = new TradingReport
        {
            Account = CreateTestAccount(),
            RecentDeals = new List<Deal>()
        };

        // Act
        var winRate = report.WinRate;

        // Assert
        winRate.Should().Be(0);
    }

    [Fact]
    public void WinRate_WithOnlyBalanceDeals_ShouldReturnZero()
    {
        // Arrange
        var report = new TradingReport
        {
            Account = CreateTestAccount(),
            RecentDeals = new List<Deal>
            {
                CreateDeal("DEAL_TYPE_BALANCE", 1000),
                CreateDeal("DEAL_TYPE_CREDIT", 500)
            }
        };

        // Act
        var winRate = report.WinRate;

        // Assert
        winRate.Should().Be(0);
    }

    [Fact]
    public void WinRate_ShouldExcludeBreakEvenTrades()
    {
        // Arrange - Scenario from GitHub issue: 2 wins, 2 losses, 4 break-even = 8 total trades
        var report = new TradingReport
        {
            Account = CreateTestAccount(),
            RecentDeals = new List<Deal>
            {
                CreateDeal("DEAL_TYPE_BUY", 50),      // Win
                CreateDeal("DEAL_TYPE_SELL", 30),     // Win
                CreateDeal("DEAL_TYPE_BUY", -20),     // Loss
                CreateDeal("DEAL_TYPE_SELL", -10),    // Loss
                CreateDeal("DEAL_TYPE_BUY", 0),       // Break-even
                CreateDeal("DEAL_TYPE_SELL", 0),      // Break-even
                CreateDeal("DEAL_TYPE_BUY", 0),       // Break-even
                CreateDeal("DEAL_TYPE_SELL", 0)       // Break-even
            }
        };

        // Act
        var winRate = report.WinRate;

        // Assert
        // Win Rate should be 50% (2 wins out of 2 wins + 2 losses = 4 decisive trades)
        // NOT 25% (2 wins out of 8 total trades including break-even)
        winRate.Should().Be(50m);
    }

    [Fact]
    public void WinRate_WithOnlyBreakEvenTrades_ShouldReturnZero()
    {
        // Arrange
        var report = new TradingReport
        {
            Account = CreateTestAccount(),
            RecentDeals = new List<Deal>
            {
                CreateDeal("DEAL_TYPE_BUY", 0),
                CreateDeal("DEAL_TYPE_SELL", 0),
                CreateDeal("DEAL_TYPE_BUY", 0)
            }
        };

        // Act
        var winRate = report.WinRate;

        // Assert
        winRate.Should().Be(0);
    }
}
