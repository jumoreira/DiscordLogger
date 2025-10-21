using DiscordLogger;
using DiscordLogger.Formatters;
using DiscordLogger.Performance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("=== DiscordLogger - Exemplo de Uso ===\n");
Console.WriteLine("Escolha o modo de exemplo:");
Console.WriteLine("1. IDiscordLogger (API direta)");
Console.WriteLine("2. Microsoft.Extensions.Logging (ILogger<T>)");
Console.WriteLine("3. Recursos Avançados v1.2.0 (Scopes, Batching, Filtros)");
Console.WriteLine("4. Performance & Escalabilidade v1.3.0 (High Volume, Priority, Buffering)");
Console.Write("\nDigite 1, 2, 3 ou 4: ");

var choice = Console.ReadLine();

// Carregar configurações do arquivo appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Configurar as opções do logger a partir do arquivo de configuração
var options = new DiscordLoggerOptions();
configuration.GetSection("DiscordLogger").Bind(options);

// Validar se o webhook foi configurado
if (string.IsNullOrEmpty(options.WebhookUrl) || 
    options.WebhookUrl.Contains("YOUR_WEBHOOK") ||
    options.WebhookUrl == "PASTE_YOUR_DISCORD_WEBHOOK_URL_HERE")
{
    Console.WriteLine("❌ Webhook do Discord não configurado!");
    Console.WriteLine("\nPara configurar:");
    Console.WriteLine("1. Abra o arquivo 'appsettings.json'");
    Console.WriteLine("2. Substitua o valor de 'WebhookUrl' pela URL do seu webhook do Discord");
    Console.WriteLine("\nPara criar um webhook no Discord:");
    Console.WriteLine("1. Abra seu servidor no Discord");
    Console.WriteLine("2. Vá em Configurações do Canal > Integrações > Webhooks");
    Console.WriteLine("3. Clique em 'Novo Webhook'");
    Console.WriteLine("4. Copie a URL do webhook");
    Console.WriteLine("\nPressione qualquer tecla para sair...");
    Console.ReadKey();
    return;
}

if (choice == "1")
{
    await RunDirectApiExample(options);
}
else if (choice == "2")
{
    await RunMicrosoftLoggingExample(options);
}
else if (choice == "3")
{
    await RunAdvancedFeaturesExample(options);
}
else if (choice == "4")
{
    await RunPerformanceDemo(options);
}
else
{
    Console.WriteLine("Opção inválida! Executando ambos os exemplos...\n");
    await RunDirectApiExample(options);
    Console.WriteLine("\n" + new string('=', 60) + "\n");
    await RunMicrosoftLoggingExample(options);
}

Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();

// ===== Exemplo 1: Usando IDiscordLogger diretamente =====
static async Task RunDirectApiExample(DiscordLoggerOptions options)
{
    Console.WriteLine("\n=== Exemplo 1: Usando IDiscordLogger (API Direta) ===\n");
    
    using var logger = new DiscordLogger.DiscordLogger(options);

    try
    {
        // Exemplo 1: Log de Debug
        Console.WriteLine("1. Enviando log de Debug...");
        await logger.LogDebugAsync("Este é um log de debug com informações detalhadas do sistema.");
        await Task.Delay(1000);

        // Exemplo 2: Log de Information
        Console.WriteLine("2. Enviando log de Information...");
        await logger.LogInformationAsync("Aplicação iniciada com sucesso! Todos os módulos carregados.");
        await Task.Delay(1000);

        // Exemplo 3: Log de Warning
        Console.WriteLine("3. Enviando log de Warning...");
        await logger.LogWarningAsync("Cache expirado. Recarregando dados do servidor...");
        await Task.Delay(1000);

        // Exemplo 4: Log de Error com exceção
        Console.WriteLine("4. Enviando log de Error com exceção...");
        try
        {
            throw new InvalidOperationException("Operação inválida detectada no processamento de dados.");
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync("Erro ao processar dados do usuário", ex);
        }
        await Task.Delay(1000);

        // Exemplo 5: Log de Critical com exceção complexa
        Console.WriteLine("5. Enviando log de Critical com exceção aninhada...");
        try
        {
            try
            {
                throw new ArgumentNullException("connectionString", "String de conexão não pode ser nula");
            }
            catch (Exception innerEx)
            {
                throw new InvalidOperationException("Falha crítica ao conectar com o banco de dados", innerEx);
            }
        }
        catch (Exception ex)
        {
            await logger.LogCriticalAsync("Sistema não pode continuar: falha na conexão com banco de dados", ex);
        }
        await Task.Delay(1000);

        Console.WriteLine("\n✅ Todos os logs foram enviados com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Erro: {ex.Message}");
    }
}

