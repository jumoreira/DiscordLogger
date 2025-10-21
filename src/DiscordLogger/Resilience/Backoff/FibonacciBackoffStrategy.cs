namespace DiscordLogger.Resilience.Backoff;

/// <summary>
/// Estratégia de backoff Fibonacci.
/// </summary>
public class FibonacciBackoffStrategy : IBackoffStrategy
{
    private readonly TimeSpan _baseDelay;
    private readonly TimeSpan _maxDelay;
    private readonly bool _useJitter;
    private readonly Random _random;

    /// <summary>
    /// Inicializa o backoff Fibonacci.
    /// </summary>
    /// <param name="baseDelay">Delay base para multiplicar pela sequência Fibonacci.</param>
    /// <param name="maxDelay">Delay máximo.</param>
    /// <param name="useJitter">Adiciona jitter aleatório. Padrão: true.</param>
    public FibonacciBackoffStrategy(
        TimeSpan? baseDelay = null,
        TimeSpan? maxDelay = null,
        bool useJitter = true)
    {
        _baseDelay = baseDelay ?? TimeSpan.FromSeconds(1);
        _maxDelay = maxDelay ?? TimeSpan.FromMinutes(5);
        _useJitter = useJitter;
        _random = new Random();
    }

    /// <inheritdoc />
    public TimeSpan GetDelay(int attemptNumber)
    {
        if (attemptNumber <= 0)
            attemptNumber = 1;

        // Calcula número Fibonacci
        var fibonacci = CalculateFibonacci(attemptNumber);

        // Calcula delay: baseDelay * fibonacci
        var delay = _baseDelay.TotalMilliseconds * fibonacci;

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
        // Não há estado para resetar em fibonacci backoff stateless
    }

    private static int CalculateFibonacci(int n)
    {
        if (n <= 1)
            return n;

        int a = 0, b = 1;
        for (int i = 2; i <= n; i++)
        {
            int temp = a + b;
            a = b;
            b = temp;
        }

        return b;
    }
}
