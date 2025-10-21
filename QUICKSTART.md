# 🚀 Guia de Início Rápido - DiscordLogger

Este guia vai te ajudar a começar a usar o DiscordLogger em menos de 5 minutos!

## 📋 Pré-requisitos

- .NET 8.0 ou superior
- Um servidor Discord (ou acesso a um canal)
- Permissões para criar webhooks no canal

## 🎯 Passo 1: Criar um Webhook no Discord

1. Abra o Discord e vá até o servidor/canal onde deseja receber os logs
2. Clique com o botão direito no canal > **Editar Canal**
3. Vá em **Integrações** > **Webhooks**
4. Clique em **Novo Webhook**
5. Dê um nome ao webhook (ex: "App Logger")
6. (Opcional) Personalize o avatar
7. Clique em **Copiar URL do Webhook**
8. Salve esta URL - você vai precisar dela!

## 📦 Passo 2: Instalar o Pacote

No seu projeto .NET, execute:

```bash
dotnet add package DiscordLogger
```

## 💻 Passo 3: Usar o Logger

### Código Mínimo

```csharp
using DiscordLogger;

var options = new DiscordLoggerOptions
{
    WebhookUrl = "SUA_URL_WEBHOOK_AQUI"
};

using var logger = new DiscordLogger.DiscordLogger(options);

await logger.LogInformationAsync("Olá, Discord! 👋");
```

### Exemplo Completo

```csharp
using DiscordLogger;

// Configurar o logger
var options = new DiscordLoggerOptions
{
    WebhookUrl = "https://discord.com/api/webhooks/123456/abcdef",
    Username = "MyApp",
    MinimumLevel = LogLevel.Information
};

using var logger = new DiscordLogger.DiscordLogger(options);

// Usar em diferentes cenários
await logger.LogInformationAsync("Aplicação iniciada!");

try
{
    // Seu código aqui
    ProcessData();
}
catch (Exception ex)
{
    await logger.LogErrorAsync("Erro ao processar dados", ex);
}
```

## ⚙️ Passo 4: Configurações Avançadas (Opcional)

### Usar appsettings.json

**appsettings.json:**
```json
{
  "DiscordLogger": {
    "WebhookUrl": "https://discord.com/api/webhooks/123456/abcdef",
    "Username": "MyApp Logger",
    "MinimumLevel": "Information"
  }
}
```

**Program.cs:**
```csharp
using Microsoft.Extensions.Configuration;
using DiscordLogger;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var options = configuration
    .GetSection("DiscordLogger")
    .Get<DiscordLoggerOptions>();

using var logger = new DiscordLogger.DiscordLogger(options);
```

## 🎨 Passo 5: Entender os Níveis de Log

```csharp
// Debug (Cinza) - Informações de desenvolvimento
await logger.LogDebugAsync("Valor da variável X: 123");

// Information (Azul) - Fluxo normal
await logger.LogInformationAsync("Usuário logado com sucesso");

// Warning (Amarelo) - Atenção necessária
await logger.LogWarningAsync("Cache quase cheio (85%)");

// Error (Laranja) - Erro recuperável
await logger.LogErrorAsync("Falha ao enviar email", exception);

// Critical (Vermelho) - Erro crítico
await logger.LogCriticalAsync("Banco de dados inacessível!", exception);
```

## ✅ Verificar se Está Funcionando

Após executar seu código, verifique o canal do Discord. Você deve ver uma mensagem formatada com:
- 🎨 Cor correspondente ao nível de log
- 📝 Título com emoji
- 💬 Sua mensagem
- 🕒 Timestamp
- 📋 Detalhes da exceção (se houver)

## 🐛 Troubleshooting

### "WebhookUrl é obrigatório"
✅ Certifique-se de ter copiado corretamente a URL do webhook do Discord.

### "Falha ao enviar mensagem"
✅ Verifique se a URL do webhook está correta e ainda é válida.
✅ Certifique-se de que o webhook não foi deletado no Discord.

### Mensagens não aparecem
✅ Verifique se o `MinimumLevel` não está muito alto (ex: `Critical` bloqueará todos os outros).
✅ Verifique se você tem permissões para ver o canal.

### Rate Limit (429)
✅ O logger já trata isso automaticamente! Ele aguarda e retenta.

## 📚 Próximos Passos

- ✅ Leia a [documentação completa](../README.md)
- ✅ Veja os [exemplos avançados](../EXAMPLES.md)
- ✅ Explore o [projeto de exemplo](../examples/ConsoleExample/)
- ✅ Contribua no [GitHub](https://github.com/jumoreira/DiscordLogger)

## 💡 Dicas

1. **Não commite a URL do webhook** no código! Use variáveis de ambiente ou appsettings.
2. **Use níveis apropriados**: Reserve `Critical` para erros realmente graves.
3. **Configure `MinimumLevel`**: Em produção, use `Information` ou `Warning`.
4. **Trate erros de rede**: O logger já tem retry automático, mas pode falhar eventualmente.

## 🎉 Pronto!

Você agora tem um sistema de logging para Discord funcionando! 🚀

Se tiver dúvidas, abra uma [issue no GitHub](https://github.com/jumoreira/DiscordLogger/issues).