// ===== Exemplo 2: Usando Microsoft.Extensions.Logging =====
static async Task RunMicrosoftLoggingExample(DiscordLoggerOptions options)
{
    Console.WriteLine("\n=== Exemplo 2: Usando Microsoft.Extensions.Logging (ILogger<T>) ===\n");

    // Configurar Dependency Injection
    var services = new ServiceCollection();

    // Adicionar logging com Discord
    services.AddLogging(builder =>
    {
        builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
        
        // Adicionar Console Logger para ver logs no terminal também
        builder.AddConsole();
        
        // Adicionar Discord Logger
        builder.AddDiscordLogger(discordOptions =>
        {
            discordOptions.WebhookUrl = options.WebhookUrl;
            discordOptions.Username = options.Username;
            discordOptions.AvatarUrl = options.AvatarUrl;
            discordOptions.MinimumLevel = options.MinimumLevel;
            discordOptions.TimeoutSeconds = options.TimeoutSeconds;
            discordOptions.MaxRetryAttempts = options.MaxRetryAttempts;
        });
    });

    // Registrar serviços de exemplo
    services.AddTransient<ExampleService>();
    services.AddTransient<PaymentService>();

    // Construir o service provider
    var serviceProvider = services.BuildServiceProvider();

    try
    {
        // Exemplo com ILogger<T>
        var exampleService = serviceProvider.GetRequiredService<ExampleService>();
        await exampleService.DoWork();
        await Task.Delay(1000);

        var paymentService = serviceProvider.GetRequiredService<PaymentService>();
        await paymentService.ProcessPayment(1234);
        await Task.Delay(1000);

        // Exemplo com LoggerFactory
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var customLogger = loggerFactory.CreateLogger("CustomCategory");
        
        Console.WriteLine("6. Enviando log de categoria customizada...");
        customLogger.LogInformation("Log de uma categoria customizada criada dinamicamente");
        await Task.Delay(1000);

        // Exemplo com EventId
        Console.WriteLine("7. Enviando log com EventId...");
        var eventId = new EventId(1001, "UserLogin");
        customLogger.LogInformation(eventId, "Usuário {Username} fez login às {Time}", "admin", DateTime.Now);
        await Task.Delay(1000);

        Console.WriteLine("\n✅ Todos os logs foram enviados com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Erro: {ex.Message}");
    }
    finally
    {
        await serviceProvider.DisposeAsync();
    }
}

