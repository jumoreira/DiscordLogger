using DiscordLogger;

Console.WriteLine("=== DiscordLogger - Exemplo de Uso ===\n");

// Configurar as opções do logger
var options = new DiscordLoggerOptions
{
    // IMPORTANTE: Substitua pela sua URL de webhook do Discord
    // Para criar um webhook: Configurações do Canal > Integrações > Webhooks > Novo Webhook
    WebhookUrl = "https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN",
    
    Username = "MyApp Logger",
    MinimumLevel = LogLevel.Debug,
    TimeoutSeconds = 30,
    MaxRetryAttempts = 3
};

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
    Console.WriteLine("\nPor favor, configure um webhook válido do Discord:");
    Console.WriteLine("1. Abra seu servidor no Discord");
    Console.WriteLine("2. Vá em Configurações do Canal > Integrações > Webhooks");
    Console.WriteLine("3. Clique em 'Novo Webhook'");
    Console.WriteLine("4. Copie a URL do webhook");
    Console.WriteLine("5. Substitua a URL no código");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Erro inesperado: {ex.Message}");
}

Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();
