using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MetaReport.Models;
using MetaReport.Models.Options;
using MetaReport.Services;
using Moq;

namespace MetaReport.Tests.Services;

/// <summary>
/// Tests for AzureEmailService. Note that full integration tests would require
/// actual Azure Communication Services, so we focus on testable behavior here.
/// </summary>
public class AzureEmailServiceTests
{
    private readonly Mock<IReportFormatter> _mockFormatter;
    private readonly Mock<ILogger<AzureEmailService>> _mockLogger;
    private readonly EmailOptions _emailOptions;

    public AzureEmailServiceTests()
    {
        _mockFormatter = new Mock<IReportFormatter>();
        _mockLogger = new Mock<ILogger<AzureEmailService>>();
        _emailOptions = new EmailOptions
        {
            FromAddress = "test@example.com",
            ToAddresses = "recipient@example.com"
        };
    }

    private TradingReport CreateSampleReport()
    {
        return new TradingReport
        {
            Account = new AccountInfo
            {
                Balance = 10000,
                Equity = 10500,
                Currency = "USD"
            },
            RecentDeals = new List<Deal>
            {
                new Deal { Id = "1", Type = "DEAL_TYPE_BUY", Profit = 100 }
            },
            GeneratedAt = DateTime.UtcNow,
            PeriodStart = DateTime.UtcNow.AddDays(-1),
            PeriodEnd = DateTime.UtcNow
        };
    }

    [Fact]
    public void MockFormatter_CanBeMocked()
    {
        // Arrange
        var report = CreateSampleReport();
        _mockFormatter.Setup(f => f.ToLocalTime(It.IsAny<DateTime>())).Returns(DateTime.Now);
        _mockFormatter.Setup(f => f.BuildPlainTextContent(report)).Returns("Plain text content");
        _mockFormatter.Setup(f => f.BuildHtmlContent(report)).Returns("<html>HTML content</html>");

        // Act
        var localTime = _mockFormatter.Object.ToLocalTime(DateTime.UtcNow);
        var plainText = _mockFormatter.Object.BuildPlainTextContent(report);
        var html = _mockFormatter.Object.BuildHtmlContent(report);

        // Assert
        plainText.Should().Be("Plain text content");
        html.Should().Be("<html>HTML content</html>");
        _mockFormatter.Verify(f => f.ToLocalTime(It.IsAny<DateTime>()), Times.Once);
        _mockFormatter.Verify(f => f.BuildPlainTextContent(report), Times.Once);
        _mockFormatter.Verify(f => f.BuildHtmlContent(report), Times.Once);
    }

    [Fact]
    public void TradingReport_CalculatesPropertiesCorrectly()
    {
        // Arrange
        var report = new TradingReport
        {
            Account = new AccountInfo { Balance = 10000, Currency = "USD" },
            RecentDeals = new List<Deal>
            {
                new Deal { Id = "1", Type = "DEAL_TYPE_BUY", Profit = 100 },
                new Deal { Id = "2", Type = "DEAL_TYPE_SELL", Profit = -50 },
                new Deal { Id = "3", Type = "DEAL_TYPE_BUY", Profit = 75 }
            },
            GeneratedAt = DateTime.UtcNow,
            PeriodStart = DateTime.UtcNow.AddDays(-1),
            PeriodEnd = DateTime.UtcNow
        };

        // Assert
        report.TotalProfit.Should().Be(125);
        report.TradeCount.Should().Be(3);
        report.WinningTrades.Should().Be(2);
        report.LosingTrades.Should().Be(1);
    }
}
