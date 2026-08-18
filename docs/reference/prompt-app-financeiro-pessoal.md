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

## 3. Escopo e status das sprints

### Concluídas e validadas (não retrabalhar sem motivo explícito)

1. **Sprint A — Schema**: modelagem do banco de dados, migrations do EF Core, seed de dados de exemplo. ✅ Concluída.
2. **Sprint B — Núcleo**: endpoints (Minimal API) + páginas Next.js para CRUD de categorias, contas bancárias e lançamentos, com autenticação JWT e escopo por usuário. ✅ Concluída e validada manualmente (isolamento entre usuários testado, inclusive tentativa de acesso direto por ID).
3. **Sprint C — Mobile (React Native/Expo)**: app mobile consumindo a mesma API, autenticação com `expo-secure-store`, CRUD do núcleo replicado. ✅ Concluída.
4. **Sprint D — Dashboard (resumo mensal)**: endpoint de agregação (receita/despesa/saldo do mês + agrupamento por categoria), página `/dashboard` com gráfico. ✅ Concluída. *(Nota: foi implementada antes da Sprint D.1, fora da ordem original do prompt — D.1 continua pendente e deve cobrir a página do dashboard também, não só as telas anteriores.)*
5. **Sprint E — Deploy**: backend no Render (`https://controlefacil.onrender.com`), banco no Neon (Postgres 17, região São Paulo), frontend na Vercel (`https://controle-facil-alpha.vercel.app`). ✅ Concluída e validada manualmente (registro, login, CRUD completo e persistência testados em produção).

### Próxima sprint (fazer AGORA)

**Sprint D.1 — Responsividade mobile e loading states do frontend web**
- Cobre **todas** as páginas existentes, incluindo o `/dashboard` que acabou de ser implementado na Sprint D (fora de ordem — foi implementado antes desta sprint, então também precisa passar por esta revisão).
- As páginas já existentes (login, cadastro, categorias, contas bancárias, lançamentos) não estão responsivas em telas pequenas — validado testando no navegador de um celular físico.
- Revisar todas as páginas existentes usando classes responsivas do Tailwind (`sm:`, `md:`, `lg:`), com **375px de largura como referência mínima** de teste (tamanho comum de tela de celular).
- Confirmar que existe a meta tag de viewport correta (`width=device-width, initial-scale=1`) em `_document.js` ou `_app.js`.
- Atenção especial a: tabelas de listagem (Lançamentos principalmente, que tem muitas colunas — considerar layout de cards em telas pequenas em vez de tabela horizontal), formulários de criação/edição (inputs não podem ultrapassar a largura da tela), e a página de login/cadastro.
- Testar redimensionando a janela do navegador ou usando o modo de dispositivo do DevTools (F12 → device toolbar) simulando um iPhone/Android padrão, antes de considerar concluído.
- **Loading state entre navegações**: hoje a troca de página (ex: Categorias → Lançamentos, ou submit de formulário) não dá nenhum feedback visual enquanto os dados carregam, o que parece travamento. Implementar:
  - Um indicador de carregamento global de navegação entre rotas do Next.js (ex: usando os eventos `routeChangeStart`/`routeChangeComplete` do `next/router` para mostrar uma barra de progresso no topo, ou um spinner simples).
  - Estado de loading local em cada página que busca dados da API (listagens e formulários), desabilitando botões de submit durante a requisição e mostrando um indicador (skeleton ou spinner) em vez de tela vazia/quebrada enquanto os dados não chegam.
  - Isso vale tanto para requisições rápidas (evita "flash" de conteúdo vazio) quanto para quando a API estiver com cold start em produção no Render (onde a demora é perceptível, então o feedback visual é ainda mais importante).
- Isso é ajuste de UI apenas — não deve alterar nenhum contrato de API nem lógica de negócio.

### Backlog (não iniciar sem decisão explícita futura)

Documentado aqui só para não perder o contexto do que existe na planilha original — **não implementar** enquanto não houver uma nova instrução explícita priorizando algum destes itens:
- Metas de longo prazo (`CF_LP` da planilha) — objetivos com cálculo de aporte mensal necessário
- Investimentos (`CF_inv`) — acompanhamento mensal por categoria de bem
- Controle de faturas por cartão (`CF_fat`)
- Relatórios (DRE, contas a pagar/receber — abas `REL_*`)

Se, ao implementar o Dashboard (Sprint D), você perceber que alguma decisão de schema vai dificultar esses itens do backlog no futuro, pode ajustar — mas registre a decisão em comentário no código ou no PR.

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

## 8. Definition of Done

### Sprint A + B + C + D + E — ✅ já cumprido (referência histórica)

- [x] Migrations do EF Core criam todas as tabelas sem erro
- [x] Seed popula usuário de teste, plano de contas completo, contas bancárias de exemplo
- [x] Endpoints CRUD completos, autenticados e escopados por usuário
- [x] Validação de entrada funcionando
- [x] Testes xUnit passando
- [x] Páginas Next.js de Categorias, Contas Bancárias e Lançamentos
- [x] App mobile (Expo) replicando o núcleo, autenticação com token seguro
- [x] Isolamento por usuário validado manualmente (web e mobile)
- [x] Endpoint de resumo mensal + agrupamento por categoria
- [x] Página `/dashboard` com gráfico
- [x] Backend publicado no Render, banco no Neon, frontend na Vercel — todos acessíveis via HTTPS
- [x] Variáveis de ambiente de produção configuradas fora do repositório
- [x] CORS ajustado para a origem de produção
- [x] Fluxo completo testado em produção (registro, login, CRUD, persistência)

### Sprint D.1 — Responsividade e Loading States (pendente)

- [ ] Todas as páginas existentes (login, cadastro, categorias, contas bancárias, lançamentos, **dashboard**) testadas em viewport de 375px sem quebra de layout, texto cortado ou scroll horizontal indevido
- [ ] Meta tag de viewport confirmada
- [ ] Tabela de Lançamentos legível/utilizável em tela pequena (cards ou scroll horizontal controlado, não simplesmente cortada)
- [ ] Formulários utilizáveis em tela pequena (inputs não ultrapassam a largura da tela, botões acessíveis sem zoom)
- [ ] Indicador de progresso/loading visível durante navegação entre páginas (ex: barra de progresso no topo)
- [ ] Toda página que busca dados da API mostra estado de loading (spinner/skeleton) em vez de tela vazia enquanto aguarda resposta
- [ ] Botões de submit desabilitados durante requisições em andamento (evita duplo clique/duplo submit)
- [ ] Testado em pelo menos um navegador mobile real, não só no DevTools

---

## 9. Antes de começar

Se qualquer um destes pontos não estiver claro, **pergunte antes de implementar**:
1. Se já existe um repositório/estrutura de pastas iniciada para este projeto, ou se é para criar do zero.
2. Se o `AppDbContext` e padrão de repositório do ImobiCrm devem ser replicados literalmente aqui, ou se há liberdade para ajustar.
3. Se a autenticação deve reaproveitar algum provedor já usado no ImobiCrm ou começar do zero.
