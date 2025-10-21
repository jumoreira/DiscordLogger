namespace DiscordLogger.Tests;

public class DiscordLoggerOptionsTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var options = new DiscordLoggerOptions();

        // Assert
        Assert.Equal(string.Empty, options.WebhookUrl);
        Assert.Null(options.Username);
        Assert.Null(options.AvatarUrl);
        Assert.Equal(LogLevel.Information, options.MinimumLevel);
        Assert.Equal(30, options.TimeoutSeconds);
        Assert.Equal(3, options.MaxRetryAttempts);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new DiscordLoggerOptions();

        // Act
        options.WebhookUrl = "https://discord.com/api/webhooks/test";
        options.Username = "TestBot";
        options.AvatarUrl = "https://example.com/avatar.png";
        options.MinimumLevel = LogLevel.Warning;
        options.TimeoutSeconds = 60;
        options.MaxRetryAttempts = 5;

        // Assert
        Assert.Equal("https://discord.com/api/webhooks/test", options.WebhookUrl);
        Assert.Equal("TestBot", options.Username);
        Assert.Equal("https://example.com/avatar.png", options.AvatarUrl);
        Assert.Equal(LogLevel.Warning, options.MinimumLevel);
        Assert.Equal(60, options.TimeoutSeconds);
        Assert.Equal(5, options.MaxRetryAttempts);
    }
}
