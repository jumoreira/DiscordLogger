using System.Collections.Concurrent;
using System.Threading.Channels;
using DiscordLogger.Batching;

namespace DiscordLogger.Performance;

/// <summary>
/// Processador de logs de alta performance com suporte a prioridades, buffering e backpressure.
/// </summary>
internal sealed class HighPerformanceLogProcessor : IDisposable
{
    private readonly Channel<QueuedLogMessage> _channel;
    private readonly PriorityQueue<QueuedLogMessage>? _priorityQueue;
    private readonly MessageBuffer? _messageBuffer;
    private readonly FileAttachmentManager? _fileAttachmentManager;
    private readonly DiscordLoggerOptions _options;
    private readonly Func<QueuedLogMessage, Task> _sendMessageFunc;
    private readonly CancellationTokenSource _cts;
    private readonly Task _processorTask;
    private readonly SemaphoreSlim _backpressureSemaphore;
    private bool _disposed;

    // Métricas de performance
    private long _totalMessagesProcessed;
    private long _totalMessagesDropped;
    private long _totalBytesProcessed;

    public HighPerformanceLogProcessor(
        DiscordLoggerOptions options,
        Func<QueuedLogMessage, Task> sendMessageFunc)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _sendMessageFunc = sendMessageFunc ?? throw new ArgumentNullException(nameof(sendMessageFunc));

        // Configurar canal
        var channelOptions = new BoundedChannelOptions(options.Batching.MaxQueueSize)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<QueuedLogMessage>(channelOptions);

        // Configurar componentes opcionais
        if (options.EnablePriorityQueue)
        {
            _priorityQueue = new PriorityQueue<QueuedLogMessage>();
        }

        if (options.Buffering.Enabled)
        {
            _messageBuffer = new MessageBuffer(options.Buffering, async messages =>
            {
                foreach (var msg in messages)
                {
                    await ProcessMessageAsync(msg);
                }
            });
        }

        if (options.FileAttachment.Enabled)
        {
            _fileAttachmentManager = new FileAttachmentManager(options.FileAttachment);
        }

        // Backpressure: limita mensagens em processamento simultâneo
        _backpressureSemaphore = new SemaphoreSlim(100, 100);

