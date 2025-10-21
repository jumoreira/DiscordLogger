using DiscordLogger.Formatters;
using Xunit;

namespace DiscordLogger.Tests.Formatters;

public class DefaultMessageFormatterTests
{
    private readonly DiscordLoggerOptions _options;

    public DefaultMessageFormatterTests()
    {
        _options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/test",
            Username = "TestBot",
            AvatarUrl = "https://example.com/avatar.png"
        };
    }

    [Fact]
    public void FormatMessage_ShouldCreateValidMessage()
    {
        // Arrange
        var formatter = new DefaultMessageFormatter();

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            "Test message",
            null,
            _options);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(_options.Username, message.Username);
        Assert.Equal(_options.AvatarUrl, message.AvatarUrl);
        Assert.NotNull(message.Embeds);
        Assert.Single(message.Embeds);
    }

    [Fact]
    public void FormatMessage_ShouldSetCorrectEmbedTitle()
    {
        // Arrange
        var formatter = new DefaultMessageFormatter();

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Error,
            "Error occurred",
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.Contains("Error", embed.Title);
        Assert.Contains("❌", embed.Title);
    }

    [Fact]
    public void FormatMessage_WithException_ShouldIncludeExceptionFields()
    {
        // Arrange
        var formatter = new DefaultMessageFormatter();
        var exception = new InvalidOperationException("Test exception");

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Error,
            "Error occurred",
            exception,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.NotNull(embed.Fields);
        Assert.Contains(embed.Fields, f => f.Name == "Exception Type");
        Assert.Contains(embed.Fields, f => f.Name == "Exception Message");
    }

    [Fact]
    public void FormatMessage_WithScopeInfo_ShouldIncludeScopeField()
    {
        // Arrange
        var formatter = new DefaultMessageFormatter();
        var scopeInfo = "RequestId=123, UserId=456";

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            "Test message",
            null,
            _options,
            scopeInfo);

        // Assert
        var embed = message.Embeds![0];
        Assert.NotNull(embed.Fields);
        Assert.Contains(embed.Fields, f => f.Name == "Context" && f.Value!.Contains(scopeInfo));
    }

    [Theory]
    [InlineData(LogLevel.Debug, 0x808080)]
    [InlineData(LogLevel.Information, 0x0099FF)]
    [InlineData(LogLevel.Warning, 0xFFCC00)]
    [InlineData(LogLevel.Error, 0xFF6600)]
    [InlineData(LogLevel.Critical, 0xFF0000)]
    public void FormatMessage_ShouldSetCorrectColor(LogLevel level, int expectedColor)
    {
        // Arrange
        var formatter = new DefaultMessageFormatter();

        // Act
        var message = formatter.FormatMessage(level, "Test", null, _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.Equal(expectedColor, embed.Color);
    }

    [Fact]
    public void FormatMessage_ShouldTruncateLongMessages()
    {
        // Arrange
        var formatter = new DefaultMessageFormatter();
        var longMessage = new string('x', 5000);

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            longMessage,
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.True(embed.Description!.Length <= 4096);
        Assert.EndsWith("...", embed.Description);
    }

    [Fact]
    public void FormatBatch_ShouldCreateMultipleEmbeds()
    {
        // Arrange
        var formatter = new DefaultMessageFormatter();
        var messages = new List<(LogLevel, string, Exception?)>
        {
            (LogLevel.Information, "Message 1", null),
            (LogLevel.Warning, "Message 2", null),
            (LogLevel.Error, "Message 3", new Exception("Test"))
        };

        // Act
        var webhookMessage = formatter.FormatBatch(messages, _options);

        // Assert
        Assert.NotNull(webhookMessage.Embeds);
        Assert.Equal(3, webhookMessage.Embeds.Count);
    }

    [Fact]
    public void FormatBatch_ShouldLimitTo10Embeds()
    {
        // Arrange
        var formatter = new DefaultMessageFormatter();
        var messages = new List<(LogLevel, string, Exception?)>();
        for (int i = 0; i < 15; i++)
        {
            messages.Add((LogLevel.Information, $"Message {i}", null));
        }

        // Act
        var webhookMessage = formatter.FormatBatch(messages, _options);

        // Assert
        Assert.NotNull(webhookMessage.Embeds);
        Assert.Equal(10, webhookMessage.Embeds.Count);
    }

    [Fact]
    public void FormatMessage_WithInnerException_ShouldIncludeInnerExceptionField()
    {
        // Arrange
        var formatter = new DefaultMessageFormatter();
        var innerException = new ArgumentException("Inner error");
        var exception = new InvalidOperationException("Outer error", innerException);

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Error,
            "Error occurred",
            exception,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.Contains(embed.Fields!, f => f.Name == "Inner Exception");
    }

    [Fact]
    public void FormatMessage_ShouldIncludeTimestamp()
    {
        // Arrange
        var formatter = new DefaultMessageFormatter();

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            "Test",
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.NotNull(embed.Timestamp);
        Assert.NotEmpty(embed.Timestamp);
    }

    [Fact]
    public void FormatMessage_ShouldIncludeFooter()
    {
        // Arrange
        var formatter = new DefaultMessageFormatter();

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            "Test",
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.NotNull(embed.Footer);
        Assert.Contains("Logged at", embed.Footer.Text);
    }
}
