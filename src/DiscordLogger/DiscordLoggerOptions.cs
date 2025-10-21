namespace DiscordLogger;

/// <summary>
/// Representa as opções de configuração para o DiscordLogger.
/// </summary>
public class DiscordLoggerOptions
{
    /// <summary>
    /// URL do webhook do Discord para envio dos logs.
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// Nome de usuário que aparecerá nas mensagens do Discord.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// URL do avatar que aparecerá nas mensagens do Discord.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Nível mínimo de log que será enviado ao Discord.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Timeout para requisições HTTP em segundos. Padrão: 30 segundos.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Número máximo de tentativas em caso de falha. Padrão: 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}
