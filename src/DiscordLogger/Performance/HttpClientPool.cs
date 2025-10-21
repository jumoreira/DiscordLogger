using System.Collections.Concurrent;

namespace DiscordLogger.Performance;

/// <summary>
/// Pool de HttpClient para otimização de conexões HTTP.
/// </summary>
internal sealed class HttpClientPool : IDisposable
{
    private readonly ConcurrentBag<PooledHttpClient> _availableClients = new();
    private readonly int _maxPoolSize;
    private readonly TimeSpan _timeout;
    private readonly TimeSpan _clientLifetime;
    private int _currentPoolSize;
    private bool _disposed;

    public HttpClientPool(int maxPoolSize = 10, int timeoutSeconds = 30, int clientLifetimeMinutes = 5)
    {
        _maxPoolSize = maxPoolSize;
        _timeout = TimeSpan.FromSeconds(timeoutSeconds);
        _clientLifetime = TimeSpan.FromMinutes(clientLifetimeMinutes);
    }

    /// <summary>
    /// Obtém um HttpClient do pool ou cria um novo se necessário.
    /// </summary>
    public HttpClient GetClient()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(HttpClientPool));
        }

        // Remove clientes expirados
        CleanupExpiredClients();

        // Tenta obter um cliente existente
        while (_availableClients.TryTake(out var pooledClient))
        {
            if (!pooledClient.IsExpired)
            {
                return pooledClient.Client;
            }

            // Cliente expirado, descarta
            pooledClient.Dispose();
            Interlocked.Decrement(ref _currentPoolSize);
        }

        // Cria novo cliente se não atingiu o limite
        if (_currentPoolSize < _maxPoolSize)
        {
            Interlocked.Increment(ref _currentPoolSize);
            return CreateNewClient();
        }

        // Limite atingido, cria cliente temporário (será descartado)
        return CreateNewClient();
    }

    /// <summary>
    /// Retorna um HttpClient ao pool para reutilização.
    /// </summary>
    public void ReturnClient(HttpClient client)
    {
        if (_disposed || client == null)
        {
            return;
        }

        var pooledClient = new PooledHttpClient(client, DateTime.UtcNow.Add(_clientLifetime));
        
        if (_availableClients.Count < _maxPoolSize)
        {
            _availableClients.Add(pooledClient);
        }
        else
        {
            // Pool cheio, descarta o cliente
            pooledClient.Dispose();
            Interlocked.Decrement(ref _currentPoolSize);
        }
    }

    /// <summary>
    /// Cria um novo HttpClient configurado.
    /// </summary>
    private HttpClient CreateNewClient()
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = _clientLifetime,
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 10
        };

        var client = new HttpClient(handler)
        {
            Timeout = _timeout
        };

        return client;
    }

    /// <summary>
    /// Remove clientes expirados do pool.
    /// </summary>
    private void CleanupExpiredClients()
    {
        var tempList = new List<PooledHttpClient>();

        // Coleta todos os clientes
        while (_availableClients.TryTake(out var client))
        {
            tempList.Add(client);
        }

        // Devolve apenas os não expirados
        foreach (var client in tempList)
        {
            if (!client.IsExpired)
            {
                _availableClients.Add(client);
            }
            else
            {
                client.Dispose();
                Interlocked.Decrement(ref _currentPoolSize);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        while (_availableClients.TryTake(out var client))
        {
            client.Dispose();
        }
    }

    /// <summary>
    /// Cliente HTTP poolado com tempo de expiração.
    /// </summary>
    private sealed class PooledHttpClient : IDisposable
    {
        public HttpClient Client { get; }
        public DateTime ExpiresAt { get; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public PooledHttpClient(HttpClient client, DateTime expiresAt)
        {
            Client = client;
            ExpiresAt = expiresAt;
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}

/// <summary>
/// Gerenciador de HttpClient com pooling automático.
/// </summary>
internal sealed class HttpClientManager : IDisposable
{
    private static HttpClientManager? _instance;
    private static readonly object _lock = new();

    private readonly HttpClientPool _pool;
    private bool _disposed;

    private HttpClientManager(bool enablePooling, int timeoutSeconds)
    {
        if (enablePooling)
        {
            _pool = new HttpClientPool(timeoutSeconds: timeoutSeconds);
        }
        else
        {
            _pool = new HttpClientPool(maxPoolSize: 1, timeoutSeconds: timeoutSeconds);
        }
    }

    /// <summary>
    /// Obtém a instância singleton do gerenciador.
    /// </summary>
    public static HttpClientManager GetInstance(bool enablePooling = true, int timeoutSeconds = 30)
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new HttpClientManager(enablePooling, timeoutSeconds);
                }
            }
        }

        return _instance;
    }

    /// <summary>
    /// Obtém um HttpClient para uso.
    /// </summary>
    public HttpClient GetClient()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(HttpClientManager));
        }

        return _pool.GetClient();
    }

    /// <summary>
    /// Retorna um HttpClient ao pool.
    /// </summary>
    public void ReturnClient(HttpClient client)
    {
        if (_disposed || client == null)
        {
            return;
        }

        _pool.ReturnClient(client);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _pool.Dispose();

        lock (_lock)
        {
            _instance = null;
        }
    }
}
