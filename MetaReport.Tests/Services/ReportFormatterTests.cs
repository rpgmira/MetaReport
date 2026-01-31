using FluentAssertions;
using MetaReport.Models;
using MetaReport.Services;

namespace MetaReport.Tests.Services;

public class ReportFormatterTests
{
    private static ReportFormatter CreateFormatter(string timeZoneId = "UTC")
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return new ReportFormatter(timeZone);
    }

    private static TradingReport CreateTestReport()
    {
        return new TradingReport
        {
            Account = new AccountInfo
            {
                Name = "Test Account",
                Login = 12345,
                Balance = 10000.00m,
                Equity = 10500.00m,
                Currency = "USD",
                Broker = "Test Broker",
                Server = "Test-Server",
                Platform = "mt5",
                FreeMargin = 9500.00m,
                Leverage = 100
            },
            GeneratedAt = new DateTime(2026, 1, 30, 15, 0, 0, DateTimeKind.Utc),
            PeriodStart = new DateTime(2026, 1, 29, 15, 0, 0, DateTimeKind.Utc),
            PeriodEnd = new DateTime(2026, 1, 30, 15, 0, 0, DateTimeKind.Utc),
            RecentDeals = new List<Deal>
            {
                new()
                {
                    Id = "1",
                    Symbol = "EURUSD",
                    Type = "DEAL_TYPE_BUY",
                    Profit = 50.00m,
                    Volume = 0.1m,
                    Time = new DateTime(2026, 1, 30, 10, 30, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = "2",
                    Symbol = "GBPUSD",
                    Type = "DEAL_TYPE_SELL",
                    Profit = -20.00m,
                    Volume = 0.2m,
                    Time = new DateTime(2026, 1, 30, 14, 45, 0, DateTimeKind.Utc)
                }
            }
        };
    }

    [Theory]
    [InlineData("UTC", "UTC")]
    [InlineData("SA Pacific Standard Time", "COT")]
    [InlineData("Eastern Standard Time", "EST")]
    [InlineData("Pacific Standard Time", "PST")]
    [InlineData("Central Standard Time", "CST")]
    public void GetTimeZoneAbbreviation_ShouldReturnCorrectAbbreviation(string timeZoneId, string expectedAbbreviation)
    {
        // Arrange
        var formatter = CreateFormatter(timeZoneId);

        // Act
        var abbreviation = formatter.GetTimeZoneAbbreviation();

        // Assert
        abbreviation.Should().Be(expectedAbbreviation);
    }

    [Fact]
    public void ToLocalTime_ShouldConvertUtcToLocalTime()
    {
        // Arrange
        var formatter = CreateFormatter("SA Pacific Standard Time"); // UTC-5
        var utcTime = new DateTime(2026, 1, 30, 20, 0, 0, DateTimeKind.Utc);

        // Act
        var localTime = formatter.ToLocalTime(utcTime);

        // Assert (20:00 UTC - 5 hours = 15:00 COT)
        localTime.Hour.Should().Be(15);
    }

    [Fact]
    public void BuildPlainTextContent_ShouldContainAccountInfo()
    {
        // Arrange
        var formatter = CreateFormatter();
        var report = CreateTestReport();

        // Act
        var content = formatter.BuildPlainTextContent(report);

        // Assert
        content.Should().Contain("Test Account");
        content.Should().Contain("12345");
        content.Should().Contain("Test Broker");
        content.Should().Contain("Balance:"); // Number format varies by locale
        content.Should().Contain("USD");
    }

    [Fact]
    public void BuildPlainTextContent_ShouldContainTradingSummary()
    {
        // Arrange
        var formatter = CreateFormatter();
        var report = CreateTestReport();

        // Act
        var content = formatter.BuildPlainTextContent(report);

        // Assert
        content.Should().Contain("Total Trades: 2");
        content.Should().Contain("Total Profit/Loss:"); // Number format varies by locale
        content.Should().Contain("Win Rate:");
    }

    [Fact]
    public void BuildPlainTextContent_ShouldContainDeals()
    {
        // Arrange
        var formatter = CreateFormatter();
        var report = CreateTestReport();

        // Act
        var content = formatter.BuildPlainTextContent(report);

        // Assert
        content.Should().Contain("EURUSD");
        content.Should().Contain("GBPUSD");
        content.Should().Contain("DEAL_TYPE_BUY");
        content.Should().Contain("DEAL_TYPE_SELL");
    }

    [Fact]
    public void BuildPlainTextContent_ShouldShowTimezoneAbbreviation()
    {
        // Arrange
        var formatter = CreateFormatter("SA Pacific Standard Time");
        var report = CreateTestReport();

        // Act
        var content = formatter.BuildPlainTextContent(report);

        // Assert
        content.Should().Contain("(COT)");
    }

    [Fact]
    public void BuildHtmlContent_ShouldContainHtmlStructure()
    {
        // Arrange
        var formatter = CreateFormatter();
        var report = CreateTestReport();

        // Act
        var content = formatter.BuildHtmlContent(report);

        // Assert
        content.Should().Contain("<!DOCTYPE html>");
        content.Should().Contain("<html>");
        content.Should().Contain("</html>");
        content.Should().Contain("MetaReport");
    }

    [Fact]
    public void BuildHtmlContent_ShouldContainAccountInfo()
    {
        // Arrange
        var formatter = CreateFormatter();
        var report = CreateTestReport();

        // Act
        var content = formatter.BuildHtmlContent(report);

        // Assert
        content.Should().Contain("Test Account");
        content.Should().Contain("Balance (USD)"); // Number format varies by locale
        content.Should().Contain("Equity (USD)");
    }

    [Fact]
    public void BuildHtmlContent_ShouldContainDealsTable()
    {
        // Arrange
        var formatter = CreateFormatter();
        var report = CreateTestReport();

        // Act
        var content = formatter.BuildHtmlContent(report);

        // Assert
        content.Should().Contain("<table");
        content.Should().Contain("EURUSD");
        content.Should().Contain("GBPUSD");
    }

    [Fact]
    public void BuildHtmlContent_WithNoDeals_ShouldShowNoDealsMessage()
    {
        // Arrange
        var formatter = CreateFormatter();
        var report = CreateTestReport();
        report.RecentDeals = new List<Deal>();

        // Act
        var content = formatter.BuildHtmlContent(report);

        // Assert
        content.Should().Contain("No deals in this period");
    }

    [Fact]
    public void BuildHtmlContent_ShouldUseGreenColorForProfit()
    {
        // Arrange
        var formatter = CreateFormatter();
        var report = CreateTestReport(); // Has positive total profit

        // Act
        var content = formatter.BuildHtmlContent(report);

        // Assert
        content.Should().Contain("#28a745"); // Green color for profit
    }

    [Fact]
    public void BuildHtmlContent_ShouldUseRedColorForLoss()
    {
        // Arrange
        var formatter = CreateFormatter();
        var report = CreateTestReport();
        report.RecentDeals = new List<Deal>
        {
            new()
            {
                Id = "1",
                Symbol = "EURUSD",
                Type = "DEAL_TYPE_BUY",
                Profit = -100.00m,
                Volume = 0.1m,
                Time = DateTime.UtcNow
            }
        };

        // Act
        var content = formatter.BuildHtmlContent(report);

        // Assert
        content.Should().Contain("#dc3545"); // Red color for loss
    }
}
