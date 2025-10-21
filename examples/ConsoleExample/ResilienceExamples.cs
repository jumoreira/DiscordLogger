using DiscordLogger;
using DiscordLogger.Resilience;
using DiscordLogger.Resilience.Backoff;
using DiscordLogger.Resilience.CircuitBreaker;
using DiscordLogger.Resilience.RateLimiting;
using DiscordLogger.Resilience.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordLogger.Examples;

/// <summary>
/// Exemplos de uso dos recursos de Resiliência v1.4.0.
/// </summary>
public class ResilienceExamples
{
    /// <summary>
    /// Exemplo básico: Rate Limiting e Circuit Breaker.
    /// </summary>
    public static async Task BasicResilienceExample()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL";
                
                // Habilita resiliência
                options.Resilience.Enabled = true;
                
                // Rate Limiting: Token Bucket (50 req/s burst, 50 req/s sustentável)
                options.Resilience.RateLimiting.Enabled = true;
                options.Resilience.RateLimiting.Capacity = 50;
                options.Resilience.RateLimiting.RefillRate = 50;
                options.Resilience.RateLimiting.RefillIntervalSeconds = 1;
                
                // Circuit Breaker: abre após 5 falhas, aguarda 30s
                options.Resilience.CircuitBreaker.Enabled = true;
                options.Resilience.CircuitBreaker.FailureThreshold = 5;
                options.Resilience.CircuitBreaker.OpenTimeoutSeconds = 30;
                options.Resilience.CircuitBreaker.FallbackStrategy = FallbackStrategy.Queue;
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ResilienceExamples>>();

        // Envio protegido por rate limiting e circuit breaker
        for (int i = 0; i < 100; i++)
        {
            logger.LogInformation("Mensagem protegida #{Number}", i);
        }

