using System.Text.Json;
using DiscordLogger.Models;
using DiscordLogger.Resilience.Backoff;
using DiscordLogger.Resilience.CircuitBreaker;
using DiscordLogger.Resilience.Persistence;
using DiscordLogger.Resilience.RateLimiting;
using DiscordLogger.Resilience.Routing;

namespace DiscordLogger.Resilience;

/// <summary>
/// Cliente HTTP resiliente para comunicação com Discord webhooks.
/// Integra Rate Limiting, Circuit Breaker, Backoff e Persistência.
/// </summary>
public sealed class ResilientWebhookClient : IDisposable
{
    private readonly DiscordLoggerOptions _options;
    private readonly DiscordWebhookClient _innerClient;
    private readonly IRateLimiter? _rateLimiter;
    private readonly ICircuitBreaker? _circuitBreaker;
    private readonly IBackoffStrategy _backoffStrategy;
    private readonly IDeadLetterQueue? _deadLetterQueue;
    private readonly IFileFallback? _fileFallback;
    private readonly IWebhookRouter? _webhookRouter;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public ResilientWebhookClient(DiscordLoggerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _innerClient = new DiscordWebhookClient(options);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        // Inicializa componentes de resiliência se habilitado
        if (_options.Resilience.Enabled)
        {
            // Rate Limiting
            if (_options.Resilience.RateLimiting.Enabled)
            {
                _rateLimiter = CreateRateLimiter();
            }

            // Circuit Breaker
            if (_options.Resilience.CircuitBreaker.Enabled)
            {
                _circuitBreaker = new CircuitBreaker.CircuitBreaker(_options.Resilience.CircuitBreaker);
            }

            // Backoff Strategy
            _backoffStrategy = CreateBackoffStrategy();

            // Dead Letter Queue
            if (_options.Resilience.Persistence.EnableDeadLetterQueue)
            {
                _deadLetterQueue = new FileBasedDeadLetterQueue(
                    _options.Resilience.Persistence.DeadLetterQueuePath
                );
            }

            // File Fallback
            if (_options.Resilience.Persistence.EnableFileFallback)
            {
                _fileFallback = new FileFallback(
                    _options.Resilience.Persistence.FileFallbackPath,
                    _options.Resilience.Persistence.MaxFileFallbackSizeMb
                );
            }

            // Webhook Router
            if (_options.Resilience.MultiWebhook.Enabled)
            {
                _webhookRouter = new WebhookRouter(_options.Resilience.MultiWebhook);
            }
        }
        else
        {
            // Usa backoff simples mesmo sem resiliência habilitada
            _backoffStrategy = new ExponentialBackoffStrategy();
        }
    }

