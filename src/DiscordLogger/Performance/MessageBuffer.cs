using System.Collections.Concurrent;
using System.Text.Json;
using DiscordLogger.Batching;

namespace DiscordLogger.Performance;

/// <summary>
/// Buffer inteligente para mensagens de log com suporte a múltiplas estratégias de flush e persistência.
/// </summary>
internal sealed class MessageBuffer : IDisposable
{
    private readonly BufferingOptions _options;
    private readonly ConcurrentQueue<QueuedLogMessage> _buffer;
    private readonly Timer? _flushTimer;
    private readonly SemaphoreSlim _flushLock;
    private readonly Func<IList<QueuedLogMessage>, Task> _flushAction;
    private readonly MessagePersistence? _persistence;
    private int _messageCount;
    private bool _disposed;

    public MessageBuffer(BufferingOptions options, Func<IList<QueuedLogMessage>, Task> flushAction)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _flushAction = flushAction ?? throw new ArgumentNullException(nameof(flushAction));
        _buffer = new ConcurrentQueue<QueuedLogMessage>();
        _flushLock = new SemaphoreSlim(1, 1);

        if (_options.EnablePersistence)
        {
            _persistence = new MessagePersistence(_options);
        }

        if (_options.FlushStrategy == FlushStrategy.Timer || _options.FlushStrategy == FlushStrategy.Auto)
        {
            var interval = TimeSpan.FromSeconds(_options.AutoFlushIntervalSeconds);
            _flushTimer = new Timer(OnTimerFlush, null, interval, interval);
        }
    }

    /// <summary>
    /// Número atual de mensagens no buffer.
    /// </summary>
    public int Count => _messageCount;

    /// <summary>
    /// Adiciona uma mensagem ao buffer.
    /// </summary>
    public async Task<bool> AddAsync(QueuedLogMessage message)
    {
        if (_disposed)
        {
            return false;
        }

        // Verifica overflow
        if (_messageCount >= _options.BufferCapacity)
        {
            await HandleOverflowAsync(message);
            return true;
        }

        _buffer.Enqueue(message);
        Interlocked.Increment(ref _messageCount);

        // Verifica se deve fazer flush por threshold
        if (_options.FlushStrategy == FlushStrategy.Threshold || _options.FlushStrategy == FlushStrategy.Auto)
        {
            if (_messageCount >= _options.FlushThreshold)
            {
                _ = Task.Run(() => FlushAsync());
            }
        }

        return true;
    }

    /// <summary>
    /// Força o flush do buffer.
    /// </summary>
    public async Task FlushAsync()
    {
        if (_disposed || _messageCount == 0)
        {
            return;
        }

        await _flushLock.WaitAsync();
        try
        {
            var messages = new List<QueuedLogMessage>();

            while (_buffer.TryDequeue(out var message))
            {
                messages.Add(message);
                Interlocked.Decrement(ref _messageCount);
            }

            if (messages.Count > 0)
            {
                try
                {
                    await _flushAction(messages);
                }
                catch (Exception ex)
                {
                    // Em caso de erro, tenta persistir as mensagens
                    if (_persistence != null)
                    {
                        await _persistence.PersistAsync(messages);
                    }

                    System.Diagnostics.Debug.WriteLine($"Error flushing buffer: {ex.Message}");
                }
            }
        }
        finally
        {
            _flushLock.Release();
        }
    }

    /// <summary>
    /// Trata overflow do buffer conforme a estratégia configurada.
    /// </summary>
    private async Task HandleOverflowAsync(QueuedLogMessage newMessage)
    {
        switch (_options.OverflowBehavior)
        {
            case OverflowBehavior.DropOldest:
                if (_buffer.TryDequeue(out _))
                {
                    Interlocked.Decrement(ref _messageCount);
                }
                _buffer.Enqueue(newMessage);
                Interlocked.Increment(ref _messageCount);
                break;

            case OverflowBehavior.DropNewest:
                // Simplesmente não adiciona a nova mensagem
                break;

            case OverflowBehavior.Block:
                // Aguarda até ter espaço
                while (_messageCount >= _options.BufferCapacity && !_disposed)
                {
                    await Task.Delay(100);
                }
                _buffer.Enqueue(newMessage);
                Interlocked.Increment(ref _messageCount);
                break;

            case OverflowBehavior.Persist:
                if (_persistence != null)
                {
                    await _persistence.PersistAsync(new[] { newMessage });
                }
                else
                {
                    // Fallback para DropOldest se persistência não está habilitada
                    if (_buffer.TryDequeue(out _))
                    {
                        Interlocked.Decrement(ref _messageCount);
                    }
                    _buffer.Enqueue(newMessage);
                    Interlocked.Increment(ref _messageCount);
                }
                break;
        }
    }

    /// <summary>
    /// Callback do timer de flush automático.
    /// </summary>
    private void OnTimerFlush(object? state)
    {
        _ = Task.Run(() => FlushAsync());
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _flushTimer?.Dispose();

        // Flush final
        FlushAsync().GetAwaiter().GetResult();

        _flushLock.Dispose();
        _persistence?.Dispose();
    }
}

