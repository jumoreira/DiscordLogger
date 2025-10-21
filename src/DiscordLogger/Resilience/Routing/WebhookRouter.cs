using System.Text.RegularExpressions;

namespace DiscordLogger.Resilience.Routing;

/// <summary>
/// Interface para roteamento de webhooks.
/// </summary>
public interface IWebhookRouter
{
    /// <summary>
    /// Resolve qual(is) webhook(s) usar para uma mensagem de log.
    /// </summary>
    /// <param name="logLevel">Nível de log.</param>
    /// <param name="category">Categoria/nome do logger.</param>
    /// <returns>Lista de webhooks URLs para usar.</returns>
    IList<string> ResolveWebhooks(LogLevel logLevel, string? category);

    /// <summary>
    /// Obtém estatísticas de roteamento.
    /// </summary>
    RoutingStatistics GetStatistics();
}

/// <summary>
/// Estatísticas de roteamento.
/// </summary>
public class RoutingStatistics
{
    /// <summary>
    /// Total de requisições roteadas.
    /// </summary>
    public long TotalRouted { get; set; }

    /// <summary>
    /// Requisições por rota.
    /// </summary>
    public Dictionary<string, long> RouteCounts { get; set; } = new();

    /// <summary>
    /// Requisições que usaram fallback.
    /// </summary>
    public long FallbackCount { get; set; }
}

/// <summary>
/// Implementação do roteador de webhooks.
/// </summary>
public sealed class WebhookRouter : IWebhookRouter
{
    private readonly MultiWebhookOptions _options;
    private readonly List<WebhookRoute> _routes;
    private readonly object _lock = new();
    private int _roundRobinIndex = 0;
    private readonly Random _random = new();
    private long _totalRouted = 0;
    private long _fallbackCount = 0;
    private readonly Dictionary<string, long> _routeCounts = new();

    public WebhookRouter(MultiWebhookOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        
        // Ordena rotas por prioridade (maior primeiro)
        _routes = options.Routes
            .OrderByDescending(r => r.Priority)
            .ToList();
    }

    /// <inheritdoc />
    public IList<string> ResolveWebhooks(LogLevel logLevel, string? category)
    {
        lock (_lock)
        {
            _totalRouted++;

            if (!_options.Enabled || _routes.Count == 0)
            {
                // Se não está habilitado ou não há rotas, usa o webhook padrão
                return !string.IsNullOrEmpty(_options.DefaultWebhookUrl)
                    ? new List<string> { _options.DefaultWebhookUrl }
                    : new List<string>();
            }

            // Filtra rotas que correspondem
            var matchingRoutes = _routes.Where(r => MatchesRoute(r, logLevel, category)).ToList();

            if (matchingRoutes.Count == 0)
            {
                // Nenhuma rota correspondeu - usa fallback
                _fallbackCount++;
                var fallbackRoute = _routes.FirstOrDefault(r => r.IsFallback);
                
                if (fallbackRoute != null)
                {
                    TrackRoute(fallbackRoute.WebhookUrl);
                    return new List<string> { fallbackRoute.WebhookUrl };
                }

                if (!string.IsNullOrEmpty(_options.DefaultWebhookUrl))
                {
                    TrackRoute(_options.DefaultWebhookUrl);
                    return new List<string> { _options.DefaultWebhookUrl };
                }

                return new List<string>();
            }

            // Aplica estratégia de load balancing
            var selectedWebhooks = ApplyLoadBalancing(matchingRoutes);
            
            foreach (var webhook in selectedWebhooks)
            {
                TrackRoute(webhook);
            }

            return selectedWebhooks;
        }
    }

    /// <inheritdoc />
    public RoutingStatistics GetStatistics()
    {
        lock (_lock)
        {
            return new RoutingStatistics
            {
                TotalRouted = _totalRouted,
                RouteCounts = new Dictionary<string, long>(_routeCounts),
                FallbackCount = _fallbackCount
            };
        }
    }

    private bool MatchesRoute(WebhookRoute route, LogLevel logLevel, string? category)
    {
        // Verifica nível de log
        if (_options.EnableLogLevelRouting && route.LogLevels != null && route.LogLevels.Count > 0)
        {
            if (!route.LogLevels.Contains(logLevel))
            {
                return false;
            }
        }

        // Verifica categoria
        if (_options.EnableCategoryRouting && route.CategoryPatterns != null && route.CategoryPatterns.Count > 0)
        {
            if (string.IsNullOrEmpty(category))
            {
                return false;
            }

            var matched = false;
            foreach (var pattern in route.CategoryPatterns)
            {
                if (MatchesPattern(category, pattern))
                {
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                return false;
            }
        }

        return true;
    }

    private bool MatchesPattern(string category, string pattern)
    {
        // Converte wildcard pattern para regex
        // * = qualquer coisa
        // ? = um caractere
        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";

        return Regex.IsMatch(category, regexPattern, RegexOptions.IgnoreCase);
    }

    private IList<string> ApplyLoadBalancing(List<WebhookRoute> matchingRoutes)
    {
        switch (_options.LoadBalancing)
        {
            case LoadBalancingStrategy.Priority:
                // Já está ordenado por prioridade, pega o primeiro
                return new List<string> { matchingRoutes[0].WebhookUrl };

            case LoadBalancingStrategy.RoundRobin:
                var index = _roundRobinIndex % matchingRoutes.Count;
                _roundRobinIndex++;
                return new List<string> { matchingRoutes[index].WebhookUrl };

            case LoadBalancingStrategy.Random:
                var randomIndex = _random.Next(matchingRoutes.Count);
                return new List<string> { matchingRoutes[randomIndex].WebhookUrl };

            case LoadBalancingStrategy.Broadcast:
                return matchingRoutes.Select(r => r.WebhookUrl).ToList();

            default:
                return new List<string> { matchingRoutes[0].WebhookUrl };
        }
    }

    private void TrackRoute(string webhookUrl)
    {
        if (!_routeCounts.ContainsKey(webhookUrl))
        {
            _routeCounts[webhookUrl] = 0;
        }
        _routeCounts[webhookUrl]++;
    }
}
