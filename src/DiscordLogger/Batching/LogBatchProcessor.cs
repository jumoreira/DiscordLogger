using System.Threading.Channels;

namespace DiscordLogger.Batching;

/// <summary>
/// Processador de batches de mensagens de log.
/// </summary>
internal sealed class LogBatchProcessor : IDisposable
{
    private readonly Channel<QueuedLogMessage> _channel;
    private readonly BatchingOptions _options;
    private readonly Func<IList<QueuedLogMessage>, Task> _sendBatchFunc;
    private readonly CancellationTokenSource _cts;
    private readonly Task _processorTask;
    private bool _disposed;

    /// <summary>
    /// Inicializa uma nova instância do LogBatchProcessor.
    /// </summary>
    /// <param name="options">Opções de batching.</param>
    /// <param name="sendBatchFunc">Função para enviar um batch de mensagens.</param>
    public LogBatchProcessor(BatchingOptions options, Func<IList<QueuedLogMessage>, Task> sendBatchFunc)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _sendBatchFunc = sendBatchFunc ?? throw new ArgumentNullException(nameof(sendBatchFunc));

        var channelOptions = new BoundedChannelOptions(_options.MaxQueueSize)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<QueuedLogMessage>(channelOptions);
        _cts = new CancellationTokenSource();
        _processorTask = Task.Run(() => ProcessBatchesAsync(_cts.Token));
    }

    /// <summary>
    /// Enfileira uma mensagem de log para processamento em batch.
    /// </summary>
    /// <param name="message">Mensagem a ser enfileirada.</param>
    /// <returns>True se a mensagem foi enfileirada com sucesso, false caso contrário.</returns>
    public bool EnqueueMessage(QueuedLogMessage message)
    {
        if (_disposed)
        {
            return false;
        }

        return _channel.Writer.TryWrite(message);
    }

    /// <summary>
    /// Processa batches de mensagens continuamente.
    /// </summary>
    private async Task ProcessBatchesAsync(CancellationToken cancellationToken)
    {
        var batch = new List<QueuedLogMessage>(_options.MaxBatchSize);
        var flushInterval = TimeSpan.FromSeconds(_options.FlushIntervalSeconds);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(flushInterval);

                try
                {
                    // Tenta ler mensagens até atingir o tamanho do batch ou timeout
                    while (batch.Count < _options.MaxBatchSize)
                    {
                        if (await _channel.Reader.WaitToReadAsync(timeoutCts.Token))
                        {
                            if (_channel.Reader.TryRead(out var message))
                            {
                                batch.Add(message);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Timeout ou cancellation - flush o batch atual se houver mensagens
                }

                // Envia o batch se houver mensagens
                if (batch.Count > 0)
                {
                    try
                    {
                        await _sendBatchFunc(batch);
                    }
                    catch (Exception ex)
                    {
                        // Log do erro (silenciosamente para evitar loops)
                        System.Diagnostics.Debug.WriteLine($"Error sending batch: {ex.Message}");
                    }
                    finally
                    {
                        batch.Clear();
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown normal
        }
        finally
        {
            // Flush final de mensagens restantes
            while (_channel.Reader.TryRead(out var message))
            {
                batch.Add(message);

                if (batch.Count >= _options.MaxBatchSize)
                {
                    try
                    {
                        await _sendBatchFunc(batch);
                    }
                    catch
                    {
                        // Ignora erros no shutdown
                    }
                    batch.Clear();
                }
            }

            // Envia o último batch se houver
            if (batch.Count > 0)
            {
                try
                {
                    await _sendBatchFunc(batch);
                }
                catch
                {
                    // Ignora erros no shutdown
                }
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _channel.Writer.Complete();
        _cts.Cancel();

        try
        {
            _processorTask.Wait(TimeSpan.FromSeconds(5));
        }
        catch
        {
            // Ignora erros no shutdown
        }

        _cts.Dispose();
        _disposed = true;
    }
}
