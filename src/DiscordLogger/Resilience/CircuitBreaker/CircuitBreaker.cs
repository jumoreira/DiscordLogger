using System.Diagnostics;

namespace DiscordLogger.Resilience.CircuitBreaker;

/// <summary>
/// Implementação do Circuit Breaker pattern para resiliência.
/// </summary>
public sealed class CircuitBreaker : ICircuitBreaker, IDisposable
{
    private readonly CircuitBreakerOptions _options;
    private readonly object _lock = new();
    private readonly Queue<bool> _recentResults;
    private readonly Queue<long> _recentResponseTimes;
    
    private CircuitBreakerState _state;
    private int _consecutiveFailures;
    private DateTimeOffset _lastStateChange;
    private DateTimeOffset _openedAt;
    private long _totalSuccesses;
    private long _totalFailures;
    private bool _disposed;

    /// <summary>
    /// Inicializa o Circuit Breaker.
    /// </summary>
    /// <param name="options">Opções de configuração.</param>
    public CircuitBreaker(CircuitBreakerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _state = CircuitBreakerState.Closed;
        _lastStateChange = DateTimeOffset.UtcNow;
        _recentResults = new Queue<bool>(_options.SamplingDuration);
        _recentResponseTimes = new Queue<long>(_options.SamplingDuration);
    }

    /// <inheritdoc />
    public CircuitBreakerState State
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
    }

    /// <inheritdoc />
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(CircuitBreaker));

        if (!CanExecute())
        {
            throw new CircuitBreakerOpenException("Circuit breaker está aberto. Requisições bloqueadas temporariamente.");
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await action();
            stopwatch.Stop();

            RecordSuccess(stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            RecordFailure(stopwatch.ElapsedMilliseconds);
            throw new CircuitBreakerException("Falha durante execução protegida pelo circuit breaker.", ex);
        }
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async () =>
        {
            await action();
            return true;
        }, cancellationToken);
    }

    /// <inheritdoc />
    public void Reset()
    {
        lock (_lock)
        {
            _state = CircuitBreakerState.Closed;
            _consecutiveFailures = 0;
            _lastStateChange = DateTimeOffset.UtcNow;
            _recentResults.Clear();
            _recentResponseTimes.Clear();
        }
    }

    /// <inheritdoc />
    public CircuitBreakerStatistics GetStatistics()
    {
        lock (_lock)
        {
            var totalRequests = _totalSuccesses + _totalFailures;
            var successRate = totalRequests > 0 ? (double)_totalSuccesses / totalRequests : 0;

            var avgResponseTime = _recentResponseTimes.Count > 0
                ? _recentResponseTimes.Average()
                : 0;

            return new CircuitBreakerStatistics
            {
                State = _state,
                FailureCount = _consecutiveFailures,
                SuccessRate = successRate,
                AverageResponseTime = avgResponseTime,
                LastStateChange = _lastStateChange,
                TotalSuccesses = _totalSuccesses,
                TotalFailures = _totalFailures
            };
        }
    }

    private bool CanExecute()
    {
        lock (_lock)
        {
            switch (_state)
            {
                case CircuitBreakerState.Closed:
                    return true;

                case CircuitBreakerState.Open:
                    // Verifica se já passou o tempo de timeout
                    if (DateTimeOffset.UtcNow - _openedAt >= TimeSpan.FromSeconds(_options.OpenTimeoutSeconds))
                    {
                        TransitionTo(CircuitBreakerState.HalfOpen);
                        return true;
                    }
                    return false;

                case CircuitBreakerState.HalfOpen:
                    return true;

                default:
                    return false;
            }
        }
    }

    private void RecordSuccess(long responseTimeMs)
    {
        lock (_lock)
        {
            _totalSuccesses++;
            _consecutiveFailures = 0;
            
            TrackResult(true, responseTimeMs);

            if (_state == CircuitBreakerState.HalfOpen)
            {
                // Testa se a taxa de sucesso está boa o suficiente
                if (GetSuccessRate() >= _options.SuccessThreshold)
                {
                    TransitionTo(CircuitBreakerState.Closed);
                }
            }

            // Verifica tempo de resposta (health monitoring)
            if (_options.EnableHealthMonitoring && responseTimeMs > _options.SlowRequestThresholdMs)
            {
                // Muitas requisições lentas podem indicar degradação do serviço
                var recentSlow = _recentResponseTimes.Count(t => t > _options.SlowRequestThresholdMs);
                var slowRate = _recentResponseTimes.Count > 0
                    ? (double)recentSlow / _recentResponseTimes.Count
                    : 0;

                if (slowRate > 0.5) // Mais de 50% lentas
                {
                    _consecutiveFailures++; // Trata como degradação
                }
            }
        }
    }

    private void RecordFailure(long responseTimeMs)
    {
        lock (_lock)
        {
            _totalFailures++;
            _consecutiveFailures++;
            
            TrackResult(false, responseTimeMs);

            // Transições de estado baseadas em falhas
            if (_state == CircuitBreakerState.Closed)
            {
                if (_consecutiveFailures >= _options.FailureThreshold)
                {
                    TransitionTo(CircuitBreakerState.Open);
                    _openedAt = DateTimeOffset.UtcNow;
                }
            }
            else if (_state == CircuitBreakerState.HalfOpen)
            {
                // Uma falha em HalfOpen volta para Open
                TransitionTo(CircuitBreakerState.Open);
                _openedAt = DateTimeOffset.UtcNow;
            }
        }
    }

    private void TrackResult(bool success, long responseTimeMs)
    {
        _recentResults.Enqueue(success);
        _recentResponseTimes.Enqueue(responseTimeMs);

        // Mantém apenas os últimos N resultados
        while (_recentResults.Count > _options.SamplingDuration)
        {
            _recentResults.Dequeue();
        }

        while (_recentResponseTimes.Count > _options.SamplingDuration)
        {
            _recentResponseTimes.Dequeue();
        }
    }

    private double GetSuccessRate()
    {
        if (_recentResults.Count == 0)
            return 0;

        return (double)_recentResults.Count(r => r) / _recentResults.Count;
    }

    private void TransitionTo(CircuitBreakerState newState)
    {
        if (_state != newState)
        {
            _state = newState;
            _lastStateChange = DateTimeOffset.UtcNow;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
    }
}

/// <summary>
/// Exceção lançada quando o circuit breaker está aberto.
/// </summary>
public class CircuitBreakerOpenException : Exception
{
    public CircuitBreakerOpenException(string message) : base(message) { }
}

/// <summary>
/// Exceção lançada quando ocorre falha durante execução protegida.
/// </summary>
public class CircuitBreakerException : Exception
{
    public CircuitBreakerException(string message, Exception innerException) 
        : base(message, innerException) { }
}