    /// <summary>
    /// Envia uma mensagem de forma resiliente.
    /// </summary>
    public async Task SendMessageAsync(
        DiscordWebhookMessage message,
        LogLevel logLevel,
        string? category,
        CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ResilientWebhookClient));

        var attempt = 0;
        var maxAttempts = _options.Resilience.Persistence.MaxAttemptsBeforeDLQ;
        Exception? lastException = null;

        while (attempt < maxAttempts)
        {
            attempt++;

            try
            {
                // Rate Limiting
                if (_rateLimiter != null)
                {
                    await _rateLimiter.WaitAsync(cancellationToken);
                }

                // Circuit Breaker
                if (_circuitBreaker != null)
                {
                    await _circuitBreaker.ExecuteAsync(async () =>
                    {
                        await SendToWebhookAsync(message, logLevel, category, cancellationToken);
                    }, cancellationToken);
                }
                else
                {
                    await SendToWebhookAsync(message, logLevel, category, cancellationToken);
                }

                // Sucesso - reporta ao rate limiter
                _rateLimiter?.ReportSuccess();
                return;
            }
            catch (CircuitBreakerOpenException ex)
            {
                // Circuit breaker aberto - usa fallback
                lastException = ex;
                await HandleFallbackAsync(message, logLevel, category, "Circuit breaker aberto", cancellationToken);
                return; // Não tenta novamente quando circuit está aberto
            }
            catch (Exception ex)
            {
                lastException = ex;
                _rateLimiter?.ReportFailure();

                if (attempt < maxAttempts)
                {
                    // Aguarda com backoff
                    var delay = _backoffStrategy.GetDelay(attempt);
                    
                    if (_options.Resilience.EnableVerboseLogging)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[DiscordLogger] Tentativa {attempt}/{maxAttempts} falhou. Aguardando {delay.TotalSeconds:F2}s. Erro: {ex.Message}"
                        );
                    }

                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        // Esgotou todas as tentativas - move para Dead Letter Queue
        await HandleDeadLetterAsync(message, logLevel, category, lastException, cancellationToken);
    }

    private async Task SendToWebhookAsync(
        DiscordWebhookMessage message,
        LogLevel logLevel,
        string? category,
        CancellationToken cancellationToken)
    {
        // Resolve webhook(s) via roteador
        var webhookUrls = ResolveWebhooks(logLevel, category);

        if (webhookUrls.Count == 0)
        {
            throw new InvalidOperationException("Nenhum webhook configurado ou resolvido.");
        }

        // Envia para todos os webhooks resolvidos (broadcast se configurado)
        var tasks = webhookUrls.Select(url => SendToSingleWebhookAsync(url, message, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task SendToSingleWebhookAsync(
        string webhookUrl,
        DiscordWebhookMessage message,
        CancellationToken cancellationToken)
    {
        // Temporariamente altera o webhook URL do cliente interno
        var originalUrl = _options.WebhookUrl;
        try
        {
            _options.WebhookUrl = webhookUrl;
            await _innerClient.SendMessageAsync(message, cancellationToken);
        }
        finally
        {
            _options.WebhookUrl = originalUrl;
        }
    }

    private IList<string> ResolveWebhooks(LogLevel logLevel, string? category)
    {
        if (_webhookRouter != null)
        {
            return _webhookRouter.ResolveWebhooks(logLevel, category);
        }

        // Se não há roteador, usa o webhook padrão
        if (!string.IsNullOrEmpty(_options.WebhookUrl))
        {
            return new List<string> { _options.WebhookUrl };
        }

        return new List<string>();
    }

    private async Task HandleFallbackAsync(
        DiscordWebhookMessage message,
        LogLevel logLevel,
        string? category,
        string reason,
        CancellationToken cancellationToken)
    {
        var strategy = _options.Resilience.CircuitBreaker.FallbackStrategy;

        switch (strategy)
        {
            case FallbackStrategy.File:
                if (_fileFallback != null)
                {
                    var messageText = FormatMessageForFallback(message);
                    await _fileFallback.SaveAsync(messageText, logLevel, cancellationToken);
                }
                break;

            case FallbackStrategy.Queue:
                // Enfileira para retry posterior (via DLQ)
                await HandleDeadLetterAsync(message, logLevel, category, new Exception(reason), cancellationToken);
                break;

            case FallbackStrategy.AlternativeWebhook:
                if (!string.IsNullOrEmpty(_options.Resilience.CircuitBreaker.FallbackWebhookUrl))
                {
                    try
                    {
                        await SendToSingleWebhookAsync(
                            _options.Resilience.CircuitBreaker.FallbackWebhookUrl,
                            message,
                            cancellationToken
                        );
                    }
                    catch
                    {
                        // Se webhook alternativo também falha, usa file fallback
                        if (_fileFallback != null)
                        {
                            var messageText = FormatMessageForFallback(message);
                            await _fileFallback.SaveAsync(messageText, logLevel, cancellationToken);
                        }
                    }
                }
                break;

            case FallbackStrategy.Discard:
                // Descarta mensagem (perda de dados)
                if (_options.Resilience.EnableVerboseLogging)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[DiscordLogger] Mensagem descartada: {reason}"
                    );
                }
                break;
        }
    }

    private async Task HandleDeadLetterAsync(
        DiscordWebhookMessage message,
        LogLevel logLevel,
        string? category,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        if (_deadLetterQueue == null)
        {
            // Se DLQ não está habilitado, tenta file fallback
            if (_fileFallback != null)
            {
                var messageText = FormatMessageForFallback(message);
                await _fileFallback.SaveAsync(messageText, logLevel, cancellationToken);
            }
            return;
        }

        var failedMessage = new FailedLogMessage
        {
            FirstAttempt = DateTimeOffset.UtcNow,
            LastAttempt = DateTimeOffset.UtcNow,
            AttemptCount = _options.Resilience.Persistence.MaxAttemptsBeforeDLQ,
            MessageContent = JsonSerializer.Serialize(message, _jsonOptions),
            LogLevel = logLevel,
            LastError = exception?.Message,
            WebhookUrl = _options.WebhookUrl,
            Category = category
        };

        await _deadLetterQueue.EnqueueAsync(failedMessage, cancellationToken);

        if (_options.Resilience.EnableVerboseLogging)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[DiscordLogger] Mensagem movida para DLQ após {failedMessage.AttemptCount} tentativas. Erro: {exception?.Message}"
            );
        }
    }

    private string FormatMessageForFallback(DiscordWebhookMessage message)
    {
        return JsonSerializer.Serialize(message, _jsonOptions);
    }

    private IRateLimiter CreateRateLimiter()
    {
        var options = _options.Resilience.RateLimiting;

        if (options.EnableAdaptive)
        {
            return new AdaptiveRateLimiter(
                initialCapacity: options.Capacity,
                minCapacity: options.MinCapacity,
                maxCapacity: options.MaxCapacity,
                refillInterval: TimeSpan.FromSeconds(options.RefillIntervalSeconds),
                increaseMultiplier: options.IncreaseMultiplier,
                decreaseMultiplier: options.DecreaseMultiplier
            );
        }

        return new TokenBucketRateLimiter(
            capacity: options.Capacity,
            refillRate: options.RefillRate,
            refillInterval: TimeSpan.FromSeconds(options.RefillIntervalSeconds)
        );
    }

    private IBackoffStrategy CreateBackoffStrategy()
    {
        var options = _options.Resilience.Backoff;

        return options.Type switch
        {
            BackoffType.Linear => new LinearBackoffStrategy(
                increment: TimeSpan.FromSeconds(options.LinearIncrementSeconds),
                maxDelay: TimeSpan.FromSeconds(options.MaxDelaySeconds),
                useJitter: options.UseJitter
            ),
            BackoffType.Fibonacci => new FibonacciBackoffStrategy(
                baseDelay: TimeSpan.FromSeconds(options.InitialDelaySeconds),
                maxDelay: TimeSpan.FromSeconds(options.MaxDelaySeconds),
                useJitter: options.UseJitter
            ),
            _ => new ExponentialBackoffStrategy(
                initialDelay: TimeSpan.FromSeconds(options.InitialDelaySeconds),
                maxDelay: TimeSpan.FromSeconds(options.MaxDelaySeconds),
                multiplier: options.ExponentialMultiplier,
                useJitter: options.UseJitter
            )
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _innerClient?.Dispose();
        (_rateLimiter as IDisposable)?.Dispose();
        (_circuitBreaker as IDisposable)?.Dispose();
        (_deadLetterQueue as IDisposable)?.Dispose();
        (_fileFallback as IDisposable)?.Dispose();
    }
}
