using DiscordLogger.Models;

namespace DiscordLogger;

/// <summary>
/// Formatador de mensagens para embeds do Discord.
/// </summary>
internal static class MessageFormatter
{
    private static readonly Dictionary<LogLevel, int> LogLevelColors = new()
    {
        { LogLevel.Debug, 0x808080 },      // Cinza
        { LogLevel.Information, 0x0099FF }, // Azul
        { LogLevel.Warning, 0xFFCC00 },     // Amarelo
        { LogLevel.Error, 0xFF6600 },       // Laranja
        { LogLevel.Critical, 0xFF0000 }     // Vermelho
    };

    private static readonly Dictionary<LogLevel, string> LogLevelEmojis = new()
    {
        { LogLevel.Debug, "🔍" },
        { LogLevel.Information, "ℹ️" },
        { LogLevel.Warning, "⚠️" },
        { LogLevel.Error, "❌" },
        { LogLevel.Critical, "🔥" }
    };

    /// <summary>
    /// Cria uma mensagem formatada para o Discord.
    /// </summary>
    public static DiscordWebhookMessage CreateMessage(
        LogLevel level,
        string message,
        Exception? exception,
        DiscordLoggerOptions options)
    {
        var embed = CreateEmbed(level, message, exception);

        return new DiscordWebhookMessage
        {
            Username = options.Username,
            AvatarUrl = options.AvatarUrl,
            Embeds = new List<DiscordEmbed> { embed }
        };
    }

    /// <summary>
    /// Cria um embed formatado para o log.
    /// </summary>
    private static DiscordEmbed CreateEmbed(LogLevel level, string message, Exception? exception)
    {
        var embed = new DiscordEmbed
        {
            Title = $"{GetLogLevelEmoji(level)} {GetLogLevelName(level)}",
            Description = TruncateMessage(message, 4096),
            Color = GetLogLevelColor(level),
            Timestamp = DateTime.UtcNow.ToString("o"),
            Fields = new List<DiscordEmbedField>()
        };

        // Adiciona informações da exceção se houver
        if (exception != null)
        {
            embed.Fields.Add(new DiscordEmbedField
            {
                Name = "Exception Type",
                Value = TruncateMessage(exception.GetType().FullName ?? exception.GetType().Name, 1024),
                Inline = true
            });

            if (!string.IsNullOrWhiteSpace(exception.Message))
            {
                embed.Fields.Add(new DiscordEmbedField
                {
                    Name = "Exception Message",
                    Value = TruncateMessage(exception.Message, 1024),
                    Inline = false
                });
            }

            if (!string.IsNullOrWhiteSpace(exception.StackTrace))
            {
                embed.Fields.Add(new DiscordEmbedField
                {
                    Name = "Stack Trace",
                    Value = FormatStackTrace(exception.StackTrace),
                    Inline = false
                });
            }

            // Inner exception
            if (exception.InnerException != null)
            {
                embed.Fields.Add(new DiscordEmbedField
                {
                    Name = "Inner Exception",
                    Value = TruncateMessage(
                        $"{exception.InnerException.GetType().Name}: {exception.InnerException.Message}",
                        1024
                    ),
                    Inline = false
                });
            }
        }

        // Adiciona timestamp legível no footer
        embed.Footer = new DiscordEmbedFooter
        {
            Text = $"Logged at {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
        };

        return embed;
    }

    /// <summary>
    /// Obtém a cor correspondente ao nível de log.
    /// </summary>
    private static int GetLogLevelColor(LogLevel level)
    {
        return LogLevelColors.TryGetValue(level, out var color) ? color : 0x808080;
    }

    /// <summary>
    /// Obtém o emoji correspondente ao nível de log.
    /// </summary>
    private static string GetLogLevelEmoji(LogLevel level)
    {
        return LogLevelEmojis.TryGetValue(level, out var emoji) ? emoji : "📝";
    }

    /// <summary>
    /// Obtém o nome do nível de log.
    /// </summary>
    private static string GetLogLevelName(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => "Debug",
            LogLevel.Information => "Information",
            LogLevel.Warning => "Warning",
            LogLevel.Error => "Error",
            LogLevel.Critical => "Critical",
            _ => level.ToString()
        };
    }

    /// <summary>
    /// Trunca uma mensagem para o tamanho máximo permitido.
    /// </summary>
    private static string TruncateMessage(string message, int maxLength)
    {
        if (string.IsNullOrEmpty(message))
        {
            return message;
        }

        if (message.Length <= maxLength)
        {
            return message;
        }

        return message.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>
    /// Formata o stack trace para o Discord.
    /// </summary>
    private static string FormatStackTrace(string stackTrace)
    {
        const int maxLength = 1024;
        
        if (string.IsNullOrEmpty(stackTrace))
        {
            return stackTrace;
        }

        // Formata como bloco de código
        var formatted = $"```\n{stackTrace}\n```";

        // Trunca se necessário
        if (formatted.Length > maxLength)
        {
            var truncatedStackTrace = stackTrace.Substring(0, maxLength - 10);
            formatted = $"```\n{truncatedStackTrace}\n...```";
        }

        return formatted;
    }
}
