using FluentAssertions;
using MetaReport.Models;

namespace MetaReport.Tests.Models;

public class DealTests
{
    [Fact]
    public void NetProfit_ShouldCalculateCorrectly()
    {
        // Arrange
        var deal = new Deal
        {
            Profit = 100.00m,
            Swap = -2.50m,
            Commission = -1.00m
        };

        // Act
        var netProfit = deal.NetProfit;

        // Assert
        netProfit.Should().Be(96.50m);
    }

    [Fact]
    public void NetProfit_WithZeroValues_ShouldReturnZero()
    {
        // Arrange
        var deal = new Deal();

        // Act
        var netProfit = deal.NetProfit;

        // Assert
        netProfit.Should().Be(0);
    }

    [Fact]
    public void NetProfit_WithNegativeProfit_ShouldCalculateCorrectly()
    {
        // Arrange
        var deal = new Deal
        {
            Profit = -50.00m,
            Swap = -1.00m,
            Commission = -0.50m
        };

        // Act
        var netProfit = deal.NetProfit;

        // Assert
        netProfit.Should().Be(-51.50m);
    }

    [Theory]
    [InlineData("DEAL_TYPE_BUY", true)]
    [InlineData("DEAL_TYPE_SELL", true)]
    [InlineData("deal_type_buy", true)]
    [InlineData("deal_type_sell", true)]
    [InlineData("DEAL_TYPE_BALANCE", false)]
    [InlineData("DEAL_TYPE_CREDIT", false)]
    [InlineData("", false)]
    public void IsTradeDeal_ShouldReturnCorrectValue(string dealType, bool expected)
    {
        // Arrange
        var deal = new Deal { Type = dealType };

        // Act
        var isTradeDeal = deal.IsTradeDeal;

        // Assert
        isTradeDeal.Should().Be(expected);
    }
}
