namespace DiscordLogger.Resilience.RateLimiting;

/// <summary>
/// Opções de configuração para rate limiting.
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// Habilita rate limiting avançado. Padrão: true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Tipo de rate limiter a ser usado. Padrão: TokenBucket.
    /// </summary>
    public RateLimiterType Type { get; set; } = RateLimiterType.TokenBucket;

    /// <summary>
    /// Capacidade máxima do bucket (burst size). Padrão: 50.
    /// </summary>
    public int Capacity { get; set; } = 50;

    /// <summary>
    /// Taxa de refill (tokens por intervalo). Padrão: 50.
    /// </summary>
    public int RefillRate { get; set; } = 50;

    /// <summary>
    /// Intervalo de refill em segundos. Padrão: 1 segundo.
    /// </summary>
    public int RefillIntervalSeconds { get; set; } = 1;

    /// <summary>
    /// Habilita ajuste adaptativo (apenas para AdaptiveRateLimiter). Padrão: false.
    /// </summary>
    public bool EnableAdaptive { get; set; } = false;

    /// <summary>
    /// Capacidade mínima para rate limiter adaptativo. Padrão: 10.
    /// </summary>
    public int MinCapacity { get; set; } = 10;

    /// <summary>
    /// Capacidade máxima para rate limiter adaptativo. Padrão: 200.
    /// </summary>
    public int MaxCapacity { get; set; } = 200;

    /// <summary>
    /// Multiplicador de aumento para rate limiter adaptativo. Padrão: 1.2 (+20%).
    /// </summary>
    public double IncreaseMultiplier { get; set; } = 1.2;

    /// <summary>
    /// Multiplicador de redução para rate limiter adaptativo. Padrão: 0.8 (-20%).
    /// </summary>
    public double DecreaseMultiplier { get; set; } = 0.8;

    /// <summary>
    /// Habilita priorização por nível de log. Padrão: true.
    /// Logs críticos e de erro são priorizados sobre warnings e info.
    /// </summary>
    public bool EnablePriorityQueue { get; set; } = true;
}

/// <summary>
/// Tipos de rate limiter disponíveis.
/// </summary>
public enum RateLimiterType
{
    /// <summary>
    /// Token Bucket Algorithm - permite burst de requisições.
    /// </summary>
    TokenBucket,

    /// <summary>
    /// Adaptive Rate Limiter - ajusta automaticamente baseado em sucesso/falha.
    /// </summary>
    Adaptive
}