// ===== Exemplo 3: Recursos Avançados v1.2.0 =====
static async Task RunAdvancedFeaturesExample(DiscordLoggerOptions baseOptions)
{
    Console.WriteLine("\n=== Exemplo 3: Recursos Avançados v1.2.0 ===\n");
    
    var services = new ServiceCollection();

    services.AddLogging(builder =>
    {
        builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
        builder.AddConsole();
        
        builder.AddDiscordLogger(options =>
        {
            options.WebhookUrl = baseOptions.WebhookUrl;
            options.Username = "AdvancedBot v1.2.0";
            options.AvatarUrl = baseOptions.AvatarUrl;
            
            // 1. Habilitar Scopes
            options.EnableScopes = true;
            
            // 2. Configurar Batching
            options.Batching.Enabled = true;
            options.Batching.MaxBatchSize = 5;
            options.Batching.FlushIntervalSeconds = 3;
            
            // 3. Configurar Filtros
            options.Filters.ExcludeCategories.Add("System.*");
            options.Filters.MessagePatterns.Add("Health check.*");
            options.Filters.MessagePatternsAsWhitelist = false;
            
            // 4. Formatador Personalizado
            options.MessageFormatter = new TemplateMessageFormatter(
                titleTemplate: "🎯 [{level}] - {date}",
                descriptionTemplate: "⏰ {time} | {message}"
            );
        });
    });

    var serviceProvider = services.BuildServiceProvider();

    try
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        
        // Exemplo 1: Scopes
        Console.WriteLine("1. Testando Scopes...");
        using (logger.BeginScope("RequestId: {RequestId}", Guid.NewGuid().ToString("N").Substring(0, 8)))
        {
            using (logger.BeginScope("UserId: {UserId}", "admin@example.com"))
            {
                logger.LogInformation("Processando requisição com contexto completo");
            }
        }
        await Task.Delay(1000);
        
        // Exemplo 2: Batching
        Console.WriteLine("2. Testando Batching (5 mensagens)...");
        for (int i = 1; i <= 5; i++)
        {
            logger.LogInformation("Mensagem em batch #{Number}", i);
        }
        Console.WriteLine("   Aguardando flush do batch (3 segundos)...");
        await Task.Delay(4000);
        
        // Exemplo 3: Filtros
        Console.WriteLine("3. Testando Filtros...");
        var systemLogger = serviceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("System.Internal");
        systemLogger.LogInformation("Esta mensagem será filtrada (categoria System.*)");
        
        logger.LogInformation("Health check passed"); // Será filtrada pelo pattern
        logger.LogInformation("Mensagem normal que será enviada");
        await Task.Delay(1000);
        
        // Exemplo 4: Formatador Personalizado
        Console.WriteLine("4. Testando Formatador Personalizado...");
        logger.LogWarning("Aviso com formatação customizada");
        logger.LogError(new Exception("Erro de teste"), "Erro com template personalizado");
        await Task.Delay(4000); // Aguarda último batch
        
        Console.WriteLine("\n✅ Exemplos de recursos avançados concluídos!");
        Console.WriteLine("\nRecursos demonstrados:");
        Console.WriteLine("  ✓ Scopes com contexto");
        Console.WriteLine("  ✓ Batching de mensagens");
        Console.WriteLine("  ✓ Filtros avançados");
        Console.WriteLine("  ✓ Formatador personalizado");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Erro: {ex.Message}");
    }
    finally
    {
        await serviceProvider.DisposeAsync();
    }
}

