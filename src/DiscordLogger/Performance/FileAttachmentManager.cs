using System.Text;
using DiscordLogger.Models;

namespace DiscordLogger.Performance;

/// <summary>
/// Gerenciador de anexos de arquivo para mensagens grandes.
/// </summary>
internal sealed class FileAttachmentManager
{
    private readonly FileAttachmentOptions _options;

    public FileAttachmentManager(FileAttachmentOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Verifica se uma mensagem precisa de anexo.
    /// </summary>
    public bool RequiresAttachment(string content)
    {
        return _options.Enabled && !string.IsNullOrEmpty(content) && content.Length > _options.MessageThreshold;
    }

    /// <summary>
    /// Processa uma mensagem grande criando um anexo se necessário.
    /// </summary>
    public ProcessedMessage ProcessMessage(string content, string? title = null)
    {
        if (!RequiresAttachment(content))
        {
            return new ProcessedMessage
            {
                Content = content,
                RequiresAttachment = false
            };
        }

        var fileName = GenerateFileName(title);
        var preview = CreatePreview(content, fileName);
        var fileContent = CreateFileContent(content, title);

        return new ProcessedMessage
        {
            Content = preview,
            RequiresAttachment = true,
            FileName = fileName,
            FileContent = fileContent,
            OriginalLength = content.Length
        };
    }

    /// <summary>
    /// Cria o preview da mensagem.
    /// </summary>
    private string CreatePreview(string content, string fileName)
    {
        var preview = content.Length > _options.PreviewLength
            ? content.Substring(0, _options.PreviewLength) + "..."
            : content;

        return _options.PreviewFormat
            .Replace("{preview}", preview)
            .Replace("{totalLength}", content.Length.ToString())
            .Replace("{fileName}", fileName);
    }

    /// <summary>
    /// Cria o conteúdo do arquivo de anexo.
    /// </summary>
    private byte[] CreateFileContent(string content, string? title)
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrEmpty(title))
        {
            builder.AppendLine($"=== {title} ===");
            builder.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            builder.AppendLine($"Length: {content.Length} characters");
            builder.AppendLine();
            builder.AppendLine(new string('=', 50));
            builder.AppendLine();
        }

        builder.AppendLine(content);

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// Gera o nome do arquivo de anexo.
    /// </summary>
    private string GenerateFileName(string? title)
    {
        var builder = new StringBuilder(_options.FilePrefix);

        if (!string.IsNullOrEmpty(title))
        {
            var sanitizedTitle = SanitizeFileName(title);
            if (!string.IsNullOrEmpty(sanitizedTitle))
            {
                builder.Append('_');
                builder.Append(sanitizedTitle);
            }
        }

        if (_options.IncludeTimestamp)
        {
            builder.Append('_');
            builder.Append(DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
        }

        builder.Append(".txt");

        return builder.ToString();
    }

    /// <summary>
    /// Remove caracteres inválidos de nome de arquivo.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder();

        foreach (var c in fileName)
        {
            if (!invalidChars.Contains(c) && !char.IsWhiteSpace(c))
            {
                builder.Append(c);
            }
            else if (char.IsWhiteSpace(c))
            {
                builder.Append('_');
            }
        }

        var result = builder.ToString();
        
        // Limita o tamanho
        if (result.Length > 50)
        {
            result = result.Substring(0, 50);
        }

        return result;
    }
}

/// <summary>
/// Resultado do processamento de uma mensagem.
/// </summary>
internal sealed class ProcessedMessage
{
    /// <summary>
    /// Conteúdo da mensagem (preview se houver anexo).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Indica se a mensagem requer anexo.
    /// </summary>
    public bool RequiresAttachment { get; set; }

    /// <summary>
    /// Nome do arquivo de anexo.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Conteúdo do arquivo de anexo em bytes.
    /// </summary>
    public byte[]? FileContent { get; set; }

    /// <summary>
    /// Tamanho original da mensagem em caracteres.
    /// </summary>
    public int OriginalLength { get; set; }
}
