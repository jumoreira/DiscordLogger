namespace DiscordLogger.Resilience.Routing;

/// <summary>
/// Configuração de roteamento de webhook.
/// </summary>
public class WebhookRoute
{
    /// <summary>
    /// URL do webhook.
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// Nome descritivo do webhook.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Níveis de log para este webhook (null = todos).
    /// </summary>
    public HashSet<LogLevel>? LogLevels { get; set; }

    /// <summary>
    /// Padrões de categoria/classe (suporta wildcards).
    /// Exemplos: "*.Service", "*Controller", "MyApp.Business.*"
    /// </summary>
    public HashSet<string>? CategoryPatterns { get; set; }

    /// <summary>
    /// Prioridade da rota (maior = mais prioritário). Padrão: 0.
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Determina se esta é uma rota de fallback.
    /// </summary>
    public bool IsFallback { get; set; } = false;

    /// <summary>
    /// Username personalizado para este webhook.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Avatar URL personalizado para este webhook.
    /// </summary>
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// Opções de configuração para múltiplos webhooks.
/// </summary>
public class MultiWebhookOptions
{
    /// <summary>
    /// Habilita suporte a múltiplos webhooks. Padrão: false.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Lista de rotas de webhook.
    /// </summary>
    public List<WebhookRoute> Routes { get; set; } = new();

    /// <summary>
    /// Webhook padrão (fallback quando nenhuma rota corresponde).
    /// </summary>
    public string? DefaultWebhookUrl { get; set; }

    /// <summary>
    /// Habilita roteamento por nível de log. Padrão: true.
    /// </summary>
    public bool EnableLogLevelRouting { get; set; } = true;

    /// <summary>
    /// Habilita roteamento por categoria/classe. Padrão: true.
    /// </summary>
    public bool EnableCategoryRouting { get; set; } = true;

    /// <summary>
    /// Estratégia de load balancing quando múltiplas rotas correspondem. Padrão: Priority.
    /// </summary>
    public LoadBalancingStrategy LoadBalancing { get; set; } = LoadBalancingStrategy.Priority;

    /// <summary>
    /// Habilita broadcast (envio para múltiplas rotas). Padrão: false.
    /// </summary>
    public bool EnableBroadcast { get; set; } = false;
}

/// <summary>
/// Estratégias de load balancing.
/// </summary>
public enum LoadBalancingStrategy
{
    /// <summary>
    /// Usa a rota com maior prioridade.
    /// </summary>
    Priority,

    /// <summary>
    /// Round-robin entre rotas que correspondem.
    /// </summary>
    RoundRobin,

    /// <summary>
    /// Escolhe aleatoriamente entre rotas que correspondem.
    /// </summary>
    Random,

    /// <summary>
    /// Envia para todas as rotas que correspondem (broadcast).
    /// </summary>
    Broadcast
}
