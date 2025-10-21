# DiscordLogger

[![NuGet](https://img.shields.io/nuget/v/DiscordLogger.svg)](https://www.nuget.org/packages/DiscordLogger/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)

Uma biblioteca de logging para .NET que encaminha logs para o Discord via Webhooks, com suporte completo ao `Microsoft.Extensions.Logging`.

## 📋 Descrição

DiscordLogger é uma solução simples e eficiente para enviar logs da sua aplicação .NET diretamente para canais do Discord. Ideal para monitoramento em tempo real, notificações de erros e acompanhamento de eventos importantes.

**Duas APIs disponíveis:**
- 🎯 **API Direta (`IDiscordLogger`)**: Para uso direto e controle total
- 🔌 **Microsoft.Extensions.Logging**: Integração com `ILogger<T>` e Dependency Injection

## ✨ Características

- 🚀 Fácil integração com projetos .NET
- 🔌 **Integração completa com Microsoft.Extensions.Logging**
- 🎨 Suporte a mensagens formatadas com Embeds do Discord
- 🔧 Configurável via código ou arquivo de configuração
- 📊 Diferentes níveis de log (Debug, Info, Warning, Error, Critical)
- ⚡ Assíncrono e performático
- 🔄 Retry automático com backoff exponencial
- 🎯 Suporte a .NET 8.0+
- 🎨 Cores diferentes para cada nível de log
- 📝 Formatação automática de exceções
- 💉 Dependency Injection ready
- 🏗️ ASP.NET Core compatible

## 📦 Instalação

```bash
dotnet add package DiscordLogger
```

Ou via NuGet Package Manager:

```
Install-Package DiscordLogger
```

## 🚀 Início Rápido

### Opção 1: Microsoft.Extensions.Logging (Recomendado)

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
logger.LogError(exception, "An error occurred!");
```

### Opção 2: API Direta

```csharp
using DiscordLogger;

var options = new DiscordLoggerOptions
{
    WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL",
    MinimumLevel = LogLevel.Information
};

using var logger = new DiscordLogger.DiscordLogger(options);

await logger.LogInformationAsync("Application started!");
await logger.LogErrorAsync("Error occurred", exception);
```

## 📖 Uso Detalhado

### Microsoft.Extensions.Logging

#### Com ASP.NET Core

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

#### Com Worker Service

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddDiscordLogger(options =>
{
    options.WebhookUrl = builder.Configuration["DiscordWebhook"]!;
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
```

#### Injeção em Serviços

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
            _logger.LogError(ex, "Failed to process order {OrderId}", orderId);
        }
    }
}
```

### API Direta (IDiscordLogger)

```csharp
// Configuração
var options = new DiscordLoggerOptions
{
    WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN",
    MinimumLevel = LogLevel.Information,
    Username = "MyApp Logger",
    MaxRetryAttempts = 3,
    TimeoutSeconds = 30
};

using var logger = new DiscordLogger.DiscordLogger(options);

// Diferentes níveis de log
await logger.LogDebugAsync("Processando item #123");
await logger.LogInformationAsync("Usuário autenticado com sucesso");
await logger.LogWarningAsync("Taxa de uso da API atingiu 80%");
await logger.LogErrorAsync("Falha ao enviar email", exception);
await logger.LogCriticalAsync("Falha na conexão com banco de dados", exception);
```

### Configuração com appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "DiscordLogger": {
    "WebhookUrl": "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL",
    "Username": "Production Logger",
    "AvatarUrl": "https://example.com/avatar.png",
    "MinimumLevel": 2,
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3
  }
}
```

```csharp
// Carregar configuração
builder.Logging.AddDiscordLogger(options =>
{
    builder.Configuration.GetSection("DiscordLogger").Bind(options);
});
```

## ⚙️ Configuração

### DiscordLoggerOptions

| Propriedade | Tipo | Padrão | Descrição |
|------------|------|--------|-----------|
| `WebhookUrl` | `string` | *obrigatório* | URL do webhook do Discord |
| `Username` | `string?` | `null` | Nome de usuário nas mensagens |
| `AvatarUrl` | `string?` | `null` | URL do avatar nas mensagens |
| `MinimumLevel` | `LogLevel` | `Information` | Nível mínimo de log |
| `TimeoutSeconds` | `int` | `30` | Timeout para requisições HTTP |
| `MaxRetryAttempts` | `int` | `3` | Número máximo de tentativas |

### Extension Methods

```csharp
// IServiceCollection
services.AddDiscordLogger(options => { ... });

// ILoggingBuilder
builder.AddDiscordLogger();
builder.AddDiscordLogger(options => { ... });
builder.AddDiscordLogger("webhookUrl");
builder.AddDiscordLogger("webhookUrl", LogLevel.Warning);
```

