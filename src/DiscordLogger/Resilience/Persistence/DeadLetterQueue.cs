using System.Text.Json;

namespace DiscordLogger.Resilience.Persistence;

/// <summary>
/// Representa uma mensagem de log que falhou múltiplas vezes.
/// </summary>
public class FailedLogMessage
{
    /// <summary>
    /// ID único da mensagem.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp da primeira tentativa.
    /// </summary>
    public DateTimeOffset FirstAttempt { get; set; }

    /// <summary>
    /// Timestamp da última tentativa.
    /// </summary>
    public DateTimeOffset LastAttempt { get; set; }

    /// <summary>
    /// Número de tentativas.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Conteúdo da mensagem original (JSON serializado).
    /// </summary>
    public string MessageContent { get; set; } = string.Empty;

    /// <summary>
    /// Nível de log original.
    /// </summary>
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// Última exceção/erro.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Webhook URL original.
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// Categoria/Nome do logger.
    /// </summary>
    public string? Category { get; set; }
}

/// <summary>
/// Dead Letter Queue para mensagens que falharam múltiplas vezes.
/// </summary>
public interface IDeadLetterQueue
{
    /// <summary>
    /// Adiciona uma mensagem à Dead Letter Queue.
    /// </summary>
    Task EnqueueAsync(FailedLogMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tenta recuperar mensagens da DLQ.
    /// </summary>
    Task<IList<FailedLogMessage>> DequeueAsync(int maxCount = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma mensagem da DLQ após processamento bem-sucedido.
    /// </summary>
    Task RemoveAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o número de mensagens na DLQ.
    /// </summary>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Limpa todas as mensagens da DLQ.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementação file-based da Dead Letter Queue.
/// </summary>
public sealed class FileBasedDeadLetterQueue : IDeadLetterQueue, IDisposable
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public FileBasedDeadLetterQueue(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _fileLock = new SemaphoreSlim(1, 1);
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Garante que o diretório existe
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(FailedLogMessage message, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var messages = await LoadMessagesAsync(cancellationToken);
            messages.Add(message);
            await SaveMessagesAsync(messages, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IList<FailedLogMessage>> DequeueAsync(int maxCount = 100, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var messages = await LoadMessagesAsync(cancellationToken);
            return messages.Take(maxCount).ToList();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var messages = await LoadMessagesAsync(cancellationToken);
            messages.RemoveAll(m => m.Id == messageId);
            await SaveMessagesAsync(messages, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var messages = await LoadMessagesAsync(cancellationToken);
            return messages.Count;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<FailedLogMessage>> LoadMessagesAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return new List<FailedLogMessage>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
            return JsonSerializer.Deserialize<List<FailedLogMessage>>(json, _jsonOptions)
                   ?? new List<FailedLogMessage>();
        }
        catch
        {
            return new List<FailedLogMessage>();
        }
    }

    private async Task SaveMessagesAsync(List<FailedLogMessage> messages, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(messages, _jsonOptions);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _fileLock?.Dispose();
    }
}
