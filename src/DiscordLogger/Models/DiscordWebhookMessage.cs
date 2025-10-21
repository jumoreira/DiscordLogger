namespace DiscordLogger.Models;

/// <summary>
/// Representa uma mensagem do Discord webhook.
/// </summary>
public class DiscordWebhookMessage
{
    /// <summary>
    /// Conteúdo texto da mensagem.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Nome de usuário a ser exibido.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// URL do avatar.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Lista de embeds da mensagem.
    /// </summary>
    public List<DiscordEmbed>? Embeds { get; set; }
}

/// <summary>
/// Representa um embed do Discord.
/// </summary>
public class DiscordEmbed
{
    /// <summary>
    /// Título do embed.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Descrição do embed.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Cor do embed (formato decimal).
    /// </summary>
    public int? Color { get; set; }

    /// <summary>
    /// Timestamp do embed.
    /// </summary>
    public string? Timestamp { get; set; }

    /// <summary>
    /// Campos do embed.
    /// </summary>
    public List<DiscordEmbedField>? Fields { get; set; }

    /// <summary>
    /// Footer do embed.
    /// </summary>
    public DiscordEmbedFooter? Footer { get; set; }
}

/// <summary>
/// Representa um campo de embed do Discord.
/// </summary>
public class DiscordEmbedField
{
    /// <summary>
    /// Nome do campo.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Valor do campo.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Se o campo deve ser exibido inline.
    /// </summary>
    public bool Inline { get; set; }
}

/// <summary>
/// Representa o footer de um embed do Discord.
/// </summary>
public class DiscordEmbedFooter
{
    /// <summary>
    /// Texto do footer.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}
