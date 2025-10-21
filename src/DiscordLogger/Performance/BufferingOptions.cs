namespace DiscordLogger.Performance;

/// <summary>
/// Configurações para o sistema de buffering.
/// </summary>
public class BufferingOptions
{
    /// <summary>
    /// Habilita o buffering de mensagens. Padrão: true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Capacidade máxima do buffer em número de mensagens. Padrão: 100.
    /// </summary>
    public int BufferCapacity { get; set; } = 100;

    /// <summary>
    /// Estratégia de flush do buffer. Padrão: Auto.
    /// </summary>
    public FlushStrategy FlushStrategy { get; set; } = FlushStrategy.Auto;

    /// <summary>
    /// Intervalo de flush automático em segundos. Padrão: 10.
    /// </summary>
    public int AutoFlushIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Número de mensagens que dispara um flush. Padrão: 10.
    /// </summary>
    public int FlushThreshold { get; set; } = 10;

    /// <summary>
    /// Comportamento quando o buffer fica cheio. Padrão: DropOldest.
    /// </summary>
    public OverflowBehavior OverflowBehavior { get; set; } = OverflowBehavior.DropOldest;

    /// <summary>
    /// Habilita persistência em disco quando o buffer fica cheio. Padrão: false.
    /// </summary>
    public bool EnablePersistence { get; set; } = false;

    /// <summary>
    /// Diretório para persistência de mensagens. Se null, usa o diretório temp do sistema.
    /// </summary>
    public string? PersistenceDirectory { get; set; }

    /// <summary>
    /// Tamanho máximo do arquivo de persistência em MB. Padrão: 10 MB.
    /// </summary>
    public int MaxPersistenceFileSizeMB { get; set; } = 10;

    /// <summary>
    /// Tempo máximo para manter arquivos de persistência em horas. Padrão: 24 horas.
    /// </summary>
    public int PersistenceRetentionHours { get; set; } = 24;
}

/// <summary>
/// Estratégias de flush do buffer.
/// </summary>
public enum FlushStrategy
{
    /// <summary>
    /// Flush manual - apenas quando chamado explicitamente.
    /// </summary>
    Manual,

    /// <summary>
    /// Flush baseado em intervalo de tempo.
    /// </summary>
    Timer,

    /// <summary>
    /// Flush baseado em número de mensagens.
    /// </summary>
    Threshold,

    /// <summary>
    /// Flush automático - combina timer e threshold.
    /// </summary>
    Auto
}

/// <summary>
/// Comportamento quando o buffer atinge capacidade máxima.
/// </summary>
public enum OverflowBehavior
{
    /// <summary>
    /// Descarta mensagens mais antigas.
    /// </summary>
    DropOldest,

    /// <summary>
    /// Descarta mensagens mais novas.
    /// </summary>
    DropNewest,

    /// <summary>
    /// Bloqueia até haver espaço (pode causar perda de performance).
    /// </summary>
    Block,

    /// <summary>
    /// Persiste em disco e continua.
    /// </summary>
    Persist
}
