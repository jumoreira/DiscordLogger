# 🔒 Fase 9: Resiliência e Confiabilidade (v1.4.0)

## 📋 Visão Geral

A Fase 9 introduz recursos avançados de **resiliência e confiabilidade** para garantir entrega de logs sem perda de dados, recuperação automática de falhas e roteamento inteligente por classe/categoria.

## 🎯 Recursos Implementados

### ✅ 1. Rate Limiting Avançado

#### Token Bucket Algorithm
- Permite burst de requisições
- Mantém taxa média sustentável
- Refill automático de tokens

```csharp
services.AddLogging(builder =>
{
    builder.AddDiscordLogger(options =>
    {
        options.Resilience.RateLimiting.Enabled = true;
        options.Resilience.RateLimiting.Type = RateLimiterType.TokenBucket;
        options.Resilience.RateLimiting.Capacity = 50;        // Burst de 50 req
        options.Resilience.RateLimiting.RefillRate = 50;      // 50 tokens/segundo
        options.Resilience.RateLimiting.RefillIntervalSeconds = 1;
    });
});
```

#### Adaptive Rate Limiting
- Ajusta automaticamente baseado em sucesso/falha
- Aumenta capacidade quando taxa de sucesso > 95%
- Reduz capacidade quando taxa de sucesso < 80%

```csharp
options.Resilience.RateLimiting.Type = RateLimiterType.Adaptive;
options.Resilience.RateLimiting.EnableAdaptive = true;
options.Resilience.RateLimiting.MinCapacity = 10;
options.Resilience.RateLimiting.MaxCapacity = 200;
options.Resilience.RateLimiting.IncreaseMultiplier = 1.2; // +20%
options.Resilience.RateLimiting.DecreaseMultiplier = 0.8; // -20%
```

### ✅ 2. Circuit Breaker

Implementação do Circuit Breaker pattern com 3 estados:
- **Closed**: Operações normais
- **Open**: Bloqueia requisições após threshold de falhas
- **HalfOpen**: Permite requisições de teste após timeout

```csharp
options.Resilience.CircuitBreaker.Enabled = true;
options.Resilience.CircuitBreaker.FailureThreshold = 5;      // Abre após 5 falhas
options.Resilience.CircuitBreaker.OpenTimeoutSeconds = 30;   // Aguarda 30s antes de HalfOpen
options.Resilience.CircuitBreaker.SuccessThreshold = 0.8;    // 80% sucesso para fechar
options.Resilience.CircuitBreaker.EnableHealthMonitoring = true;
options.Resilience.CircuitBreaker.SlowRequestThresholdMs = 5000; // Considera >5s como lento
```

#### Estratégias de Fallback
- **Discard**: Descarta mensagem (perda de dados)
- **Queue**: Enfileira para retry (DLQ)
- **File**: Salva em arquivo local
- **AlternativeWebhook**: Envia para webhook alternativo

```csharp
options.Resilience.CircuitBreaker.FallbackStrategy = FallbackStrategy.File;
options.Resilience.CircuitBreaker.FileFallbackPath = "./logs/discord-fallback.log";
// Ou
options.Resilience.CircuitBreaker.FallbackStrategy = FallbackStrategy.AlternativeWebhook;
options.Resilience.CircuitBreaker.FallbackWebhookUrl = "https://discord.com/api/webhooks/BACKUP";
```

### ✅ 3. Backoff Strategies

#### Exponential Backoff (Padrão)
- Delay aumenta exponencialmente: 1s → 2s → 4s → 8s...
- Recomendado para APIs com rate limiting

```csharp
options.Resilience.Backoff.Type = BackoffType.Exponential;
options.Resilience.Backoff.InitialDelaySeconds = 1;
options.Resilience.Backoff.MaxDelaySeconds = 300;
options.Resilience.Backoff.ExponentialMultiplier = 2.0;
```

