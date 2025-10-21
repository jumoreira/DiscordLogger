# Integração com Microsoft.Extensions.Logging

Esta documentação descreve como usar o DiscordLogger com o sistema de logging padrão do .NET via `Microsoft.Extensions.Logging`.

## Visão Geral

O DiscordLogger oferece duas APIs:

1. **API Direta (`IDiscordLogger`)**: Para uso direto e controle total
2. **Integração com Microsoft.Extensions.Logging**: Para uso com `ILogger<T>` e Dependency Injection

## Instalação

```bash
dotnet add package DiscordLogger
```

## Uso Básico com ILogger<T>

### 1. Configuração Simples

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DiscordLogger;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddDiscordLogger("https://discord.com/api/webhooks/YOUR_WEBHOOK_URL");
});

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Hello from Discord!");
```

### 2. Configuração Completa

```csharp
services.AddLogging(builder =>
{
    builder.AddDiscordLogger(options =>
    {
        options.WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL";
        options.Username = "MyApp Bot";
        options.MinimumLevel = LogLevel.Warning;
        options.TimeoutSeconds = 30;
        options.MaxRetryAttempts = 3;
    });
});
```

### 3. Com ASP.NET Core

```csharp
var builder = WebApplication.CreateBuilder(args);

// Adicionar Discord Logger
builder.Logging.AddDiscordLogger(options =>
{
    options.WebhookUrl = builder.Configuration["DiscordLogger:WebhookUrl"]!;
    options.MinimumLevel = LogLevel.Error;
});

var app = builder.Build();
```

### 4. Com appsettings.json

**appsettings.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "DiscordLogger": {
    "WebhookUrl": "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL",
    "Username": "MyApp",
    "MinimumLevel": 2,
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3
  }
}
```

**Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddDiscordLogger(options =>
{
    builder.Configuration.GetSection("DiscordLogger").Bind(options);
});
```

## Uso com Dependency Injection

### Injeção em Serviços

```csharp
public class PaymentService
{
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessPayment(int orderId)
    {
        _logger.LogInformation("Processing payment for order {OrderId}", orderId);
        
        try
        {
            // Process payment...
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment for order {OrderId}", orderId);
        }
    }
}
```

### Registrar e Usar

```csharp
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddDiscordLogger(webhookUrl));
services.AddTransient<PaymentService>();

var serviceProvider = services.BuildServiceProvider();
var paymentService = serviceProvider.GetRequiredService<PaymentService>();

await paymentService.ProcessPayment(1234);
```

## Recursos Avançados

### 1. Múltiplos Providers

Você pode usar Discord Logger junto com outros providers:

```csharp
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddDiscordLogger(webhookUrl);
```

### 2. Filtros de Log Level

```csharp
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
builder.Logging.AddDiscordLogger(options =>
{
    options.WebhookUrl = webhookUrl;
    options.MinimumLevel = DiscordLogger.LogLevel.Error; // Apenas Error e Critical
});
```

### 3. EventIds

```csharp
var eventId = new EventId(1001, "UserLogin");
logger.LogInformation(eventId, "User {Username} logged in", "admin");
// Resultado no Discord: [YourApp] [1001:UserLogin] User admin logged in
```

### 4. Logging Estruturado

```csharp
logger.LogInformation(
    "Order {OrderId} completed in {Duration}ms with total {Amount:C}",
    order.Id,
    stopwatch.ElapsedMilliseconds,
    order.Total
);
```

### 5. LoggerFactory

```csharp
var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
var categoryLogger = loggerFactory.CreateLogger("CustomCategory");

categoryLogger.LogInformation("Message from custom category");
```

## Mapeamento de Log Levels

| Microsoft.Extensions.Logging | DiscordLogger | Discord Color |
|------------------------------|---------------|---------------|
| `Trace` | `Debug` | Cinza |
| `Debug` | `Debug` | Cinza |
| `Information` | `Information` | Azul |
| `Warning` | `Warning` | Amarelo |
| `Error` | `Error` | Laranja |
| `Critical` | `Critical` | Vermelho |
| `None` | (ignorado) | - |

## Comparação: IDiscordLogger vs ILogger<T>

### Use `IDiscordLogger` quando:
- Precisar de controle total sobre o envio de logs
- Quiser usar `async/await` explicitamente
- Não estiver usando Dependency Injection
- Precisar de uma API simples e direta

```csharp
var logger = new DiscordLogger.DiscordLogger(options);
await logger.LogErrorAsync("Error occurred", exception);
```

### Use `ILogger<T>` quando:
- Estiver usando ASP.NET Core ou Dependency Injection
- Quiser integração com outros logging providers
- Precisar de logging estruturado
- Quiser usar padrões do .NET

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;
    
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }
    
    public void DoWork()
    {
        _logger.LogError(exception, "Error occurred");
    }
}
```

## Performance

- O `MicrosoftDiscordLogger` usa `Task.Run` para não bloquear threads
- Logs são enviados de forma assíncrona em background
- Falhas no envio não quebram a aplicação (fail silently)

## Limitações

- **Scopes**: Atualmente não há suporte para `BeginScope`. Retorna `null`.
- **Async**: Logs são enviados de forma fire-and-forget para não bloquear

## Exemplos Completos

Veja o projeto `ConsoleExample` para exemplos completos de:
- Uso com IDiscordLogger
- Uso com ILogger<T>
- Configuração com Dependency Injection
- Logging estruturado
- Tratamento de exceções

## Troubleshooting

### Logs não aparecem no Discord

1. Verifique se o `MinimumLevel` está configurado corretamente
2. Confirme se o webhook URL está correto
3. Verifique se o log level está acima do mínimo configurado

### Performance lenta

1. Considere aumentar `TimeoutSeconds` se estiver em rede lenta
2. Ajuste `MaxRetryAttempts` para falhar mais rápido
3. Use `MinimumLevel` mais alto para reduzir volume de logs

## Migração de IDiscordLogger para ILogger<T>

**Antes:**
```csharp
var logger = new DiscordLogger.DiscordLogger(options);
await logger.LogErrorAsync("Error", exception);
```

**Depois:**
```csharp
services.AddLogging(b => b.AddDiscordLogger(options));
var logger = serviceProvider.GetRequiredService<ILogger<MyClass>>();
logger.LogError(exception, "Error");
```

## Próximos Passos

- Veja [README.md](../../README.md) para documentação geral
- Explore [ConsoleExample](../../examples/ConsoleExample) para exemplos práticos
- Consulte os testes em [DiscordLogger.Tests](../../tests/DiscordLogger.Tests) para mais casos de uso
