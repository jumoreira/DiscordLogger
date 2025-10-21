using System.Diagnostics;

namespace DiscordLogger.Resilience.RateLimiting;

/// <summary>
/// Rate limiter adaptativo que ajusta automaticamente a taxa baseado em sucesso/falha.
/// </summary>
public sealed class AdaptiveRateLimiter : IRateLimiter, IDisposable
{
    private readonly int _minCapacity;
    private readonly int _maxCapacity;
    private readonly TimeSpan _refillInterval;
    private readonly double _increaseMultiplier;
    private readonly double _decreaseMultiplier;
    private readonly object _lock = new();
    
    private TokenBucketRateLimiter _innerLimiter;
    private int _currentCapacity;
    private int _successCount;
    private int _failureCount;
    private long _lastAdjustmentTicks;
    private bool _disposed;

    /// <summary>
    /// Inicializa o Adaptive Rate Limiter.
    /// </summary>
    /// <param name="initialCapacity">Capacidade inicial.</param>
    /// <param name="minCapacity">Capacidade mínima.</param>
    /// <param name="maxCapacity">Capacidade máxima.</param>
    /// <param name="refillInterval">Intervalo de refill.</param>
    /// <param name="increaseMultiplier">Multiplicador de aumento (ex: 1.2 = +20%).</param>
    /// <param name="decreaseMultiplier">Multiplicador de redução (ex: 0.8 = -20%).</param>
    public AdaptiveRateLimiter(
        int initialCapacity = 50,
        int minCapacity = 10,
        int maxCapacity = 200,
        TimeSpan? refillInterval = null,
        double increaseMultiplier = 1.2,
        double decreaseMultiplier = 0.8)
    {
        if (initialCapacity < minCapacity || initialCapacity > maxCapacity)
            throw new ArgumentException("InitialCapacity deve estar entre MinCapacity e MaxCapacity.");

        _currentCapacity = initialCapacity;
        _minCapacity = minCapacity;
        _maxCapacity = maxCapacity;
        _refillInterval = refillInterval ?? TimeSpan.FromSeconds(1);
        _increaseMultiplier = increaseMultiplier;
        _decreaseMultiplier = decreaseMultiplier;
        _lastAdjustmentTicks = Stopwatch.GetTimestamp();

        _innerLimiter = new TokenBucketRateLimiter(
            _currentCapacity,
            _currentCapacity,
            _refillInterval
        );
    }

    /// <inheritdoc />
    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AdaptiveRateLimiter));

            AdjustIfNeeded();
            return _innerLimiter.WaitAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public bool TryAcquire()
    {
        lock (_lock)
        {
            if (_disposed)
                return false;

            AdjustIfNeeded();
            return _innerLimiter.TryAcquire();
        }
    }

    /// <inheritdoc />
    public void ReportSuccess()
    {
        lock (_lock)
        {
            if (_disposed)
                return;

            _successCount++;
        }
    }

    /// <inheritdoc />
    public void ReportFailure()
    {
        lock (_lock)
        {
            if (_disposed)
                return;

            _failureCount++;
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
        lock (_lock)
        {
            _innerLimiter?.Reset();
            _successCount = 0;
            _failureCount = 0;
            _lastAdjustmentTicks = Stopwatch.GetTimestamp();
        }
    }

    private void AdjustIfNeeded()
    {
        var now = Stopwatch.GetTimestamp();
        var elapsed = TimeSpan.FromTicks((now - _lastAdjustmentTicks) * TimeSpan.TicksPerSecond / Stopwatch.Frequency);

        // Ajusta a cada 10 segundos
        if (elapsed < TimeSpan.FromSeconds(10))
            return;

        var totalRequests = _successCount + _failureCount;
        if (totalRequests == 0)
            return;

        var successRate = (double)_successCount / totalRequests;

        int newCapacity = _currentCapacity;

        // Taxa de sucesso > 95%: aumenta capacidade
        if (successRate > 0.95)
        {
            newCapacity = (int)Math.Min(_maxCapacity, _currentCapacity * _increaseMultiplier);
        }
        // Taxa de sucesso < 80%: reduz capacidade
        else if (successRate < 0.80)
        {
            newCapacity = (int)Math.Max(_minCapacity, _currentCapacity * _decreaseMultiplier);
        }

        if (newCapacity != _currentCapacity)
        {
            _currentCapacity = newCapacity;
            
            // Recria o limiter interno com nova capacidade
            _innerLimiter?.Dispose();
            _innerLimiter = new TokenBucketRateLimiter(
                _currentCapacity,
                _currentCapacity,
                _refillInterval
            );
        }

        _successCount = 0;
        _failureCount = 0;
        _lastAdjustmentTicks = now;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _innerLimiter?.Dispose();
    }
}
