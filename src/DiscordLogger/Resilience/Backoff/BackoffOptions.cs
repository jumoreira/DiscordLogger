namespace DiscordLogger.Resilience.Backoff;

/// <summary>
/// Opções de configuração para estratégias de backoff.
/// </summary>
public class BackoffOptions
{
    /// <summary>
    /// Tipo de estratégia de backoff. Padrão: Exponential.
    /// </summary>
    public BackoffType Type { get; set; } = BackoffType.Exponential;

    /// <summary>
    /// Delay inicial (usado por todas as estratégias). Padrão: 1 segundo.
    /// </summary>
    public int InitialDelaySeconds { get; set; } = 1;

    /// <summary>
    /// Delay máximo. Padrão: 5 minutos.
    /// </summary>
    public int MaxDelaySeconds { get; set; } = 300;

    /// <summary>
    /// Multiplicador exponencial (apenas para Exponential). Padrão: 2.
    /// </summary>
    public double ExponentialMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Incremento linear em segundos (apenas para Linear). Padrão: 2 segundos.
    /// </summary>
    public int LinearIncrementSeconds { get; set; } = 2;

    /// <summary>
    /// Habilita jitter aleatório (±30%). Padrão: true.
    /// Recomendado para evitar thundering herd problem.
    /// </summary>
    public bool UseJitter { get; set; } = true;
}

/// <summary>
/// Tipos de estratégia de backoff disponíveis.
/// </summary>
public enum BackoffType
{
    /// <summary>
    /// Backoff exponencial (mais agressivo).
    /// </summary>
    Exponential,

    /// <summary>
    /// Backoff linear (mais previsível).
    /// </summary>
    Linear,

    /// <summary>
    /// Backoff Fibonacci (equilibrado).
    /// </summary>
    Fibonacci
}
