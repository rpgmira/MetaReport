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

    [Fact]
    public void IsTradeDeal_WithNullType_ShouldReturnFalse()
    {
        // Arrange - use reflection to set Type to null (edge case from deserialization)
        var deal = new Deal();
        typeof(Deal).GetProperty("Type")!.SetValue(deal, null);

        // Act
        var isTradeDeal = deal.IsTradeDeal;

        // Assert - should not throw and should return false
        isTradeDeal.Should().BeFalse();
    }

    [Fact]
    public void Id_DefaultsToEmptyString()
    {
        // Arrange & Act
        var deal = new Deal();

        // Assert
        deal.Id.Should().BeEmpty();
    }

    [Fact]
    public void Symbol_DefaultsToEmptyString()
    {
        // Arrange & Act
        var deal = new Deal();

        // Assert
        deal.Symbol.Should().BeEmpty();
    }

    [Fact]
    public void Type_DefaultsToEmptyString()
    {
        // Arrange & Act
        var deal = new Deal();

        // Assert
        deal.Type.Should().BeEmpty();
    }

    [Fact]
    public void Price_CanBeSet()
    {
        // Arrange & Act
        var deal = new Deal { Price = 1.2345m };

        // Assert
        deal.Price.Should().Be(1.2345m);
    }

    [Fact]
    public void Volume_CanBeSet()
    {
        // Arrange & Act
        var deal = new Deal { Volume = 0.10m };

        // Assert
        deal.Volume.Should().Be(0.10m);
    }

    [Fact]
    public void Time_CanBeSet()
    {
        // Arrange
        var time = new DateTime(2025, 6, 15, 10, 30, 0);
        
        // Act
        var deal = new Deal { Time = time };

        // Assert
        deal.Time.Should().Be(time);
    }

    [Fact]
    public void OrderId_IsNullable()
    {
        // Arrange & Act
        var deal = new Deal { OrderId = null };

        // Assert
        deal.OrderId.Should().BeNull();
    }

    [Fact]
    public void PositionId_IsNullable()
    {
        // Arrange & Act
        var deal = new Deal { PositionId = null };

        // Assert
        deal.PositionId.Should().BeNull();
    }

    [Fact]
    public void EntryType_IsNullable()
    {
        // Arrange & Act
        var deal = new Deal { EntryType = null };

        // Assert
        deal.EntryType.Should().BeNull();
    }

    [Fact]
    public void Comment_IsNullable()
    {
        // Arrange & Act
        var deal = new Deal { Comment = null };

        // Assert
        deal.Comment.Should().BeNull();
    }

    [Fact]
    public void BrokerTime_IsNullable()
    {
        // Arrange & Act
        var deal = new Deal { BrokerTime = null };

        // Assert
        deal.BrokerTime.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSetTogether()
    {
        // Arrange
        var time = DateTime.UtcNow;
        
        // Act
        var deal = new Deal
        {
            Id = "123456",
            OrderId = "order-1",
            PositionId = "pos-1",
            Symbol = "EURUSD",
            Type = "DEAL_TYPE_BUY",
            EntryType = "DEAL_ENTRY_IN",
            Price = 1.1234m,
            Volume = 0.01m,
            Profit = 50.00m,
            Swap = -0.50m,
            Commission = -0.10m,
            Time = time,
            BrokerTime = "2025.06.15 10:30:00",
            Comment = "Test trade"
        };

        // Assert
        deal.Id.Should().Be("123456");
        deal.OrderId.Should().Be("order-1");
        deal.PositionId.Should().Be("pos-1");
        deal.Symbol.Should().Be("EURUSD");
        deal.Type.Should().Be("DEAL_TYPE_BUY");
        deal.EntryType.Should().Be("DEAL_ENTRY_IN");
        deal.Price.Should().Be(1.1234m);
        deal.Volume.Should().Be(0.01m);
        deal.Profit.Should().Be(50.00m);
        deal.Swap.Should().Be(-0.50m);
        deal.Commission.Should().Be(-0.10m);
        deal.Time.Should().Be(time);
        deal.BrokerTime.Should().Be("2025.06.15 10:30:00");
        deal.Comment.Should().Be("Test trade");
        deal.NetProfit.Should().Be(49.40m);
        deal.IsTradeDeal.Should().BeTrue();
    }
}
