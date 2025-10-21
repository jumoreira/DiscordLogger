using DiscordLogger.Models;
using DiscordLogger.Formatters;

namespace DiscordLogger;

/// <summary>
/// Formatador de mensagens para embeds do Discord (wrapper para DefaultMessageFormatter).
/// </summary>
internal static class MessageFormatter
{
    private static readonly DefaultMessageFormatter _defaultFormatter = new();

    /// <summary>
    /// Cria uma mensagem formatada para o Discord.
    /// </summary>
    public static DiscordWebhookMessage CreateMessage(
        LogLevel level,
        string message,
        Exception? exception,
        DiscordLoggerOptions options)
    {
        return _defaultFormatter.FormatMessage(level, message, exception, options);
    }
}
