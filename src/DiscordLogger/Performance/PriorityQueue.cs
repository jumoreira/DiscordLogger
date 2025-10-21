using System.Diagnostics.CodeAnalysis;

namespace DiscordLogger.Performance;

/// <summary>
/// Fila de prioridade thread-safe para mensagens de log.
/// </summary>
/// <typeparam name="T">Tipo do item na fila.</typeparam>
internal sealed class PriorityQueue<T>
{
    private readonly List<PriorityQueueItem<T>>[] _queues;
    private readonly object _lock = new();
    private int _count;

    public PriorityQueue()
    {
        // Uma lista para cada nível de prioridade
        _queues = new List<PriorityQueueItem<T>>[4];
        for (int i = 0; i < _queues.Length; i++)
        {
            _queues[i] = new List<PriorityQueueItem<T>>();
        }
    }

    /// <summary>
    /// Número total de itens na fila.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _count;
            }
        }
    }

    /// <summary>
    /// Enfileira um item com a prioridade especificada.
    /// </summary>
    public void Enqueue(T item, MessagePriority priority)
    {
        lock (_lock)
        {
            var queueItem = new PriorityQueueItem<T>(item, priority, DateTime.UtcNow);
            _queues[(int)priority].Add(queueItem);
            _count++;
        }
    }

    /// <summary>
    /// Tenta desenfileirar um item, priorizando itens de maior prioridade.
    /// </summary>
    public bool TryDequeue([MaybeNullWhen(false)] out T item, out MessagePriority priority)
    {
        lock (_lock)
        {
            // Procura do maior para o menor prioridade
            for (int i = _queues.Length - 1; i >= 0; i--)
            {
                var queue = _queues[i];
                if (queue.Count > 0)
                {
                    var queueItem = queue[0];
                    queue.RemoveAt(0);
                    _count--;
                    
                    item = queueItem.Item;
                    priority = queueItem.Priority;
                    return true;
                }
            }

            item = default;
            priority = MessagePriority.Normal;
            return false;
        }
    }

    /// <summary>
    /// Tenta espiar o próximo item sem removê-lo.
    /// </summary>
    public bool TryPeek([MaybeNullWhen(false)] out T item, out MessagePriority priority)
    {
        lock (_lock)
        {
            for (int i = _queues.Length - 1; i >= 0; i--)
            {
                var queue = _queues[i];
                if (queue.Count > 0)
                {
                    var queueItem = queue[0];
                    item = queueItem.Item;
                    priority = queueItem.Priority;
                    return true;
                }
            }

            item = default;
            priority = MessagePriority.Normal;
            return false;
        }
    }

    /// <summary>
    /// Remove todos os itens da fila.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            foreach (var queue in _queues)
            {
                queue.Clear();
            }
            _count = 0;
        }
    }

    /// <summary>
    /// Remove itens antigos baseado em um tempo limite.
    /// </summary>
    public int RemoveOldItems(TimeSpan maxAge)
    {
        var cutoffTime = DateTime.UtcNow - maxAge;
        var removedCount = 0;

        lock (_lock)
        {
            foreach (var queue in _queues)
            {
                var itemsToRemove = queue.Where(x => x.EnqueuedAt < cutoffTime).ToList();
                removedCount += itemsToRemove.Count;

                foreach (var item in itemsToRemove)
                {
                    queue.Remove(item);
                }
            }

            _count -= removedCount;
        }

        return removedCount;
    }

    /// <summary>
    /// Obtém a contagem de itens por prioridade.
    /// </summary>
    public Dictionary<MessagePriority, int> GetCountByPriority()
    {
        lock (_lock)
        {
            return new Dictionary<MessagePriority, int>
            {
                { MessagePriority.Low, _queues[0].Count },
                { MessagePriority.Normal, _queues[1].Count },
                { MessagePriority.High, _queues[2].Count },
                { MessagePriority.Critical, _queues[3].Count }
            };
        }
    }
}

/// <summary>
/// Item da fila de prioridade.
/// </summary>
internal sealed class PriorityQueueItem<T>
{
    public T Item { get; }
    public MessagePriority Priority { get; }
    public DateTime EnqueuedAt { get; }

    public PriorityQueueItem(T item, MessagePriority priority, DateTime enqueuedAt)
    {
        Item = item;
        Priority = priority;
        EnqueuedAt = enqueuedAt;
    }
}
