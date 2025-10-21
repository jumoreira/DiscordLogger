namespace DiscordLogger.Batching;

/// <summary>
/// Configurações para o sistema de batching de mensagens.
/// </summary>
public class BatchingOptions
{
    /// <summary>
    /// Habilita ou desabilita o batching de mensagens. Padrão: false.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Número máximo de mensagens em um batch. Padrão: 10.
    /// Discord permite até 10 embeds por mensagem.
    /// </summary>
    public int MaxBatchSize { get; set; } = 10;

    /// <summary>
    /// Intervalo de tempo máximo (em segundos) antes de enviar um batch incompleto. Padrão: 5.
    /// </summary>
    public int FlushIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Tamanho máximo da fila de mensagens. Padrão: 1000.
    /// Se a fila ficar cheia, mensagens mais antigas serão descartadas.
    /// </summary>
    public int MaxQueueSize { get; set; } = 1000;
}
