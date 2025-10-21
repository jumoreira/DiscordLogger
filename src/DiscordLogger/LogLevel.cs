namespace DiscordLogger;

/// <summary>
/// Níveis de log suportados pelo DiscordLogger.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Logs de debug para informações de desenvolvimento.
    /// </summary>
    Debug = 0,

    /// <summary>
    /// Logs informativos sobre o fluxo da aplicação.
    /// </summary>
    Information = 1,

    /// <summary>
    /// Logs de avisos sobre comportamentos inesperados.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Logs de erros que não impedem a execução da aplicação.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Logs de erros críticos que podem causar falha da aplicação.
    /// </summary>
    Critical = 4
}
