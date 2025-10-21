using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DiscordLogger.Tests;

public class DiscordLoggerExtensionsTests
{
    [Fact]
    public void AddDiscordLogger_ToServiceCollection_RegistersIDiscordLogger()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDiscordLogger(options =>
        {
            options.WebhookUrl = "https://discord.com/api/webhooks/test";
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var discordLogger = serviceProvider.GetService<IDiscordLogger>();
        Assert.NotNull(discordLogger);
    }

    [Fact]
    public void AddDiscordLogger_ToServiceCollection_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddDiscordLogger(options => { }));
    }

    [Fact]
    public void AddDiscordLogger_ToServiceCollection_WithNullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            services.AddDiscordLogger(null!));
    }

    [Fact]
    public void AddDiscordLogger_ToLoggingBuilder_RegistersLoggerProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/test";
            });
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var loggerProvider = serviceProvider.GetService<ILoggerProvider>();

        // Assert
        Assert.NotNull(loggerProvider);
    }

    [Fact]
    public void AddDiscordLogger_ToLoggingBuilder_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        ILoggingBuilder builder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.AddDiscordLogger());
    }

    [Fact]
    public void AddDiscordLogger_ToLoggingBuilder_WithConfigure_RegistersOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        const string testUrl = "https://discord.com/api/webhooks/test123";

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = testUrl;
                options.MinimumLevel = LogLevel.Warning;
            });
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<DiscordLoggerOptions>>();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(testUrl, options.Value.WebhookUrl);
        Assert.Equal(LogLevel.Warning, options.Value.MinimumLevel);
    }

    [Fact]
    public void AddDiscordLogger_ToLoggingBuilder_WithWebhookUrl_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        const string testUrl = "https://discord.com/api/webhooks/direct";

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(testUrl);
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<DiscordLoggerOptions>>();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(testUrl, options.Value.WebhookUrl);
    }

    [Fact]
    public void AddDiscordLogger_ToLoggingBuilder_WithEmptyWebhookUrl_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            services.AddLogging(builder => builder.AddDiscordLogger("")));
    }

    [Fact]
    public void AddDiscordLogger_ToLoggingBuilder_WithWebhookUrlAndLevel_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        const string testUrl = "https://discord.com/api/webhooks/withlevel";

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(testUrl, LogLevel.Critical);
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<DiscordLoggerOptions>>();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(testUrl, options.Value.WebhookUrl);
        Assert.Equal(LogLevel.Critical, options.Value.MinimumLevel);
    }

    [Fact]
    public void AddDiscordLogger_MultipleConfigurations_LastWins()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/first";
                options.MinimumLevel = LogLevel.Debug;
            });
        });

        services.Configure<DiscordLoggerOptions>(options =>
        {
            options.WebhookUrl = "https://discord.com/api/webhooks/second";
            options.MinimumLevel = LogLevel.Error;
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<DiscordLoggerOptions>>();

        // Assert
        Assert.NotNull(options);
        Assert.Equal("https://discord.com/api/webhooks/second", options.Value.WebhookUrl);
        Assert.Equal(LogLevel.Error, options.Value.MinimumLevel);
    }

    [Fact]
    public void AddDiscordLogger_CreatesLoggerFactory_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/factory";
            });
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("TestCategory");

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void AddDiscordLogger_GenericLogger_CanBeResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/generic";
            });
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<DiscordLoggerExtensionsTests>>();

        // Assert
        Assert.NotNull(logger);
    }
}
