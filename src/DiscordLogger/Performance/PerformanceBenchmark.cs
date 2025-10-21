using System.Diagnostics;

namespace DiscordLogger.Performance;

/// <summary>
/// Utilitário para benchmarking e análise de performance.
/// </summary>
public sealed class PerformanceBenchmark : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly long _initialMemory;
    private readonly string _operationName;
    private bool _disposed;

    private PerformanceBenchmark(string operationName)
    {
        _operationName = operationName;
        _initialMemory = GC.GetTotalMemory(forceFullCollection: false);
        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Inicia um novo benchmark.
    /// </summary>
    public static PerformanceBenchmark Start(string operationName)
    {
        return new PerformanceBenchmark(operationName);
    }

    /// <summary>
    /// Para o benchmark e retorna os resultados.
    /// </summary>
    public BenchmarkResult Stop()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(PerformanceBenchmark));
        }

        _stopwatch.Stop();
        var finalMemory = GC.GetTotalMemory(forceFullCollection: false);

        return new BenchmarkResult
        {
            OperationName = _operationName,
            ElapsedMilliseconds = _stopwatch.ElapsedMilliseconds,
            ElapsedTicks = _stopwatch.ElapsedTicks,
            MemoryAllocatedBytes = finalMemory - _initialMemory,
            Timestamp = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _stopwatch.Stop();
    }
}

/// <summary>
/// Resultado de um benchmark.
/// </summary>
public sealed class BenchmarkResult
{
    /// <summary>
    /// Nome da operação medida.
    /// </summary>
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// Tempo decorrido em milissegundos.
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// Tempo decorrido em ticks.
    /// </summary>
    public long ElapsedTicks { get; set; }

    /// <summary>
    /// Memória alocada durante a operação em bytes.
    /// </summary>
    public long MemoryAllocatedBytes { get; set; }

    /// <summary>
    /// Timestamp do resultado.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Memória alocada em megabytes.
    /// </summary>
    public double MemoryAllocatedMB => MemoryAllocatedBytes / (1024.0 * 1024.0);

    /// <summary>
    /// Tempo decorrido como TimeSpan.
    /// </summary>
    public TimeSpan Elapsed => TimeSpan.FromMilliseconds(ElapsedMilliseconds);

    public override string ToString()
    {
        return $"[{OperationName}] {ElapsedMilliseconds}ms, {MemoryAllocatedMB:F2}MB allocated";
    }
}

/// <summary>
/// Coletor de métricas de performance contínuas.
/// </summary>
public sealed class PerformanceMonitor : IDisposable
{
    private readonly List<BenchmarkResult> _results = new();
    private readonly object _lock = new();
    private readonly Timer? _reportTimer;
    private bool _disposed;

    public PerformanceMonitor(bool enableAutoReporting = false, int reportIntervalSeconds = 60)
    {
        if (enableAutoReporting)
        {
            _reportTimer = new Timer(
                _ => GenerateReport(),
                null,
                TimeSpan.FromSeconds(reportIntervalSeconds),
                TimeSpan.FromSeconds(reportIntervalSeconds)
            );
        }
    }

    /// <summary>
    /// Adiciona um resultado de benchmark.
    /// </summary>
    public void AddResult(BenchmarkResult result)
    {
        lock (_lock)
        {
            _results.Add(result);

            // Limita o histórico a 1000 resultados
            if (_results.Count > 1000)
            {
                _results.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Mede a execução de uma operação.
    /// </summary>
    public async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation)
    {
        using var benchmark = PerformanceBenchmark.Start(operationName);
        try
        {
            return await operation();
        }
        finally
        {
            var result = benchmark.Stop();
            AddResult(result);
        }
    }

    /// <summary>
    /// Mede a execução de uma operação síncrona.
    /// </summary>
    public T Measure<T>(string operationName, Func<T> operation)
    {
        using var benchmark = PerformanceBenchmark.Start(operationName);
        try
        {
            return operation();
        }
        finally
        {
            var result = benchmark.Stop();
            AddResult(result);
        }
    }

    /// <summary>
    /// Gera um relatório de performance.
    /// </summary>
    public PerformanceReport GenerateReport()
    {
        lock (_lock)
        {
            if (_results.Count == 0)
            {
                return new PerformanceReport();
            }

            var grouped = _results
                .GroupBy(r => r.OperationName)
                .Select(g => new OperationStats
                {
                    OperationName = g.Key,
                    Count = g.Count(),
                    AverageMilliseconds = g.Average(r => r.ElapsedMilliseconds),
                    MinMilliseconds = g.Min(r => r.ElapsedMilliseconds),
                    MaxMilliseconds = g.Max(r => r.ElapsedMilliseconds),
                    TotalMemoryMB = g.Sum(r => r.MemoryAllocatedMB),
                    AverageMemoryMB = g.Average(r => r.MemoryAllocatedMB)
                })
                .ToList();

            return new PerformanceReport
            {
                TotalOperations = _results.Count,
                OperationStats = grouped,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Limpa o histórico de resultados.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _results.Clear();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _reportTimer?.Dispose();
    }
}

/// <summary>
/// Relatório de performance.
/// </summary>
public sealed class PerformanceReport
{
    /// <summary>
    /// Total de operações medidas.
    /// </summary>
    public int TotalOperations { get; set; }

    /// <summary>
    /// Estatísticas por operação.
    /// </summary>
    public List<OperationStats> OperationStats { get; set; } = new();

    /// <summary>
    /// Data e hora de geração do relatório.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    public override string ToString()
    {
        var sb = ObjectPool.GetStringBuilder();
        try
        {
            sb.AppendLine($"=== Performance Report - {GeneratedAt:yyyy-MM-dd HH:mm:ss} ===");
            sb.AppendLine($"Total Operations: {TotalOperations}");
            sb.AppendLine();

            foreach (var stat in OperationStats.OrderByDescending(s => s.Count))
            {
                sb.AppendLine($"Operation: {stat.OperationName}");
                sb.AppendLine($"  Count: {stat.Count}");
                sb.AppendLine($"  Avg Time: {stat.AverageMilliseconds:F2}ms");
                sb.AppendLine($"  Min Time: {stat.MinMilliseconds}ms");
                sb.AppendLine($"  Max Time: {stat.MaxMilliseconds}ms");
                sb.AppendLine($"  Total Memory: {stat.TotalMemoryMB:F2}MB");
                sb.AppendLine($"  Avg Memory: {stat.AverageMemoryMB:F4}MB");
                sb.AppendLine();
            }

            return sb.ToString();
        }
        finally
        {
            ObjectPool.ReturnStringBuilder(sb);
        }
    }
}

/// <summary>
/// Estatísticas de uma operação.
/// </summary>
public sealed class OperationStats
{
    public string OperationName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageMilliseconds { get; set; }
    public long MinMilliseconds { get; set; }
    public long MaxMilliseconds { get; set; }
    public double TotalMemoryMB { get; set; }
    public double AverageMemoryMB { get; set; }
}
