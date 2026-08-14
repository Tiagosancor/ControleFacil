# ControleFacil

App de controle financeiro pessoal, inspirado em uma planilha de orçamento pessoal (plano de contas hierárquico, contas bancárias, lançamentos). Ver especificação completa em [docs/reference/prompt-app-financeiro-pessoal.md](docs/reference/prompt-app-financeiro-pessoal.md).

## Stack

- **Backend**: ASP.NET Core 8 (Minimal APIs), EF Core, PostgreSQL
- **Frontend**: Next.js 13 (Pages Router), Tailwind CSS
- **Arquitetura**: Clean Architecture (Domain / Application / Infrastructure / Api)

## Estrutura

```
backend/
  ControleFacil.sln
  src/
    ControleFacil.Domain/          # entidades, enums, interfaces de repositório
    ControleFacil.Application/     # DTOs, services, validators, interfaces
    ControleFacil.Infrastructure/  # EF Core, migrations, repositórios, auth
    ControleFacil.Api/             # Minimal API endpoints, Program.cs
  tests/
    ControleFacil.Api.Tests/
frontend/                          # Next.js (Sprint B)
```

## Subindo o ambiente (Sprint A)

1. Copie o arquivo de variáveis de ambiente e ajuste os valores:

   ```bash
   cp .env.example .env
   ```

2. Suba o PostgreSQL (porta **5433** no host, para não conflitar com instalação nativa):

   ```bash
   docker compose up -d db
   ```

3. Configure a connection string de desenvolvimento em `backend/src/ControleFacil.Api/appsettings.Development.json` (arquivo local, fora do controle de versão):

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5433;Database=controlefacil_dev;Username=postgres;Password=postgres"
     },
     "Jwt": { "Key": "uma-chave-de-desenvolvimento-qualquer" }
   }
   ```

4. Aplique as migrations:

   ```bash
   cd backend
   dotnet ef database update \
     --project src/ControleFacil.Infrastructure/ControleFacil.Infrastructure.csproj \
     --startup-project src/ControleFacil.Api/ControleFacil.Api.csproj
   ```

5. Rode a API (o seed de dados de exemplo roda automaticamente na inicialização, se o banco estiver vazio):

   ```bash
   dotnet run --project src/ControleFacil.Api/ControleFacil.Api.csproj
   ```

   A API sobe em `http://localhost:5000` (dev) e expõe `GET /health`.

### Subindo tudo via Docker Compose (API + Postgres)

```bash
docker compose up -d
```

A API expõe a porta `5080` no host (mapeada para `8080` no container) e aplica migrations/seed automaticamente ao iniciar.

## Dados de seed

- 1 usuário de teste: `teste@controlefacil.com` / `Teste@123`
- Plano de contas completo (grupos e subcategorias de receita/despesa da planilha de referência)
- 3 contas bancárias de exemplo (Banco 1, Banco 2, Caixinha)
- 2 lançamentos de exemplo

## Rodando os testes

```bash
cd backend
dotnet test
```
