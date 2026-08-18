# ControleFacil

App de controle financeiro pessoal, inspirado em uma planilha de orçamento pessoal (plano de contas hierárquico, contas bancárias, lançamentos, dashboard mensal).

> 🔗 **Versão publicada**: _(pendente — ver [Deploy](#deploy-sprint-e) abaixo)_. Login de demonstração: `teste@controlefacil.com` / `Teste@123`.

## Stack

- **Backend**: ASP.NET Core 8 (Minimal APIs), EF Core, PostgreSQL
- **Frontend Web**: Next.js 13 (Pages Router), Tailwind CSS
- **Mobile**: React Native + Expo (Expo Router, NativeWind), consumindo a mesma API REST do backend
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
mobile/
  app/                              # rotas (Expo Router) — login, register, (app)/{transactions,categories,bank-accounts}
  components/                       # FormInput, FormSelect, LogoutButton, ui/*
  contexts/AuthContext.js
  services/                         # mesmos services do frontend, adaptados (axios + expo-secure-store)
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
- `POST /api/auth/forgot-password` (rate limit: 5/min) — sempre responde com sucesso genérico, mesmo se o e-mail não existir; gera um token de reset de uso único (expira em 45 min) e envia por e-mail via Resend
- `POST /api/auth/reset-password` — recebe `token` + `newPassword`; token inválido, expirado ou já usado retorna `400`
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

## Configurando o Resend (recuperação de senha)

O fluxo de "esqueci minha senha" (`/forgot-password`, `/reset-password`) envia o e-mail de recuperação via [Resend](https://resend.com) (free tier, sem cartão de crédito), com o domínio `semeiagrana.com.br` verificado — remetente `Semeia Grana <naoresponda@semeiagrana.com.br>`.

1. Crie uma conta gratuita em [resend.com](https://resend.com) e verifique o domínio de envio (adicionar os registros DNS indicados pelo Resend; a propagação pode levar até ~2h).
2. Em **API Keys**, gere uma chave e configure-a **apenas como variável de ambiente**, nunca commitada:
   - **Dev local**: em `backend/src/ControleFacil.Api/appsettings.Development.json` (arquivo local, fora do controle de versão), na seção `Resend.ApiKey`; ou via `RESEND_API_KEY` no `.env` da raiz, se for subir pelo `docker compose`.
   - **Produção (Render)**: variável de ambiente `Resend__ApiKey` (mesmo padrão do `Jwt__Key`).
3. `Resend.FromEmail`/`Resend.FromName` (em `appsettings.json`, não são segredo) controlam o remetente exibido — hoje `naoresponda@semeiagrana.com.br` / `Semeia Grana`. Enquanto um domínio próprio não está verificado numa nova instalação deste projeto, use `onboarding@resend.dev` como remetente temporário (o free tier só entrega, nesse caso, para o e-mail cadastrado na própria conta Resend).

## Rodando o app mobile

```bash
cd mobile
npm install
cp .env.example .env   # ajuste EXPO_PUBLIC_API_URL se necessário (ver comentários no arquivo)
npx expo start
```

Escaneie o QR code com o app **Expo Go** (Android/iOS) ou pressione `a`/`i` para abrir num emulador. Faça login com o usuário de seed acima.

- **Emulador Android**: `localhost` não aponta pra sua máquina — use `http://10.0.2.2:5000` no `.env`.
- **Dispositivo físico**: use o IP da sua máquina na rede local (ex. `http://192.168.0.10:5000`); o dispositivo precisa estar na mesma rede.
- **Token JWT**: armazenado via `expo-secure-store` (Keychain no iOS, Keystore no Android) — nunca em `AsyncStorage` puro, conforme exigido na especificação.
- **Testando via `expo start --web`** (usado para validar este Sprint C sem simulador disponível no ambiente de desenvolvimento): `expo-secure-store` não tem implementação web, então `services/api.js` cai para `localStorage` só nesse alvo — não é o caminho usado em iOS/Android. Além disso, o navegador aplica CORS (que não existe no app nativo real); adicione a origem do Expo web (por padrão `http://localhost:8081`) em `Cors:AllowedOrigins` no seu `appsettings.Development.json` local se for testar assim.

## Rodando os testes

```bash
cd backend
dotnet test
```

Cobrem: herança do `Type` da subcategoria a partir do grupo pai, limite de 2 níveis de hierarquia, escopo por usuário, e o cálculo do resumo mensal do dashboard.

## Deploy (Sprint E)

Banco no **Neon** (Postgres serverless), backend na **Render** (Docker), frontend web na **Vercel**. Os três fazem deploy direto do GitHub (push na `main` → redeploy automático), sem precisar de CLI local. Este é um monorepo, então tanto na Render quanto na Vercel é preciso apontar o **Root Directory** do serviço para a pasta certa (`backend` ou `frontend`) — os dois vivem no mesmo repositório.

### 1. Banco de dados (Neon)

1. Crie uma conta em [neon.tech](https://neon.tech) e um projeto Postgres (região São Paulo, por exemplo).
2. No painel do projeto, pegue a connection string (aba **Connect**). O Neon já gera no formato `postgresql://usuario:senha@host/banco?sslmode=require`, mas o Npgsql (.NET) usa uma sintaxe própria — monte a `ConnectionStrings__DefaultConnection` assim, usando os mesmos valores de host/usuário/senha/banco que o Neon te deu:

   ```
   Host=<host-do-neon>;Port=5432;Database=<nome-do-banco>;Username=<usuario>;Password=<senha>;SSL Mode=VerifyFull;Channel Binding=Require
   ```

   `SSL Mode=VerifyFull;Channel Binding=Require` é o nível de segurança que o próprio Neon recomenda para Npgsql (valida certificado e faz channel binding, não só criptografa).
3. Não precisa criar tabelas manualmente — a API aplica as migrations e roda o seed automaticamente na primeira inicialização (mesma lógica do `dotnet run` local).

### 2. Backend (Render)

.NET não tem runtime nativo (buildpack) na Render — só Node, Python, Ruby, Go, Rust e Elixir têm. O caminho suportado é **Docker**, usando o `backend/Dockerfile` que já existe no repo (multi-stage, testado localmente com `docker build`/`docker run` em modo `Production`). Por isso não há "Build Command"/"Start Command" pra preencher — esses campos só existem pros runtimes nativos; num serviço Docker a Render builda pelo próprio Dockerfile e roda o `ENTRYPOINT` dele.

1. Crie uma conta em [render.com](https://render.com) (dá pra usar login do GitHub) e crie um **Web Service** a partir do repositório `Tiagosancor/ControleFacil`.
2. Configure:

   | Campo | Valor |
   |---|---|
   | Language/Environment | **Docker** |
   | Root Directory | `backend` |
   | Dockerfile Path | `Dockerfile` (relativo ao Root Directory) |
   | Docker Context | `.` |
   | Instance Type | Free (ou a de sua preferência) |

3. Em **Environment Variables**, defina:

   | Variável | Valor |
   |---|---|
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `ConnectionStrings__DefaultConnection` | a connection string do Neon montada no passo 1 |
   | `Jwt__Key` | uma string aleatória longa (≥32 caracteres) — **nunca reutilize a de dev** |
   | `Jwt__Issuer` | `ControleFacil` |
   | `Jwt__Audience` | `ControleFacilUsers` |
   | `Cors__AllowedOrigins__0` | a URL da Vercel do passo 3 abaixo (dá pra deixar `http://localhost:3000` por enquanto e ajustar depois) |
   | `Resend__ApiKey` | a API key gerada no [Resend](#configurando-o-resend-recuperação-de-senha) — nunca reutilize chaves de dev em produção |
   | `Frontend__BaseUrl` | a URL da Vercel do passo 3 abaixo (usada para montar o link de `/reset-password?token=...` no e-mail) |

   Não defina `PORT` — a Render injeta essa variável sozinha (10000 por padrão) e a API já lê `PORT` automaticamente (com fallback pra 8080 se não existir, usado no `docker-compose` local).
4. Deploy. A Render expõe a API em algo como `https://controlefacil-api.onrender.com`.
5. Confirme que subiu: `GET https://<seu-dominio>.onrender.com/health` deve responder `{"status":"ok"}`.

> ⚠️ **Cold start do free tier**: no plano gratuito da Render, o serviço "dorme" depois de ~15 minutos sem tráfego e a próxima requisição leva **até ~1 minuto** pra acordar o container (nada quebrado — só lento na primeira chamada depois de um tempo parado). Se for demonstrar o app ao vivo, abra a API alguns minutos antes ou avise sobre essa demora esperada.

### 3. Frontend web (Vercel)

1. Crie uma conta em [vercel.com](https://vercel.com) (login do GitHub) e importe o mesmo repositório.
2. Em **Root Directory**, selecione `frontend`. O preset Next.js é detectado automaticamente (não precisa de `vercel.json`).
3. Em **Environment Variables**, adicione `NEXT_PUBLIC_API_URL` apontando para o domínio da Render (passo 4 acima), ex.: `https://controlefacil-api.onrender.com`.
4. Deploy. A Vercel gera uma URL do tipo `https://controlefacil.vercel.app`.
5. Volte na Render e atualize `Cors__AllowedOrigins__0` para essa URL da Vercel (redeploy manual ou automático conforme a configuração do serviço).

### 4. Apontando o mobile para produção (opcional, só pra demonstrar sem backend local rodando)

O `.env` do mobile é só de desenvolvimento (aponta pro IP da sua máquina na rede local) e não deve ser versionado. Pra demonstrar o app mobile usando a API publicada, troque temporariamente `EXPO_PUBLIC_API_URL` no `mobile/.env` para a URL da Render e rode `npx expo start --clear`. Vale lembrar do cold start acima — a primeira chamada pode demorar.

### Depois do primeiro deploy

- Atualize o link no topo deste README com a URL da Vercel.
- Segredos (`Jwt__Key`, senha do Neon) ficam só nas variáveis de ambiente dos provedores — nunca commitados.
