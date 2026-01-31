using FluentAssertions;
using MetaReport.Models.Options;

namespace MetaReport.Tests.Models.Options;

public class MetaApiOptionsTests
{
    [Fact]
    public void BaseUrl_HasDefaultValue()
    {
        // Arrange & Act
        var options = new MetaApiOptions();

        // Assert
        options.BaseUrl.Should().Be("https://mt-client-api-v1.new-york.agiliumtrade.ai");
    }

    [Fact]
    public void Token_DefaultsToEmptyString()
    {
        // Arrange & Act
        var options = new MetaApiOptions();

        // Assert
        options.Token.Should().BeEmpty();
    }

    [Fact]
    public void AccountId_DefaultsToEmptyString()
    {
        // Arrange & Act
        var options = new MetaApiOptions();

        // Assert
        options.AccountId.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var options = new MetaApiOptions
        {
            BaseUrl = "https://api.example.com",
            Token = "my-secret-token",
            AccountId = "account-12345"
        };

        // Assert
        options.BaseUrl.Should().Be("https://api.example.com");
        options.Token.Should().Be("my-secret-token");
        options.AccountId.Should().Be("account-12345");
    }

    [Fact]
    public void Properties_CanBeModified()
    {
        // Arrange
        var options = new MetaApiOptions
        {
            BaseUrl = "https://old.api.com"
        };

        // Act
        options.BaseUrl = "https://new.api.com";

        // Assert
        options.BaseUrl.Should().Be("https://new.api.com");
    }

    [Fact]
    public void SectionName_IsMetaApi()
    {
        // Assert
        MetaApiOptions.SectionName.Should().Be("MetaApi");
    }
}
