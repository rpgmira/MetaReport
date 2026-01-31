using FluentAssertions;
using MetaReport.Models.Options;

namespace MetaReport.Tests.Models.Options;

public class EmailOptionsTests
{
    [Fact]
    public void GetRecipients_WithCommaSeparatedAddresses_ShouldParseCorrectly()
    {
        // Arrange
        var options = new EmailOptions
        {
            ToAddresses = "user1@test.com,user2@test.com,user3@test.com"
        };

        // Act
        var recipients = options.GetRecipients();

        // Assert
        recipients.Should().HaveCount(3);
        recipients.Should().Contain("user1@test.com");
        recipients.Should().Contain("user2@test.com");
        recipients.Should().Contain("user3@test.com");
    }

    [Fact]
    public void GetRecipients_WithSpacesAroundCommas_ShouldTrimAddresses()
    {
        // Arrange
        var options = new EmailOptions
        {
            ToAddresses = " user1@test.com , user2@test.com , user3@test.com "
        };

        // Act
        var recipients = options.GetRecipients();

        // Assert
        recipients.Should().HaveCount(3);
        recipients.Should().Contain("user1@test.com");
        recipients.Should().Contain("user2@test.com");
        recipients.Should().Contain("user3@test.com");
    }

    [Fact]
    public void GetRecipients_WithEmptyToAddresses_ShouldReturnEmpty()
    {
        // Arrange
        var options = new EmailOptions
        {
            ToAddresses = ""
        };

        // Act
        var recipients = options.GetRecipients();

        // Assert
        recipients.Should().BeEmpty();
    }

    [Fact]
    public void GetRecipients_WithLegacyToAddress_ShouldIncludeIt()
    {
        // Arrange
        var options = new EmailOptions
        {
            ToAddress = "legacy@test.com"
        };

        // Act
        var recipients = options.GetRecipients();

        // Assert
        recipients.Should().HaveCount(1);
        recipients.Should().Contain("legacy@test.com");
    }

    [Fact]
    public void GetRecipients_WithBothToAddressesAndLegacyToAddress_ShouldIncludeBoth()
    {
        // Arrange
        var options = new EmailOptions
        {
            ToAddresses = "user1@test.com,user2@test.com",
            ToAddress = "legacy@test.com"
        };

        // Act
        var recipients = options.GetRecipients();

        // Assert
        recipients.Should().HaveCount(3);
        recipients.Should().Contain("user1@test.com");
        recipients.Should().Contain("user2@test.com");
        recipients.Should().Contain("legacy@test.com");
    }

    [Fact]
    public void GetRecipients_WithDuplicateLegacyAddress_ShouldNotDuplicate()
    {
        // Arrange
        var options = new EmailOptions
        {
            ToAddresses = "user1@test.com,user2@test.com",
            ToAddress = "user1@test.com" // Duplicate
        };

        // Act
        var recipients = options.GetRecipients();

        // Assert
        recipients.Should().HaveCount(2); // Should not have duplicate
    }

    [Fact]
    public void GetRecipients_WithDuplicateLegacyAddressDifferentCase_ShouldNotDuplicate()
    {
        // Arrange
        var options = new EmailOptions
        {
            ToAddresses = "User1@Test.com,user2@test.com",
            ToAddress = "user1@test.com" // Duplicate with different case
        };

        // Act
        var recipients = options.GetRecipients();

        // Assert
        recipients.Should().HaveCount(2); // Should not have duplicate (case-insensitive)
    }

    [Fact]
    public void GetRecipients_WithEmptyEntriesInList_ShouldIgnoreThem()
    {
        // Arrange
        var options = new EmailOptions
        {
            ToAddresses = "user1@test.com,,user2@test.com,,"
        };

        // Act
        var recipients = options.GetRecipients();

        // Assert
        recipients.Should().HaveCount(2);
    }
}
