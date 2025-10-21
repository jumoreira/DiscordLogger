using System.Net.Http.Json;
using System.Text.Json;
using DiscordLogger.Models;

namespace DiscordLogger;

/// <summary>
/// Cliente HTTP para comunicação com a API de webhooks do Discord.
/// </summary>
internal class DiscordWebhookClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;
    private readonly int _maxRetryAttempts;
    private readonly JsonSerializerOptions _jsonOptions;

    public DiscordWebhookClient(DiscordLoggerOptions options)
    {
        _webhookUrl = options.WebhookUrl;
        _maxRetryAttempts = options.MaxRetryAttempts;
        
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Envia uma mensagem para o webhook do Discord.
    /// </summary>
    public async Task SendMessageAsync(DiscordWebhookMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_webhookUrl))
        {
            throw new InvalidOperationException("WebhookUrl não foi configurado.");
        }

        var attempt = 0;
        Exception? lastException = null;

        while (attempt < _maxRetryAttempts)
        {
            attempt++;

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    _webhookUrl,
                    message,
                    _jsonOptions,
                    cancellationToken
                );

                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                // Se for rate limit (429), aguarda antes de tentar novamente
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(5);
                    await Task.Delay(retryAfter, cancellationToken);
                    continue;
                }

                // Para outros erros HTTP, tenta novamente com backoff exponencial
                lastException = new HttpRequestException(
                    $"Discord API retornou status code {response.StatusCode}"
                );

                if (attempt < _maxRetryAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
                }
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                
                if (attempt < _maxRetryAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                lastException = new TimeoutException("Timeout ao enviar mensagem para o Discord.", ex);
                
                if (attempt < _maxRetryAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
                }
            }
        }

        throw new InvalidOperationException(
            $"Falha ao enviar mensagem para o Discord após {_maxRetryAttempts} tentativas.",
            lastException
        );
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
