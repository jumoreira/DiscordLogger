using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DiscordLogger.Scopes;
using DiscordLogger.Filters;

namespace DiscordLogger;

/// <summary>
/// Provider para criar instâncias de MicrosoftDiscordLogger.
/// </summary>
[ProviderAlias("Discord")]
public sealed class DiscordLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly IDiscordLogger _discordLogger;
    private readonly DiscordLoggerOptions _options;
    private readonly DiscordScopeProvider? _scopeProvider;
    private readonly LogFilter? _filter;
    private IExternalScopeProvider? _externalScopeProvider;
    private bool _disposed;

    /// <summary>
    /// Inicializa uma nova instância do DiscordLoggerProvider.
    /// </summary>
    /// <param name="options">Opções de configuração do logger.</param>
    public DiscordLoggerProvider(IOptions<DiscordLoggerOptions> options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _options = options.Value ?? throw new ArgumentNullException(nameof(options.Value));
        _discordLogger = new DiscordLogger(_options);

        // Inicializa scope provider se habilitado
        if (_options.EnableScopes)
        {
            _scopeProvider = new DiscordScopeProvider();
        }

        // Inicializa filtro se configurado
        if (HasFiltersConfigured(_options.Filters))
        {
            _filter = new LogFilter(_options.Filters);
        }
    }

    /// <summary>
    /// Inicializa uma nova instância do DiscordLoggerProvider com opções diretamente.
    /// </summary>
    /// <param name="options">Opções de configuração do logger.</param>
    public DiscordLoggerProvider(DiscordLoggerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _discordLogger = new DiscordLogger(_options);

        // Inicializa scope provider se habilitado
        if (_options.EnableScopes)
        {
            _scopeProvider = new DiscordScopeProvider();
        }

        // Inicializa filtro se configurado
        if (HasFiltersConfigured(_options.Filters))
        {
            _filter = new LogFilter(_options.Filters);
        }
    }

    /// <inheritdoc />
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DiscordLoggerProvider));
        }

        return new MicrosoftDiscordLogger(categoryName, _discordLogger, _options, _scopeProvider, _filter);
    }

    /// <inheritdoc />
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _externalScopeProvider = scopeProvider;
    }

    /// <summary>
    /// Verifica se há filtros configurados.
    /// </summary>
    private static bool HasFiltersConfigured(LogFilterOptions filters)
    {
        return filters.IncludeCategories.Count > 0 ||
               filters.ExcludeCategories.Count > 0 ||
               filters.IncludeEventIds.Count > 0 ||
               filters.ExcludeEventIds.Count > 0 ||
               filters.MessagePatterns.Count > 0 ||
               filters.EnabledLevels.Count > 0;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _scopeProvider?.Dispose();

        if (_discordLogger is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _disposed = true;
    }
}
