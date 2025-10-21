namespace DiscordLogger.Resilience.CircuitBreaker;

/// <summary>
/// Estados do Circuit Breaker.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>
    /// Circuito fechado - operações normais.
    /// </summary>
    Closed,

    /// <summary>
    /// Circuito aberto - todas as requisições são rejeitadas.
    /// </summary>
    Open,

    /// <summary>
    /// Circuito meio aberto - permitindo requisições de teste.
    /// </summary>
    HalfOpen
}

/// <summary>
/// Interface para Circuit Breaker pattern.
/// </summary>
public interface ICircuitBreaker
{
    /// <summary>
    /// Estado atual do circuit breaker.
    /// </summary>
    CircuitBreakerState State { get; }

    /// <summary>
    /// Executa uma ação protegida pelo circuit breaker.
    /// </summary>
    /// <typeparam name="T">Tipo de retorno da ação.</typeparam>
    /// <param name="action">Ação a ser executada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado da ação.</returns>
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa uma ação sem retorno protegida pelo circuit breaker.
    /// </summary>
    /// <param name="action">Ação a ser executada.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reseta o circuit breaker para o estado Closed.
    /// </summary>
    void Reset();

    /// <summary>
    /// Obtém estatísticas do circuit breaker.
    /// </summary>
    CircuitBreakerStatistics GetStatistics();
}

/// <summary>
/// Estatísticas do Circuit Breaker.
/// </summary>
public class CircuitBreakerStatistics
{
    /// <summary>
    /// Estado atual.
    /// </summary>
    public CircuitBreakerState State { get; set; }

    /// <summary>
    /// Número de falhas consecutivas.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Taxa de sucesso (0.0 a 1.0).
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Tempo médio de resposta em milissegundos.
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Timestamp da última transição de estado.
    /// </summary>
    public DateTimeOffset LastStateChange { get; set; }

    /// <summary>
    /// Total de requisições bem-sucedidas.
    /// </summary>
    public long TotalSuccesses { get; set; }

    /// <summary>
    /// Total de requisições falhadas.
    /// </summary>
    public long TotalFailures { get; set; }
}