#### Linear Backoff
- Delay aumenta linearmente: 2s → 4s → 6s → 8s...
- Mais previsível

```csharp
options.Resilience.Backoff.Type = BackoffType.Linear;
options.Resilience.Backoff.LinearIncrementSeconds = 2;
options.Resilience.Backoff.MaxDelaySeconds = 60;
```

#### Fibonacci Backoff
- Delay segue sequência Fibonacci: 1s → 1s → 2s → 3s → 5s → 8s...
- Balanceado entre exponencial e linear

```csharp
options.Resilience.Backoff.Type = BackoffType.Fibonacci;
options.Resilience.Backoff.InitialDelaySeconds = 1;
options.Resilience.Backoff.MaxDelaySeconds = 120;
```

#### Jitter (Recomendado)
- Adiciona aleatoriedade (±30%) para evitar thundering herd
- Habilitado por padrão

```csharp
options.Resilience.Backoff.UseJitter = true;
```

### ✅ 4. Persistência de Falhas

#### Dead Letter Queue (DLQ)
- Armazena mensagens que falharam múltiplas vezes
- Formato JSON file-based
- Auto-recovery automático

```csharp
options.Resilience.Persistence.EnableDeadLetterQueue = true;
options.Resilience.Persistence.DeadLetterQueuePath = "./logs/discord-dlq.json";
options.Resilience.Persistence.MaxAttemptsBeforeDLQ = 5;
```

#### File Fallback
- Salvamento automático em arquivo local
- Rotação automática quando atinge tamanho máximo
- Ideal para quando Discord está offline

```csharp
options.Resilience.Persistence.EnableFileFallback = true;
options.Resilience.Persistence.FileFallbackPath = "./logs/discord-fallback.log";
options.Resilience.Persistence.MaxFileFallbackSizeMb = 100;
```

#### Auto Recovery
- Tenta reenviar mensagens da DLQ automaticamente
- Executa periodicamente em background
- Configurable interval e batch size

```csharp
options.Resilience.Persistence.EnableAutoRecovery = true;
options.Resilience.Persistence.RecoveryIntervalSeconds = 300;      // 5 minutos
options.Resilience.Persistence.MaxMessagesPerRecoveryCycle = 100;
```

### ✅ 5. Múltiplos Webhooks e Roteamento

#### Roteamento por Nível de Log

```csharp
options.Resilience.MultiWebhook.Enabled = true;

// Webhook para Critical/Error
options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
{
    Name = "Errors",
    WebhookUrl = "https://discord.com/api/webhooks/ERROR_CHANNEL",
    LogLevels = new HashSet<LogLevel> { LogLevel.Critical, LogLevel.Error },
    Priority = 10
});

// Webhook para Warning
options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
{
    Name = "Warnings",
    WebhookUrl = "https://discord.com/api/webhooks/WARNING_CHANNEL",
    LogLevels = new HashSet<LogLevel> { LogLevel.Warning },
    Priority = 5
});
```

#### Roteamento por Categoria/Classe (Wildcards)

```csharp
options.Resilience.MultiWebhook.EnableCategoryRouting = true;

// Controllers
options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
{
    WebhookUrl = "https://discord.com/api/webhooks/API_CHANNEL",
    CategoryPatterns = new HashSet<string> 
    { 
        "*Controller",       // HomeController, UserController, etc.
        "*.Controllers.*"    // MyApp.Controllers.HomeController
    }
});

// Services
options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
{
    WebhookUrl = "https://discord.com/api/webhooks/BUSINESS_CHANNEL",
    CategoryPatterns = new HashSet<string> 
    { 
        "*.Service",         // UserService, OrderService
        "*.Services.*",      // MyApp.Services.UserService
        "*Service"
    }
});

// Namespace específico
options.Resilience.MultiWebhook.Routes.Add(new WebhookRoute
{
    WebhookUrl = "https://discord.com/api/webhooks/PAYMENT_CHANNEL",
    CategoryPatterns = new HashSet<string> { "MyApp.Payment.*" }
});
```

