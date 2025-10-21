namespace DiscordLogger.Resilience.Backoff;

/// <summary>
/// Interface para estratégias de backoff.
/// </summary>
public interface IBackoffStrategy
{
    /// <summary>
    /// Calcula o tempo de espera para a próxima tentativa.
    /// </summary>
    /// <param name="attemptNumber">Número da tentativa (começa em 1).</param>
    /// <returns>Tempo de espera.</returns>
    TimeSpan GetDelay(int attemptNumber);

    /// <summary>
    /// Reseta o estado do backoff.
    /// </summary>
    void Reset();
}
