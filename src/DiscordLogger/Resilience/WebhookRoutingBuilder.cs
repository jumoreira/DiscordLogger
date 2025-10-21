using DiscordLogger.Resilience.Routing;

namespace DiscordLogger.Resilience;

/// <summary>
/// Builder fluente para configuração de rotas de webhook.
/// </summary>
public class WebhookRoutingBuilder
{
    private readonly MultiWebhookOptions _options;

    public WebhookRoutingBuilder(MultiWebhookOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Adiciona uma rota de webhook.
    /// </summary>
    public WebhookRouteBuilder AddRoute(string webhookUrl, string? name = null)
    {
        var route = new WebhookRoute
        {
            WebhookUrl = webhookUrl,
            Name = name
        };

        _options.Routes.Add(route);
        return new WebhookRouteBuilder(route);
    }

    /// <summary>
    /// Define o webhook padrão (fallback).
    /// </summary>
    public WebhookRoutingBuilder WithDefaultWebhook(string webhookUrl)
    {
        _options.DefaultWebhookUrl = webhookUrl;
        return this;
    }

    /// <summary>
    /// Define a estratégia de load balancing.
    /// </summary>
    public WebhookRoutingBuilder WithLoadBalancing(LoadBalancingStrategy strategy)
    {
        _options.LoadBalancing = strategy;
        return this;
    }

    /// <summary>
    /// Habilita broadcast (envio para múltiplas rotas).
    /// </summary>
    public WebhookRoutingBuilder EnableBroadcast()
    {
        _options.EnableBroadcast = true;
        _options.LoadBalancing = LoadBalancingStrategy.Broadcast;
        return this;
    }
}

/// <summary>
/// Builder fluente para configuração de uma rota individual.
/// </summary>
public class WebhookRouteBuilder
{
    private readonly WebhookRoute _route;

    public WebhookRouteBuilder(WebhookRoute route)
    {
        _route = route ?? throw new ArgumentNullException(nameof(route));
    }

    /// <summary>
    /// Define níveis de log para esta rota.
    /// </summary>
    public WebhookRouteBuilder ForLogLevels(params LogLevel[] logLevels)
    {
        _route.LogLevels = new HashSet<LogLevel>(logLevels);
        return this;
    }

    /// <summary>
    /// Define que esta rota é apenas para logs críticos.
    /// </summary>
    public WebhookRouteBuilder ForCriticalOnly()
    {
        return ForLogLevels(LogLevel.Critical);
    }

    /// <summary>
    /// Define que esta rota é apenas para erros e críticos.
    /// </summary>
    public WebhookRouteBuilder ForErrorsOnly()
    {
        return ForLogLevels(LogLevel.Error, LogLevel.Critical);
    }

    /// <summary>
    /// Define padrões de categoria para esta rota.
    /// </summary>
    public WebhookRouteBuilder ForCategories(params string[] patterns)
    {
        _route.CategoryPatterns = new HashSet<string>(patterns);
        return this;
    }

    /// <summary>
    /// Define que esta rota é para controllers.
    /// </summary>
    public WebhookRouteBuilder ForControllers()
    {
        return ForCategories("*Controller", "*Controllers.*");
    }

    /// <summary>
    /// Define que esta rota é para services.
    /// </summary>
    public WebhookRouteBuilder ForServices()
    {
        return ForCategories("*.Service", "*.Services.*", "*Service");
    }

    /// <summary>
    /// Define que esta rota é para repositories.
    /// </summary>
    public WebhookRouteBuilder ForRepositories()
    {
        return ForCategories("*.Repository", "*.Repositories.*", "*Repository");
    }

    /// <summary>
    /// Define prioridade da rota (maior = mais prioritário).
    /// </summary>
    public WebhookRouteBuilder WithPriority(int priority)
    {
        _route.Priority = priority;
        return this;
    }

    /// <summary>
    /// Define esta rota como fallback.
    /// </summary>
    public WebhookRouteBuilder AsFallback()
    {
        _route.IsFallback = true;
        return this;
    }

    /// <summary>
    /// Define username customizado para esta rota.
    /// </summary>
    public WebhookRouteBuilder WithUsername(string username)
    {
        _route.Username = username;
        return this;
    }

    /// <summary>
    /// Define avatar customizado para esta rota.
    /// </summary>
    public WebhookRouteBuilder WithAvatar(string avatarUrl)
    {
        _route.AvatarUrl = avatarUrl;
        return this;
    }
}

/// <summary>
/// Extensões para facilitar configuração fluente.
/// </summary>
public static class WebhookRoutingExtensions
{
    /// <summary>
    /// Configura roteamento de webhooks de forma fluente.
    /// </summary>
    public static DiscordLoggerOptions ConfigureWebhookRouting(
        this DiscordLoggerOptions options,
        Action<WebhookRoutingBuilder> configure)
    {
        options.Resilience.MultiWebhook.Enabled = true;
        var builder = new WebhookRoutingBuilder(options.Resilience.MultiWebhook);
        configure(builder);
        return options;
    }
}