#### API Fluente (Builder Pattern)

```csharp
options.ConfigureWebhookRouting(routing =>
{
    // Critical
    routing.AddRoute("https://discord.com/api/webhooks/CRITICAL", "Critical")
        .ForCriticalOnly()
        .WithPriority(100)
        .WithUsername("🚨 Critical Alert")
        .WithAvatar("https://example.com/critical-avatar.png");
    
    // Errors em Controllers
    routing.AddRoute("https://discord.com/api/webhooks/API_ERRORS")
        .ForErrorsOnly()
        .ForControllers()
        .WithPriority(50);
    
    // Services
    routing.AddRoute("https://discord.com/api/webhooks/SERVICES")
        .ForServices()
        .WithPriority(30);
    
    // Repositories
    routing.AddRoute("https://discord.com/api/webhooks/DATA")
        .ForRepositories()
        .WithPriority(20);
    
    // Default fallback
    routing.WithDefaultWebhook("https://discord.com/api/webhooks/DEFAULT");
});
```

#### Load Balancing

**Priority** (Padrão)
```csharp
options.Resilience.MultiWebhook.LoadBalancing = LoadBalancingStrategy.Priority;
```

**Round-Robin**
```csharp
options.Resilience.MultiWebhook.LoadBalancing = LoadBalancingStrategy.RoundRobin;
```

**Random**
```csharp
options.Resilience.MultiWebhook.LoadBalancing = LoadBalancingStrategy.Random;
```

**Broadcast** (Envia para todos)
```csharp
options.ConfigureWebhookRouting(routing =>
{
    routing.AddRoute("https://discord.com/api/webhooks/CHANNEL_1");
    routing.AddRoute("https://discord.com/api/webhooks/CHANNEL_2");
    routing.AddRoute("https://discord.com/api/webhooks/CHANNEL_3");
    routing.EnableBroadcast(); // Envia para TODOS
});
```

### ✅ 6. Atributo para Roteamento

```csharp
// Em uma classe
[DiscordWebhook("https://discord.com/api/webhooks/PAYMENT_CHANNEL")]
public class PaymentService
{
    private readonly ILogger<PaymentService> _logger;
    
    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
    }
    
    public void ProcessPayment()
    {
        // Logs desta classe serão roteados para PAYMENT_CHANNEL
        _logger.LogInformation("Processing payment...");
    }
}

// Com configurações adicionais
[DiscordWebhook(
    WebhookUrl = "https://discord.com/api/webhooks/AUTH_CHANNEL",
    Username = "🔐 Auth Service",
    LogLevels = "Critical,Error"
)]
public class AuthenticationService
{
    // ...
}
```

## 📊 Configuração Completa

