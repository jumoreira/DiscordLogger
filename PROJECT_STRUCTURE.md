# DiscordLogger - Estrutura do Projeto

## 📁 Estrutura de Diretórios

```
DiscordLogger/
├── .github/
│   └── workflows/
│       ├── build.yml              # CI/CD - Build e testes automáticos
│       └── publish.yml            # Publicação automática no NuGet
│
├── src/
│   └── DiscordLogger/             # Projeto principal
│       ├── Models/
│       │   └── DiscordWebhookMessage.cs  # Modelos para API do Discord
│       ├── DiscordLogger.csproj   # Configuração do projeto
│       ├── DiscordLoggerOptions.cs # Opções de configuração
│       ├── IDiscordLogger.cs      # Interface principal
│       └── LogLevel.cs            # Enum de níveis de log
│
├── tests/
│   └── DiscordLogger.Tests/       # Projeto de testes
│       ├── DiscordLogger.Tests.csproj
│       └── UnitTest1.cs
│
├── .editorconfig                  # Configurações de estilo de código
├── .gitignore                     # Arquivos ignorados pelo Git
├── CHANGELOG.md                   # Histórico de mudanças
├── Directory.Build.props          # Propriedades compartilhadas
├── DiscordLogger.sln              # Solution do projeto
├── EXAMPLES.md                    # Exemplos de uso
├── LICENSE                        # Licença MIT
├── nuget.config                   # Configuração do NuGet
├── PUBLISHING.md                  # Guia de publicação
├── README.md                      # Documentação principal
└── ROADMAP.md                     # Plano de desenvolvimento
```

## ✅ Arquivos Criados

### Configuração do Projeto

1. **DiscordLogger.sln** - Solution principal
2. **Directory.Build.props** - Propriedades compartilhadas entre projetos
3. **nuget.config** - Configuração de sources do NuGet
4. **.editorconfig** - Padrões de código
5. **.gitignore** - Exclusões do Git

### Projeto Principal (src/DiscordLogger)

1. **DiscordLogger.csproj** - Configuração do pacote NuGet
2. **DiscordLoggerOptions.cs** - Classe de configuração
3. **LogLevel.cs** - Enum de níveis de log
4. **IDiscordLogger.cs** - Interface principal do logger
5. **Models/DiscordWebhookMessage.cs** - Modelos de mensagem do Discord

### Documentação

1. **README.md** - Documentação principal com badges e instruções
2. **LICENSE** - Licença MIT
3. **CHANGELOG.md** - Registro de versões e mudanças
4. **PUBLISHING.md** - Guia completo de publicação no NuGet
5. **EXAMPLES.md** - Exemplos de uso da biblioteca
6. **ROADMAP.md** - Plano de desenvolvimento futuro

### CI/CD (GitHub Actions)

1. **.github/workflows/build.yml** - Build e testes automáticos
2. **.github/workflows/publish.yml** - Publicação automática no NuGet

## 🎯 Próximos Passos

### 1. Implementar Classes Core

- [ ] Criar classe `DiscordLogger` implementando `IDiscordLogger`
- [ ] Criar `DiscordWebhookClient` para comunicação HTTP
- [ ] Criar `MessageFormatter` para formatação de embeds

### 2. Adicionar Dependências

```bash
cd src/DiscordLogger
dotnet add package System.Text.Json
```

### 3. Implementar Testes

```bash
cd tests/DiscordLogger.Tests
# Criar testes unitários para todas as funcionalidades
```

### 4. Integração com Microsoft.Extensions.Logging

- [ ] Criar `DiscordLoggerProvider`
- [ ] Criar extension methods para IServiceCollection

### 5. Preparar para Publicação

```bash
# Build
dotnet build -c Release

# Testes
dotnet test -c Release

# Criar pacote
dotnet pack src/DiscordLogger/DiscordLogger.csproj -c Release -o ./artifacts
```

## 🔧 Configuração Atual

### Versão do .NET
- Projeto principal: **.NET 8.0**
- Projeto de testes: **.NET 8.0**

### Pacotes Instalados

**DiscordLogger:**
- Microsoft.Extensions.Logging.Abstractions (8.0.0)

**DiscordLogger.Tests:**
- Microsoft.NET.Test.Sdk (17.14.1)
- xUnit (2.9.3)
- xUnit.runner.visualstudio (3.1.4)
- coverlet.collector (6.0.4)

## 📝 Configurações do NuGet

| Propriedade | Valor |
|-------------|-------|
| PackageId | DiscordLogger |
| Version | 1.0.0 |
| Authors | Elefanti |
| License | MIT |
| Tags | discord, logging, logger, webhook, notification |

## 🚀 Como Usar (Após Implementação)

```csharp
// 1. Instalar o pacote
// dotnet add package DiscordLogger

// 2. Configurar
var options = new DiscordLoggerOptions
{
    WebhookUrl = "sua-webhook-url",
    MinimumLevel = LogLevel.Information
};

// 3. Usar (após implementação)
// var logger = new DiscordLogger(options);
// await logger.LogInformationAsync("Hello Discord!");
```

## 📊 Status do Projeto

- ✅ Estrutura criada
- ✅ Configuração do NuGet completa
- ✅ CI/CD configurado
- ✅ Documentação inicial
- ✅ Interfaces e modelos base
- ⏳ Implementação core (próximo passo)
- ⏳ Testes unitários
- ⏳ Publicação no NuGet

## 🤝 Contribuindo

Para contribuir com o projeto:

1. Fork o repositório
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo LICENSE para mais detalhes.
