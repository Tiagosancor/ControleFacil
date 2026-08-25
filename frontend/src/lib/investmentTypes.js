// Espelha o catálogo do backend (ControleFacil.Domain.Enums.InvestmentTypeCatalog) —
// os values aqui precisam bater exatamente com os nomes dos enums InvestmentGroup/
// InvestmentType, já que são enviados como string pra API.

export const INVESTMENT_GROUPS = [
  { value: 'RendaFixa', label: 'Renda Fixa' },
  { value: 'RendaVariavel', label: 'Renda Variável' },
  { value: 'FundoInvestimento', label: 'Fundo de Investimento' },
  { value: 'PrevidenciaPrivada', label: 'Previdência Privada' },
  { value: 'Outros', label: 'Outros' },
]

export const INVESTMENT_TYPES_BY_GROUP = {
  RendaFixa: [
    { value: 'TesouroDireto', label: 'Tesouro Direto' },
    { value: 'CDB', label: 'CDB' },
    { value: 'RDB', label: 'RDB' },
    { value: 'LCI', label: 'LCI' },
    { value: 'LCA', label: 'LCA' },
    { value: 'CRI', label: 'CRI' },
    { value: 'CRA', label: 'CRA' },
    { value: 'Debenture', label: 'Debênture' },
    { value: 'Poupanca', label: 'Poupança' },
  ],
  RendaVariavel: [
    { value: 'Acoes', label: 'Ações' },
    { value: 'FII', label: 'FII' },
    { value: 'ETF', label: 'ETF' },
    { value: 'BDR', label: 'BDR' },
    { value: 'Opcoes', label: 'Opções' },
  ],
  FundoInvestimento: [
    { value: 'FundoMultimercado', label: 'Fundo Multimercado' },
    { value: 'FundoAcoes', label: 'Fundo de Ações' },
    { value: 'FundoRendaFixa', label: 'Fundo de Renda Fixa' },
    { value: 'FundoCambial', label: 'Fundo Cambial' },
    { value: 'FundoInfraestrutura', label: 'Fundo de Infraestrutura (FI-Infra)' },
  ],
  PrevidenciaPrivada: [
    { value: 'PGBL', label: 'PGBL' },
    { value: 'VGBL', label: 'VGBL' },
  ],
  Outros: [
    { value: 'COE', label: 'COE' },
    { value: 'Criptomoeda', label: 'Criptomoeda' },
    { value: 'Outros', label: 'Outros' },
  ],
}

export const GROUPS_WITH_INTEREST_RATE = ['RendaFixa', 'PrevidenciaPrivada']

// Tipos com busca de ativo real via brapi.dev (autocomplete de ticker) — os demais
// continuam com campo de nome livre.
export const BRAPI_TYPE_BY_INVESTMENT_TYPE = {
  Acoes: 'stock',
  FII: 'fund',
}

export function groupOfType(type) {
  for (const [group, types] of Object.entries(INVESTMENT_TYPES_BY_GROUP)) {
    if (types.some(t => t.value === type)) return group
  }
  return null
}

export function typeLabel(type) {
  for (const types of Object.values(INVESTMENT_TYPES_BY_GROUP)) {
    const found = types.find(t => t.value === type)
    if (found) return found.label
  }
  return type
}

export function groupLabel(group) {
  return INVESTMENT_GROUPS.find(g => g.value === group)?.label || group
}
