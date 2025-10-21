using System.Buffers;
using System.Text;

namespace DiscordLogger.Performance;

/// <summary>
/// Pool de objetos reutilizáveis para otimização de performance.
/// </summary>
internal static class ObjectPool
{
    private static readonly ArrayPool<char> CharArrayPool = ArrayPool<char>.Shared;
    private static readonly StringBuilderPool StringBuilders = new();

    /// <summary>
    /// Obtém um StringBuilder do pool.
    /// </summary>
    public static StringBuilder GetStringBuilder()
    {
        return StringBuilders.Get();
    }

    /// <summary>
    /// Retorna um StringBuilder ao pool.
    /// </summary>
    public static void ReturnStringBuilder(StringBuilder builder)
    {
        StringBuilders.Return(builder);
    }

    /// <summary>
    /// Aluga um array de chars do pool.
    /// </summary>
    public static char[] RentCharArray(int minimumLength)
    {
        return CharArrayPool.Rent(minimumLength);
    }

    /// <summary>
    /// Retorna um array de chars ao pool.
    /// </summary>
    public static void ReturnCharArray(char[] array, bool clearArray = false)
    {
        CharArrayPool.Return(array, clearArray);
    }
}

/// <summary>
/// Pool específico para StringBuilder.
/// </summary>
internal sealed class StringBuilderPool
{
    private const int MaxCapacity = 8192; // 8KB
    private const int DefaultCapacity = 256;
    private readonly Stack<StringBuilder> _pool = new();
    private readonly object _lock = new();

    public StringBuilder Get()
    {
        lock (_lock)
        {
            if (_pool.Count > 0)
            {
                var builder = _pool.Pop();
                return builder;
            }
        }

        return new StringBuilder(DefaultCapacity);
    }

    public void Return(StringBuilder builder)
    {
        if (builder.Capacity > MaxCapacity)
        {
            // Não retorna ao pool se o capacity for muito grande
            return;
        }

        builder.Clear();

        lock (_lock)
        {
            if (_pool.Count < 16) // Limita o tamanho do pool
            {
                _pool.Push(builder);
            }
        }
    }
}
