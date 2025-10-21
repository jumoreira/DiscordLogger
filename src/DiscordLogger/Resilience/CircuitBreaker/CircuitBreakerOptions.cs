namespace DiscordLogger.Resilience.CircuitBreaker;

/// <summary>
/// Opções de configuração para Circuit Breaker.
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// Habilita o circuit breaker. Padrão: true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Número de falhas consecutivas antes de abrir o circuito. Padrão: 5.
    /// </summary>
    public int FailureThreshold { get; set; } = 5;

    /// <summary>
    /// Taxa de sucesso mínima (0.0 a 1.0) para fechar o circuito em HalfOpen. Padrão: 0.8 (80%).
    /// </summary>
    public double SuccessThreshold { get; set; } = 0.8;

    /// <summary>
    /// Tempo em segundos que o circuito permanece aberto antes de tentar HalfOpen. Padrão: 30.
    /// </summary>
    public int OpenTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Número de requisições recentes para calcular taxa de sucesso. Padrão: 100.
    /// </summary>
    public int SamplingDuration { get; set; } = 100;

    /// <summary>
    /// Habilita health monitoring (tempo de resposta). Padrão: true.
    /// </summary>
    public bool EnableHealthMonitoring { get; set; } = true;

    /// <summary>
    /// Threshold de tempo de resposta lento em ms. Padrão: 5000ms (5s).
    /// </summary>
    public int SlowRequestThresholdMs { get; set; } = 5000;

    /// <summary>
    /// Estratégia de fallback quando o circuito está aberto. Padrão: Queue.
    /// </summary>
    public FallbackStrategy FallbackStrategy { get; set; } = FallbackStrategy.Queue;

    /// <summary>
    /// Webhook alternativo para fallback. Opcional.
    /// </summary>
    public string? FallbackWebhookUrl { get; set; }

    /// <summary>
    /// Caminho do arquivo para fallback baseado em arquivo. Padrão: "./logs/discord-fallback.log".
    /// </summary>
    public string FileFallbackPath { get; set; } = "./logs/discord-fallback.log";

    /// <summary>
    /// Habilita auto-recovery (tentativas automáticas de recuperação). Padrão: true.
    /// </summary>
    public bool EnableAutoRecovery { get; set; } = true;

    /// <summary>
    /// Intervalo em segundos para tentativas de auto-recovery. Padrão: 60.
    /// </summary>
    public int AutoRecoveryIntervalSeconds { get; set; } = 60;
}

/// <summary>
/// Estratégias de fallback quando o circuit breaker está aberto.
/// </summary>
public enum FallbackStrategy
{
    /// <summary>
    /// Descarta a mensagem (perda de dados).
    /// </summary>
    Discard,

    /// <summary>
    /// Enfileira para retry posterior (sem perda de dados).
    /// </summary>
    Queue,

    /// <summary>
    /// Salva em arquivo local.
    /// </summary>
    File,

    /// <summary>
    /// Envia para webhook alternativo.
    /// </summary>
    AlternativeWebhook
}
