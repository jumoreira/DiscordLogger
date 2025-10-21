using DiscordLogger.Resilience.Backoff;
using DiscordLogger.Resilience.CircuitBreaker;
using DiscordLogger.Resilience.Persistence;
using DiscordLogger.Resilience.RateLimiting;
using DiscordLogger.Resilience.Routing;

namespace DiscordLogger.Resilience;

/// <summary>
/// Opções consolidadas de resiliência e confiabilidade.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Opções de Rate Limiting avançado.
    /// </summary>
    public RateLimitingOptions RateLimiting { get; set; } = new();

    /// <summary>
    /// Opções de Circuit Breaker.
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Opções de estratégias de Backoff.
    /// </summary>
    public BackoffOptions Backoff { get; set; } = new();

    /// <summary>
    /// Opções de persistência de falhas (DLQ, File Fallback, Retry Queue).
    /// </summary>
    public PersistenceOptions Persistence { get; set; } = new();

    /// <summary>
    /// Opções de múltiplos webhooks e roteamento.
    /// </summary>
    public MultiWebhookOptions MultiWebhook { get; set; } = new();

    /// <summary>
    /// Habilita todos os recursos de resiliência. Padrão: true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Habilita telemetria e métricas de resiliência. Padrão: true.
    /// </summary>
    public bool EnableTelemetry { get; set; } = true;

    /// <summary>
    /// Habilita logs detalhados de operações de resiliência. Padrão: false.
    /// Útil para debugging mas pode gerar muito log.
    /// </summary>
    public bool EnableVerboseLogging { get; set; } = false;
}