```csharp
services.AddLogging(builder =>
{
    builder.AddDiscordLogger(options =>
    {
        // Webhook principal
        options.WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL";
        options.Username = "Resilient Bot v1.4.0";
        
        // === RESILIÊNCIA ===
        options.Resilience.Enabled = true;
        options.Resilience.EnableVerboseLogging = true; // Debug
        options.Resilience.EnableTelemetry = true;      // Métricas
        
        // Rate Limiting Adaptativo
        options.Resilience.RateLimiting.Enabled = true;
        options.Resilience.RateLimiting.Type = RateLimiterType.Adaptive;
        options.Resilience.RateLimiting.Capacity = 50;
        options.Resilience.RateLimiting.MinCapacity = 10;
        options.Resilience.RateLimiting.MaxCapacity = 200;
        options.Resilience.RateLimiting.EnablePriorityQueue = true;
        
        // Circuit Breaker
        options.Resilience.CircuitBreaker.Enabled = true;
        options.Resilience.CircuitBreaker.FailureThreshold = 5;
        options.Resilience.CircuitBreaker.OpenTimeoutSeconds = 30;
        options.Resilience.CircuitBreaker.SuccessThreshold = 0.8;
        options.Resilience.CircuitBreaker.FallbackStrategy = FallbackStrategy.Queue;
        options.Resilience.CircuitBreaker.EnableHealthMonitoring = true;
        options.Resilience.CircuitBreaker.EnableAutoRecovery = true;
        
        // Backoff Exponencial com Jitter
        options.Resilience.Backoff.Type = BackoffType.Exponential;
        options.Resilience.Backoff.InitialDelaySeconds = 1;
        options.Resilience.Backoff.MaxDelaySeconds = 300;
        options.Resilience.Backoff.UseJitter = true;
        
        // Persistência
        options.Resilience.Persistence.EnableDeadLetterQueue = true;
        options.Resilience.Persistence.DeadLetterQueuePath = "./logs/discord-dlq.json";
        options.Resilience.Persistence.MaxAttemptsBeforeDLQ = 5;
        
        options.Resilience.Persistence.EnableFileFallback = true;
        options.Resilience.Persistence.FileFallbackPath = "./logs/discord-fallback.log";
        options.Resilience.Persistence.MaxFileFallbackSizeMb = 100;
        
        options.Resilience.Persistence.EnableAutoRecovery = true;
        options.Resilience.Persistence.RecoveryIntervalSeconds = 300;
        options.Resilience.Persistence.MaxMessagesPerRecoveryCycle = 100;
        
        // Múltiplos Webhooks
        options.ConfigureWebhookRouting(routing =>
        {
            routing.AddRoute("https://discord.com/api/webhooks/CRITICAL")
                .ForCriticalOnly()
                .WithPriority(100)
                .WithUsername("🚨 Critical Alert");
            
            routing.AddRoute("https://discord.com/api/webhooks/ERRORS")
                .ForErrorsOnly()
                .WithPriority(50);
            
            routing.AddRoute("https://discord.com/api/webhooks/API")
                .ForControllers()
                .WithPriority(30);
            
            routing.AddRoute("https://discord.com/api/webhooks/SERVICES")
                .ForServices()
                .WithPriority(20);
            
            routing.WithDefaultWebhook(options.WebhookUrl);
            routing.WithLoadBalancing(LoadBalancingStrategy.Priority);
        });
    });
});
```

## 🎯 Casos de Uso

### Zero Perda de Dados
```csharp
options.Resilience.CircuitBreaker.FallbackStrategy = FallbackStrategy.Queue;
options.Resilience.Persistence.EnableDeadLetterQueue = true;
options.Resilience.Persistence.EnableAutoRecovery = true;
```

### Alta Disponibilidade
```csharp
options.Resilience.CircuitBreaker.Enabled = true;
options.Resilience.CircuitBreaker.FallbackStrategy = FallbackStrategy.AlternativeWebhook;
options.Resilience.CircuitBreaker.FallbackWebhookUrl = "https://discord.com/api/webhooks/BACKUP";
```

### Alto Volume
```csharp
options.Resilience.RateLimiting.Type = RateLimiterType.Adaptive;
options.Resilience.RateLimiting.MaxCapacity = 500;
options.Batching.MaxBatchSize = 20;
options.Buffering.BufferCapacity = 1000;
```

### Roteamento Inteligente
```csharp
options.ConfigureWebhookRouting(routing =>
{
    routing.AddRoute("CRITICAL_WEBHOOK").ForCriticalOnly();
    routing.AddRoute("ERROR_WEBHOOK").ForErrorsOnly();
    routing.AddRoute("API_WEBHOOK").ForCategories("*.Controllers.*", "*Controller");
    routing.AddRoute("BUSINESS_WEBHOOK").ForCategories("*.Services.*", "*Service");
    routing.AddRoute("DATA_WEBHOOK").ForCategories("*.Repositories.*", "*Repository");
});
```

