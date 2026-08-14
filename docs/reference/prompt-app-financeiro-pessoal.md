# Prompt para Agente de Implementação — App de Controle Financeiro Pessoal

## 1. Contexto

Este é um projeto pessoal de portfólio, construído com o mesmo padrão de qualidade e arquitetura do projeto **ImobiCrm** (CRM SaaS para corretores de imóveis, também no meu GitHub `Tiagosancor/ImobiCrm`). O objetivo é ter um App de controle financeiro pessoal, inspirado em uma planilha de orçamento pessoal (plano de contas hierárquico, controle de lançamentos mensais, contas bancárias, metas e investimentos).

**Você (agente) deve seguir este documento como especificação principal.** Se algo estiver ambíguo ou em conflito, pare e pergunte antes de assumir.

---

## 2. Stack técnica (obrigatória, não trocar sem confirmar)

- **Backend**: ASP.NET Core 8, **Minimal APIs** (não usar Controllers)
- **ORM**: Entity Framework Core
- **Banco de dados**: PostgreSQL, rodando via Docker na **porta 5433** (para não conflitar com instalação nativa do Windows)
- **Frontend Web**: Next.js 13 (**Pages Router**, não App Router), Tailwind CSS
- **Alias de import no frontend web**: `@/` apontando para `./src/*`
- **Mobile** *(Sprint C, ver seção 3)*: React Native com Expo, consumindo a mesma API REST do backend (sem endpoints separados)
- **Testes**: xUnit (backend)
- **Containerização**: Docker + docker-compose (API + Postgres; frontends podem rodar fora do compose em dev)

---

## 3. Escopo desta fase (fazer AGORA)

Nesta fase, implementar **apenas**:

1. **Sprint A — Schema**: modelagem completa do banco de dados (entidades abaixo), migrations do EF Core, seed mínimo de dados de exemplo.
2. **Sprint B — Núcleo**: endpoints (Minimal API) + páginas Next.js para CRUD de:
   - Contas do plano de contas (categorias hierárquicas de receita/despesa)
   - Contas bancárias
   - Lançamentos (transações)

**Fora de escopo nesta fase** (não implementar ainda, apenas deixar o schema preparado para não exigir migração destrutiva depois):
- Dashboard com gráficos
- Metas de longo prazo (`CF_LP` da planilha)
- Investimentos (`CF_inv`)
- Controle de faturas por cartão (`CF_fat`)
- Relatórios (DRE, contas a pagar/receber)
- **App mobile** (ver Sprint C abaixo — só começa depois que o Sprint B estiver validado)

Se você (agente) perceber que alguma decisão de schema nesta fase vai dificultar essas features futuras, pode ajustar — mas registre a decisão em um comentário no código ou no PR.

### Sprint C — Mobile (React Native/Expo) — só depois do Sprint B validado

Não iniciar esta sprint sem confirmação explícita de que o Sprint B (web) está funcionando e validado. Quando chegar a vez:
- O app mobile consome a **mesma API** do Sprint B, sem endpoint dedicado.
- Reaproveitar as DTOs/contratos já existentes — se algo precisar mudar na API pra atender o mobile, é sinal de que o contrato do Sprint B não estava "client-agnostic" (ver regra na seção 5) e deve ser corrigido com cuidado, avaliando impacto no web.
- Autenticação: mesmo fluxo JWT do backend; no mobile, armazenar o token em local seguro (ex: `expo-secure-store`), nunca em `AsyncStorage` puro.
- Escopo funcional do mobile nesta primeira leva: as mesmas telas do núcleo (contas, bancos, lançamentos) — não adiantar dashboard/metas/investimentos no mobile antes de existirem no web.

---

## 4. Modelo de domínio (extraído da planilha de referência)

A planilha original tem estas partes relevantes, que devem virar entidades:

### 4.1 `Category` (Plano de Contas — abas `PC_rec` e `PC_des`)

Estrutura hierárquica de 2 níveis: Grupo (ex: "DESPESAS COM MORADIA") → Subcategoria (ex: "Aluguel", "Condomínio").

Campos:
- `Id` (Guid ou int, seguir o padrão já usado no ImobiCrm)
- `Name` (string, obrigatório)
- `Type` (enum: `Income` | `Expense` — corresponde a "Receita"/"Despesa" na planilha)
- `ParentCategoryId` (FK nullable, auto-relacionamento — grupo pai; null = é um grupo raiz)
- `UserId` (FK — ver seção 4.4 sobre multiusuário)
- `IsActive` (bool, default true — permitir "desativar" categoria sem deletar histórico)

