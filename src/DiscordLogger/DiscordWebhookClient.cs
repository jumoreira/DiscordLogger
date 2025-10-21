using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DiscordLogger.Models;
using DiscordLogger.Performance;

namespace DiscordLogger;

/// <summary>
/// Cliente HTTP para comunicação com a API de webhooks do Discord com suporte a pooling e anexos.
/// </summary>
internal class DiscordWebhookClient : IDisposable
{
    private readonly HttpClientManager? _httpClientManager;
    private readonly HttpClient? _dedicatedClient;
    private readonly string _webhookUrl;
    private readonly int _maxRetryAttempts;
    private readonly bool _usePooling;
    private readonly JsonSerializerOptions _jsonOptions;

    public DiscordWebhookClient(DiscordLoggerOptions options)
    {
        _webhookUrl = options.WebhookUrl;
        _maxRetryAttempts = options.MaxRetryAttempts;
        _usePooling = options.EnableHttpClientPooling;

        if (_usePooling)
        {
            _httpClientManager = HttpClientManager.GetInstance(true, options.TimeoutSeconds);
        }
        else
        {
            _dedicatedClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
            };
        }

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
            HttpClient? client = null;

            try
            {
                client = GetHttpClient();

                var response = await client.PostAsJsonAsync(
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
            finally
            {
                ReturnHttpClient(client);
            }
        }

        throw new InvalidOperationException(
            $"Falha ao enviar mensagem para o Discord após {_maxRetryAttempts} tentativas.",
            lastException
        );
    }

    /// <summary>
    /// Envia uma mensagem com anexo de arquivo para o webhook do Discord.
    /// </summary>
    public async Task SendMessageWithAttachmentAsync(
        DiscordWebhookMessage message,
        string fileName,
        byte[] fileContent,
        CancellationToken cancellationToken = default)
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
            HttpClient? client = null;

            try
            {
                client = GetHttpClient();

                using var content = new MultipartFormDataContent();

                // Adiciona o payload JSON
                var json = JsonSerializer.Serialize(message, _jsonOptions);
                content.Add(new StringContent(json, Encoding.UTF8, "application/json"), "payload_json");

                // Adiciona o arquivo
                var fileStreamContent = new ByteArrayContent(fileContent);
                fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
                content.Add(fileStreamContent, "file", fileName);

                var response = await client.PostAsync(_webhookUrl, content, cancellationToken);

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
            finally
            {
                ReturnHttpClient(client);
            }
        }

        throw new InvalidOperationException(
            $"Falha ao enviar mensagem com anexo para o Discord após {_maxRetryAttempts} tentativas.",
            lastException
        );
    }

    private HttpClient GetHttpClient()
    {
        if (_usePooling && _httpClientManager != null)
        {
            return _httpClientManager.GetClient();
        }

        return _dedicatedClient!;
    }

    private void ReturnHttpClient(HttpClient? client)
    {
        if (_usePooling && _httpClientManager != null && client != null && client != _dedicatedClient)
        {
            _httpClientManager.ReturnClient(client);
        }
    }

    public void Dispose()
    {
        _dedicatedClient?.Dispose();
        _httpClientManager?.Dispose();
    }
}
