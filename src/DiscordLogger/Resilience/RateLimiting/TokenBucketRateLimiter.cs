using System.Diagnostics;

namespace DiscordLogger.Resilience.RateLimiting;

/// <summary>
/// Implementação de Token Bucket Algorithm para rate limiting.
/// Permite burst de requisições enquanto mantém uma taxa média.
/// </summary>
public sealed class TokenBucketRateLimiter : IRateLimiter, IDisposable
{
    private readonly int _capacity;
    private readonly int _refillRate;
    private readonly TimeSpan _refillInterval;
    private readonly SemaphoreSlim _semaphore;
    private readonly Timer _refillTimer;
    private readonly object _lock = new();
    
    private int _tokens;
    private long _lastRefillTicks;
    private bool _disposed;

    /// <summary>
    /// Inicializa o Token Bucket Rate Limiter.
    /// </summary>
    /// <param name="capacity">Capacidade máxima do bucket (burst size).</param>
    /// <param name="refillRate">Número de tokens adicionados por intervalo.</param>
    /// <param name="refillInterval">Intervalo de tempo para refill.</param>
    public TokenBucketRateLimiter(int capacity, int refillRate, TimeSpan refillInterval)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity deve ser maior que zero.", nameof(capacity));
        if (refillRate <= 0)
            throw new ArgumentException("RefillRate deve ser maior que zero.", nameof(refillRate));
        if (refillInterval <= TimeSpan.Zero)
            throw new ArgumentException("RefillInterval deve ser maior que zero.", nameof(refillInterval));

        _capacity = capacity;
        _refillRate = refillRate;
        _refillInterval = refillInterval;
        _tokens = capacity;
        _lastRefillTicks = Stopwatch.GetTimestamp();
        _semaphore = new SemaphoreSlim(1, 1);

        // Timer para refill automático
        _refillTimer = new Timer(
            _ => Refill(),
            null,
            refillInterval,
            refillInterval
        );
    }

    /// <inheritdoc />
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        while (!_disposed)
        {
            if (TryAcquire())
            {
                return;
            }

            // Aguarda até o próximo refill
            var waitTime = CalculateWaitTime();
            if (waitTime > TimeSpan.Zero)
            {
                await Task.Delay(waitTime, cancellationToken);
            }
        }

        throw new ObjectDisposedException(nameof(TokenBucketRateLimiter));
    }

    /// <inheritdoc />
    public bool TryAcquire()
    {
        lock (_lock)
        {
            if (_disposed)
                return false;

            Refill();

            if (_tokens > 0)
            {
                _tokens--;
                return true;
            }

            return false;
        }
    }

    /// <inheritdoc />
    public void ReportSuccess()
    {
        // Token bucket não ajusta baseado em sucesso/falha
        // Pode ser usado em implementações adaptativas
    }

    /// <inheritdoc />
    public void ReportFailure()
    {
        // Token bucket não ajusta baseado em sucesso/falha
        // Pode ser usado em implementações adaptativas
    }

    /// <inheritdoc />
    public void Reset()
    {
        lock (_lock)
        {
            _tokens = _capacity;
            _lastRefillTicks = Stopwatch.GetTimestamp();
        }
    }

    private void Refill()
    {
        lock (_lock)
        {
            if (_disposed)
                return;

            var now = Stopwatch.GetTimestamp();
            var elapsed = TimeSpan.FromTicks((now - _lastRefillTicks) * TimeSpan.TicksPerSecond / Stopwatch.Frequency);

            if (elapsed >= _refillInterval)
            {
                var intervalsElapsed = (int)(elapsed.TotalMilliseconds / _refillInterval.TotalMilliseconds);
                var tokensToAdd = intervalsElapsed * _refillRate;

                _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
                _lastRefillTicks = now;
            }
        }
    }

    private TimeSpan CalculateWaitTime()
    {
        lock (_lock)
        {
            var now = Stopwatch.GetTimestamp();
            var elapsed = TimeSpan.FromTicks((now - _lastRefillTicks) * TimeSpan.TicksPerSecond / Stopwatch.Frequency);
            var remaining = _refillInterval - elapsed;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _refillTimer?.Dispose();
        _semaphore?.Dispose();
    }
}
