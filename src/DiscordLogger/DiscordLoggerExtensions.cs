using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DiscordLogger;

/// <summary>
/// Extension methods para configuração do DiscordLogger.
/// </summary>
public static class DiscordLoggerExtensions
{
    /// <summary>
    /// Adiciona o DiscordLogger ao IServiceCollection.
    /// </summary>
    /// <param name="services">O IServiceCollection.</param>
    /// <param name="configure">Action para configurar as opções do DiscordLogger.</param>
    /// <returns>O IServiceCollection para encadeamento.</returns>
    public static IServiceCollection AddDiscordLogger(
        this IServiceCollection services,
        Action<DiscordLoggerOptions> configure)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        services.Configure(configure);
        services.TryAddSingleton<IDiscordLogger>(sp =>
        {
            var options = new DiscordLoggerOptions();
            configure(options);
            return new DiscordLogger(options);
        });

        return services;
    }

    /// <summary>
    /// Adiciona o DiscordLogger ao ILoggingBuilder.
    /// </summary>
    /// <param name="builder">O ILoggingBuilder.</param>
    /// <returns>O ILoggingBuilder para encadeamento.</returns>
    public static ILoggingBuilder AddDiscordLogger(this ILoggingBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, DiscordLoggerProvider>());

        return builder;
    }

    /// <summary>
    /// Adiciona o DiscordLogger ao ILoggingBuilder com configuração.
    /// </summary>
    /// <param name="builder">O ILoggingBuilder.</param>
    /// <param name="configure">Action para configurar as opções do DiscordLogger.</param>
    /// <returns>O ILoggingBuilder para encadeamento.</returns>
    public static ILoggingBuilder AddDiscordLogger(
        this ILoggingBuilder builder,
        Action<DiscordLoggerOptions> configure)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        builder.AddDiscordLogger();
        builder.Services.Configure(configure);

        return builder;
    }

    /// <summary>
    /// Adiciona o DiscordLogger ao ILoggingBuilder com webhook URL.
    /// </summary>
    /// <param name="builder">O ILoggingBuilder.</param>
    /// <param name="webhookUrl">URL do webhook do Discord.</param>
    /// <returns>O ILoggingBuilder para encadeamento.</returns>
    public static ILoggingBuilder AddDiscordLogger(
        this ILoggingBuilder builder,
        string webhookUrl)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            throw new ArgumentException("WebhookUrl não pode ser vazio.", nameof(webhookUrl));
        }

        return builder.AddDiscordLogger(options =>
        {
            options.WebhookUrl = webhookUrl;
        });
    }

    /// <summary>
    /// Adiciona o DiscordLogger ao ILoggingBuilder com configuração fluente.
    /// </summary>
    /// <param name="builder">O ILoggingBuilder.</param>
    /// <param name="webhookUrl">URL do webhook do Discord.</param>
    /// <param name="minimumLevel">Nível mínimo de log.</param>
    /// <returns>O ILoggingBuilder para encadeamento.</returns>
    public static ILoggingBuilder AddDiscordLogger(
        this ILoggingBuilder builder,
        string webhookUrl,
        LogLevel minimumLevel)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            throw new ArgumentException("WebhookUrl não pode ser vazio.", nameof(webhookUrl));
        }

        return builder.AddDiscordLogger(options =>
        {
            options.WebhookUrl = webhookUrl;
            options.MinimumLevel = minimumLevel;
        });
    }
}
