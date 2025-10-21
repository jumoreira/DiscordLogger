namespace DiscordLogger.Batching;

/// <summary>
/// Representa uma mensagem de log enfileirada para batching.
/// </summary>
internal sealed class QueuedLogMessage
{
    /// <summary>
    /// Nível de log da mensagem.
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// Conteúdo da mensagem.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Exceção associada, se houver.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Timestamp de quando a mensagem foi enfileirada.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