        await serviceProvider.DisposeAsync();
    }

    /// <summary>
    /// Exemplo avançado: Rate Limiting Adaptativo.
    /// </summary>
    public static void AdaptiveRateLimitingExample()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL";
                
                options.Resilience.Enabled = true;
                
                // Rate Limiter Adaptativo: ajusta automaticamente baseado em sucesso/falha
                options.Resilience.RateLimiting.Enabled = true;
                options.Resilience.RateLimiting.Type = RateLimiterType.Adaptive;
                options.Resilience.RateLimiting.EnableAdaptive = true;
                options.Resilience.RateLimiting.Capacity = 50;
                options.Resilience.RateLimiting.MinCapacity = 10;  // Mínimo em caso de muitas falhas
                options.Resilience.RateLimiting.MaxCapacity = 200; // Máximo em caso de muitos sucessos
                options.Resilience.RateLimiting.IncreaseMultiplier = 1.2; // +20% quando taxa sucesso > 95%
                options.Resilience.RateLimiting.DecreaseMultiplier = 0.8; // -20% quando taxa sucesso < 80%
            });
        });
    }

    /// <summary>
    /// Exemplo: Circuit Breaker com Fallback para arquivo.
    /// </summary>
    public static void CircuitBreakerWithFileFallbackExample()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL";
                
                options.Resilience.Enabled = true;
                
                options.Resilience.CircuitBreaker.Enabled = true;
                options.Resilience.CircuitBreaker.FailureThreshold = 3;
                options.Resilience.CircuitBreaker.OpenTimeoutSeconds = 60;
                
                // Quando circuit breaker abre, salva em arquivo
                options.Resilience.CircuitBreaker.FallbackStrategy = FallbackStrategy.File;
                options.Resilience.CircuitBreaker.FileFallbackPath = "./logs/discord-fallback.log";
                
                options.Resilience.Persistence.EnableFileFallback = true;
                options.Resilience.Persistence.FileFallbackPath = "./logs/discord-fallback.log";
                options.Resilience.Persistence.MaxFileFallbackSizeMb = 100;
            });
        });
    }

    /// <summary>
    /// Exemplo: Dead Letter Queue com Auto Recovery.
    /// </summary>
    public static void DeadLetterQueueExample()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL";
                
                options.Resilience.Enabled = true;
                
                // Dead Letter Queue para mensagens que falharam múltiplas vezes
                options.Resilience.Persistence.EnableDeadLetterQueue = true;
                options.Resilience.Persistence.DeadLetterQueuePath = "./logs/discord-dlq.json";
                options.Resilience.Persistence.MaxAttemptsBeforeDLQ = 5;
                
                // Auto Recovery: tenta reenviar mensagens da DLQ a cada 5 minutos
                options.Resilience.Persistence.EnableAutoRecovery = true;
                options.Resilience.Persistence.RecoveryIntervalSeconds = 300;
                options.Resilience.Persistence.MaxMessagesPerRecoveryCycle = 100;
            });
        });
    }

    /// <summary>
    /// Exemplo: Backoff Strategies (Exponencial, Linear, Fibonacci).
    /// </summary>
    public static void BackoffStrategiesExample()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL";
                
                options.Resilience.Enabled = true;
                
                // Backoff Exponencial com Jitter
                options.Resilience.Backoff.Type = BackoffType.Exponential;
                options.Resilience.Backoff.InitialDelaySeconds = 1;
                options.Resilience.Backoff.MaxDelaySeconds = 300; // 5 minutos
                options.Resilience.Backoff.ExponentialMultiplier = 2.0;
                options.Resilience.Backoff.UseJitter = true; // Adiciona ±30% aleatoriedade
            });
        });

        // Alternativa: Linear Backoff
        var servicesLinear = new ServiceCollection();
        servicesLinear.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL";
                
                options.Resilience.Backoff.Type = BackoffType.Linear;
                options.Resilience.Backoff.LinearIncrementSeconds = 2; // +2s por tentativa
                options.Resilience.Backoff.MaxDelaySeconds = 60;
            });
        });

        // Alternativa: Fibonacci Backoff
        var servicesFibonacci = new ServiceCollection();
        servicesFibonacci.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL";
                
                options.Resilience.Backoff.Type = BackoffType.Fibonacci;
                options.Resilience.Backoff.InitialDelaySeconds = 1;
                options.Resilience.Backoff.MaxDelaySeconds = 120;
            });
        });
    }

    /// <summary>
    /// Exemplo: Múltiplos Webhooks com Roteamento por Nível de Log.
    /// </summary>
    public static void MultiWebhookByLogLevelExample()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.Resilience.Enabled = true;
                options.Resilience.MultiWebhook.Enabled = true;
                
                // Webhook para Critical/Error
                options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
                {
                    Name = "Errors",
                    WebhookUrl = "https://discord.com/api/webhooks/ERROR_CHANNEL",
                    LogLevels = new HashSet<LogLevel> { LogLevel.Critical, LogLevel.Error },
                    Priority = 10
                });
                
                // Webhook para Warning
                options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
                {
                    Name = "Warnings",
                    WebhookUrl = "https://discord.com/api/webhooks/WARNING_CHANNEL",
                    LogLevels = new HashSet<LogLevel> { LogLevel.Warning },
                    Priority = 5
                });
                
                // Webhook para Info/Debug
                options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
                {
                    Name = "General",
                    WebhookUrl = "https://discord.com/api/webhooks/GENERAL_CHANNEL",
                    LogLevels = new HashSet<LogLevel> { LogLevel.Information, LogLevel.Debug },
                    Priority = 1
                });
                
                // Fallback para mensagens que não correspondem
                options.Resilience.MultiWebhook.DefaultWebhookUrl = 
                    "https://discord.com/api/webhooks/DEFAULT_CHANNEL";
            });
        });
    }

    /// <summary>
    /// Exemplo: Múltiplos Webhooks com Roteamento por Categoria/Classe (Wildcards).
    /// </summary>
    public static void MultiWebhookByCategoryExample()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.Resilience.Enabled = true;
                options.Resilience.MultiWebhook.Enabled = true;
                options.Resilience.MultiWebhook.EnableCategoryRouting = true;
                
                // Webhook para Controllers
                options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
                {
                    Name = "API Controllers",
                    WebhookUrl = "https://discord.com/api/webhooks/API_CHANNEL",
                    CategoryPatterns = new HashSet<string> 
                    { 
                        "*Controller", 
                        "*.Controllers.*" 
                    },
                    Priority = 10
                });
                
                // Webhook para Services
                options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
                {
                    Name = "Business Services",
                    WebhookUrl = "https://discord.com/api/webhooks/BUSINESS_CHANNEL",
                    CategoryPatterns = new HashSet<string> 
                    { 
                        "*.Service", 
                        "*.Services.*",
                        "*Service"
                    },
                    Priority = 8
                });
                
                // Webhook para Repositories
                options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
                {
                    Name = "Data Layer",
                    WebhookUrl = "https://discord.com/api/webhooks/DATA_CHANNEL",
                    CategoryPatterns = new HashSet<string> 
                    { 
                        "*.Repository", 
                        "*.Repositories.*" 
                    },
                    Priority = 5
                });
            });
        });
    }

    /// <summary>
    /// Exemplo: Roteamento Fluente (API Builder).
    /// </summary>
    public static void FluentRoutingExample()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.ConfigureWebhookRouting(routing =>
                {
                    // Webhook para erros críticos
                    routing.AddRoute("https://discord.com/api/webhooks/CRITICAL", "Critical")
                        .ForCriticalOnly()
                        .WithPriority(100)
                        .WithUsername("🚨 Critical Alert Bot");
                    
                    // Webhook para erros em controllers
                    routing.AddRoute("https://discord.com/api/webhooks/API_ERRORS", "API Errors")
                        .ForErrorsOnly()
                        .ForControllers()
                        .WithPriority(50);
                    
                    // Webhook para services
                    routing.AddRoute("https://discord.com/api/webhooks/SERVICES", "Services")
                        .ForServices()
                        .WithPriority(30);
                    
                    // Webhook padrão
                    routing.WithDefaultWebhook("https://discord.com/api/webhooks/DEFAULT");
                });
            });
        });
    }

    /// <summary>
    /// Exemplo: Load Balancing (Round-Robin, Random, Broadcast).
    /// </summary>
    public static void LoadBalancingExample()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.Resilience.Enabled = true;
                options.Resilience.MultiWebhook.Enabled = true;
                
                // Round-Robin entre múltiplos webhooks
                options.Resilience.MultiWebhook.LoadBalancing = LoadBalancingStrategy.RoundRobin;
                
                options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/WEBHOOK_1"
                });
                
                options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/WEBHOOK_2"
                });
                
                options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
                {
                    WebhookUrl = "https://discord.com/api/webhooks/WEBHOOK_3"
                });
            });
        });

        // Alternativa: Broadcast (envia para todos)
        var servicesBroadcast = new ServiceCollection();
        servicesBroadcast.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                options.ConfigureWebhookRouting(routing =>
                {
                    routing.AddRoute("https://discord.com/api/webhooks/CHANNEL_1");
                    routing.AddRoute("https://discord.com/api/webhooks/CHANNEL_2");
                    routing.AddRoute("https://discord.com/api/webhooks/CHANNEL_3");
                    routing.EnableBroadcast(); // Envia para todos
                });
            });
        });
    }

    /// <summary>
    /// Exemplo: Configuração Completa com todos os recursos de resiliência.
    /// </summary>
    public static void CompleteResilienceExample()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddDiscordLogger(options =>
            {
                // Webhook principal
                options.WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL";
                options.Username = "Resilient Bot v1.4.0";
                
                // === RESILIÊNCIA ===
                options.Resilience.Enabled = true;
                options.Resilience.EnableVerboseLogging = true; // Debug
                
                // Rate Limiting Adaptativo
                options.Resilience.RateLimiting.Enabled = true;
                options.Resilience.RateLimiting.Type = RateLimiterType.Adaptive;
                options.Resilience.RateLimiting.Capacity = 50;
                options.Resilience.RateLimiting.MinCapacity = 10;
                options.Resilience.RateLimiting.MaxCapacity = 200;
                
                // Circuit Breaker
                options.Resilience.CircuitBreaker.Enabled = true;
                options.Resilience.CircuitBreaker.FailureThreshold = 5;
                options.Resilience.CircuitBreaker.OpenTimeoutSeconds = 30;
                options.Resilience.CircuitBreaker.FallbackStrategy = FallbackStrategy.Queue;
                options.Resilience.CircuitBreaker.EnableHealthMonitoring = true;
                options.Resilience.CircuitBreaker.EnableAutoRecovery = true;
                
                // Backoff Exponencial com Jitter
                options.Resilience.Backoff.Type = BackoffType.Exponential;
                options.Resilience.Backoff.UseJitter = true;
                
                // Persistência (DLQ + File Fallback + Auto Recovery)
                options.Resilience.Persistence.EnableDeadLetterQueue = true;
                options.Resilience.Persistence.EnableFileFallback = true;
                options.Resilience.Persistence.EnableRetryQueue = true;
                options.Resilience.Persistence.EnableAutoRecovery = true;
                options.Resilience.Persistence.RecoveryIntervalSeconds = 300;
                
                // Múltiplos Webhooks
                options.ConfigureWebhookRouting(routing =>
                {
                    routing.AddRoute("https://discord.com/api/webhooks/CRITICAL")
                        .ForCriticalOnly()
                        .WithPriority(100);
                    
                    routing.AddRoute("https://discord.com/api/webhooks/ERRORS")
                        .ForErrorsOnly()
                        .WithPriority(50);
                    
                    routing.AddRoute("https://discord.com/api/webhooks/GENERAL")
                        .WithPriority(1);
                    
                    routing.WithDefaultWebhook(options.WebhookUrl);
                });
            });
        });

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ResilienceExamples>>();

        // Uso normal - toda a resiliência é transparente
        logger.LogInformation("Mensagem protegida por todos os recursos de resiliência");
        logger.LogError("Erro protegido - irá para DLQ se falhar após todas as tentativas");
        logger.LogCritical("Crítico - roteado para webhook específico com máxima prioridade");
    }
}
