using System.Runtime.CompilerServices;

namespace DiscordLogger.Performance;

/// <summary>
/// Utilitários para otimização de memória usando Span&lt;T&gt;.
/// </summary>
internal static class SpanHelpers
{
    /// <summary>
    /// Trunca uma string usando Span para evitar alocações.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string TruncateString(string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        var suffixLength = suffix.Length;
        var targetLength = maxLength - suffixLength;

        if (targetLength <= 0)
        {
            return suffix;
        }

        return string.Concat(value.AsSpan(0, targetLength), suffix);
    }

    /// <summary>
    /// Copia uma string para um buffer Span de forma eficiente.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCopyTo(string source, Span<char> destination, out int charsWritten)
    {
        if (source.AsSpan().TryCopyTo(destination))
        {
            charsWritten = source.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    /// <summary>
    /// Conta linhas em uma string usando Span para melhor performance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountLines(ReadOnlySpan<char> text)
    {
        var count = 1;
        foreach (var c in text)
        {
            if (c == '\n')
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Remove espaços em branco extras de forma eficiente.
    /// </summary>
    public static string NormalizeWhitespace(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var source = value.AsSpan();
        var builder = ObjectPool.GetStringBuilder();
        
        try
        {
            var lastWasWhitespace = false;

            foreach (var c in source)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasWhitespace)
                    {
                        builder.Append(' ');
                        lastWasWhitespace = true;
                    }
                }
                else
                {
                    builder.Append(c);
                    lastWasWhitespace = false;
                }
            }

            return builder.ToString().Trim();
        }
        finally
        {
            ObjectPool.ReturnStringBuilder(builder);
        }
    }

    /// <summary>
    /// Escapa caracteres especiais de forma eficiente.
    /// </summary>
    public static string EscapeMarkdown(ReadOnlySpan<char> text)
    {
        var builder = ObjectPool.GetStringBuilder();
        
        try
        {
            foreach (var c in text)
            {
                switch (c)
                {
                    case '*':
                    case '_':
                    case '`':
                    case '~':
                    case '|':
                    case '>':
                        builder.Append('\\');
                        break;
                }
                builder.Append(c);
            }

            return builder.ToString();
        }
        finally
        {
            ObjectPool.ReturnStringBuilder(builder);
        }
    }
}
