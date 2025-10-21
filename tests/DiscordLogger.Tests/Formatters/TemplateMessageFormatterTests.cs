using DiscordLogger.Formatters;
using Xunit;

namespace DiscordLogger.Tests.Formatters;

public class TemplateMessageFormatterTests
{
    private readonly DiscordLoggerOptions _options;

    public TemplateMessageFormatterTests()
    {
        _options = new DiscordLoggerOptions
        {
            WebhookUrl = "https://discord.com/api/webhooks/test",
            Username = "TestBot"
        };
    }

    [Fact]
    public void FormatMessage_WithDefaultTemplate_ShouldWork()
    {
        // Arrange
        var formatter = new TemplateMessageFormatter();

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            "Test message",
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.Contains("Information", embed.Title);
        Assert.Contains("ℹ️", embed.Title);
    }

    [Fact]
    public void FormatMessage_WithCustomTitleTemplate_ShouldUseTemplate()
    {
        // Arrange
        var formatter = new TemplateMessageFormatter(
            titleTemplate: "[{level}] {emoji}");

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Warning,
            "Test message",
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.StartsWith("[Warning]", embed.Title);
        Assert.Contains("⚠️", embed.Title);
    }

    [Fact]
    public void FormatMessage_WithCustomDescriptionTemplate_ShouldUseTemplate()
    {
        // Arrange
        var formatter = new TemplateMessageFormatter(
            descriptionTemplate: "{timestamp}: {message}");

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            "Test message",
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.Contains("Test message", embed.Description);
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", embed.Description);
    }

    [Fact]
    public void FormatMessage_WithTimestampPlaceholder_ShouldReplaceCorrectly()
    {
        // Arrange
        var formatter = new TemplateMessageFormatter(
            titleTemplate: "{timestamp}");

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            "Test",
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", embed.Title);
    }

    [Fact]
    public void FormatMessage_WithDatePlaceholder_ShouldReplaceCorrectly()
    {
        // Arrange
        var formatter = new TemplateMessageFormatter(
            titleTemplate: "{date}");

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            "Test",
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", embed.Title);
    }

    [Fact]
    public void FormatMessage_WithTimePlaceholder_ShouldReplaceCorrectly()
    {
        // Arrange
        var formatter = new TemplateMessageFormatter(
            titleTemplate: "{time}");

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            "Test",
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.Matches(@"\d{2}:\d{2}:\d{2}", embed.Title);
    }

    [Fact]
    public void FormatMessage_WithExceptionPlaceholder_ShouldReplaceCorrectly()
    {
        // Arrange
        var formatter = new TemplateMessageFormatter(
            titleTemplate: "{emoji} {level} - {exception}");

        var exception = new InvalidOperationException("Test error");

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Error,
            "Error occurred",
            exception,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.Contains("InvalidOperationException", embed.Title);
    }

    [Fact]
    public void FormatMessage_WithoutException_ShouldNotShowExceptionPlaceholder()
    {
        // Arrange
        var formatter = new TemplateMessageFormatter(
            titleTemplate: "{emoji} {level} - {exception}");

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            "Normal message",
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.DoesNotContain("Exception", embed.Title);
        Assert.EndsWith(" - ", embed.Title); // Empty placeholder
    }

    [Fact]
    public void FormatMessage_WithAllPlaceholders_ShouldReplaceAll()
    {
        // Arrange
        var formatter = new TemplateMessageFormatter(
            descriptionTemplate: "{emoji} [{level}] {date} {time} - {message}");

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Warning,
            "Test warning",
            null,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.Contains("⚠️", embed.Description);
        Assert.Contains("Warning", embed.Description);
        Assert.Contains("Test warning", embed.Description);
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", embed.Description);
        Assert.Matches(@"\d{2}:\d{2}:\d{2}", embed.Description);
    }

    [Fact]
    public void FormatMessage_WithScopeInfo_ShouldStillIncludeScopeField()
    {
        // Arrange
        var formatter = new TemplateMessageFormatter();
        var scopeInfo = "RequestId=123";

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Information,
            "Test",
            null,
            _options,
            scopeInfo);

        // Assert
        var embed = message.Embeds![0];
        Assert.Contains(embed.Fields!, f => f.Name == "Context" && f.Value!.Contains(scopeInfo));
    }

    [Fact]
    public void FormatMessage_WithException_ShouldStillIncludeExceptionFields()
    {
        // Arrange
        var formatter = new TemplateMessageFormatter();
        var exception = new Exception("Test error");

        // Act
        var message = formatter.FormatMessage(
            LogLevel.Error,
            "Error",
            exception,
            _options);

        // Assert
        var embed = message.Embeds![0];
        Assert.Contains(embed.Fields!, f => f.Name == "Exception Type");
        Assert.Contains(embed.Fields!, f => f.Name == "Exception Message");
    }
}