Regra de negócio: uma categoria filha deve ter o mesmo `Type` da categoria pai.

### 4.2 `BankAccount` (aba `PC_bancos`)

Campos:
- `Id`
- `Name` (string — ex: "Banco 1", "Caixinha")
- `InitialBalance` (decimal)
- `UserId`
- `IsActive` (bool)

### 4.3 `Transaction` (lançamento — abas `JAN` a `DEZ`)

Este é o coração do sistema. Da planilha, os campos relevantes são:

| Campo na planilha | Campo na entidade | Tipo | Observação |
|---|---|---|---|
| Data do Lançamento | `EntryDate` | DateOnly | data em que o lançamento foi registrado |
| Classificação + Grupo de Contas | `CategoryId` | FK | referência à `Category` (nível folha) |
| Item (Descrição) | `Description` | string | |
| Forma de Pgto | `PaymentMethod` | enum (`Cash`, `Debit`, `Credit`, ...) | ajustar enum conforme necessidade real |
| Banco | `BankAccountId` | FK | |
| Valor | `Amount` | decimal | sempre positivo; o sinal (entrada/saída) vem do `Category.Type` |
| Data de Pagamento | `PaymentDate` | DateOnly, nullable | pode ser diferente da data do lançamento |
| Pago ou não pago | `Status` | enum (`Paid`, `Pending`) | |
| repet / parcelas | `InstallmentNumber`, `TotalInstallments` | int, nullable | suportar lançamentos parcelados (ex: "2/5") |
| — | `UserId` | FK | |
| — | `CreatedAt`, `UpdatedAt` | DateTime | auditoria |

Regra de negócio importante: **um lançamento parcelado/recorrente na planilha gera várias linhas** (uma por mês, ligadas por um número de repetição). Ao implementar, crie uma tabela auxiliar `TransactionSeries` (ou campo `SeriesId` na própria `Transaction`) para agrupar parcelas da mesma compra/recorrência, em vez de duplicar tudo sem vínculo. Isso facilita editar/cancelar a série inteira depois.

### 4.4 Usuário e escopo de dados

A planilha é de uso pessoal (um usuário), mas para servir de peça de portfólio e seguir boas práticas de segurança, implemente:

- Entidade `User` com autenticação (ver seção 5).
- **Todas** as entidades acima (`Category`, `BankAccount`, `Transaction`) devem ter `UserId` e todas as queries devem ser filtradas por usuário autenticado — nunca confiar em um `userId` vindo do corpo da requisição, sempre extrair do token/claims.

---

## 5. Arquitetura obrigatória (Clean Architecture dentro do projeto Minimal API)

Mesmo usando Minimal APIs, **não colocar tudo no `Program.cs` ou em endpoints "gordos"**. Separar em camadas, seguindo o padrão abaixo (adaptar nomes de pastas ao que já existe no ImobiCrm, se houver convenção lá):

```
/src
  /Domain
    /Entities        -> Category, BankAccount, Transaction, User, etc. (POCOs, sem dependência de EF)
    /Enums
    /Interfaces       -> IRepository<T>, IUnitOfWork, etc.
  /Application
    /DTOs             -> Request/Response DTOs (NUNCA expor entidades de domínio direto na API)
    /Services         -> regras de negócio (ex: CategoryService, TransactionService)
    /Validators       -> FluentValidation (ou Data Annotations, se for o padrão do ImobiCrm)
    /Mappings          -> AutoMapper profiles ou mapeamento manual
  /Infrastructure
    /Data
      AppDbContext.cs
      /Configurations  -> EF Core Fluent API (IEntityTypeConfiguration<T> por entidade)
      /Migrations
    /Repositories      -> implementação concreta dos repositórios
  /Api
    /Endpoints         -> um arquivo por grupo de recurso (ex: CategoryEndpoints.cs, TransactionEndpoints.cs)
    Program.cs
```

Regras:
- **Endpoints não devem conter lógica de negócio.** Endpoint recebe DTO → chama Service → retorna DTO.
- **DTOs de entrada e saída são obrigatórios** (nunca serializar a entidade de domínio direto na resposta).
- Usar `IEntityTypeConfiguration<T>` para cada entidade (não Data Annotations dentro da entidade de domínio, para manter o domínio "limpo").
- **DTOs devem ser "client-agnostic"**: não moldar formato de resposta pensando só na tela web do Next.js. Nomes de campos, formatos de data (ISO 8601) e paginação devem ser genéricos o suficiente para o app mobile (Sprint C) consumir sem exigir mudança de contrato depois.