        _cts = new CancellationTokenSource();
        _processorTask = Task.Run(() => ProcessMessagesAsync(_cts.Token));
    }

    /// <summary>
    /// Métricas de performance do processador.
    /// </summary>
    public PerformanceMetrics GetMetrics()
    {
        return new PerformanceMetrics
        {
            TotalMessagesProcessed = Interlocked.Read(ref _totalMessagesProcessed),
            TotalMessagesDropped = Interlocked.Read(ref _totalMessagesDropped),
            TotalBytesProcessed = Interlocked.Read(ref _totalBytesProcessed),
            QueueDepth = _priorityQueue?.Count ?? 0,
            BufferCount = _messageBuffer?.Count ?? 0
        };
    }

    /// <summary>
    /// Enfileira uma mensagem para processamento.
    /// </summary>
    public async Task<bool> EnqueueAsync(QueuedLogMessage message, MessagePriority priority = MessagePriority.Normal)
    {
        if (_disposed)
        {
            return false;
        }

        // Controle de backpressure
        if (!await _backpressureSemaphore.WaitAsync(TimeSpan.FromSeconds(1)))
        {
            Interlocked.Increment(ref _totalMessagesDropped);
            return false;
        }

        try
        {
            // Se buffering está habilitado, adiciona ao buffer
            if (_messageBuffer != null)
            {
                return await _messageBuffer.AddAsync(message);
            }

            // Se fila de prioridade está habilitada
            if (_priorityQueue != null)
            {
                _priorityQueue.Enqueue(message, priority);
                return true;
            }

            // Caso contrário, usa canal direto
            return _channel.Writer.TryWrite(message);
        }
        finally
        {
            _backpressureSemaphore.Release();
        }
    }

    /// <summary>
    /// Processa mensagens continuamente.
    /// </summary>
    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                QueuedLogMessage? message = null;

                // Prioriza fila de prioridade se habilitada
                if (_priorityQueue != null && _priorityQueue.TryDequeue(out message, out _))
                {
                    await ProcessMessageAsync(message);
                }
                // Depois tenta o canal
                else if (await _channel.Reader.WaitToReadAsync(cancellationToken))
                {
                    if (_channel.Reader.TryRead(out message))
                    {
                        await ProcessMessageAsync(message);
                    }
                }

                // Pequeno delay para evitar consumo excessivo de CPU
                if (message == null)
                {
                    await Task.Delay(10, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Shutdown normal
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in message processor: {ex}");
        }
    }

    /// <summary>
    /// Processa uma mensagem individual.
    /// </summary>
    private async Task ProcessMessageAsync(QueuedLogMessage message)
    {
        try
        {
            // Processa anexos de arquivo se necessário
            if (_fileAttachmentManager != null && message.Message != null)
            {
                var processed = _fileAttachmentManager.ProcessMessage(message.Message);
                
                if (processed.RequiresAttachment)
                {
                    // Atualiza a mensagem com o preview
                    message = new QueuedLogMessage(
                        message.Level,
                        processed.Content,
                        message.Exception,
                        message.Timestamp,
                        message.ScopeInfo
                    );

                    // TODO: Implementar envio de arquivo via multipart/form-data
                    // Por enquanto, apenas truncamos a mensagem
                }
            }

            await _sendMessageFunc(message);

            // Atualiza métricas
            Interlocked.Increment(ref _totalMessagesProcessed);
            if (message.Message != null)
            {
                Interlocked.Add(ref _totalBytesProcessed, message.Message.Length);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing message: {ex}");
            Interlocked.Increment(ref _totalMessagesDropped);
        }
    }

    /// <summary>
    /// Força o flush de mensagens pendentes.
    /// </summary>
    public async Task FlushAsync()
    {
        if (_messageBuffer != null)
        {
            await _messageBuffer.FlushAsync();
        }
    }

    /// <summary>
    /// Realiza um graceful shutdown do processador.
    /// </summary>
    public async Task GracefulShutdownAsync(TimeSpan timeout)
    {
        if (_disposed)
        {
            return;
        }

        // Para de aceitar novas mensagens
        _channel.Writer.Complete();

        // Flush de mensagens pendentes
        if (_messageBuffer != null)
        {
            await _messageBuffer.FlushAsync();
        }

        // Aguarda processamento de mensagens restantes
        try
        {
            using var timeoutCts = new CancellationTokenSource(timeout);
            
            // Processa mensagens restantes da fila de prioridade
            if (_priorityQueue != null)
            {
                while (_priorityQueue.TryDequeue(out var message, out _) && !timeoutCts.Token.IsCancellationRequested)
                {
                    await ProcessMessageAsync(message);
                }
            }

            // Processa mensagens restantes do canal
            while (_channel.Reader.TryRead(out var message) && !timeoutCts.Token.IsCancellationRequested)
            {
                await ProcessMessageAsync(message);
            }
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Graceful shutdown timeout exceeded");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Graceful shutdown
        var timeout = TimeSpan.FromSeconds(_options.GracefulShutdownTimeoutSeconds);
        GracefulShutdownAsync(timeout).GetAwaiter().GetResult();

        // Cancela processamento
        _cts.Cancel();

        try
        {
            _processorTask.Wait(timeout);
        }
        catch
        {
            // Ignora erros no shutdown
        }

        _messageBuffer?.Dispose();
        _cts.Dispose();
        _backpressureSemaphore.Dispose();
    }
}

/// <summary>
/// Métricas de performance do processador.
/// </summary>
public sealed class PerformanceMetrics
{
    /// <summary>
    /// Total de mensagens processadas com sucesso.
    /// </summary>
    public long TotalMessagesProcessed { get; set; }

    /// <summary>
    /// Total de mensagens descartadas.
    /// </summary>
    public long TotalMessagesDropped { get; set; }

    /// <summary>
    /// Total de bytes processados.
    /// </summary>
    public long TotalBytesProcessed { get; set; }

    /// <summary>
    /// Profundidade atual da fila.
    /// </summary>
    public int QueueDepth { get; set; }

    /// <summary>
    /// Número de mensagens no buffer.
    /// </summary>
    public int BufferCount { get; set; }

    /// <summary>
    /// Taxa de mensagens descartadas.
    /// </summary>
    public double DropRate => TotalMessagesProcessed > 0 
        ? (double)TotalMessagesDropped / (TotalMessagesProcessed + TotalMessagesDropped) 
        : 0;
}
