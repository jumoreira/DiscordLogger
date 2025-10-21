namespace DiscordLogger.Resilience.Persistence;

/// <summary>
/// Opções de configuração para persistência de falhas.
/// </summary>
public class PersistenceOptions
{
    /// <summary>
    /// Habilita Dead Letter Queue. Padrão: true.
    /// </summary>
    public bool EnableDeadLetterQueue { get; set; } = true;

    /// <summary>
    /// Caminho do arquivo para DLQ. Padrão: "./logs/discord-dlq.json".
    /// </summary>
    public string DeadLetterQueuePath { get; set; } = "./logs/discord-dlq.json";

    /// <summary>
    /// Número máximo de tentativas antes de mover para DLQ. Padrão: 5.
    /// </summary>
    public int MaxAttemptsBeforeDLQ { get; set; } = 5;

    /// <summary>
    /// Habilita File Fallback automático. Padrão: true.
    /// </summary>
    public bool EnableFileFallback { get; set; } = true;

    /// <summary>
    /// Caminho do arquivo para fallback. Padrão: "./logs/discord-fallback.log".
    /// </summary>
    public string FileFallbackPath { get; set; } = "./logs/discord-fallback.log";

    /// <summary>
    /// Tamanho máximo do arquivo de fallback em MB. Padrão: 100.
    /// </summary>
    public int MaxFileFallbackSizeMb { get; set; } = 100;

    /// <summary>
    /// Habilita Retry Queue. Padrão: true.
    /// </summary>
    public bool EnableRetryQueue { get; set; } = true;

    /// <summary>
    /// Tamanho máximo da retry queue. Padrão: 10000.
    /// </summary>
    public int RetryQueueMaxSize { get; set; } = 10000;

    /// <summary>
    /// Intervalo em segundos para processar retry queue. Padrão: 60.
    /// </summary>
    public int RetryQueueProcessIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Habilita mecanismo de recovery automático. Padrão: true.
    /// </summary>
    public bool EnableAutoRecovery { get; set; } = true;

    /// <summary>
    /// Intervalo em segundos para tentar recovery de mensagens da DLQ. Padrão: 300 (5 minutos).
    /// </summary>
    public int RecoveryIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Número máximo de mensagens a recuperar por ciclo. Padrão: 100.
    /// </summary>
    public int MaxMessagesPerRecoveryCycle { get; set; } = 100;
}
