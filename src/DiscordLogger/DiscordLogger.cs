namespace DiscordLogger;

/// <summary>
/// Implementação do logger que envia logs para o Discord via webhooks.
/// </summary>
public class DiscordLogger : IDiscordLogger, IDisposable
{
    private readonly DiscordLoggerOptions _options;
    private readonly DiscordWebhookClient _client;
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
    }

    /// <inheritdoc />
    public async Task LogAsync(
        LogLevel level,
        string message,
        Exception? exception = null,
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
            var webhookMessage = MessageFormatter.CreateMessage(level, message, exception, _options);
            await _client.SendMessageAsync(webhookMessage, cancellationToken);
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

    /// <inheritdoc />
    public Task LogDebugAsync(string message, CancellationToken cancellationToken = default)
    {
        return LogAsync(LogLevel.Debug, message, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task LogInformationAsync(string message, CancellationToken cancellationToken = default)
    {
        return LogAsync(LogLevel.Information, message, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task LogWarningAsync(string message, CancellationToken cancellationToken = default)
    {
        return LogAsync(LogLevel.Warning, message, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        return LogAsync(LogLevel.Error, message, exception, cancellationToken);
    }

    /// <inheritdoc />
    public Task LogCriticalAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        return LogAsync(LogLevel.Critical, message, exception, cancellationToken);
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

        _client?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
