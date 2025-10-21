using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DiscordLogger.Tests;

public class DiscordLoggerProviderTests
{
    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DiscordLoggerProvider((DiscordLoggerOptions)null!));
    }

    [Fact]
    public void CreateLogger_ReturnsMicrosoftDiscordLogger()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/test"
        };
        var provider = new DiscordLoggerProvider(options);

        // Act
        var logger = provider.CreateLogger("TestCategory");

        // Assert
        Assert.NotNull(logger);
        Assert.IsAssignableFrom<Microsoft.Extensions.Logging.ILogger>(logger);
    }

    [Fact]
    public void CreateLogger_WithDifferentCategories_ReturnsDistinctLoggers()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/test"
        };
        var provider = new DiscordLoggerProvider(options);

        // Act
        var logger1 = provider.CreateLogger("Category1");
        var logger2 = provider.CreateLogger("Category2");

        // Assert
        Assert.NotNull(logger1);
        Assert.NotNull(logger2);
        Assert.NotSame(logger1, logger2);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/test"
        };
        var provider = new DiscordLoggerProvider(options);

        // Act & Assert - Should not throw
        provider.Dispose();
        provider.Dispose();
    }

    [Fact]
    public void CreateLogger_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/test"
        };
        var provider = new DiscordLoggerProvider(options);
        provider.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => provider.CreateLogger("Test"));
    }

    [Fact]
    public void SetScopeProvider_DoesNotThrow()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/test"
        };
        var provider = new DiscordLoggerProvider(options);

        // Act & Assert - Should not throw
        provider.SetScopeProvider(null);
    }
}