## 🎨 Cores dos Embeds

O DiscordLogger utiliza cores diferentes para cada nível de log:

- 🔍 **Debug**: Cinza (#808080)
- ℹ️ **Information**: Azul (#0099FF)
- ⚠️ **Warning**: Amarelo (#FFCC00)
- ❌ **Error**: Laranja (#FF6600)
- 🔥 **Critical**: Vermelho (#FF0000)

## 🔧 Recursos Avançados

### Logging Estruturado

```csharp
logger.LogInformation(
    "Order {OrderId} completed in {Duration}ms with total {Amount:C}",
    order.Id,
    stopwatch.ElapsedMilliseconds,
    order.Total
);
```

### EventIds

```csharp
var eventId = new EventId(1001, "UserLogin");
logger.LogInformation(eventId, "User {Username} logged in", username);
// Discord: [YourApp] [1001:UserLogin] User admin logged in
```

### Múltiplos Providers

```csharp
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddDiscordLogger(webhookUrl);
```

### Retry Automático

O logger automaticamente tenta reenviar mensagens em caso de falha, com backoff exponencial:
- Tentativa 1: Imediato
- Tentativa 2: Aguarda 2 segundos
- Tentativa 3: Aguarda 4 segundos

### Rate Limiting

O logger respeita o rate limiting do Discord (HTTP 429) e aguarda o tempo especificado antes de retentar.

## 📚 Documentação

- [Integração com Microsoft.Extensions.Logging](docs/MicrosoftExtensionsLogging.md)
- [Exemplos Completos](examples/ConsoleExample/)

## 🏗️ Estrutura do Projeto

```
DiscordLogger/
├── src/
│   └── DiscordLogger/               # Biblioteca principal
│       ├── DiscordLogger.cs         # API direta
│       ├── IDiscordLogger.cs
│       ├── MicrosoftDiscordLogger.cs    # Adapter para ILogger
│       ├── DiscordLoggerProvider.cs     # ILoggerProvider
│       ├── DiscordLoggerExtensions.cs   # Extension methods
│       ├── DiscordLoggerOptions.cs
│       ├── DiscordWebhookClient.cs
│       ├── MessageFormatter.cs
│       └── LogLevel.cs
├── tests/
│   └── DiscordLogger.Tests/         # 69 testes
│       ├── DiscordLoggerTests.cs
│       ├── MicrosoftDiscordLoggerTests.cs
│       ├── DiscordLoggerProviderTests.cs
│       ├── DiscordLoggerExtensionsTests.cs
│       └── IntegrationTests.cs
├── examples/
│   └── ConsoleExample/              # Exemplos de uso
├── docs/
│   ├── MicrosoftExtensionsLogging.md
│   └── Fase6-Implementacao.md
├── README.md
└── DiscordLogger.sln
```

## 🧪 Testes

O projeto possui **69 testes** cobrindo:
- ✅ API direta (IDiscordLogger)
- ✅ Integração com Microsoft.Extensions.Logging
- ✅ Extension methods
- ✅ Dependency Injection
- ✅ Cenários de erro
- ✅ Integração end-to-end

```bash
dotnet test
```

## 🛠️ Desenvolvimento

### Pré-requisitos

- .NET 8.0 SDK ou superior
- Visual Studio 2022 ou VS Code

### Build

```bash
dotnet build
```

### Executar Exemplo

```bash
cd examples/ConsoleExample
dotnet run
```

### Publicar Pacote NuGet

```bash
dotnet pack -c Release
```

## 📝 Roadmap

- [x] Implementação básica do logger
- [x] Suporte a Embeds personalizados
- [x] Rate limiting e retry automático
- [x] Formatação automática de exceções
- [x] **Integração com Microsoft.Extensions.Logging**
- [x] **Dependency Injection**
- [x] **ASP.NET Core compatibility**
- [x] **69 testes unitários e de integração**
- [ ] Suporte a log scopes
- [ ] Batching de mensagens
- [ ] Múltiplos webhooks
- [ ] Health checks
- [ ] Metrics/Telemetry

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues e pull requests.

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo LICENSE para detalhes.

## 👤 Autor

**Elefanti**

- GitHub: [@jumoreira](https://github.com/jumoreira)
- Repository: [DiscordLogger](https://github.com/jumoreira/DiscordLogger)

## 🙏 Agradecimentos

Obrigado a todos que contribuírem para este projeto!

---

**⭐ Se este projeto foi útil para você, considere dar uma estrela no GitHub!**
