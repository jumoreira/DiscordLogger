using DiscordLogger.Resilience.Routing;
using Xunit;

namespace DiscordLogger.Tests.Resilience;

public class WebhookRouterTests
{
    [Fact]
    public void WebhookRouter_ReturnsDefaultWhenNoRoutesMatch()
    {
        // Arrange
        var options = new MultiWebhookOptions
        {
            Enabled = true,
            DefaultWebhookUrl = "https://discord.com/api/webhooks/default"
        };
        var router = new WebhookRouter(options);

        // Act
        var webhooks = router.ResolveWebhooks(LogLevel.Information, "TestCategory");

        // Assert
        Assert.Single(webhooks);
        Assert.Equal("https://discord.com/api/webhooks/default", webhooks[0]);
    }

    [Fact]
    public void WebhookRouter_RoutesBasedOnLogLevel()
    {
        // Arrange
        var options = new MultiWebhookOptions
        {
            Enabled = true,
            EnableLogLevelRouting = true,
            Routes = new List<WebhookRoute>
            {
                new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/critical",
                    LogLevels = new HashSet<LogLevel> { LogLevel.Critical }
                },
                new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/info",
                    LogLevels = new HashSet<LogLevel> { LogLevel.Information }
                }
            }
        };
        var router = new WebhookRouter(options);

        // Act
        var criticalWebhooks = router.ResolveWebhooks(LogLevel.Critical, null);
        var infoWebhooks = router.ResolveWebhooks(LogLevel.Information, null);

        // Assert
        Assert.Single(criticalWebhooks);
        Assert.Equal("https://discord.com/api/webhooks/critical", criticalWebhooks[0]);
        
        Assert.Single(infoWebhooks);
        Assert.Equal("https://discord.com/api/webhooks/info", infoWebhooks[0]);
    }

    [Fact]
    public void WebhookRouter_RoutesBasedOnCategory()
    {
        // Arrange
        var options = new MultiWebhookOptions
        {
            Enabled = true,
            EnableCategoryRouting = true,
            Routes = new List<WebhookRoute>
            {
                new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/controllers",
                    CategoryPatterns = new HashSet<string> { "*Controller" }
                },
                new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/services",
                    CategoryPatterns = new HashSet<string> { "*Service" } // Corrigido: removido o ponto antes de Service
                }
            }
        };
        var router = new WebhookRouter(options);

        // Act
        var controllerWebhooks = router.ResolveWebhooks(LogLevel.Information, "HomeController");
        var serviceWebhooks = router.ResolveWebhooks(LogLevel.Information, "MyApp.Business.UserService");

        // Assert
        Assert.Single(controllerWebhooks);
        Assert.Equal("https://discord.com/api/webhooks/controllers", controllerWebhooks[0]);
        
        Assert.Single(serviceWebhooks);
        Assert.Equal("https://discord.com/api/webhooks/services", serviceWebhooks[0]);
    }

    [Fact]
    public void WebhookRouter_RespectsRoutePriority()
    {
        // Arrange
        var options = new MultiWebhookOptions
        {
            Enabled = true,
            LoadBalancing = LoadBalancingStrategy.Priority,
            Routes = new List<WebhookRoute>
            {
                new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/low",
                    Priority = 1
                },
                new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/high",
                    Priority = 10
                }
            }
        };
        var router = new WebhookRouter(options);

        // Act
        var webhooks = router.ResolveWebhooks(LogLevel.Information, null);

        // Assert - Deve retornar a rota de maior prioridade
        Assert.Single(webhooks);
        Assert.Equal("https://discord.com/api/webhooks/high", webhooks[0]);
    }

    [Fact]
    public void WebhookRouter_BroadcastsToMultipleRoutes()
    {
        // Arrange
        var options = new MultiWebhookOptions
        {
            Enabled = true,
            LoadBalancing = LoadBalancingStrategy.Broadcast,
            Routes = new List<WebhookRoute>
            {
                new WebhookRoute { WebhookUrl = "https://discord.com/api/webhooks/1" },
                new WebhookRoute { WebhookUrl = "https://discord.com/api/webhooks/2" },
                new WebhookRoute { WebhookUrl = "https://discord.com/api/webhooks/3" }
            }
        };
        var router = new WebhookRouter(options);

        // Act
        var webhooks = router.ResolveWebhooks(LogLevel.Information, null);

        // Assert - Deve retornar todas as rotas
        Assert.Equal(3, webhooks.Count);
    }

    [Fact]
    public void WebhookRouter_UsesFallbackRoute()
    {
        // Arrange
        var options = new MultiWebhookOptions
        {
            Enabled = true,
            EnableCategoryRouting = true,
            Routes = new List<WebhookRoute>
            {
                new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/specific",
                    CategoryPatterns = new HashSet<string> { "SpecificCategory" }
                },
                new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/fallback",
                    IsFallback = true
                }
            }
        };
        var router = new WebhookRouter(options);

        // Act - Categoria que não corresponde
        var webhooks = router.ResolveWebhooks(LogLevel.Information, "RandomCategory");

        // Assert - Deve usar fallback
        Assert.Single(webhooks);
        Assert.Equal("https://discord.com/api/webhooks/fallback", webhooks[0]);
    }

    [Fact]
    public void WebhookRouter_TracksStatistics()
    {
        // Arrange
        var options = new MultiWebhookOptions
        {
            Enabled = true,
            Routes = new List<WebhookRoute>
            {
                new WebhookRoute { WebhookUrl = "https://discord.com/api/webhooks/test" }
            }
        };
        var router = new WebhookRouter(options);

        // Act
        router.ResolveWebhooks(LogLevel.Information, null);
        router.ResolveWebhooks(LogLevel.Error, null);
        router.ResolveWebhooks(LogLevel.Warning, null);

        var stats = router.GetStatistics();

        // Assert
        Assert.Equal(3, stats.TotalRouted);
        Assert.True(stats.RouteCounts.ContainsKey("https://discord.com/api/webhooks/test"));
        Assert.Equal(3, stats.RouteCounts["https://discord.com/api/webhooks/test"]);
    }

    [Fact]
    public void WebhookRouter_MatchesWildcardPatterns()
    {
        // Arrange
        var options = new MultiWebhookOptions
        {
            Enabled = true,
            EnableCategoryRouting = true,
            Routes = new List<WebhookRoute>
            {
                new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/namespace",
                    CategoryPatterns = new HashSet<string> { "MyApp.Business.*" }
                }
            }
        };
        var router = new WebhookRouter(options);

        // Act
        var webhooks1 = router.ResolveWebhooks(LogLevel.Information, "MyApp.Business.UserService");
        var webhooks2 = router.ResolveWebhooks(LogLevel.Information, "MyApp.Business.OrderService");
        var webhooks3 = router.ResolveWebhooks(LogLevel.Information, "OtherApp.Service");

        // Assert
        Assert.Single(webhooks1);
        Assert.Single(webhooks2);
        Assert.Empty(webhooks3); // Não corresponde ao padrão
    }
}
