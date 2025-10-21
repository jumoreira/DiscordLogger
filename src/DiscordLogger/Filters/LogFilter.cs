using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace DiscordLogger.Filters;

/// <summary>
/// Filtro avançado para logs do Discord.
/// </summary>
internal sealed class LogFilter
{
    private readonly LogFilterOptions _options;
    private readonly List<Regex>? _compiledPatterns;

    /// <summary>
    /// Inicializa uma nova instância do LogFilter.
    /// </summary>
    /// <param name="options">Opções de filtro.</param>
    public LogFilter(LogFilterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        // Compila as expressões regulares antecipadamente
        if (_options.MessagePatterns.Count > 0)
        {
            _compiledPatterns = new List<Regex>();
            foreach (var pattern in _options.MessagePatterns)
            {
                try
                {
                    _compiledPatterns.Add(new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
                }
                catch (ArgumentException)
                {
                    // Ignora padrões inválidos
                    System.Diagnostics.Debug.WriteLine($"Invalid regex pattern: {pattern}");
                }
            }
        }
    }

    /// <summary>
    /// Verifica se um log deve ser permitido com base nos filtros configurados.
    /// </summary>
    /// <param name="logLevel">Nível de log.</param>
    /// <param name="eventId">ID do evento.</param>
    /// <param name="category">Categoria do log.</param>
    /// <param name="message">Mensagem do log.</param>
    /// <returns>True se o log deve ser permitido, false caso contrário.</returns>
    public bool ShouldLog(
        Microsoft.Extensions.Logging.LogLevel logLevel,
        EventId eventId,
        string category,
        string message)
    {
        // Filtro por nível de log
        if (!IsLogLevelEnabled(logLevel))
        {
            return false;
        }

        // Filtro por categoria
        if (!IsCategoryAllowed(category))
        {
            return false;
        }

        // Filtro por EventId
        if (!IsEventIdAllowed(eventId.Id))
        {
            return false;
        }

        // Filtro por padrão de mensagem
        if (!IsMessageAllowed(message))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Verifica se o nível de log está habilitado.
    /// </summary>
    private bool IsLogLevelEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        if (_options.EnabledLevels.Count == 0)
        {
            return true;
        }

        return _options.EnabledLevels.TryGetValue(logLevel, out var enabled) && enabled;
    }

    /// <summary>
    /// Verifica se a categoria é permitida.
    /// </summary>
    private bool IsCategoryAllowed(string category)
    {
        // Se está na blacklist, bloqueia
        if (_options.ExcludeCategories.Count > 0 && 
            _options.ExcludeCategories.Any(excluded => MatchesPattern(category, excluded)))
        {
            return false;
        }

        // Se há whitelist e não está nela, bloqueia
        if (_options.IncludeCategories.Count > 0 && 
            !_options.IncludeCategories.Any(included => MatchesPattern(category, included)))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Verifica se o EventId é permitido.
    /// </summary>
    private bool IsEventIdAllowed(int eventId)
    {
        if (eventId == 0)
        {
            return true; // EventId 0 é considerado "não definido"
        }

        // Se está na blacklist, bloqueia
        if (_options.ExcludeEventIds.Contains(eventId))
        {
            return false;
        }

        // Se há whitelist e não está nela, bloqueia
        if (_options.IncludeEventIds.Count > 0 && !_options.IncludeEventIds.Contains(eventId))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Verifica se a mensagem é permitida baseada em padrões regex.
    /// </summary>
    private bool IsMessageAllowed(string message)
    {
        if (_compiledPatterns == null || _compiledPatterns.Count == 0)
        {
            return true;
        }

        var hasMatch = _compiledPatterns.Any(pattern => pattern.IsMatch(message));

        // Se é whitelist, retorna true apenas se houver match
        // Se é blacklist, retorna false se houver match
        return _options.MessagePatternsAsWhitelist ? hasMatch : !hasMatch;
    }

    /// <summary>
    /// Verifica se um texto corresponde a um padrão (suporta wildcards).
    /// </summary>
    private static bool MatchesPattern(string text, string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return true;
        }

        if (pattern == "*")
        {
            return true;
        }

        // Suporte a wildcards simples
        if (pattern.Contains('*'))
        {
            var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(text, regexPattern, RegexOptions.IgnoreCase);
        }

        // Match exato (case-insensitive)
        return text.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }
}
