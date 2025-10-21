using Microsoft.Extensions.Logging;
using Xunit;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DiscordLogger.Tests;

public class MicrosoftDiscordLoggerTests
{
    private class TestDiscordLogger : IDiscordLogger
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> LoggedMessages { get; } = new();

        public Task LogAsync(LogLevel level, string message, Exception? exception = null, string? scopeInfo = null, CancellationToken cancellationToken = default)
        {
            LoggedMessages.Add((level, message, exception));
            return Task.CompletedTask;
        }

        public Task LogDebugAsync(string message, CancellationToken cancellationToken = default)
            => LogAsync(LogLevel.Debug, message, null, null, cancellationToken);

        public Task LogInformationAsync(string message, CancellationToken cancellationToken = default)
            => LogAsync(LogLevel.Information, message, null, null, cancellationToken);

        public Task LogWarningAsync(string message, CancellationToken cancellationToken = default)
            => LogAsync(LogLevel.Warning, message, null, null, cancellationToken);

        public Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default)
            => LogAsync(LogLevel.Error, message, exception, null, cancellationToken);

        public Task LogCriticalAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default)
            => LogAsync(LogLevel.Critical, message, exception, null, cancellationToken);
    }

    [Fact]
    public void IsEnabled_WithNoneLogLevel_ReturnsFalse()
    {
        // Arrange
        var testLogger = new TestDiscordLogger();
        var options = new DiscordLoggerOptions { WebhookUrl = "https://discord.com/api/webhooks/test" };
        var logger = (Microsoft.Extensions.Logging.ILogger)new MicrosoftDiscordLogger("Test", testLogger, options);

        // Act
        var result = logger.IsEnabled(MsLogLevel.None);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(MsLogLevel.Trace, LogLevel.Debug)]
    [InlineData(MsLogLevel.Debug, LogLevel.Debug)]
    [InlineData(MsLogLevel.Information, LogLevel.Information)]
    [InlineData(MsLogLevel.Warning, LogLevel.Warning)]
    [InlineData(MsLogLevel.Error, LogLevel.Error)]
    [InlineData(MsLogLevel.Critical, LogLevel.Critical)]
    public void IsEnabled_RespectsMinimumLevel(MsLogLevel msLevel, LogLevel discordLevel)
    {
        // Arrange
        var testLogger = new TestDiscordLogger();
        var options = new DiscordLoggerOptions 
        { 
            WebhookUrl = "https://discord.com/api/webhooks/test",
            MinimumLevel = discordLevel
        };
        var logger = (Microsoft.Extensions.Logging.ILogger)new MicrosoftDiscordLogger("Test", testLogger, options);

        // Act
        var result = logger.IsEnabled(msLevel);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEnabled_BelowMinimumLevel_ReturnsFalse()
    {
        // Arrange
        var testLogger = new TestDiscordLogger();
        var options = new DiscordLoggerOptions 
        { 
            WebhookUrl = "https://discord.com/api/webhooks/test",
            MinimumLevel = LogLevel.Warning
        };
        var logger = (Microsoft.Extensions.Logging.ILogger)new MicrosoftDiscordLogger("Test", testLogger, options);

        // Act
        var result = logger.IsEnabled(MsLogLevel.Information);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Log_WithValidMessage_CallsDiscordLogger()
    {
        // Arrange
        var testLogger = new TestDiscordLogger();
        var options = new DiscordLoggerOptions { WebhookUrl = "https://discord.com/api/webhooks/test" };
        var logger = (Microsoft.Extensions.Logging.ILogger)new MicrosoftDiscordLogger("TestCategory", testLogger, options);

        // Act
        logger.LogInformation("Test message");
        
        // Wait for async operation
        await Task.Delay(100);

        // Assert
        Assert.Single(testLogger.LoggedMessages);
        var logged = testLogger.LoggedMessages[0];
        Assert.Equal(LogLevel.Information, logged.Level);
        Assert.Contains("TestCategory", logged.Message);
        Assert.Contains("Test message", logged.Message);
    }

    [Fact]
    public async Task Log_WithEventId_IncludesEventIdInMessage()
    {
        // Arrange
        var testLogger = new TestDiscordLogger();
        var options = new DiscordLoggerOptions { WebhookUrl = "https://discord.com/api/webhooks/test" };
        var logger = (Microsoft.Extensions.Logging.ILogger)new MicrosoftDiscordLogger("TestCategory", testLogger, options);
        var eventId = new Microsoft.Extensions.Logging.EventId(100, "TestEvent");

        // Act
        logger.Log(MsLogLevel.Information, eventId, "Test message", null, (msg, ex) => msg);
        
        // Wait for async operation
        await Task.Delay(100);

        // Assert
        Assert.Single(testLogger.LoggedMessages);
        var logged = testLogger.LoggedMessages[0];
        Assert.Contains("100", logged.Message);
        Assert.Contains("TestEvent", logged.Message);
    }

    [Fact]
    public async Task Log_WithException_PassesExceptionToDiscordLogger()
    {
        // Arrange
        var testLogger = new TestDiscordLogger();
        var options = new DiscordLoggerOptions { WebhookUrl = "https://discord.com/api/webhooks/test" };
        var logger = (Microsoft.Extensions.Logging.ILogger)new MicrosoftDiscordLogger("TestCategory", testLogger, options);
        var exception = new InvalidOperationException("Test exception");

        // Act
        logger.LogError(exception, "Error occurred");
        
        // Wait for async operation
        await Task.Delay(100);

        // Assert
        Assert.Single(testLogger.LoggedMessages);
        var logged = testLogger.LoggedMessages[0];
        Assert.Equal(LogLevel.Error, logged.Level);
        Assert.Same(exception, logged.Exception);
    }

    [Fact]
    public void Log_WithNullFormatter_ThrowsArgumentNullException()
    {
        // Arrange
        var testLogger = new TestDiscordLogger();
        var options = new DiscordLoggerOptions { WebhookUrl = "https://discord.com/api/webhooks/test" };
        var logger = (Microsoft.Extensions.Logging.ILogger)new MicrosoftDiscordLogger("Test", testLogger, options);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            logger.Log(MsLogLevel.Information, default, "state", null, null!));
    }

    [Fact]
    public void Log_BelowMinimumLevel_DoesNotCallDiscordLogger()
    {
        // Arrange
        var testLogger = new TestDiscordLogger();
        var options = new DiscordLoggerOptions 
        { 
            WebhookUrl = "https://discord.com/api/webhooks/test",
            MinimumLevel = LogLevel.Warning
        };
        var logger = (Microsoft.Extensions.Logging.ILogger)new MicrosoftDiscordLogger("Test", testLogger, options);

        // Act
        logger.LogInformation("This should not be logged");

        // Assert
        Assert.Empty(testLogger.LoggedMessages);
    }

    [Fact]
    public void BeginScope_ReturnsNull()
    {
        // Arrange
        var testLogger = new TestDiscordLogger();
        var options = new DiscordLoggerOptions { WebhookUrl = "https://discord.com/api/webhooks/test" };
        var logger = (Microsoft.Extensions.Logging.ILogger)new MicrosoftDiscordLogger("Test", testLogger, options);

        // Act
        var scope = logger.BeginScope("test scope");

        // Assert
        Assert.Null(scope);
    }

    [Theory]
    [InlineData(MsLogLevel.Trace, LogLevel.Debug)]
    [InlineData(MsLogLevel.Debug, LogLevel.Debug)]
    [InlineData(MsLogLevel.Information, LogLevel.Information)]
    [InlineData(MsLogLevel.Warning, LogLevel.Warning)]
    [InlineData(MsLogLevel.Error, LogLevel.Error)]
    [InlineData(MsLogLevel.Critical, LogLevel.Critical)]
    public async Task Log_ConvertsLogLevelCorrectly(MsLogLevel msLevel, LogLevel expectedDiscordLevel)
    {
        // Arrange
        var testLogger = new TestDiscordLogger();
        var options = new DiscordLoggerOptions 
        { 
            WebhookUrl = "https://discord.com/api/webhooks/test",
            MinimumLevel = LogLevel.Debug
        };
        var logger = (Microsoft.Extensions.Logging.ILogger)new MicrosoftDiscordLogger("Test", testLogger, options);

        // Act
        logger.Log(msLevel, default, "test", null, (msg, ex) => msg);
        
        // Wait for async operation
        await Task.Delay(100);

        // Assert
        Assert.Single(testLogger.LoggedMessages);
        Assert.Equal(expectedDiscordLevel, testLogger.LoggedMessages[0].Level);
    }
}
