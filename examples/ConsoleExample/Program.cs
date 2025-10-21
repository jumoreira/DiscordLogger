using DiscordLogger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

Console.WriteLine("=== DiscordLogger - Exemplo de Uso ===\n");
Console.WriteLine("Escolha o modo de exemplo:");
Console.WriteLine("1. IDiscordLogger (API direta)");
Console.WriteLine("2. Microsoft.Extensions.Logging (ILogger<T>)");
Console.Write("\nDigite 1 ou 2: ");

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
