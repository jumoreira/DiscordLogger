namespace DiscordLogger.Tests;

public class DiscordLoggerTests
{
    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DiscordLogger(null!));
    }

    [Fact]
    public void Constructor_WithEmptyWebhookUrl_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = string.Empty
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DiscordLogger(options));
        Assert.Contains("WebhookUrl", exception.Message);
    }

    [Fact]
    public void Constructor_WithNullWebhookUrl_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = null!
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new DiscordLogger(options));
        Assert.Contains("WebhookUrl", exception.Message);
    }

    [Fact]
    public void Constructor_WithValidOptions_ShouldCreateInstance()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/123/abc"
        };

        // Act
        using var logger = new DiscordLogger(options);

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public async Task LogAsync_WithEmptyMessage_ShouldThrowArgumentException()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/123/abc"
        };

        using var logger = new DiscordLogger(options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await logger.LogAsync(LogLevel.Information, string.Empty)
        );
    }

    [Fact]
    public async Task LogDebugAsync_ShouldNotThrow()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/123/abc",
            MinimumLevel = LogLevel.Debug
        };

        using var logger = new DiscordLogger(options);

        // Act & Assert
        // Nota: Isso irá falhar ao tentar enviar para o webhook, mas não deve lançar exceção
        // devido ao tratamento de erro interno
        await logger.LogDebugAsync("Test debug message");
    }

    [Fact]
    public async Task LogInformationAsync_ShouldNotThrow()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/123/abc"
        };

        using var logger = new DiscordLogger(options);

        // Act & Assert
        await logger.LogInformationAsync("Test information message");
    }

    [Fact]
    public async Task LogWarningAsync_ShouldNotThrow()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/123/abc"
        };

        using var logger = new DiscordLogger(options);

        // Act & Assert
        await logger.LogWarningAsync("Test warning message");
    }

    [Fact]
    public async Task LogErrorAsync_WithException_ShouldNotThrow()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/123/abc"
        };

        using var logger = new DiscordLogger(options);
        var exception = new InvalidOperationException("Test exception");

        // Act & Assert
        await logger.LogErrorAsync("Test error message", exception);
    }

    [Fact]
    public async Task LogCriticalAsync_WithException_ShouldNotThrow()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/123/abc"
        };

        using var logger = new DiscordLogger(options);
        var exception = new InvalidOperationException("Test critical exception");

        // Act & Assert
        await logger.LogCriticalAsync("Test critical message", exception);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/123/abc"
        };

        var logger = new DiscordLogger(options);

        // Act & Assert
        logger.Dispose();
    }

    [Fact]
    public void Dispose_CalledTwice_ShouldNotThrow()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/123/abc"
        };

        var logger = new DiscordLogger(options);

        // Act & Assert
        logger.Dispose();
        logger.Dispose(); // Segunda chamada não deve lançar exceção
    }

    [Fact]
    public async Task LogAsync_AfterDispose_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/123/abc"
        };

        var logger = new DiscordLogger(options);
        logger.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await logger.LogAsync(LogLevel.Information, "Test message")
        );
    }
}
