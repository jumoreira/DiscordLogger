using System.Text.Json;
using DiscordLogger.Models;
using DiscordLogger.Resilience.Persistence;

namespace DiscordLogger.Resilience;

/// <summary>
/// Serviço de recuperação automática de mensagens da Dead Letter Queue.
/// </summary>
public sealed class RecoveryService : IDisposable
{
    private readonly DiscordLoggerOptions _options;
    private readonly IDeadLetterQueue _deadLetterQueue;
    private readonly ResilientWebhookClient _webhookClient;
    private readonly Timer? _recoveryTimer;
    private readonly SemaphoreSlim _recoverySemaphore;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;
    private long _totalRecovered;
    private long _failedRecoveries;

    public RecoveryService(
        DiscordLoggerOptions options,
        IDeadLetterQueue deadLetterQueue,
        ResilientWebhookClient webhookClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _deadLetterQueue = deadLetterQueue ?? throw new ArgumentNullException(nameof(deadLetterQueue));
        _webhookClient = webhookClient ?? throw new ArgumentNullException(nameof(webhookClient));
        _recoverySemaphore = new SemaphoreSlim(1, 1);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Inicia timer de recovery se auto-recovery está habilitado
        if (_options.Resilience.Persistence.EnableAutoRecovery)
        {
            var interval = TimeSpan.FromSeconds(_options.Resilience.Persistence.RecoveryIntervalSeconds);
            _recoveryTimer = new Timer(
                _ => _ = RecoverMessagesAsync(),
                null,
                interval,
                interval
            );
        }
    }

    /// <summary>
    /// Tenta recuperar mensagens da Dead Letter Queue.
    /// </summary>
    public async Task RecoverMessagesAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || !await _recoverySemaphore.WaitAsync(0, cancellationToken))
        {
            return; // Já está executando recovery
        }

        try
        {
            var maxMessages = _options.Resilience.Persistence.MaxMessagesPerRecoveryCycle;
            var messages = await _deadLetterQueue.DequeueAsync(maxMessages, cancellationToken);

            if (messages.Count == 0)
            {
                return;
            }

            if (_options.Resilience.EnableVerboseLogging)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[DiscordLogger Recovery] Tentando recuperar {messages.Count} mensagens da DLQ..."
                );
            }

            foreach (var failedMessage in messages)
            {
                try
                {
                    // Desserializa a mensagem original
                    var message = JsonSerializer.Deserialize<DiscordWebhookMessage>(
                        failedMessage.MessageContent,
                        _jsonOptions
                    );

                    if (message == null)
                    {
                        await _deadLetterQueue.RemoveAsync(failedMessage.Id, cancellationToken);
                        continue;
                    }

                    // Tenta reenviar
                    await _webhookClient.SendMessageAsync(
                        message,
                        failedMessage.LogLevel,
                        failedMessage.Category,
                        cancellationToken
                    );

                    // Sucesso - remove da DLQ
                    await _deadLetterQueue.RemoveAsync(failedMessage.Id, cancellationToken);
                    _totalRecovered++;

                    if (_options.Resilience.EnableVerboseLogging)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[DiscordLogger Recovery] Mensagem {failedMessage.Id} recuperada com sucesso."
                        );
                    }
                }
                catch (Exception ex)
                {
                    _failedRecoveries++;

                    if (_options.Resilience.EnableVerboseLogging)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[DiscordLogger Recovery] Falha ao recuperar mensagem {failedMessage.Id}: {ex.Message}"
                        );
                    }

                    // Atualiza contadores de falha
                    failedMessage.AttemptCount++;
                    failedMessage.LastAttempt = DateTimeOffset.UtcNow;
                    failedMessage.LastError = ex.Message;

                    // Se excedeu limite de tentativas, mantém na DLQ mas não tenta mais
                    if (failedMessage.AttemptCount >= _options.Resilience.Persistence.MaxAttemptsBeforeDLQ * 2)
                    {
                        if (_options.Resilience.EnableVerboseLogging)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"[DiscordLogger Recovery] Mensagem {failedMessage.Id} excedeu limite de tentativas. Mantendo na DLQ permanentemente."
                            );
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (_options.Resilience.EnableVerboseLogging)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[DiscordLogger Recovery] Erro durante recovery: {ex.Message}"
                );
            }
        }
        finally
        {
            _recoverySemaphore.Release();
        }
    }

    /// <summary>
    /// Obtém estatísticas de recovery.
    /// </summary>
    public RecoveryStatistics GetStatistics()
    {
        return new RecoveryStatistics
        {
            TotalRecovered = _totalRecovered,
            FailedRecoveries = _failedRecoveries
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _recoveryTimer?.Dispose();
        _recoverySemaphore?.Dispose();
    }
}

/// <summary>
/// Estatísticas do Recovery Service.
/// </summary>
public class RecoveryStatistics
{
    /// <summary>
    /// Total de mensagens recuperadas com sucesso.
    /// </summary>
    public long TotalRecovered { get; set; }

    /// <summary>
    /// Total de falhas durante recovery.
    /// </summary>
    public long FailedRecoveries { get; set; }
}
