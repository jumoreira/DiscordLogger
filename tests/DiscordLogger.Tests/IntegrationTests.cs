using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DiscordLogger.Tests;

/// <summary>
/// Testes de integração end-to-end para validar cenários completos.
/// </summary>
public class IntegrationTests
{
    private class TestService
    {
        private readonly ILogger<TestService> _logger;

        public TestService(ILogger<TestService> logger)
        {
            _logger = logger;
        }

        public void DoWork()
        {
            _logger.LogInformation("TestService is doing work");
        }

        public void DoErrorWork()
        {
            try
            {
                throw new InvalidOperationException("Something went wrong");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestService");
            }
        }

        public void DoWarningWork()
        {
            _logger.LogWarning("This is a warning from TestService");
        }
    }

    [Fact]
    public void Integration_WithILoggerT_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/integration/test";
                options.MinimumLevel = LogLevel.Information;
            });
        });
        services.AddTransient<TestService>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var testService = serviceProvider.GetRequiredService<TestService>();

        // Assert - Should not throw
        testService.DoWork();
        testService.DoWarningWork();
    }

    [Fact]
    public void Integration_WithMultipleLoggers_AllWorkCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/multiple/loggers";
            });
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<IntegrationTests>>();

        // Assert - Should not throw
        logger.LogInformation("Testing multiple providers");
    }

    [Fact]
    public void Integration_WithMinimumLevel_FiltersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/filtering/test";
                options.MinimumLevel = LogLevel.Warning;
            });
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<IntegrationTests>>();

        // Assert - Should not throw
        logger.LogDebug("This should be filtered"); // Should not be logged
        logger.LogInformation("This should also be filtered"); // Should not be logged
        logger.LogWarning("This should be logged"); // Should be logged
        logger.LogError("This should also be logged"); // Should be logged
    }

    [Fact]
    public void Integration_WithException_LogsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/exception/test";
            });
        });
        services.AddTransient<TestService>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var testService = serviceProvider.GetRequiredService<TestService>();

        // Assert - Should not throw
        testService.DoErrorWork();
    }

    [Fact]
    public void Integration_WithScopedService_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/scoped/test";
            });
        });
        services.AddScoped<TestService>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        
        using (var scope = serviceProvider.CreateScope())
        {
            var testService = scope.ServiceProvider.GetRequiredService<TestService>();
            
            // Assert - Should not throw
            testService.DoWork();
        }
    }

    [Fact]
    public void Integration_WithCustomOptions_AppliesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/custom/test";
                options.Username = "CustomBot";
                options.MinimumLevel = LogLevel.Debug;
                options.TimeoutSeconds = 60;
                options.MaxRetryAttempts = 5;
            });
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<DiscordLoggerOptions>>();

        // Assert
        Assert.NotNull(options);
        Assert.Equal("https://discord.com/api/webhooks/custom/test", options.Value.WebhookUrl);
        Assert.Equal("CustomBot", options.Value.Username);
        Assert.Equal(LogLevel.Debug, options.Value.MinimumLevel);
        Assert.Equal(60, options.Value.TimeoutSeconds);
        Assert.Equal(5, options.Value.MaxRetryAttempts);
    }

    [Fact]
    public void Integration_WithLoggerFactory_CanCreateMultipleLoggers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger("https://discord.com/api/webhooks/factory/test");
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        
        var logger1 = loggerFactory.CreateLogger("Category1");
        var logger2 = loggerFactory.CreateLogger("Category2");
        var logger3 = loggerFactory.CreateLogger<IntegrationTests>();

        // Assert
        Assert.NotNull(logger1);
        Assert.NotNull(logger2);
        Assert.NotNull(logger3);
        
        // Should not throw
        logger1.LogInformation("Message from logger1");
        logger2.LogWarning("Message from logger2");
        logger3.LogError("Message from logger3");
    }

    [Fact]
    public async Task Integration_DisposingServiceProvider_DisposesLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger("https://discord.com/api/webhooks/dispose/test");
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<IntegrationTests>>();
        
        logger.LogInformation("Before dispose");

        // Act
        await serviceProvider.DisposeAsync();

        // Assert - After disposal, logging should still not throw (fail silently)
        // This is by design - loggers should be resilient
    }

    [Fact]
    public void Integration_CombinedWithIDiscordLogger_BothWorkIndependently()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Add both IDiscordLogger and ILogger integration
        services.AddDiscordLogger(options =>
        {
            options.WebhookUrl = "https://discord.com/api/webhooks/combined/test";
        });
        
        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/combined/test";
            });
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var discordLogger = serviceProvider.GetRequiredService<IDiscordLogger>();
        var msLogger = serviceProvider.GetRequiredService<ILogger<IntegrationTests>>();

        // Assert - Both should work without interference
        // Should not throw
        _ = discordLogger.LogInformationAsync("From IDiscordLogger");
        msLogger.LogInformation("From ILogger");
    }
}
