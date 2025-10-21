using DiscordLogger;
using Microsoft.Extensions.Configuration;

Console.WriteLine("=== DiscordLogger - Exemplo de Uso ===\n");

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

// Criar o logger
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
        // Simular um erro
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
    Console.WriteLine("Verifique seu canal do Discord para ver as mensagens.");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"\n❌ Erro de configuração: {ex.Message}");
    Console.WriteLine("\nVerifique o arquivo 'appsettings.json' e certifique-se de que:");
    Console.WriteLine("1. O webhook URL está correto");
    Console.WriteLine("2. Todas as propriedades estão configuradas corretamente");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Erro inesperado: {ex.Message}");
}

Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();
