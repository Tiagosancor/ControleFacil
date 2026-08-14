# Referência de Dados — Planilha de Orçamento Pessoal

> Este arquivo é uma extração limpa da planilha original (`Planilha_Orçamento_Pessoal.xlsx`), contendo apenas o que é relevante para modelar o schema do app. Usar em conjunto com `prompt-app-financeiro-pessoal.md`.

---

## 1. Plano de Contas — Receitas (Category, Type = Income)

| Grupo | Subcategorias |
|---|---|
| **RENDA FAMILIAR** | Salários, Bônus, Comissões, Participação em lucros, 13º Salário, Férias, Outras fontes de renda familiar |
| **RECEITAS FINANCEIRAS** | Poupança, Títulos, Ações, Outras receitas com aplicações |
| **OUTRAS RECEITAS** | Herança, Aluguel de imóveis, Outras receitas gerais |

## 2. Plano de Contas — Despesas (Category, Type = Expense)

| Grupo | Subcategorias |
|---|---|
| **DESPESAS COM MORADIA** | Condomínio, Aluguel, Conta de luz, Conta de gás, Conta de água, TV por assinatura, Telefone / Internet, Empregada, Diarista, Babá, Reforma, Compra de móveis, Decoração, IPTU, Eletrodomésticos |
| **DESPESAS COM ALIMENTAÇÃO** | Compras no supermercado, Compras na padaria, Cafés da manhã, Almoços, Lanches, Jantares, Feira |
| **DESPESAS COM SAÚDE** | Plano de saúde, Consulta com médicos, Consulta com dentista, Remédios, Seguro de vida, Seguro funeral |
| **DESPESAS COM TRANSPORTES** | Combustível, Seguro veicular, Estacionamento, IPVA, Oficina mecânica, Táxi, Uber / Cabify, Revisão, Transporte público, Pedágios, Multas, Lavagem |
| **DESPESAS COM LAZER** | Viagens, Alimentação, Cinema, Teatro, Museus, Esportes, Passeios, Livros e revistas, Academia, Clube, Salão de beleza, Vestuário, Cursos |
| **DESPESAS COM DEPENDENTES** | Escola, Faculdade, Livros didáticos, Serviços jurídicos, Roupas, Despesas bancárias, Mesada, Cursos extras |
| **IMPOSTOS** | Imposto de renda, INSS |
| **INVESTIMENTOS (BENS)** *(fora de escopo agora — ver `CF_inv` na planilha original)* | Imóveis, Veículos, Aplicações financeiras, Previdência privada |

> Observação: cada subcategoria pertence a um único grupo, e o grupo define o `Type` (Income/Expense) — não existe subcategoria com tipo diferente do grupo pai.

---

## 3. Contas Bancárias (BankAccount) — exemplo de seed

| Nome | Saldo Inicial |
|---|---|
| Banco 1 | 2000 |
| Banco 2 | 5000 |
| Caixinha | 200 |

> A planilha permite cadastrar mais contas (o modelo é uma lista simples, sem limite fixo). Usar como seed mínimo de exemplo, não como lista fechada.

---

## 4. Estrutura de um Lançamento (Transaction) — extraído das abas mensais (JAN a DEZ)

Colunas usadas, na ordem em que aparecem na planilha original, com o nome de campo sugerido para a entidade:

| Coluna na planilha | Campo sugerido | Tipo | Exemplo de valor |
|---|---|---|---|
| Data do Lançamento | `EntryDate` | Date | 2017-01-05 |
| Classificação | — (usado só para achar a `Category`) | string | "RENDA FAMILIAR" |
| Grupo de Contas | `CategoryId` (via nome da subcategoria) | FK | "Salários" |
| Item (Descrição) | `Description` | string | "Primeira metade" |
| Forma de Pgto | `PaymentMethod` | enum | "À Vista", "Crédito" |
| Banco | `BankAccountId` (via nome) | FK | "Banco 1" |
| Valor | `Amount` | decimal | 7500 |
| Data de Pagamento | `PaymentDate` | Date, nullable | 2017-01-05 |
| Pago ou não pago | `Status` | enum | "Pago", "Não pago" |
| repet (nº da parcela) | `InstallmentNumber` | int, nullable | 1 |
| — | `TotalInstallments` | int, nullable | *(inferir por contagem de linhas da mesma série na planilha original, se necessário)* |

### Exemplos de linhas reais (para popular o seed com dados plausíveis)

1. `2017-01-05` | RENDA FAMILIAR → Salários | "Primeira metade" | À Vista | Banco 1 | **7500** | pago em `2017-01-05` | Pago
2. `2017-01-06` | DESPESAS COM MORADIA → Aluguel | "Pago ao Daniel" | À Vista | Banco 1 | **3000** | pago em `2017-01-06` | Pago

> Regra de sinal: o valor (`Amount`) é sempre armazenado positivo; se é entrada ou saída depende do `Category.Type` (Income soma, Expense subtrai no saldo).

---

## 5. Fora de escopo nesta fase (documentado só para não perder o contexto depois)

Estas abas existem na planilha original e **não** devem ser modeladas agora — só mencionadas aqui para o agente não recriar do zero quando chegar a hora:

- `META`: metas mensais de receita (valor planejado x realizado)
- `CF_LP`: objetivos de longo prazo (ex: "Carro", "Casa") com cálculo de rentabilidade/aporte mensal necessário
- `CF_inv`: acompanhamento mensal de investimentos por categoria (Imóveis, Veículos, etc.)
- `CF_fat`: controle de faturas por banco/cartão
- `REL_dre`, `REL_cp_cr`, `REL_RI`: relatórios (DRE, contas a pagar/receber)
- `DASH1` a `DASH6`: dashboards visuais
