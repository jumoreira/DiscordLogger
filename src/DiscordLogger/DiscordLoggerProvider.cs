using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordLogger;

/// <summary>
/// Provider para criar instâncias de MicrosoftDiscordLogger.
/// </summary>
[ProviderAlias("Discord")]
public sealed class DiscordLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly IDiscordLogger _discordLogger;
    private readonly DiscordLoggerOptions _options;
    private IExternalScopeProvider? _scopeProvider;
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
    }

    /// <summary>
    /// Inicializa uma nova instância do DiscordLoggerProvider com opções diretamente.
    /// </summary>
    /// <param name="options">Opções de configuração do logger.</param>
    public DiscordLoggerProvider(DiscordLoggerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _discordLogger = new DiscordLogger(_options);
    }

    /// <inheritdoc />
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DiscordLoggerProvider));
        }

        return new MicrosoftDiscordLogger(categoryName, _discordLogger, _options);
    }

    /// <inheritdoc />
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_discordLogger is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _disposed = true;
    }
}
