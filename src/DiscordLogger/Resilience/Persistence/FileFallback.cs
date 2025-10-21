using System.Text;
using System.Text.Json;

namespace DiscordLogger.Resilience.Persistence;

/// <summary>
/// Fallback baseado em arquivo para quando o Discord está indisponível.
/// </summary>
public interface IFileFallback
{
    /// <summary>
    /// Salva uma mensagem em arquivo local.
    /// </summary>
    Task SaveAsync(string message, LogLevel logLevel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém estatísticas do fallback.
    /// </summary>
    FileFallbackStatistics GetStatistics();
}

/// <summary>
/// Estatísticas do File Fallback.
/// </summary>
public class FileFallbackStatistics
{
    /// <summary>
    /// Total de mensagens salvas em arquivo.
    /// </summary>
    public long TotalMessagesSaved { get; set; }

    /// <summary>
    /// Tamanho total do arquivo em bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Caminho do arquivo.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Data da última gravação.
    /// </summary>
    public DateTimeOffset? LastWrite { get; set; }
}

/// <summary>
/// Implementação de File Fallback.
/// </summary>
public sealed class FileFallback : IFileFallback, IDisposable
{
    private readonly string _filePath;
    private readonly int _maxFileSizeMb;
    private readonly SemaphoreSlim _fileLock;
    private readonly JsonSerializerOptions _jsonOptions;
    private long _messageCount;
    private DateTimeOffset? _lastWrite;
    private bool _disposed;

    public FileFallback(string filePath, int maxFileSizeMb = 100)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _maxFileSizeMb = maxFileSizeMb;
        _fileLock = new SemaphoreSlim(1, 1);
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
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
    public async Task SaveAsync(string message, LogLevel logLevel, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            // Verifica tamanho do arquivo
            if (File.Exists(_filePath))
            {
                var fileInfo = new FileInfo(_filePath);
                if (fileInfo.Length > _maxFileSizeMb * 1024 * 1024)
                {
                    // Rotaciona arquivo
                    RotateFile();
                }
            }

            var logEntry = new
            {
                Timestamp = DateTimeOffset.UtcNow,
                LogLevel = logLevel.ToString(),
                Message = message
            };

            var json = JsonSerializer.Serialize(logEntry, _jsonOptions);
            await File.AppendAllTextAsync(_filePath, json + Environment.NewLine, cancellationToken);

            _messageCount++;
            _lastWrite = DateTimeOffset.UtcNow;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <inheritdoc />
    public FileFallbackStatistics GetStatistics()
    {
        var fileSize = File.Exists(_filePath) ? new FileInfo(_filePath).Length : 0;

        return new FileFallbackStatistics
        {
            TotalMessagesSaved = _messageCount,
            FileSizeBytes = fileSize,
            FilePath = _filePath,
            LastWrite = _lastWrite
        };
    }

    private void RotateFile()
    {
        var directory = Path.GetDirectoryName(_filePath) ?? ".";
        var fileName = Path.GetFileNameWithoutExtension(_filePath);
        var extension = Path.GetExtension(_filePath);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var rotatedFileName = $"{fileName}-{timestamp}{extension}";
        var rotatedFilePath = Path.Combine(directory, rotatedFileName);

        File.Move(_filePath, rotatedFilePath);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _fileLock?.Dispose();
    }
}
