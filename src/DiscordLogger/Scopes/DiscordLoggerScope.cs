namespace DiscordLogger.Scopes;

/// <summary>
/// Representa um scope de log que pode armazenar estado adicional.
/// </summary>
internal sealed class DiscordLoggerScope : IDisposable
{
    private readonly object? _state;
    private readonly DiscordLoggerScope? _parent;

    /// <summary>
    /// Inicializa uma nova instância do DiscordLoggerScope.
    /// </summary>
    /// <param name="state">Estado do scope.</param>
    /// <param name="parent">Scope pai, se houver.</param>
    public DiscordLoggerScope(object? state, DiscordLoggerScope? parent)
    {
        _state = state;
        _parent = parent;
    }

    /// <summary>
    /// Obtém o estado do scope.
    /// </summary>
    public object? State => _state;

    /// <summary>
    /// Obtém o scope pai.
    /// </summary>
    public DiscordLoggerScope? Parent => _parent;

    /// <summary>
    /// Obtém todos os estados dos scopes na hierarquia.
    /// </summary>
    public IEnumerable<object?> GetScopes()
    {
        var scopes = new List<object?>();
        var current = this;

        while (current != null)
        {
            if (current._state != null)
            {
                scopes.Add(current._state);
            }
            current = current._parent;
        }

        scopes.Reverse();
        return scopes;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Cleanup se necessário
    }
}
