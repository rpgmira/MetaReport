using FluentAssertions;
using MetaReport.Models;

namespace MetaReport.Tests.Models;

public class AccountInfoTests
{
    [Fact]
    public void Balance_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { Balance = 10000.50m };

        // Assert
        account.Balance.Should().Be(10000.50m);
    }

    [Fact]
    public void Equity_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { Equity = 10500.75m };

        // Assert
        account.Equity.Should().Be(10500.75m);
    }

    [Fact]
    public void Margin_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { Margin = 500.00m };

        // Assert
        account.Margin.Should().Be(500.00m);
    }

    [Fact]
    public void FreeMargin_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { FreeMargin = 9500.00m };

        // Assert
        account.FreeMargin.Should().Be(9500.00m);
    }

    [Fact]
    public void Leverage_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { Leverage = 100 };

        // Assert
        account.Leverage.Should().Be(100);
    }

    [Fact]
    public void MarginLevel_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { MarginLevel = 2000.50m };

        // Assert
        account.MarginLevel.Should().Be(2000.50m);
    }

    [Fact]
    public void MarginLevel_CanBeNull()
    {
        // Arrange & Act
        var account = new AccountInfo { MarginLevel = null };

        // Assert
        account.MarginLevel.Should().BeNull();
    }

    [Fact]
    public void Currency_DefaultsToEmptyString()
    {
        // Arrange & Act
        var account = new AccountInfo();

        // Assert
        account.Currency.Should().BeEmpty();
    }

    [Fact]
    public void Currency_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { Currency = "USD" };

        // Assert
        account.Currency.Should().Be("USD");
    }

    [Fact]
    public void Broker_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { Broker = "Test Broker" };

        // Assert
        account.Broker.Should().Be("Test Broker");
    }

    [Fact]
    public void Server_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { Server = "LiveServer-01" };

        // Assert
        account.Server.Should().Be("LiveServer-01");
    }

    [Fact]
    public void Name_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { Name = "John Doe" };

        // Assert
        account.Name.Should().Be("John Doe");
    }

    [Fact]
    public void Login_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { Login = 12345678 };

        // Assert
        account.Login.Should().Be(12345678);
    }

    [Fact]
    public void Platform_CanBeSet()
    {
        // Arrange & Act
        var account = new AccountInfo { Platform = "mt5" };

        // Assert
        account.Platform.Should().Be("mt5");
    }

    [Fact]
    public void AllProperties_CanBeSetTogether()
    {
        // Arrange & Act
        var account = new AccountInfo
        {
            Balance = 10000m,
            Equity = 10500m,
            Margin = 500m,
            FreeMargin = 10000m,
            Leverage = 100,
            MarginLevel = 2100m,
            Currency = "EUR",
            Broker = "Test Broker",
            Server = "TestServer",
            Name = "Test User",
            Login = 999999,
            Platform = "mt4"
        };

        // Assert
        account.Balance.Should().Be(10000m);
        account.Equity.Should().Be(10500m);
        account.Margin.Should().Be(500m);
        account.FreeMargin.Should().Be(10000m);
        account.Leverage.Should().Be(100);
        account.MarginLevel.Should().Be(2100m);
        account.Currency.Should().Be("EUR");
        account.Broker.Should().Be("Test Broker");
        account.Server.Should().Be("TestServer");
        account.Name.Should().Be("Test User");
        account.Login.Should().Be(999999);
        account.Platform.Should().Be("mt4");
    }
}
