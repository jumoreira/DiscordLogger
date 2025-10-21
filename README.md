# DiscordLogger

[![NuGet](https://img.shields.io/nuget/v/DiscordLogger.svg)](https://www.nuget.org/packages/DiscordLogger/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Uma biblioteca de logging para .NET que encaminha logs para o Discord via Webhooks.

## 📋 Descrição

DiscordLogger é uma solução simples e eficiente para enviar logs da sua aplicação .NET diretamente para canais do Discord. Ideal para monitoramento em tempo real, notificações de erros e acompanhamento de eventos importantes.

## ✨ Características

- 🚀 Fácil integração com projetos .NET
- 🎨 Suporte a mensagens formatadas com Embeds do Discord
- 🔧 Configurável via código ou arquivo de configuração
- 📊 Diferentes níveis de log (Debug, Info, Warning, Error, Critical)
- ⚡ Assíncrono e performático
- 🔄 Retry automático com backoff exponencial
- 🎯 Suporte a .NET 8.0+
- 🎨 Cores diferentes para cada nível de log
- 📝 Formatação automática de exceções

## 📦 Instalação

```bash
dotnet add package DiscordLogger
```

Ou via NuGet Package Manager:

```
Install-Package DiscordLogger
```

## 🚀 Uso Básico

### Configuração Simples

```csharp
using DiscordLogger;

var options = new DiscordLoggerOptions
{
    WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN",
    MinimumLevel = LogLevel.Information,
    Username = "MyApp Logger",
    MaxRetryAttempts = 3,
    TimeoutSeconds = 30
};

using var logger = new DiscordLogger(options);

// Logs simples
await logger.LogInformationAsync("Aplicação iniciada com sucesso!");
await logger.LogWarningAsync("Cache expirado, recarregando dados...");

// Logs com exceção
try
{
    // Seu código aqui
    throw new InvalidOperationException("Erro de exemplo");
}
catch (Exception ex)
{
    await logger.LogErrorAsync("Ocorreu um erro na operação", ex);
}
```

### Diferentes Níveis de Log

```csharp
// Debug - Informações detalhadas para desenvolvimento
await logger.LogDebugAsync("Processando item #123");

// Information - Fluxo normal da aplicação
await logger.LogInformationAsync("Usuário autenticado com sucesso");

// Warning - Situações inesperadas mas não críticas
await logger.LogWarningAsync("Taxa de uso da API atingiu 80%");

// Error - Erros que não impedem a execução
await logger.LogErrorAsync("Falha ao enviar email de notificação", exception);

// Critical - Erros críticos que podem causar falha total
await logger.LogCriticalAsync("Falha na conexão com o banco de dados", exception);
```

### Configuração com appsettings.json

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

```csharp
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var options = configuration.GetSection("DiscordLogger").Get<DiscordLoggerOptions>();
using var logger = new DiscordLogger(options);
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

## 🎨 Cores dos Embeds

O DiscordLogger utiliza cores diferentes para cada nível de log:

- 🔍 **Debug**: Cinza (#808080)
- ℹ️ **Information**: Azul (#0099FF)
- ⚠️ **Warning**: Amarelo (#FFCC00)
- ❌ **Error**: Laranja (#FF6600)
- 🔥 **Critical**: Vermelho (#FF0000)

## 🔧 Recursos Avançados

### Retry Automático

O logger automaticamente tenta reenviar mensagens em caso de falha, com backoff exponencial:
- Tentativa 1: Imediato
- Tentativa 2: Aguarda 2 segundos
- Tentativa 3: Aguarda 4 segundos

### Rate Limiting

O logger respeita o rate limiting do Discord (HTTP 429) e aguarda o tempo especificado antes de retentar.

### Formatação de Exceções

Exceções são automaticamente formatadas com:
- Tipo da exceção
- Mensagem
- Stack trace (formatado como bloco de código)
- Inner exception (se houver)

## 🏗️ Estrutura do Projeto

```
DiscordLogger/
├── src/
│   └── DiscordLogger/          # Biblioteca principal
├── tests/
│   └── DiscordLogger.Tests/    # Testes unitários
├── README.md
└── DiscordLogger.sln
```

## 🛠️ Desenvolvimento

### Pré-requisitos

- .NET 8.0 SDK ou superior
- Visual Studio 2022 ou VS Code

### Build

```bash
dotnet build
```

### Testes

```bash
dotnet test
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
- [ ] Integração com ILogger do .NET
- [ ] Configuração avançada de formatação
- [ ] Suporte a múltiplos webhooks
- [ ] Batching de mensagens

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

- GitHub: [@Elefanti](https://github.com/Elefanti)
- Repository: [DiscordLogger](https://github.com/jumoreira/DiscordLogger)

## 🙏 Agradecimentos

Obrigado a todos que contribuírem para este projeto!
