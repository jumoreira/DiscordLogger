# DiscordLogger

[![NuGet](https://img.shields.io/nuget/v/DiscordLogger.svg)](https://www.nuget.org/packages/DiscordLogger/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Uma biblioteca de logging para .NET que encaminha logs para o Discord via Webhooks.

## ?? Descrição

DiscordLogger é uma solução simples e eficiente para enviar logs da sua aplicação .NET diretamente para canais do Discord. Ideal para monitoramento em tempo real, notificações de erros e acompanhamento de eventos importantes.

## ? Características

- ?? Fácil integração com projetos .NET
- ?? Suporte a mensagens formatadas com Embeds do Discord
- ?? Configurável via código ou arquivo de configuração
- ?? Diferentes níveis de log (Debug, Info, Warning, Error, Critical)
- ? Assíncrono e performático
- ?? Suporte a .NET 8.0+

## ?? Instalação

```bash
dotnet add package DiscordLogger
```

Ou via NuGet Package Manager:

```
Install-Package DiscordLogger
```

## ?? Uso Básico

```csharp
// Exemplo de uso virá aqui após implementação
```

## ?? Configuração

```json
{
  "DiscordLogger": {
    "WebhookUrl": "sua-webhook-url-aqui",
    "MinimumLevel": "Information"
  }
}
```

## ??? Estrutura do Projeto

```
DiscordLogger/
??? src/
?   ??? DiscordLogger/          # Biblioteca principal
??? tests/
?   ??? DiscordLogger.Tests/    # Testes unitários
??? README.md
??? DiscordLogger.sln
```

## ??? Desenvolvimento

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

## ?? Roadmap

- [ ] Implementação básica do logger
- [ ] Integração com ILogger do .NET
- [ ] Suporte a Embeds personalizados
- [ ] Rate limiting e retry automático
- [ ] Configuração avançada de formatação
- [ ] Exemplos de uso

## ?? Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues e pull requests.

## ?? Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo LICENSE para detalhes.

## ?? Autor

**Elefanti**

- GitHub: [@Elefanti](https://github.com/Elefanti)

## ?? Agradecimentos

Obrigado a todos que contribuírem para este projeto!