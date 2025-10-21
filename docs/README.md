# 📚 Documentação do DiscordLogger

Esta pasta contém toda a documentação técnica e guias do projeto DiscordLogger.

## 📖 Índice

### Guias de Versão
- **[v1.2.0 - Recursos Avançados](./v1.2.0-ADVANCED-FEATURES.md)** ⭐ NOVO!
  - Log Scopes Completos
  - Batching de Mensagens
  - Filtros Avançados
  - Formatadores Personalizados

### Resumos de Implementação
- **[Fase 7 - Resumo](./FASE-7-RESUMO.md)** 
  - Status da implementação da v1.2.0
  - Estatísticas e métricas
  - Checklist de qualidade

## 🚀 Quick Start

### Instalação Básica
```bash
dotnet add package DiscordLogger
```

### Uso Básico
```csharp
services.AddLogging(builder =>
{
    builder.AddDiscord(options =>
    {
        options.WebhookUrl = "YOUR_WEBHOOK_URL";
    });
});
```

### Recursos Avançados (v1.2.0)
```csharp
services.AddLogging(builder =>
{
    builder.AddDiscord(options =>
    {
        options.WebhookUrl = "YOUR_WEBHOOK_URL";
        
        // Scopes
        options.EnableScopes = true;
        
        // Batching
        options.Batching.Enabled = true;
        options.Batching.MaxBatchSize = 10;
        
        // Filtros
        options.Filters.IncludeCategories.Add("MyApp.*");
        
        // Formatador personalizado
        options.MessageFormatter = new TemplateMessageFormatter();
    });
});
```

## 📋 Recursos por Versão

### v1.2.0 (Atual)
- ✅ Log Scopes Completos
- ✅ Batching de Mensagens (até 90% menos chamadas de API)
- ✅ Filtros Avançados (categoria, EventId, regex)
- ✅ Formatadores Personalizados (templates, interface)
- ✅ 55 novos testes unitários
- ✅ Totalmente compatível com v1.0.0 e v1.1.0

### v1.1.0
- ✅ Integração com Microsoft.Extensions.Logging
- ✅ Dependency Injection
- ✅ Configuração via appsettings.json

### v1.0.0
- ✅ API básica do DiscordLogger
- ✅ 5 níveis de log
- ✅ Formatação com embeds
- ✅ Retry automático

## 🎯 Casos de Uso

### 1. Logging Simples
Para aplicações que precisam apenas enviar logs para o Discord.

**Solução:** Use a API básica (v1.0.0)

### 2. Integração com ASP.NET Core
Para aplicações web que usam o logging framework do .NET.

**Solução:** Use a integração com Microsoft.Extensions.Logging (v1.1.0)

### 3. Alta Volumetria de Logs
Para aplicações que geram muitos logs e precisam reduzir chamadas de API.

**Solução:** Habilite Batching (v1.2.0)

### 4. Filtragem Seletiva
Para enviar apenas logs relevantes e evitar spam no Discord.

**Solução:** Use Filtros Avançados (v1.2.0)

### 5. Contexto Adicional
Para rastreamento de requisições e correlação de logs.

**Solução:** Habilite Scopes (v1.2.0)

### 6. Formatação Customizada
Para match com identidade visual da sua empresa.

**Solução:** Use Formatadores Personalizados (v1.2.0)

## 📊 Comparação de Recursos

| Recurso | v1.0.0 | v1.1.0 | v1.2.0 |
|---------|--------|--------|--------|
| API Direta | ✅ | ✅ | ✅ |
| Microsoft.Extensions.Logging | ❌ | ✅ | ✅ |
| Dependency Injection | ❌ | ✅ | ✅ |
| Scopes | ❌ | ❌ | ✅ |
| Batching | ❌ | ❌ | ✅ |
| Filtros Avançados | ❌ | ❌ | ✅ |
| Formatadores Custom | ❌ | ❌ | ✅ |

## 🔗 Links Úteis

- [Repositório GitHub](https://github.com/jumoreira/DiscordLogger)
- [Criar Webhook no Discord](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks)
- [Microsoft.Extensions.Logging Docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [Issues e Suporte](https://github.com/jumoreira/DiscordLogger/issues)

## 🤝 Contribuindo

Contribuições são bem-vindas! Por favor, leia nosso guia de contribuição antes de submeter PRs.

## 📄 Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo [LICENSE](../LICENSE) para detalhes.

## 📮 Contato

- **Autor:** Julio Moreira
- **Email:** [Seu email aqui]
- **GitHub:** [@jumoreira](https://github.com/jumoreira)

---

**Última atualização:** Janeiro 2025  
**Versão:** 1.2.0
