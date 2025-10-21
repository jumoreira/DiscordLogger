using System.Text;

namespace DiscordLogger.Scopes;

/// <summary>
/// Provider para gerenciar scopes de log.
/// </summary>
internal sealed class DiscordScopeProvider : IDisposable
{
    private readonly AsyncLocal<DiscordLoggerScope?> _currentScope = new();

    /// <summary>
    /// Obtém o scope atual.
    /// </summary>
    public DiscordLoggerScope? CurrentScope => _currentScope.Value;

    /// <summary>
    /// Inicia um novo scope.
    /// </summary>
    /// <typeparam name="TState">Tipo do estado do scope.</typeparam>
    /// <param name="state">Estado do scope.</param>
    /// <returns>Disposable que encerra o scope quando liberado.</returns>
    public IDisposable Push<TState>(TState state) where TState : notnull
    {
        var parent = _currentScope.Value;
        var newScope = new DiscordLoggerScope(state, parent);
        _currentScope.Value = newScope;

        return new ScopeDisposable(this);
    }

    /// <summary>
    /// Remove o scope atual.
    /// </summary>
    private void Pop()
    {
        var current = _currentScope.Value;
        _currentScope.Value = current?.Parent;
    }

    /// <summary>
    /// Formata os scopes atuais como string.
    /// </summary>
    /// <returns>String formatada dos scopes ou null se não houver scopes.</returns>
    public string? FormatScopes()
    {
        var current = _currentScope.Value;
        if (current == null)
        {
            return null;
        }

        var scopes = current.GetScopes();
        if (!scopes.Any())
        {
            return null;
        }

        var sb = new StringBuilder();
        sb.Append("**Scopes:** ");

        var scopeStrings = scopes.Select(FormatScope).Where(s => !string.IsNullOrEmpty(s));
        sb.Append(string.Join(" => ", scopeStrings));

        return sb.ToString();
    }

    /// <summary>
    /// Formata um scope individual.
    /// </summary>
    private static string FormatScope(object? scope)
    {
        if (scope == null)
        {
            return string.Empty;
        }

        // Se for um KeyValuePair ou similar, tenta extrair informação útil
        if (scope is IEnumerable<KeyValuePair<string, object>> kvps)
        {
            var items = kvps.Select(kvp => $"{kvp.Key}={kvp.Value}");
            return string.Join(", ", items);
        }

        return scope.ToString() ?? string.Empty;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _currentScope.Value = null;
    }

    /// <summary>
    /// Disposable que remove o scope quando liberado.
    /// </summary>
    private sealed class ScopeDisposable : IDisposable
    {
        private readonly DiscordScopeProvider _provider;
        private bool _disposed;

        public ScopeDisposable(DiscordScopeProvider provider)
        {
            _provider = provider;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _provider.Pop();
                _disposed = true;
            }
        }
    }
}