## 📈 Métricas e Monitoramento

```csharp
// Circuit Breaker Statistics
var stats = circuitBreaker.GetStatistics();
Console.WriteLine($"Estado: {stats.State}");
Console.WriteLine($"Taxa de Sucesso: {stats.SuccessRate:P}");
Console.WriteLine($"Tempo Médio: {stats.AverageResponseTime}ms");
Console.WriteLine($"Sucessos: {stats.TotalSuccesses}");
Console.WriteLine($"Falhas: {stats.TotalFailures}");

// Routing Statistics
var routingStats = webhookRouter.GetStatistics();
Console.WriteLine($"Total Roteado: {routingStats.TotalRouted}");
Console.WriteLine($"Uso de Fallback: {routingStats.FallbackCount}");
foreach (var (route, count) in routingStats.RouteCounts)
{
    Console.WriteLine($"  {route}: {count} mensagens");
}

// Recovery Statistics
var recoveryStats = recoveryService.GetStatistics();
Console.WriteLine($"Recuperadas: {recoveryStats.TotalRecovered}");
Console.WriteLine($"Falhas: {recoveryStats.FailedRecoveries}");
```

## 🎓 Exemplos Completos

Veja `examples/ConsoleExample/ResilienceExamples.cs` para exemplos detalhados de:
- Rate Limiting básico e adaptativo
- Circuit Breaker com diferentes fallback strategies
- Dead Letter Queue e Auto Recovery
- Backoff strategies (Exponencial, Linear, Fibonacci)
- Roteamento por nível de log
- Roteamento por categoria com wildcards
- API Fluente para configuração
- Load Balancing (Priority, Round-Robin, Random, Broadcast)
- Configuração completa com todos os recursos

## 🔧 Troubleshooting

### Mensagens não estão sendo enviadas
1. Verifique se `Resilience.Enabled = true`
2. Verifique logs de verbose: `Resilience.EnableVerboseLogging = true`
3. Verifique estado do Circuit Breaker
4. Verifique DLQ: `./logs/discord-dlq.json`

### Circuit Breaker abrindo frequentemente
1. Aumente `FailureThreshold`
2. Reduza `SlowRequestThresholdMs`
3. Habilite `EnableHealthMonitoring = false` temporariamente
4. Verifique conectividade com Discord

### Rate Limiting muito agressivo
1. Aumente `Capacity` e `MaxCapacity`
2. Use `Type = Adaptive` para ajuste automático
3. Reduza `RefillRate` se necessário

### DLQ crescendo indefinidamente
1. Verifique se `EnableAutoRecovery = true`
2. Reduza `RecoveryIntervalSeconds`
3. Aumente `MaxMessagesPerRecoveryCycle`
4. Verifique logs do Recovery Service

## ✅ Status da Implementação

- [x] Rate Limiting (Token Bucket)
- [x] Adaptive Rate Limiting
- [x] Circuit Breaker (Closed/Open/HalfOpen)
- [x] Health Monitoring
- [x] Backoff Strategies (Exponential, Linear, Fibonacci, Jitter)
- [x] Dead Letter Queue (File-based)
- [x] File Fallback
- [x] Retry Queue
- [x] Auto Recovery Service
- [x] Múltiplos Webhooks
- [x] Roteamento por Nível de Log
- [x] Roteamento por Categoria (Wildcards)
- [x] API Fluente (Builder Pattern)
- [x] Load Balancing (Priority, Round-Robin, Random, Broadcast)
- [x] Atributo `[DiscordWebhook]`
- [x] Telemetria e Estatísticas
- [x] Testes Unitários Completos

## 🚀 Próximos Passos

A Fase 9 está **COMPLETA** e pronta para produção! Todos os recursos de resiliência e confiabilidade foram implementados e testados.

**Zero perda de dados. Recuperação automática. Roteamento inteligente. 🔒**
