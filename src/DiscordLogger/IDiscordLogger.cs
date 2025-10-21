namespace DiscordLogger;

/// <summary>
/// Interface para o serviço de logger do Discord.
/// </summary>
public interface IDiscordLogger
{
    /// <summary>
    /// Registra uma mensagem de log no Discord.
    /// </summary>
    /// <param name="level">Nível do log.</param>
    /// <param name="message">Mensagem a ser registrada.</param>
    /// <param name="exception">Exceção associada ao log, se houver.</param>
    /// <param name="scopeInfo">Informações de scope, se disponíveis.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task LogAsync(LogLevel level, string message, Exception? exception = null, string? scopeInfo = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra uma mensagem de debug no Discord.
    /// </summary>
    Task LogDebugAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra uma mensagem informativa no Discord.
    /// </summary>
    Task LogInformationAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um aviso no Discord.
    /// </summary>
    Task LogWarningAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um erro no Discord.
    /// </summary>
    Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um erro crítico no Discord.
    /// </summary>
    Task LogCriticalAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default);
}
