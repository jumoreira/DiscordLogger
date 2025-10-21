using DiscordLogger.Batching;
using DiscordLogger.Filters;
using DiscordLogger.Formatters;

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

    /// <summary>
    /// Habilita o suporte a scopes de log. Padrão: false.
    /// </summary>
    public bool EnableScopes { get; set; } = false;

    /// <summary>
    /// Opções de batching de mensagens.
    /// </summary>
    public BatchingOptions Batching { get; set; } = new();

    /// <summary>
    /// Opções de filtros avançados.
    /// </summary>
    public LogFilterOptions Filters { get; set; } = new();

    /// <summary>
    /// Formatador de mensagens personalizado. Se null, usa o formatador padrão.
    /// </summary>
    public IMessageFormatter? MessageFormatter { get; set; }
}
