using DiscordLogger.Models;
using System.Text;

namespace DiscordLogger.Formatters;

/// <summary>
/// Formatador de mensagens baseado em templates personalizáveis.
/// </summary>
public class TemplateMessageFormatter : DefaultMessageFormatter
{
    private readonly string _titleTemplate;
    private readonly string _descriptionTemplate;

    /// <summary>
    /// Placeholders disponíveis:
    /// {level} - Nível do log
    /// {emoji} - Emoji do nível
    /// {message} - Mensagem do log
    /// {timestamp} - Timestamp formatado
    /// {date} - Data
    /// {time} - Hora
    /// {exception} - Nome da exceção (se houver)
    /// </summary>
    public TemplateMessageFormatter(
        string? titleTemplate = null,
        string? descriptionTemplate = null)
    {
        _titleTemplate = titleTemplate ?? "{emoji} {level}";
        _descriptionTemplate = descriptionTemplate ?? "{message}";
    }

    /// <inheritdoc />
    protected override DiscordEmbed CreateEmbed(
        LogLevel level,
        string message,
        Exception? exception,
        string? scopeInfo)
    {
        var now = DateTime.Now;
        var utcNow = DateTime.UtcNow;

        var placeholders = new Dictionary<string, string>
        {
            { "level", GetLogLevelName(level) },
            { "emoji", GetLogLevelEmoji(level) },
            { "message", message },
            { "timestamp", now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "date", now.ToString("yyyy-MM-dd") },
            { "time", now.ToString("HH:mm:ss") },
            { "exception", exception?.GetType().Name ?? string.Empty }
        };

        var title = ReplacePlaceholders(_titleTemplate, placeholders);
        var description = ReplacePlaceholders(_descriptionTemplate, placeholders);

        var embed = new DiscordEmbed
        {
            Title = TruncateMessage(title, 256),
            Description = TruncateMessage(description, 4096),
            Color = GetLogLevelColor(level),
            Timestamp = utcNow.ToString("o"),
            Fields = new List<DiscordEmbedField>()
        };

        // Adiciona informações de scope se houver
        if (!string.IsNullOrWhiteSpace(scopeInfo))
        {
            embed.Fields.Add(new DiscordEmbedField
            {
                Name = "Context",
                Value = TruncateMessage(scopeInfo, 1024),
                Inline = false
            });
        }

        // Adiciona informações da exceção se houver
        if (exception != null)
        {
            AddExceptionFields(embed, exception);
        }

        // Adiciona timestamp legível no footer
        embed.Footer = new DiscordEmbedFooter
        {
            Text = $"Logged at {now:yyyy-MM-dd HH:mm:ss}"
        };

        return embed;
    }

    /// <summary>
    /// Substitui placeholders no template.
    /// </summary>
    private static string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
    {
        var result = template;

        foreach (var kvp in placeholders)
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
        }

        return result;
    }
}
