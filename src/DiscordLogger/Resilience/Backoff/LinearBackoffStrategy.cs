namespace DiscordLogger.Resilience.Backoff;

/// <summary>
/// Estratégia de backoff linear.
/// </summary>
public class LinearBackoffStrategy : IBackoffStrategy
{
    private readonly TimeSpan _increment;
    private readonly TimeSpan _maxDelay;
    private readonly bool _useJitter;
    private readonly Random _random;

    /// <summary>
    /// Inicializa o backoff linear.
    /// </summary>
    /// <param name="increment">Incremento linear por tentativa.</param>
    /// <param name="maxDelay">Delay máximo.</param>
    /// <param name="useJitter">Adiciona jitter aleatório. Padrão: true.</param>
    public LinearBackoffStrategy(
        TimeSpan? increment = null,
        TimeSpan? maxDelay = null,
        bool useJitter = true)
    {
        _increment = increment ?? TimeSpan.FromSeconds(2);
        _maxDelay = maxDelay ?? TimeSpan.FromMinutes(5);
        _useJitter = useJitter;
        _random = new Random();
    }

    /// <inheritdoc />
    public TimeSpan GetDelay(int attemptNumber)
    {
        if (attemptNumber <= 0)
            attemptNumber = 1;

        // Calcula delay linear: increment * attemptNumber
        var delay = _increment.TotalMilliseconds * attemptNumber;

        // Aplica limite máximo
        delay = Math.Min(delay, _maxDelay.TotalMilliseconds);

        // Adiciona jitter se habilitado
        if (_useJitter)
        {
            var jitter = _random.NextDouble() * 0.3; // ±30% jitter
            delay = delay * (1 + jitter - 0.15);
        }

        return TimeSpan.FromMilliseconds(delay);
    }

    /// <inheritdoc />
    public void Reset()
    {
        // Não há estado para resetar em linear backoff stateless
    }
}
