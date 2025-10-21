namespace DiscordLogger.Resilience.Backoff;

/// <summary>
/// Estratégia de backoff exponencial.
/// </summary>
public class ExponentialBackoffStrategy : IBackoffStrategy
{
    private readonly TimeSpan _initialDelay;
    private readonly TimeSpan _maxDelay;
    private readonly double _multiplier;
    private readonly bool _useJitter;
    private readonly Random _random;

    /// <summary>
    /// Inicializa o backoff exponencial.
    /// </summary>
    /// <param name="initialDelay">Delay inicial.</param>
    /// <param name="maxDelay">Delay máximo.</param>
    /// <param name="multiplier">Multiplicador exponencial. Padrão: 2.</param>
    /// <param name="useJitter">Adiciona jitter aleatório. Padrão: true.</param>
    public ExponentialBackoffStrategy(
        TimeSpan? initialDelay = null,
        TimeSpan? maxDelay = null,
        double multiplier = 2.0,
        bool useJitter = true)
    {
        _initialDelay = initialDelay ?? TimeSpan.FromSeconds(1);
        _maxDelay = maxDelay ?? TimeSpan.FromMinutes(5);
        _multiplier = multiplier;
        _useJitter = useJitter;
        _random = new Random();
    }

    /// <inheritdoc />
    public TimeSpan GetDelay(int attemptNumber)
    {
        if (attemptNumber <= 0)
            attemptNumber = 1;

        // Calcula delay base: initialDelay * multiplier^(attemptNumber - 1)
        var delay = _initialDelay.TotalMilliseconds * Math.Pow(_multiplier, attemptNumber - 1);

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
        // Não há estado para resetar em exponential backoff stateless
    }
}
