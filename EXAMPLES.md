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

// TODO: Implementar DiscordLogger após criação da classe
```

## Exemplo 2: Integração com Microsoft.Extensions.Logging

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DiscordLogger;

var services = new ServiceCollection();

// TODO: Adicionar extension method para configuração
// services.AddDiscordLogger(options =>
// {
//     options.WebhookUrl = "https://discord.com/api/webhooks/...";
//     options.MinimumLevel = LogLevel.Warning;
// });

var serviceProvider = services.BuildServiceProvider();
```

## Exemplo 3: Log com Exceção

```csharp
try
{
    // Seu código aqui
    throw new InvalidOperationException("Erro de exemplo");
}
catch (Exception ex)
{
    // await logger.LogErrorAsync("Ocorreu um erro na operação", ex);
}
```

## Exemplo 4: Configuração via appsettings.json

```json
{
  "DiscordLogger": {
    "WebhookUrl": "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN",
    "Username": "Production Logger",
    "MinimumLevel": "Warning",
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3
  }
}
```

```csharp
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var options = configuration.GetSection("DiscordLogger").Get<DiscordLoggerOptions>();
```

## Exemplo 5: Diferentes Níveis de Log

```csharp
// Debug - para informações de desenvolvimento
// await logger.LogDebugAsync("Iniciando processamento do item X");

// Information - para fluxo normal da aplicação
// await logger.LogInformationAsync("Usuário autenticado com sucesso");

// Warning - para situações inesperadas mas não críticas
// await logger.LogWarningAsync("Cache expirado, recarregando dados");

// Error - para erros que não impedem a execução
// await logger.LogErrorAsync("Falha ao enviar email de notificação");

// Critical - para erros que podem causar falha da aplicação
// await logger.LogCriticalAsync("Falha na conexão com o banco de dados");
```

## Cores dos Embeds por Nível de Log

O DiscordLogger utiliza cores diferentes para cada nível de log:

- **Debug**: Cinza (#808080)
- **Information**: Azul (#0099FF)
- **Warning**: Amarelo (#FFCC00)
- **Error**: Laranja (#FF6600)
- **Critical**: Vermelho (#FF0000)
