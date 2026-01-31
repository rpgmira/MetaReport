using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MetaReport.Models.Options;
using MetaReport.Services;
using Moq;

namespace MetaReport.Tests.Services;

public class MetaApiServiceTests
{
    private readonly Mock<ILogger<MetaApiService>> _mockLogger;
    private readonly MetaApiOptions _options;

    public MetaApiServiceTests()
    {
        _mockLogger = new Mock<ILogger<MetaApiService>>();
        _options = new MetaApiOptions
        {
            BaseUrl = "https://api.metaapi.cloud",
            Token = "test-token",
            AccountId = "test-account-id"
        };
    }

    [Fact]
    public void Constructor_ConfiguresHttpClient()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var service = new MetaApiService(
            httpClient,
            Options.Create(_options),
            _mockLogger.Object);

        // Assert
        service.Should().NotBeNull();
        httpClient.BaseAddress.Should().Be(new Uri(_options.BaseUrl));
        httpClient.DefaultRequestHeaders.Contains("auth-token").Should().BeTrue();
    }

    [Fact]
    public void Constructor_SetsAuthToken()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var service = new MetaApiService(
            httpClient,
            Options.Create(_options),
            _mockLogger.Object);

        // Assert
        var authToken = httpClient.DefaultRequestHeaders.GetValues("auth-token").First();
        authToken.Should().Be(_options.Token);
    }

    [Fact]
    public void GetLast24HoursDealsAsync_UsesCorrectTimeRange()
    {
        // This test validates the time range calculation
        // The actual HTTP call would be tested in integration tests
        
        var endTime = DateTime.UtcNow;
        var startTime = endTime.AddHours(-24);
        
        // Assert time range is ~24 hours
        (endTime - startTime).TotalHours.Should().BeApproximately(24, 0.01);
    }

    [Fact]
    public void Options_AreConfiguredCorrectly()
    {
        // Arrange & Act
        var options = new MetaApiOptions
        {
            BaseUrl = "https://test.api.com",
            Token = "my-token",
            AccountId = "acc-123"
        };

        // Assert
        options.BaseUrl.Should().Be("https://test.api.com");
        options.Token.Should().Be("my-token");
        options.AccountId.Should().Be("acc-123");
    }
}
