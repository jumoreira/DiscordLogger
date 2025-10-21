# Exemplos de Uso do DiscordLogger

## Exemplo 1: Uso Básico

```csharp
using DiscordLogger;

var options = new DiscordLoggerOptions
{
    WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN",
    MinimumLevel = LogLevel.Information,
    Username = "MyApp Logger"
};

using var logger = new DiscordLogger.DiscordLogger(options);

await logger.LogInformationAsync("Aplicação iniciada com sucesso!");
```

## Exemplo 2: Logs com Diferentes Níveis

```csharp
using DiscordLogger;

var options = new DiscordLoggerOptions
{
    WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN",
    MinimumLevel = LogLevel.Debug
};

using var logger = new DiscordLogger.DiscordLogger(options);

// Debug - Informações detalhadas para desenvolvimento
await logger.LogDebugAsync("Iniciando processamento do item #12345");

// Information - Fluxo normal da aplicação
await logger.LogInformationAsync("Usuário 'admin' autenticado com sucesso");

// Warning - Situações inesperadas mas não críticas
await logger.LogWarningAsync("Cache expirado. Recarregando dados do servidor...");

// Error - Erros que não impedem a execução
await logger.LogErrorAsync("Falha ao enviar email de notificação");

// Critical - Erros críticos que podem causar falha da aplicação
await logger.LogCriticalAsync("Falha na conexão com o banco de dados");
```

## Exemplo 3: Log com Exceção

```csharp
using DiscordLogger;

var options = new DiscordLoggerOptions
{
    WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN"
};

using var logger = new DiscordLogger.DiscordLogger(options);

try
{
    // Simular um erro
    throw new InvalidOperationException("Operação não permitida no estado atual");
}
catch (Exception ex)
{
    await logger.LogErrorAsync("Erro ao processar requisição do usuário", ex);
}
```

## Exemplo 4: Exceção Aninhada (Inner Exception)

```csharp
using DiscordLogger;

var options = new DiscordLoggerOptions
{
    WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN"
};

using var logger = new DiscordLogger.DiscordLogger(options);

try
{
    try
    {
        throw new ArgumentNullException("connectionString", "String de conexão não pode ser nula");
    }
    catch (Exception innerEx)
    {
        throw new InvalidOperationException("Falha ao conectar com banco de dados", innerEx);
    }
}
catch (Exception ex)
{
    await logger.LogCriticalAsync("Sistema não pode continuar: falha crítica no banco", ex);
}
```

## Exemplo 5: Configuração via appsettings.json

**appsettings.json:**
```json
{
  "DiscordLogger": {
    "WebhookUrl": "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN",
    "Username": "Production Logger",
    "AvatarUrl": "https://example.com/avatar.png",
    "MinimumLevel": "Warning",
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3
  }
}
```

**Program.cs:**
```csharp
using Microsoft.Extensions.Configuration;
using DiscordLogger;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var options = configuration.GetSection("DiscordLogger").Get<DiscordLoggerOptions>();
using var logger = new DiscordLogger.DiscordLogger(options);

await logger.LogInformationAsync("Logger configurado via appsettings.json");
```

## Exemplo 6: Uso em um Serviço Web

```csharp
using DiscordLogger;

public class ProductService
{
    private readonly IDiscordLogger _logger;

    public ProductService(IDiscordLogger logger)
    {
        _logger = logger;
    }

    public async Task<Product> GetProductAsync(int id)
    {
        try
        {
            await _logger.LogDebugAsync($"Buscando produto com ID: {id}");
            
            var product = await _repository.GetByIdAsync(id);
            
            if (product == null)
            {
                await _logger.LogWarningAsync($"Produto {id} não encontrado");
                return null;
            }

            await _logger.LogInformationAsync($"Produto {id} encontrado: {product.Name}");
            return product;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Erro ao buscar produto {id}", ex);
            throw;
        }
    }
}
```

## Exemplo 7: Logger com Personalização

```csharp
using DiscordLogger;

var options = new DiscordLoggerOptions
{
    WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN",
    Username = "🤖 MyApp Bot",
    AvatarUrl = "https://i.imgur.com/AfFp7pu.png",
    MinimumLevel = LogLevel.Information,
    TimeoutSeconds = 60,
    MaxRetryAttempts = 5
};

using var logger = new DiscordLogger.DiscordLogger(options);

await logger.LogInformationAsync("Logger personalizado está ativo!");
```

## Exemplo 8: Tratamento de Erros do Logger

```csharp
using DiscordLogger;

var options = new DiscordLoggerOptions
{
    WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN"
};

using var logger = new DiscordLogger.DiscordLogger(options);

try
{
    await logger.LogInformationAsync("Testando envio de log");
}
catch (Exception ex)
{
    // O logger já trata erros internamente e escreve no Console.Error
    // Mas você pode adicionar tratamento adicional se necessário
    Console.WriteLine($"Falha crítica no logger: {ex.Message}");
}
```

## Exemplo 9: Uso com CancellationToken

```csharp
using DiscordLogger;

var options = new DiscordLoggerOptions
{
    WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN"
};

using var logger = new DiscordLogger.DiscordLogger(options);
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

try
{
    await logger.LogInformationAsync("Operação com timeout", cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operação cancelada por timeout");
}
```

## Exemplo 10: Logger em Background Worker

```csharp
using DiscordLogger;

public class MonitoringWorker : BackgroundService
{
    private readonly IDiscordLogger _logger;

    public MonitoringWorker(IDiscordLogger logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _logger.LogInformationAsync("Worker de monitoramento iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Verificar saúde do sistema
                var healthStatus = CheckSystemHealth();

                if (!healthStatus.IsHealthy)
                {
                    await _logger.LogWarningAsync(
                        $"Sistema não está saudável: {healthStatus.Message}",
                        stoppingToken
                    );
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Erro no worker de monitoramento", ex);
            }
        }

        await _logger.LogInformationAsync("Worker de monitoramento finalizado");
    }
}
```

## 🎨 Cores dos Embeds por Nível de Log

O DiscordLogger utiliza cores diferentes para cada nível de log:

- 🔍 **Debug**: Cinza (#808080)
- ℹ️ **Information**: Azul (#0099FF)
- ⚠️ **Warning**: Amarelo (#FFCC00)
- ❌ **Error**: Laranja (#FF6600)
- 🔥 **Critical**: Vermelho (#FF0000)

## 📋 Formatação de Exceções

Quando você loga uma exceção, o Discord exibirá:

1. **Exception Type**: Tipo completo da exceção
2. **Exception Message**: Mensagem de erro
3. **Stack Trace**: Stack trace formatado como bloco de código
4. **Inner Exception**: Se houver, mostra tipo e mensagem

Exemplo de saída:
```
🔥 Critical

Erro ao conectar com o banco de dados

Exception Type: System.InvalidOperationException
Exception Message: Falha ao conectar com banco de dados
Stack Trace: [código formatado]
Inner Exception: System.ArgumentNullException: String de conexão não pode ser nula
```

## 🚀 Projeto de Exemplo Completo

Para ver um exemplo completo em funcionamento, veja o projeto em:
`examples/ConsoleExample/Program.cs`

Execute com:
```bash
cd examples/ConsoleExample
dotnet run
```

**Importante**: Não se esqueça de configurar sua URL de webhook antes de executar!