/// <summary>
/// Sistema de persistência de mensagens em disco.
/// </summary>
internal sealed class MessagePersistence : IDisposable
{
    private readonly string _persistenceDirectory;
    private readonly long _maxFileSizeBytes;
    private readonly TimeSpan _retentionTime;
    private readonly object _lock = new();
    private readonly JsonSerializerOptions _jsonOptions;

    public MessagePersistence(BufferingOptions options)
    {
        _persistenceDirectory = options.PersistenceDirectory ?? Path.Combine(Path.GetTempPath(), "DiscordLogger");
        _maxFileSizeBytes = options.MaxPersistenceFileSizeMB * 1024 * 1024;
        _retentionTime = TimeSpan.FromHours(options.PersistenceRetentionHours);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        Directory.CreateDirectory(_persistenceDirectory);
        CleanupOldFiles();
    }

    /// <summary>
    /// Persiste mensagens em disco.
    /// </summary>
    public async Task PersistAsync(IEnumerable<QueuedLogMessage> messages)
    {
        var fileName = $"discordlog_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.json";
        var filePath = Path.Combine(_persistenceDirectory, fileName);

        try
        {
            lock (_lock)
            {
                // Verifica limite de tamanho total
                var totalSize = GetTotalPersistenceSize();
                if (totalSize > _maxFileSizeBytes)
                {
                    DeleteOldestFile();
                }
            }

            var json = JsonSerializer.Serialize(messages, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error persisting messages: {ex.Message}");
        }
    }

    /// <summary>
    /// Carrega mensagens persistidas.
    /// </summary>
    public async Task<IEnumerable<QueuedLogMessage>> LoadAsync()
    {
        var messages = new List<QueuedLogMessage>();

        try
        {
            var files = Directory.GetFiles(_persistenceDirectory, "discordlog_*.json")
                .OrderBy(f => f);

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var fileMessages = JsonSerializer.Deserialize<List<QueuedLogMessage>>(json, _jsonOptions);
                    
                    if (fileMessages != null)
                    {
                        messages.AddRange(fileMessages);
                    }

                    // Remove o arquivo após carregar
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading persisted file {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading persisted messages: {ex.Message}");
        }

        return messages;
    }

    /// <summary>
    /// Remove arquivos antigos baseado no tempo de retenção.
    /// </summary>
    private void CleanupOldFiles()
    {
        try
        {
            var cutoffTime = DateTime.UtcNow - _retentionTime;
            var files = Directory.GetFiles(_persistenceDirectory, "discordlog_*.json");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc < cutoffTime)
                {
                    File.Delete(file);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cleaning up old files: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtém o tamanho total dos arquivos de persistência.
    /// </summary>
    private long GetTotalPersistenceSize()
    {
        try
        {
            var files = Directory.GetFiles(_persistenceDirectory, "discordlog_*.json");
            return files.Sum(f => new FileInfo(f).Length);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Remove o arquivo mais antigo.
    /// </summary>
    private void DeleteOldestFile()
    {
        try
        {
            var oldestFile = Directory.GetFiles(_persistenceDirectory, "discordlog_*.json")
                .Select(f => new FileInfo(f))
                .OrderBy(f => f.LastWriteTimeUtc)
                .FirstOrDefault();

            oldestFile?.Delete();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting oldest file: {ex.Message}");
        }
    }

    public void Dispose()
    {
        CleanupOldFiles();
    }
}
