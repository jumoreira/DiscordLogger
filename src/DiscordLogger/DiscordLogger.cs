using DiscordLogger.Batching;
using DiscordLogger.Formatters;

namespace DiscordLogger;

/// <summary>
/// Implementação do logger que envia logs para o Discord via webhooks.
/// </summary>
public class DiscordLogger : IDiscordLogger, IDisposable
{
    private readonly DiscordLoggerOptions _options;
    private readonly DiscordWebhookClient _client;
    private readonly IMessageFormatter _formatter;
    private readonly LogBatchProcessor? _batchProcessor;
    private bool _disposed;

    /// <summary>
    /// Inicializa uma nova instância do DiscordLogger.
    /// </summary>
    /// <param name="options">Opções de configuração do logger.</param>
    /// <exception cref="ArgumentNullException">Lançado quando options é null.</exception>
    /// <exception cref="ArgumentException">Lançado quando WebhookUrl não está configurado.</exception>
    public DiscordLogger(DiscordLoggerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(_options.WebhookUrl))
        {
            throw new ArgumentException("WebhookUrl é obrigatório.", nameof(options));
        }

        _client = new DiscordWebhookClient(_options);
        _formatter = _options.MessageFormatter ?? new DefaultMessageFormatter();

        // Inicializa o batch processor se o batching estiver habilitado
        if (_options.Batching.Enabled)
        {
            _batchProcessor = new LogBatchProcessor(_options.Batching, SendBatchAsync);
        }
    }

    /// <inheritdoc />
    public async Task LogAsync(
        LogLevel level,
        string message,
        Exception? exception = null,
        string? scopeInfo = null,
        CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DiscordLogger));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("A mensagem não pode ser vazia.", nameof(message));
        }

        // Verifica se o nível de log está habilitado
        if (level < _options.MinimumLevel)
        {
            return;
        }

        try
        {
            // Se batching estiver habilitado, enfileira a mensagem
            if (_batchProcessor != null)
            {
                var queuedMessage = new QueuedLogMessage
                {
                    Level = level,
                    Message = message,
                    Exception = exception,
                    Timestamp = DateTime.UtcNow
                };

                _batchProcessor.EnqueueMessage(queuedMessage);
            }
            else
            {
                // Envia diretamente
                var webhookMessage = _formatter.FormatMessage(level, message, exception, _options, scopeInfo);
                await _client.SendMessageAsync(webhookMessage, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Em produção, você pode querer logar isso em outro lugar
            // ou silenciosamente falhar para não quebrar a aplicação
            Console.Error.WriteLine($"Falha ao enviar log para o Discord: {ex.Message}");
            
            // Re-throw se quiser que o erro seja propagado
            // throw;
        }
    }

    /// <summary>
    /// Envia um batch de mensagens para o Discord.
    /// </summary>
    private async Task SendBatchAsync(IList<QueuedLogMessage> messages)
    {
        if (messages.Count == 0)
        {
            return;
        }

        try
        {
            var messageList = messages
                .Select(m => (m.Level, m.Message, m.Exception))
                .ToList();

            var webhookMessage = _formatter.FormatBatch(messageList, _options);
            await _client.SendMessageAsync(webhookMessage, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Falha ao enviar batch de logs para o Discord: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public Task LogDebugAsync(string message, CancellationToken cancellationToken = default)
    {
        return LogAsync(LogLevel.Debug, message, null, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task LogInformationAsync(string message, CancellationToken cancellationToken = default)
    {
        return LogAsync(LogLevel.Information, message, null, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task LogWarningAsync(string message, CancellationToken cancellationToken = default)
    {
        return LogAsync(LogLevel.Warning, message, null, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        return LogAsync(LogLevel.Error, message, exception, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task LogCriticalAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        return LogAsync(LogLevel.Critical, message, exception, null, cancellationToken);
    }

    /// <summary>
    /// Libera os recursos utilizados pelo DiscordLogger.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _batchProcessor?.Dispose();
        _client?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
