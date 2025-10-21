using Microsoft.Extensions.Logging;
using DiscordLogger.Scopes;
using DiscordLogger.Filters;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

// Permite que o assembly de testes acesse classes internas
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DiscordLogger.Tests")]

namespace DiscordLogger;

/// <summary>
/// Implementação do ILogger do Microsoft.Extensions.Logging que envia logs para o Discord.
/// </summary>
internal sealed class MicrosoftDiscordLogger : Microsoft.Extensions.Logging.ILogger
{
    private readonly string _categoryName;
    private readonly IDiscordLogger _discordLogger;
    private readonly DiscordLoggerOptions _options;
    private readonly DiscordScopeProvider? _scopeProvider;
    private readonly LogFilter? _filter;

    /// <summary>
    /// Inicializa uma nova instância do MicrosoftDiscordLogger.
    /// </summary>
    /// <param name="categoryName">Nome da categoria do logger.</param>
    /// <param name="discordLogger">Instância do IDiscordLogger.</param>
    /// <param name="options">Opções de configuração.</param>
    /// <param name="scopeProvider">Provider de scopes, se habilitado.</param>
    /// <param name="filter">Filtro de logs, se configurado.</param>
    public MicrosoftDiscordLogger(
        string categoryName, 
        IDiscordLogger discordLogger, 
        DiscordLoggerOptions options,
        DiscordScopeProvider? scopeProvider = null,
        LogFilter? filter = null)
    {
        _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        _discordLogger = discordLogger ?? throw new ArgumentNullException(nameof(discordLogger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _scopeProvider = scopeProvider;
        _filter = filter;
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        if (!_options.EnableScopes || _scopeProvider == null)
        {
            return null;
        }

        return _scopeProvider.Push(state);
    }

    /// <inheritdoc />
    public bool IsEnabled(MsLogLevel logLevel)
    {
        if (logLevel == MsLogLevel.None)
        {
            return false;
        }

        var discordLogLevel = ConvertToDiscordLogLevel(logLevel);
        return discordLogLevel >= _options.MinimumLevel;
    }

    /// <inheritdoc />
    public void Log<TState>(
        MsLogLevel logLevel,
        Microsoft.Extensions.Logging.EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null)
        {
            return;
        }

        // Aplica filtros avançados
        if (_filter != null && !_filter.ShouldLog(logLevel, eventId, _categoryName, message))
        {
            return;
        }

        // Adiciona informações de categoria e eventId à mensagem
        var fullMessage = FormatMessage(_categoryName, eventId, message);
        
        // Adiciona informações de scope se habilitado
        string? scopeInfo = null;
        if (_options.EnableScopes && _scopeProvider != null)
        {
            scopeInfo = _scopeProvider.FormatScopes();
        }

        var discordLogLevel = ConvertToDiscordLogLevel(logLevel);

        // Log assíncrono sem bloquear
        // Em produção, considere usar um background queue para melhor performance
        _ = Task.Run(async () =>
        {
            try
            {
                await _discordLogger.LogAsync(discordLogLevel, fullMessage, exception, scopeInfo);
            }
            catch
            {
                // Silenciosamente falha para não quebrar a aplicação
                // O erro já é tratado no DiscordLogger
            }
        });
    }

    /// <summary>
    /// Converte LogLevel do Microsoft.Extensions.Logging para LogLevel do DiscordLogger.
    /// </summary>
    private static LogLevel ConvertToDiscordLogLevel(MsLogLevel logLevel)
    {
        return logLevel switch
        {
            MsLogLevel.Trace => LogLevel.Debug,
            MsLogLevel.Debug => LogLevel.Debug,
            MsLogLevel.Information => LogLevel.Information,
            MsLogLevel.Warning => LogLevel.Warning,
            MsLogLevel.Error => LogLevel.Error,
            MsLogLevel.Critical => LogLevel.Critical,
            MsLogLevel.None => LogLevel.Information,
            _ => LogLevel.Information
        };
    }

    /// <summary>
    /// Formata a mensagem incluindo categoria e eventId.
    /// </summary>
    private static string FormatMessage(string categoryName, Microsoft.Extensions.Logging.EventId eventId, string message)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(categoryName))
        {
            parts.Add($"[{categoryName}]");
        }

        if (eventId.Id != 0)
        {
            if (!string.IsNullOrEmpty(eventId.Name))
            {
                parts.Add($"[{eventId.Id}:{eventId.Name}]");
            }
            else
            {
                parts.Add($"[{eventId.Id}]");
            }
        }

        parts.Add(message);

        return string.Join(" ", parts);
    }
}
