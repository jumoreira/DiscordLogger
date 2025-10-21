namespace DiscordLogger.Resilience.RateLimiting;

/// <summary>
/// Interface para implementações de rate limiting.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Verifica se uma operação pode ser executada respeitando o rate limit.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Task que completa quando a operação pode ser executada.</returns>
    Task WaitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tenta executar uma operação respeitando o rate limit.
    /// </summary>
    /// <returns>True se pode executar imediatamente, false se deve aguardar.</returns>
    bool TryAcquire();

    /// <summary>
    /// Reporta sucesso de uma operação para ajuste adaptativo.
    /// </summary>
    void ReportSuccess();

    /// <summary>
    /// Reporta falha de uma operação para ajuste adaptativo.
    /// </summary>
    void ReportFailure();

    /// <summary>
    /// Reseta o rate limiter para o estado inicial.
    /// </summary>
    void Reset();
}
