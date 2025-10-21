# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Versionamento Semântico](https://semver.org/lang/pt-BR/).

## [1.4.0] - 2025-01-XX - 🔒 Resiliência e Confiabilidade

### ✨ Adicionado

#### 🎯 Rate Limiting Avançado
- **Token Bucket Rate Limiter**: Permite burst de requisições mantendo taxa média sustentável
- **Adaptive Rate Limiter**: Ajusta automaticamente capacidade baseado em taxa de sucesso/falha
- **Priority Queue**: Prioriza logs críticos e de erro sobre warnings e info
- Configuração flexível: capacidade, refill rate, intervalos, min/max capacity
- Classes: `IRateLimiter`, `TokenBucketRateLimiter`, `AdaptiveRateLimiter`, `RateLimitingOptions`

#### 🔌 Circuit Breaker
- **Circuit Breaker Pattern**: Estados Closed/Open/HalfOpen para proteção contra falhas
- **Health Monitoring**: Monitora tempo de resposta e detecta degradação do serviço
- **Auto Recovery**: Recuperação automática quando serviço volta (HalfOpen → Closed)
- **Fallback Strategies**: 4 estratégias (Discard, Queue, File, AlternativeWebhook)
- **Estatísticas em tempo real**: Taxa de sucesso, tempo médio de resposta, contadores
- Classes: `ICircuitBreaker`, `CircuitBreaker`, `CircuitBreakerOptions`, `CircuitBreakerStatistics`

#### ⏱️ Backoff Strategies
- **Exponential Backoff**: Crescimento exponencial com jitter (1s → 2s → 4s → 8s...)
- **Linear Backoff**: Incremento linear previsível (2s → 4s → 6s → 8s...)
- **Fibonacci Backoff**: Sequência Fibonacci balanceada (1s → 1s → 2s → 3s → 5s...)
- **Jitter Support**: Adiciona ±30% aleatoriedade para evitar thundering herd problem
- Classes: `IBackoffStrategy`, `ExponentialBackoffStrategy`, `LinearBackoffStrategy`, `FibonacciBackoffStrategy`, `BackoffOptions`

#### 💾 Persistência de Falhas
- **Dead Letter Queue (DLQ)**: Armazena mensagens que falharam múltiplas vezes (file-based JSON)
- **File Fallback**: Salvamento automático em arquivo local com rotação por tamanho
- **Retry Queue**: Fila dedicada para reenvio com backoff progressivo
- **Auto Recovery Service**: Tentativa automática de reenvio periódico em background
- **Zero perda de dados**: Garante que nenhuma mensagem seja perdida
- Classes: `IDeadLetterQueue`, `FileBasedDeadLetterQueue`, `FailedLogMessage`, `IFileFallback`, `FileFallback`, `RecoveryService`, `PersistenceOptions`

#### 🚦 Múltiplos Webhooks e Roteamento
- **Roteamento por Nível de Log**: Webhooks diferentes para Critical/Error/Warning/Info
- **Roteamento por Categoria**: Suporte a wildcards (*.Service, *Controller, MyApp.Business.*)
- **Pattern Matching**: Regex com wildcards (* e ?) para categorias complexas
- **API Fluente**: Builder pattern para configuração intuitiva e type-safe
- **Atributo [DiscordWebhook]**: Roteamento declarativo via atributos em classes
- **Load Balancing**: 4 estratégias (Priority, Round-Robin, Random, Broadcast)
- **Estatísticas de Roteamento**: Total roteado, contadores por rota, uso de fallback
- Classes: `IWebhookRouter`, `WebhookRouter`, `WebhookRoute`, `MultiWebhookOptions`, `RoutingStatistics`, `LoadBalancingStrategy`, `DiscordWebhookAttribute`, `WebhookRoutingBuilder`

#### 🔗 Integração e Orquestração
- **ResilientWebhookClient**: Orquestra todos os componentes de resiliência transparentemente
- **ResilienceOptions**: Configuração consolidada de todos os recursos
- Integração perfeita com `DiscordLoggerOptions` via propriedade `Resilience`

### 🧪 Testes
- 34 novos testes unitários implementados
- **RateLimiterTests.cs**: 7 testes (burst, refill, reset, adaptive)
- **CircuitBreakerTests.cs**: 7 testes (estados, transições, estatísticas)
- **WebhookRouterTests.cs**: 9 testes (roteamento, wildcards, load balancing)
- **BackoffStrategyTests.cs**: 11 testes (estratégias, limites, jitter)

### 📚 Documentação
- **Guia Completo**: `docs/PHASE_9_RESILIENCE.md` com exemplos e troubleshooting
- **Resumo de Implementação**: `docs/PHASE_9_IMPLEMENTATION_SUMMARY.md`
- **Exemplos Práticos**: `examples/ConsoleExample/ResilienceExamples.cs` (10+ exemplos)
- XML comments completos em todos os membros públicos

### ⚡ Performance
- Todos os componentes são thread-safe e assíncronos
- Operações não-bloqueantes
- Memory-efficient com pooling quando aplicável
- Overhead mínimo quando resiliência está desabilitada

