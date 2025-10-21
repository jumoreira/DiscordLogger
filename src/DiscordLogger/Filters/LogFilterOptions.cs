using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace DiscordLogger.Filters;

/// <summary>
/// Configuração de filtros avançados para logs.
/// </summary>
public class LogFilterOptions
{
    /// <summary>
    /// Lista de categorias que devem ser incluídas (whitelist).
    /// Se vazio, todas as categorias são permitidas.
    /// </summary>
    public List<string> IncludeCategories { get; set; } = new();

    /// <summary>
    /// Lista de categorias que devem ser excluídas (blacklist).
    /// Tem precedência sobre IncludeCategories.
    /// </summary>
    public List<string> ExcludeCategories { get; set; } = new();

    /// <summary>
    /// Lista de EventIds que devem ser incluídos (whitelist).
    /// Se vazio, todos os EventIds são permitidos.
    /// </summary>
    public List<int> IncludeEventIds { get; set; } = new();

    /// <summary>
    /// Lista de EventIds que devem ser excluídos (blacklist).
    /// Tem precedência sobre IncludeEventIds.
    /// </summary>
    public List<int> ExcludeEventIds { get; set; } = new();

    /// <summary>
    /// Expressões regulares para filtrar mensagens por padrão.
    /// Se a mensagem corresponder a qualquer padrão, será filtrada.
    /// </summary>
    public List<string> MessagePatterns { get; set; } = new();

    /// <summary>
    /// Modo de filtro de mensagens: true = whitelist (incluir apenas correspondentes),
    /// false = blacklist (excluir correspondentes). Padrão: false.
    /// </summary>
    public bool MessagePatternsAsWhitelist { get; set; } = false;

    /// <summary>
    /// Filtros personalizados por nível de log.
    /// </summary>
    public Dictionary<Microsoft.Extensions.Logging.LogLevel, bool> EnabledLevels { get; set; } = new();
}
