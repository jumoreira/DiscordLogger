# Guia de Publicação no NuGet

Este guia descreve como publicar o pacote DiscordLogger no NuGet.

## Pré-requisitos

1. Conta no [NuGet.org](https://www.nuget.org/)
2. API Key gerada no NuGet.org
3. .NET 8.0 SDK instalado

## Passos para Publicação Manual

### 1. Atualizar a Versão

Edite o arquivo `src/DiscordLogger/DiscordLogger.csproj` e atualize a tag `<Version>`:

```xml
<Version>1.0.1</Version>
```

Ou use o `Directory.Build.props` para versão centralizada.

### 2. Build do Projeto

```bash
dotnet build -c Release
```

### 3. Executar Testes

```bash
dotnet test -c Release
```

### 4. Criar o Pacote

```bash
dotnet pack src/DiscordLogger/DiscordLogger.csproj -c Release -o ./artifacts
```

### 5. Publicar no NuGet

```bash
dotnet nuget push ./artifacts/DiscordLogger.1.0.0.nupkg --api-key SUA_API_KEY --source https://api.nuget.org/v3/index.json
```

## Publicação Automatizada via GitHub Actions

O projeto já está configurado com GitHub Actions para publicação automática.

### Configurar Secrets no GitHub

1. Vá em Settings > Secrets and variables > Actions
2. Adicione o secret `NUGET_API_KEY` com sua API key do NuGet.org
3. (Opcional) Adicione `CODECOV_TOKEN` para reports de cobertura

### Criar uma Release

1. Vá em Releases no GitHub
2. Clique em "Draft a new release"
3. Escolha uma tag (ex: v1.0.0)
4. Preencha as informações da release
5. Clique em "Publish release"

O workflow de publicação será executado automaticamente!

## Verificar a Publicação

Após alguns minutos, verifique se o pacote está disponível em:
https://www.nuget.org/packages/DiscordLogger/

## Versionamento Semântico

Siga o padrão [Semantic Versioning](https://semver.org/):

- **MAJOR** (1.x.x): Mudanças incompatíveis com versões anteriores
- **MINOR** (x.1.x): Novas funcionalidades compatíveis
- **PATCH** (x.x.1): Correções de bugs compatíveis

## Checklist de Publicação

- [ ] Código revisado e testado
- [ ] Versão atualizada
- [ ] CHANGELOG.md atualizado
- [ ] README.md atualizado
- [ ] Todos os testes passando
- [ ] Documentação XML gerada
- [ ] Build em Release bem-sucedido
- [ ] Pacote criado e validado
- [ ] Release notes preparadas

## Troubleshooting

### Erro de autenticação
Verifique se sua API Key está correta e não expirou.

### Pacote já existe
Você não pode sobrescrever um pacote já publicado. Incremente a versão.

### Symbol package
Para incluir símbolos de debug:
```bash
dotnet pack -c Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg
```
