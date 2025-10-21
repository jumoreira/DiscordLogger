namespace DiscordLogger.Resilience.Routing;

/// <summary>
/// Atributo para especificar webhook customizado para uma classe ou método.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class DiscordWebhookAttribute : Attribute
{
    /// <summary>
    /// URL do webhook específico.
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Nome da rota (referência à configuração).
    /// </summary>
    public string? RouteName { get; set; }

    /// <summary>
    /// Username customizado para este webhook.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Avatar URL customizado para este webhook.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Níveis de log aceitos (separados por vírgula). Ex: "Critical,Error".
    /// </summary>
    public string? LogLevels { get; set; }

    public DiscordWebhookAttribute()
    {
    }

    public DiscordWebhookAttribute(string webhookUrl)
    {
        WebhookUrl = webhookUrl;
    }
}
