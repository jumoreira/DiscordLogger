namespace DiscordLogger.Performance;

/// <summary>
/// Define os níveis de prioridade para mensagens de log.
/// </summary>
public enum MessagePriority
{
    /// <summary>
    /// Prioridade baixa - logs de Debug e Trace.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Prioridade normal - logs de Information.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// Prioridade alta - logs de Warning.
    /// </summary>
    High = 2,

    /// <summary>
    /// Prioridade crítica - logs de Error e Critical.
    /// </summary>
    Critical = 3
}