### 🎯 Casos de Uso

#### Zero Perda de Dados
```csharp
options.Resilience.CircuitBreaker.FallbackStrategy = FallbackStrategy.Queue;
options.Resilience.Persistence.EnableDeadLetterQueue = true;
options.Resilience.Persistence.EnableAutoRecovery = true;
```

#### Alta Disponibilidade
```csharp
options.Resilience.CircuitBreaker.FallbackStrategy = FallbackStrategy.AlternativeWebhook;
options.Resilience.CircuitBreaker.FallbackWebhookUrl = "BACKUP_WEBHOOK";
```

#### Roteamento Inteligente
```csharp
options.ConfigureWebhookRouting(routing =>
{
    routing.AddRoute("CRITICAL_WEBHOOK").ForCriticalOnly().WithPriority(100);
    routing.AddRoute("API_WEBHOOK").ForControllers().WithPriority(50);
    routing.AddRoute("SERVICES_WEBHOOK").ForServices().WithPriority(30);
});
```

### 📦 Arquivos Adicionados
- **29 arquivos criados/modificados**
- **~3,500+ linhas de código**
- **30+ classes e interfaces**
- **4 enums**

### ⚠️ Breaking Changes
**Nenhuma!** Todos os recursos são opt-in e totalmente retrocompatíveis.

### 🔧 Compatibilidade
- ✅ Compatível com v1.0.0, v1.1.0, v1.2.0 e v1.3.0
- ✅ Todos os recursos são opt-in (desabilitados por padrão)
- ✅ API retrocompatível

## [1.3.0] - 2025-01-XX - ⚡ Performance & Escalabilidade

### ✨ Adicionado
- Buffering inteligente para alto volume
- Fila de prioridade para logs críticos
- HttpClient pooling
- Anexo de arquivos para mensagens grandes
- Benchmark e métricas de performance

## [1.2.0] - 2025-01-XX - 🎨 Recursos Avançados

### ✨ Adicionado

#### 🎯 Log Scopes Completos
- Suporte completo a `BeginScope` do Microsoft.Extensions.Logging
- Scopes aninhados com hierarquia preservada
- Formatação automática de estados de scope
- Classes: `DiscordLoggerScope`, `DiscordScopeProvider`

#### 📦 Batching de Mensagens
- Sistema de filas usando `Channel<T>` thread-safe
- Flush automático por tempo ou quantidade
- Background processing assíncrono
- Redução de até 90% nas chamadas de API
- Classes: `BatchingOptions`, `QueuedLogMessage`, `LogBatchProcessor`

#### 🔍 Filtros Avançados
- Filtros por categoria com suporte a wildcards (`*`)
- Filtros por EventId (whitelist/blacklist)
- Filtros por padrão de mensagem (regex)
- Filtros por nível de log
- Classes: `LogFilterOptions`, `LogFilter`

#### 🎨 Formatadores Personalizados
- Interface pública `IMessageFormatter`
- `DefaultMessageFormatter` com formatação melhorada
- `TemplateMessageFormatter` com placeholders personalizáveis
- Placeholders: `{level}`, `{emoji}`, `{message}`, `{timestamp}`, `{date}`, `{time}`, `{exception}`
- Suporte a formatação de batches

### 🔧 Alterado
- `DiscordLoggerOptions`: Novas propriedades `EnableScopes`, `Batching`, `Filters`, `MessageFormatter`
- `IDiscordLogger`: Parâmetro `scopeInfo` adicionado ao `LogAsync`
- `MicrosoftDiscordLogger`: Integração com scopes e filtros
- `DiscordLogger`: Suporte a batching e formatadores personalizados
- `DiscordLoggerProvider`: Inicialização de scope provider e filtros
- Models: Classes agora são públicas para formatadores customizados

### 📝 Documentação
- Guia completo: `docs/v1.2.0-ADVANCED-FEATURES.md`
- Exemplos atualizados no `Program.cs`
- 55 novos testes unitários

### ⚡ Compatibilidade
- ✅ Totalmente compatível com v1.0.0 e v1.1.0
- ✅ Todos os recursos são opt-in (desabilitados por padrão)
- ✅ Sem breaking changes

## [Unreleased]

### Planejado para v1.5.0
- Métricas e Observabilidade
- Integração com OpenTelemetry
- Dashboard de monitoramento
- Alertas inteligentes

## [1.0.0] - 2025-01-XX - 🚀 Versão Inicial

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

[Unreleased]: https://github.com/jumoreira/DiscordLogger/compare/v1.4.0...HEAD
[1.4.0]: https://github.com/jumoreira/DiscordLogger/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/jumoreira/DiscordLogger/compare/v1.2.0...v1.3.0
[1.2.0]: https://github.com/jumoreira/DiscordLogger/compare/v1.0.0...v1.2.0
[1.0.0]: https://github.com/jumoreira/DiscordLogger/releases/tag/v1.0.0
