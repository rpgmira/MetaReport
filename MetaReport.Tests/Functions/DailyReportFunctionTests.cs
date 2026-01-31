using FluentAssertions;
using Microsoft.Extensions.Logging;
using MetaReport.Functions;
using MetaReport.Models;
using MetaReport.Services;
using Moq;

namespace MetaReport.Tests.Functions;

public class DailyReportFunctionTests
{
    private readonly Mock<IMetaApiService> _mockMetaApiService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<DailyReportFunction>> _mockLogger;

    public DailyReportFunctionTests()
    {
        _mockMetaApiService = new Mock<IMetaApiService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<DailyReportFunction>>();
    }

    private DailyReportFunction CreateFunction()
    {
        return new DailyReportFunction(
            _mockMetaApiService.Object,
            _mockEmailService.Object,
            _mockLogger.Object);
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
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Act
        var function = CreateFunction();

        // Assert
        function.Should().NotBeNull();
    }

    [Fact]
    public async Task Run_GeneratesReport_AndSendsEmail()
    {
        // Arrange
        var report = CreateSampleReport();
        _mockMetaApiService
            .Setup(s => s.GenerateReportAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
        _mockEmailService
            .Setup(s => s.SendReportAsync(report, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var function = CreateFunction();
        var timerInfo = CreateTimerInfo();

        // Act
        await function.Run(timerInfo, CancellationToken.None);

        // Assert
        _mockMetaApiService.Verify(s => s.GenerateReportAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(s => s.SendReportAsync(report, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_WhenEmailFails_LogsError()
    {
        // Arrange
        var report = CreateSampleReport();
        _mockMetaApiService
            .Setup(s => s.GenerateReportAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
        _mockEmailService
            .Setup(s => s.SendReportAsync(report, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var function = CreateFunction();
        var timerInfo = CreateTimerInfo();

        // Act
        await function.Run(timerInfo, CancellationToken.None);

        // Assert
        _mockMetaApiService.Verify(s => s.GenerateReportAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(s => s.SendReportAsync(report, It.IsAny<CancellationToken>()), Times.Once);
        // Logger should have been called with error
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Run_WhenApiServiceThrows_RethrowsException()
    {
        // Arrange
        _mockMetaApiService
            .Setup(s => s.GenerateReportAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API Error"));

        var function = CreateFunction();
        var timerInfo = CreateTimerInfo();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            function.Run(timerInfo, CancellationToken.None));
    }

    [Fact]
    public async Task Run_WhenEmailServiceThrows_RethrowsException()
    {
        // Arrange
        var report = CreateSampleReport();
        _mockMetaApiService
            .Setup(s => s.GenerateReportAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
        _mockEmailService
            .Setup(s => s.SendReportAsync(report, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email error"));

        var function = CreateFunction();
        var timerInfo = CreateTimerInfo();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            function.Run(timerInfo, CancellationToken.None));
    }

    [Fact]
    public async Task Run_WithPastDueTrigger_LogsWarning()
    {
        // Arrange
        var report = CreateSampleReport();
        _mockMetaApiService
            .Setup(s => s.GenerateReportAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
        _mockEmailService
            .Setup(s => s.SendReportAsync(report, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var function = CreateFunction();
        var timerInfo = CreateTimerInfo(isPastDue: true);

        // Act
        await function.Run(timerInfo, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private static Microsoft.Azure.Functions.Worker.TimerInfo CreateTimerInfo(bool isPastDue = false)
    {
        return new Microsoft.Azure.Functions.Worker.TimerInfo
        {
            IsPastDue = isPastDue,
            ScheduleStatus = new Microsoft.Azure.Functions.Worker.ScheduleStatus
            {
                Next = DateTime.UtcNow.AddDays(1),
                Last = DateTime.UtcNow.AddDays(-1),
                LastUpdated = DateTime.UtcNow
            }
        };
    }
}