// ===== Exemplo 4: Performance Demo v1.3.0 =====
static async Task RunPerformanceDemo(DiscordLoggerOptions baseOptions)
{
    Console.WriteLine("\n=== Exemplo 4: Performance & Escalabilidade v1.3.0 ===\n");
    
    var services = new ServiceCollection();

    services.AddLogging(builder =>
    {
        builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
        builder.AddConsole();
        
        builder.AddDiscordLogger(options =>
        {
            options.WebhookUrl = baseOptions.WebhookUrl;
            options.Username = "PerformanceBot v1.3.0";
            options.AvatarUrl = baseOptions.AvatarUrl;
            
            // 1. Batching para alto volume
            options.Batching.Enabled = true;
            options.Batching.MaxBatchSize = 10;
            options.Batching.FlushIntervalSeconds = 2;
            options.Batching.MaxQueueSize = 1000;
            
            // 2. Buffering inteligente
            options.Buffering.Enabled = true;
            options.Buffering.BufferCapacity = 100;
            options.Buffering.FlushStrategy = FlushStrategy.Auto;
            options.Buffering.FlushThreshold = 20;
            
            // 3. Prioridade para logs críticos
            options.EnablePriorityQueue = true;
            
            // 4. HttpClient pooling
            options.EnableHttpClientPooling = true;
            
            // 5. Anexos para mensagens grandes
            options.FileAttachment.Enabled = true;
            options.FileAttachment.MessageThreshold = 500;
        });
    });

    var serviceProvider = services.BuildServiceProvider();

    try
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        
        Console.WriteLine("1. Testando alto volume (100 mensagens)...");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 1; i <= 100; i++)
        {
            logger.LogInformation("Mensagem de alto volume #{Number}", i);
            
            if (i % 25 == 0)
            {
                Console.WriteLine($"   Progresso: {i}/100");
            }
        }
        
        stopwatch.Stop();
        Console.WriteLine($"   ✅ 100 mensagens em {stopwatch.ElapsedMilliseconds}ms");
        
        await Task.Delay(3000);
        
        // Teste 2: Prioridades
        Console.WriteLine("\n2. Testando fila de prioridade...");
        logger.LogDebug("Prioridade baixa");
        logger.LogInformation("Prioridade normal");
        logger.LogWarning("Prioridade alta");
        logger.LogError("Prioridade crítica");
        
        await Task.Delay(2000);
        
        // Teste 3: Mensagem grande
        Console.WriteLine("\n3. Testando mensagem grande com anexo...");
        var largeMessage = "Stack Trace:\n" + string.Join("\n", 
            Enumerable.Range(1, 30).Select(i => 
                $"   at MyApp.Service.Method{i}(String param) in File{i}.cs:line {i * 10}"));
        
        logger.LogError("Erro com stack trace grande:\n{StackTrace}", largeMessage);
        
        Console.WriteLine($"   Mensagem: {largeMessage.Length} caracteres");
        Console.WriteLine("   Um preview será enviado com arquivo anexo");
        
        await Task.Delay(3000);
        
        Console.WriteLine("\n✅ Demonstração de Performance completa!");
        Console.WriteLine("\nRecursos demonstrados:");
        Console.WriteLine("  ✓ Alto volume com batching");
        Console.WriteLine("  ✓ Buffering inteligente");
        Console.WriteLine("  ✓ Fila de prioridade");
        Console.WriteLine("  ✓ HttpClient pooling");
        Console.WriteLine("  ✓ Anexo de arquivos");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Erro: {ex.Message}");
    }
    finally
    {
        await serviceProvider.DisposeAsync();
    }
}

// ===== Serviços de Exemplo =====

class ExampleService
{
    private readonly ILogger<ExampleService> _logger;

    public ExampleService(ILogger<ExampleService> logger)
    {
        _logger = logger;
    }

    public async Task DoWork()
    {
        Console.WriteLine("1. [ExampleService] Enviando logs estruturados...");
        
        _logger.LogDebug("Iniciando processamento em {ServiceName}", nameof(ExampleService));
        await Task.Delay(100);
        
        _logger.LogInformation("Processamento concluído com sucesso em {Duration}ms", 150);
        await Task.Delay(100);
        
        _logger.LogWarning("Memória em uso: {MemoryMB}MB - limite de {LimitMB}MB", 450, 512);
    }
}

class PaymentService
{
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessPayment(int orderId)
    {
        Console.WriteLine("2. [PaymentService] Processando pagamento...");
        
        try
        {
            _logger.LogInformation("Processando pagamento para pedido {OrderId}", orderId);
            await Task.Delay(100);
            
            // Simular erro de pagamento
            throw new InvalidOperationException($"Saldo insuficiente para processar pedido {orderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao processar pagamento do pedido {OrderId}", orderId);
            await Task.Delay(100);
            
            // Simular erro crítico
            _logger.LogCritical(ex, "Sistema de pagamento offline - impossível processar transações");
        }
    }
}
