# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Versionamento Semântico](https://semver.org/lang/pt-BR/).

## [Unreleased]

### Planejado
- Integração com Microsoft.Extensions.Logging
- Suporte a múltiplos webhooks
- Batching de mensagens
- Filtros personalizados de log

## [1.0.0] - 2025-01-XX

### ✨ Adicionado
- Implementação completa do `DiscordLogger`
- Classe `DiscordWebhookClient` para comunicação com Discord API
- Classe `MessageFormatter` para formatação de embeds
- Suporte a todos os níveis de log (Debug, Information, Warning, Error, Critical)
- Formatação automática de exceções com stack trace
- Retry automático com backoff exponencial
- Suporte a rate limiting do Discord (HTTP 429)
- Configuração via `DiscordLoggerOptions`
- Cores personalizadas por nível de log
- Emojis nos títulos dos embeds
- Timestamps automáticos nas mensagens
- Interface `IDiscordLogger` para facilitar testes
- Implementação de `IDisposable` para gerenciamento de recursos
- Suporte a inner exceptions
- Truncamento automático de mensagens longas

### 📚 Documentação
- README completo com exemplos de uso
- Documentação XML em todas as APIs públicas
- Arquivo LICENSE (MIT)
- Guia de publicação no NuGet
- Exemplos de uso práticos
- Roadmap de desenvolvimento

### 🧪 Testes
- 22 testes unitários implementados
- Cobertura de todos os cenários principais
- Testes de validação de entrada
- Testes de gerenciamento de recursos (Dispose)

### 🛠️ Infraestrutura
- GitHub Actions para CI/CD
- Workflow de build e testes automáticos
- Workflow de publicação no NuGet
- EditorConfig para padrões de código
- Directory.Build.props para propriedades compartilhadas

### 📦 Projeto de Exemplo
- Aplicação console demonstrando todos os níveis de log
- Exemplos de uso com exceções
- Guia de configuração de webhook

[Unreleased]: https://github.com/jumoreira/DiscordLogger/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/jumoreira/DiscordLogger/releases/tag/v1.0.0
