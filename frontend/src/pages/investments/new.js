import { useEffect, useMemo, useState } from 'react'
import Router from 'next/router'
import AppLayout from '@/components/AppLayout'
import { investmentEntryService } from '@/services/investmentEntryService'
import { investmentCategoryService } from '@/services/investmentCategoryService'
import { INVESTMENT_GROUPS, INVESTMENT_TYPES_BY_GROUP, GROUPS_WITH_INTEREST_RATE, BRAPI_TYPE_BY_INVESTMENT_TYPE, typeLabel } from '@/lib/investmentTypes'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import AssetAutocomplete from '@/components/AssetAutocomplete'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

const NEW_HOLDING = '__new__'

export default function NewInvestmentEntryPage() {
  const now = new Date()
  const [categories, setCategories] = useState([])
  const [group, setGroup] = useState('')
  const [type, setType] = useState('')
  const [holdingId, setHoldingId] = useState('')
  const [newHoldingName, setNewHoldingName] = useState('')
  const [newHoldingInterestRate, setNewHoldingInterestRate] = useState('')
  const [year, setYear] = useState(String(now.getFullYear()))
  const [month, setMonth] = useState(String(now.getMonth() + 1))
  const [value, setValue] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    investmentCategoryService.list({ includeInactive: false }).then(res => setCategories(res.data))
  }, [])

  const typeOptions = group ? INVESTMENT_TYPES_BY_GROUP[group] : []
  const showInterestRate = GROUPS_WITH_INTEREST_RATE.includes(group)
  const brapiType = BRAPI_TYPE_BY_INVESTMENT_TYPE[type]

  // Passo 1 (Categoria) -> Passo 2 (Tipo específico) sempre com as opções fixas do
  // catálogo, iguais às de /investment-categories/new — não filtradas pelo que o usuário
  // já tem cadastrado, senão categorias sem nenhum investimento ainda nunca apareceriam.
  const applyGroup = (g) => {
    setGroup(g)
    setType('')
    setHoldingId('')
    setNewHoldingName('')
  }

  // Escolhido o Tipo, resolve pra qual participação (InvestmentCategory) o valor vai.
  // Mesmo havendo só uma já cadastrada, sempre oferece "+ Novo investimento" — Ações,
  // FII etc. costumam ter vários papéis distintos do mesmo Tipo, então nunca dá pra
  // assumir que a única existente é a certa.
  const matchingHoldings = useMemo(
    () => (type ? categories.filter(c => c.type === type) : []),
    [categories, type]
  )

  const applyType = (t) => {
    setType(t)
    const matches = t ? categories.filter(c => c.type === t) : []
    setHoldingId(matches.length ? String(matches[0].id) : NEW_HOLDING)
    setNewHoldingName(t ? typeLabel(t) : '')
  }

  const creatingNew = type !== '' && (matchingHoldings.length === 0 || holdingId === NEW_HOLDING)

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!group) errs.group = 'Categoria é obrigatória'
    if (!type) errs.type = 'Tipo é obrigatório'
    if (creatingNew && !newHoldingName) errs.newHoldingName = 'Dê um nome pra esse investimento'
    if (value === '' || Number(value) < 0) errs.value = 'Informe um valor válido'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      let investmentCategoryId
      if (creatingNew) {
        const created = await investmentCategoryService.create({
          name: newHoldingName,
          type,
          appliedAmount: Number(value),
          interestRate: showInterestRate && newHoldingInterestRate ? Number(newHoldingInterestRate) : null,
        })
        investmentCategoryId = created.data.id
      } else {
        investmentCategoryId = Number(holdingId)
      }

      await investmentEntryService.create({
        investmentCategoryId,
        year: Number(year),
        month: Number(month),
        value: Number(value),
      })
      Router.push('/investments')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao lançar valor' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Lançar valor de investimento</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormSelect label="Categoria" value={group} onChange={applyGroup} error={errors.group}>
            <option value="">Selecione...</option>
            {INVESTMENT_GROUPS.map(g => <option key={g.value} value={g.value}>{g.label}</option>)}
          </FormSelect>

          <FormSelect label="Tipo" value={type} onChange={applyType} disabled={!group} error={errors.type}>
            <option value="">{group ? 'Selecione...' : 'Escolha uma categoria primeiro'}</option>
            {typeOptions.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
          </FormSelect>

          {type && matchingHoldings.length > 0 && (
            <FormSelect label="Investimento" value={holdingId} onChange={setHoldingId}>
              {matchingHoldings.map(h => <option key={h.id} value={h.id}>{h.name}</option>)}
              <option value={NEW_HOLDING}>+ Novo investimento</option>
            </FormSelect>
          )}

          {creatingNew && (
            <>
              {matchingHoldings.length === 0 && (
                <p className="text-xs text-text-secondary -mt-2 mb-3">
                  Você ainda não tem um investimento desse tipo — ele será criado junto com este lançamento.
                </p>
              )}
              {brapiType ? (
                <AssetAutocomplete
                  label="Nome"
                  placeholder="Digite o ticker ou nome, ex: PETR4"
                  value={newHoldingName}
                  onChange={setNewHoldingName}
                  assetType={brapiType}
                  error={errors.newHoldingName}
                />
              ) : (
                <FormInput
                  label="Nome"
                  placeholder="Ex: CDB Banco Inter"
                  value={newHoldingName}
                  onChange={setNewHoldingName}
                  error={errors.newHoldingName}
                />
              )}
              {showInterestRate && (
                <FormInput
                  label="Taxa de juros (% ao ano, opcional)"
                  type="number"
                  step="0.01"
                  value={newHoldingInterestRate}
                  onChange={setNewHoldingInterestRate}
                />
              )}
            </>
          )}

          <div className="grid grid-cols-2 gap-4">
            <FormSelect label="Ano" value={year} onChange={setYear}>
              {[now.getFullYear() - 1, now.getFullYear(), now.getFullYear() + 1].map(y => (
                <option key={y} value={y}>{y}</option>
              ))}
            </FormSelect>
            <FormSelect label="Mês" value={month} onChange={setMonth}>
              {MONTHS.map((m, i) => <option key={m} value={i + 1}>{m}</option>)}
            </FormSelect>
          </div>

          <FormInput
            label="Valor atual"
            type="number"
            step="0.01"
            value={value}
            onChange={setValue}
            error={errors.value}
          />

          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <Button type="submit" variant="primary" loading={submitting}>
            {submitting ? 'Salvando...' : 'Salvar'}
          </Button>
        </form>
      </Card>
    </AppLayout>
  )
}