---

## 6. Padrões de segurança obrigatórios

1. **Autenticação**: JWT (ou ASP.NET Identity + JWT). Endpoints de `Category`, `BankAccount`, `Transaction` exigem usuário autenticado (`[Authorize]` / `RequireAuthorization()`).
2. **Escopo por usuário**: toda query deve filtrar por `UserId` do usuário autenticado (via `ClaimsPrincipal`), nunca por parâmetro vindo do cliente.
3. **Validação de entrada**: validar todos os DTOs antes de tocar no banco (FluentValidation recomendado). Rejeitar valores negativos onde não fizer sentido (`Amount <= 0`, datas inválidas, etc.).
4. **Senhas**: nunca armazenar em texto puro — usar hashing (BCrypt ou o hasher padrão do ASP.NET Identity).
5. **Segredos**: connection string, JWT secret, etc. **nunca commitados no repositório**. Usar `appsettings.Development.json` no `.gitignore` + variáveis de ambiente / `dotnet user-secrets` em dev, e variáveis de ambiente em produção. Confirmar que existe um `.env.example` sem valores reais.
6. **CORS**: configurar explicitamente para aceitar apenas a origem do frontend (não usar `AllowAnyOrigin` em produção).
7. **SQL Injection**: garantido pelo uso do EF Core com LINQ (não usar SQL raw concatenado; se precisar de SQL raw, usar parâmetros).
8. **Rate limiting básico** nos endpoints de autenticação (login), para mitigar força bruta.
9. **Logs**: não logar dados sensíveis (senhas, tokens) em texto puro.

---

## 7. Convenções de código (seguir à risca)

- **Nomenclatura de entidades**: inglês, PascalCase (`Category`, `BankAccount`, `Transaction`), mesmo padrão do ImobiCrm.
- **Commits**: Conventional Commits (`feat:`, `fix:`, `refactor:`, `chore:`), no imperativo, em português, sem ponto final. Ex: `feat: adiciona entidade Category com hierarquia de grupos`.
- **Commit por escopo**: quando a mudança tocar múltiplas áreas (schema + endpoint + página), faça commits separados por arquivo/área, não um commit monolítico.
- **Docker**: PostgreSQL na porta **5433** no `docker-compose.yml`.
- **Alias de frontend**: `@/` para `./src/*`.

---

## 8. Definition of Done (Sprint A + B)

Considerar esta fase concluída quando:

- [ ] Migrations do EF Core criam todas as tabelas (`Users`, `Categories`, `BankAccounts`, `Transactions`) sem erro
- [ ] Seed popula pelo menos: 1 usuário de teste, categorias raiz de receita/despesa espelhando os grupos da planilha (Renda Familiar, Despesas com Moradia, Alimentação, Saúde, Transporte, Lazer, etc.), 2-3 contas bancárias de exemplo
- [ ] Endpoints CRUD completos (GET listagem com paginação, GET por id, POST, PUT, DELETE) para `Category`, `BankAccount` e `Transaction`, todos autenticados e escopados por usuário
- [ ] Validação de entrada funcionando (testar payload inválido retorna 400 com mensagem clara)
- [ ] Testes xUnit cobrindo pelo menos: regra de categoria filha herdar o `Type` do pai, e escopo por usuário (usuário A não consegue ver/editar dado do usuário B)
- [ ] Páginas Next.js: listagem + criação + edição para Categorias, Contas Bancárias e Lançamentos (pode reaproveitar padrão visual do módulo de Clientes do ImobiCrm)
- [ ] `README.md` do projeto documentando como subir o ambiente (Docker Compose) e rodar migrations
- [ ] Nenhum segredo commitado no repositório

---

## 9. Antes de começar

Se qualquer um destes pontos não estiver claro, **pergunte antes de implementar**:
1. Se já existe um repositório/estrutura de pastas iniciada para este projeto, ou se é para criar do zero.
2. Se o `AppDbContext` e padrão de repositório do ImobiCrm devem ser replicados literalmente aqui, ou se há liberdade para ajustar.
3. Se a autenticação deve reaproveitar algum provedor já usado no ImobiCrm ou começar do zero.
