namespace DiscordLogger.Performance;

/// <summary>
/// Configurações para anexo de arquivos em mensagens grandes.
/// </summary>
public class FileAttachmentOptions
{
    /// <summary>
    /// Habilita o anexo de arquivos para mensagens grandes. Padrão: false.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Tamanho mínimo da mensagem (em caracteres) para criar um anexo. Padrão: 1900.
    /// Discord tem limite de 2000 caracteres, deixamos margem de segurança.
    /// </summary>
    public int MessageThreshold { get; set; } = 1900;

    /// <summary>
    /// Tamanho do preview da mensagem quando anexo é criado. Padrão: 500.
    /// </summary>
    public int PreviewLength { get; set; } = 500;

    /// <summary>
    /// Prefixo para o nome do arquivo. Padrão: "log".
    /// </summary>
    public string FilePrefix { get; set; } = "log";

    /// <summary>
    /// Indica se deve incluir timestamp no nome do arquivo. Padrão: true.
    /// </summary>
    public bool IncludeTimestamp { get; set; } = true;

    /// <summary>
    /// Formato da mensagem de preview. Suporta placeholders: {preview}, {totalLength}, {fileName}.
    /// </summary>
    public string PreviewFormat { get; set; } = "**Preview** (Total: {totalLength} chars - Ver anexo {fileName}):\n```\n{preview}\n```";
}
