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
frontend/
  src/
    pages/                          # login, register, categories, bank-accounts, transactions
    components/                     # Layout, AppLayout, FormInput, FormSelect, ui/*
    contexts/AuthContext.js
    services/                       # authService, categoryService, bankAccountService, transactionService
    lib/api.js                      # instância axios com token JWT
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

## Endpoints (Sprint B)

Todos exigem `Authorization: Bearer <token>`, exceto `/api/auth/register` e `/api/auth/login`.

- `POST /api/auth/register`, `POST /api/auth/login` (rate limit: 5/min), `GET /api/auth/me`
- `GET/POST /api/categories`, `GET/PUT/DELETE /api/categories/{id}` — subcategoria sempre herda o `Type` do grupo pai; hierarquia limitada a 2 níveis; `DELETE` é soft-delete (`IsActive=false`)
- `GET/POST /api/bank-accounts`, `GET/PUT/DELETE /api/bank-accounts/{id}` — `DELETE` também é soft-delete
- `GET/POST /api/transactions`, `GET/PUT/DELETE /api/transactions/{id}` — listagem aceita filtros `year`, `month`, `categoryId`, `bankAccountId`, `status`; `POST` com `totalInstallments > 1` gera as N parcelas de uma vez (agrupadas por `TransactionSeries`)
- `DELETE /api/transactions/series/{seriesId}` — cancela a série inteira (todas as parcelas)

Todas as listagens são paginadas (`page`, `pageSize`) e escopadas ao usuário autenticado — acessar um recurso de outro usuário retorna `404`, não `403`, para não revelar a existência do dado.

## Rodando o frontend

```bash
cd frontend
npm install
npm run dev
```

Sobe em `http://localhost:3000`, apontando para a API em `http://localhost:5000` por padrão (configurável via `NEXT_PUBLIC_API_URL`). Faça login com o usuário de seed acima.

> **Nota de segurança**: o frontend está pinado em Next.js 13 (major exigida pela especificação, Pages Router). Essa linha tem vulnerabilidades conhecidas só corrigidas na v16 (breaking change de stack). Aceitável para este projeto de portfólio/uso local; reavaliar antes de qualquer deploy público.

## Rodando os testes

```bash
cd backend
dotnet test
```

Cobrem: herança do `Type` da subcategoria a partir do grupo pai, limite de 2 níveis de hierarquia, e escopo por usuário (um usuário não acessa/edita/apaga categorias, contas ou lançamentos de outro).
