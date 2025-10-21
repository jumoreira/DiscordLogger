# Console Example - DiscordLogger

Este exemplo demonstra como usar o DiscordLogger em uma aplicação console.

## Configuração

### 1. Configure o Webhook do Discord

Edite o arquivo `appsettings.json` e substitua a URL do webhook pela sua própria:

```json
{
  "DiscordLogger": {
    "WebhookUrl": "https://discord.com/api/webhooks/SEU_ID/SEU_TOKEN",
    "Username": "MyApp Logger",
    "MinimumLevel": "Debug",
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3
  }
}
```

### 2. Como criar um Webhook no Discord

1. Abra seu servidor no Discord
2. Selecione o canal onde deseja receber os logs
3. Clique na engrenagem de **Configurações do Canal**
4. Vá em **Integrações** > **Webhooks**
5. Clique em **Novo Webhook**
6. Dê um nome ao webhook (opcional)
7. Copie a **URL do Webhook**
8. Cole a URL no arquivo `appsettings.json`

### 3. Executar o Exemplo

```bash
dotnet run --project examples/ConsoleExample/ConsoleExample.csproj
```

## Opções de Configuração

| Propriedade | Tipo | Descrição | Padrão |
|------------|------|-----------|--------|
| `WebhookUrl` | string | URL do webhook do Discord (obrigatório) | - |
| `Username` | string | Nome de usuário exibido nas mensagens | "Logger" |
| `MinimumLevel` | string | Nível mínimo de log (Trace, Debug, Information, Warning, Error, Critical) | "Information" |
| `TimeoutSeconds` | int | Timeout para requisições HTTP em segundos | 30 |
| `MaxRetryAttempts` | int | Número máximo de tentativas de reenvio | 3 |

## Segurança

⚠️ **IMPORTANTE**: Nunca compartilhe ou commite sua URL de webhook do Discord em repositórios públicos!

- Use o arquivo `appsettings.example.json` como template
- Mantenha o `appsettings.json` com suas credenciais reais no `.gitignore`
- Em produção, use variáveis de ambiente ou serviços de gerenciamento de segredos

## Usando Variáveis de Ambiente

Você também pode configurar via variáveis de ambiente:

```bash
# Windows (PowerShell)
$env:DiscordLogger__WebhookUrl="https://discord.com/api/webhooks/SEU_ID/SEU_TOKEN"

# Linux/macOS
export DiscordLogger__WebhookUrl="https://discord.com/api/webhooks/SEU_ID/SEU_TOKEN"
```

Para usar variáveis de ambiente, adicione ao `ConfigurationBuilder`:

```csharp
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();
```
