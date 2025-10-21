using DiscordLogger.Models;

namespace DiscordLogger.Formatters;

/// <summary>
/// Interface para formatadores de mensagens do Discord.
/// </summary>
public interface IMessageFormatter
{
    /// <summary>
    /// Formata uma mensagem de log para envio ao Discord.
    /// </summary>
    /// <param name="level">Nível do log.</param>
    /// <param name="message">Mensagem do log.</param>
    /// <param name="exception">Exceção associada, se houver.</param>
    /// <param name="options">Opções do logger.</param>
    /// <param name="scopeInfo">Informações de scope, se disponíveis.</param>
    /// <returns>Mensagem formatada para o Discord.</returns>
    DiscordWebhookMessage FormatMessage(
        LogLevel level,
        string message,
        Exception? exception,
        DiscordLoggerOptions options,
        string? scopeInfo = null);

    /// <summary>
    /// Formata um batch de mensagens para envio ao Discord.
    /// </summary>
    /// <param name="messages">Lista de mensagens a serem formatadas.</param>
    /// <param name="options">Opções do logger.</param>
    /// <returns>Mensagem formatada contendo o batch.</returns>
    DiscordWebhookMessage FormatBatch(
        IList<(LogLevel Level, string Message, Exception? Exception)> messages,
        DiscordLoggerOptions options);
}
