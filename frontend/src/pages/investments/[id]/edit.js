import { useEffect, useMemo, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { investmentEntryService } from '@/services/investmentEntryService'
import { investmentCategoryService } from '@/services/investmentCategoryService'
import { INVESTMENT_GROUPS, INVESTMENT_TYPES_BY_GROUP, GROUPS_WITH_INTEREST_RATE, BRAPI_TYPE_BY_INVESTMENT_TYPE, groupOfType, typeLabel } from '@/lib/investmentTypes'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import AssetAutocomplete from '@/components/AssetAutocomplete'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'
import Skeleton from '@/components/ui/Skeleton'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

const NEW_HOLDING = '__new__'

export default function EditInvestmentEntryPage() {
  const router = useRouter()
  const { id } = router.query

  const [categories, setCategories] = useState([])
  const [group, setGroup] = useState('')
  const [type, setType] = useState('')
  const [holdingId, setHoldingId] = useState('')
  const [newHoldingName, setNewHoldingName] = useState('')
  const [newHoldingInterestRate, setNewHoldingInterestRate] = useState('')
  const [year, setYear] = useState('')
  const [month, setMonth] = useState('')
  const [value, setValue] = useState('')
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  const now = new Date()

  const typeOptions = group ? INVESTMENT_TYPES_BY_GROUP[group] : []
  const showInterestRate = GROUPS_WITH_INTEREST_RATE.includes(group)
  const brapiType = BRAPI_TYPE_BY_INVESTMENT_TYPE[type]

  const applyGroup = (g) => {
    setGroup(g)
    setType('')
    setHoldingId('')
    setNewHoldingName('')
  }

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

  useEffect(() => {
    if (!id) return
    Promise.all([
      investmentEntryService.getById(id),
      investmentCategoryService.list({ includeInactive: true }),
    ]).then(([entryRes, categoriesRes]) => {
      setYear(String(entryRes.data.year))
      setMonth(String(entryRes.data.month))
      setValue(String(entryRes.data.value))
      setCategories(categoriesRes.data)

      const currentCategory = categoriesRes.data.find(c => c.id === entryRes.data.investmentCategoryId)
      if (currentCategory?.type) {
        setGroup(groupOfType(currentCategory.type))
        setType(currentCategory.type)
        setHoldingId(String(currentCategory.id))
      }

      setLoading(false)
    }).catch(() => {
      alert('Lançamento não encontrado')
      router.push('/investments')
    })
  }, [id])

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

      await investmentEntryService.update(id, {
        investmentCategoryId,
        year: Number(year),
        month: Number(month),
        value: Number(value),
      })
      router.push('/investments')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar' })
    } finally {
      setSubmitting(false)
    }
  }

  const remove = async () => {
    if (!confirm('Excluir este lançamento?')) return
    setSubmitting(true)
    try {
      await investmentEntryService.remove(id)
      router.push('/investments')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <AppLayout>
        <Skeleton className="h-8 w-48 mb-6" />
        <Card className="max-w-lg">
          <Skeleton className="h-10 w-full mb-4" />
          <Skeleton className="h-10 w-full mb-4" />
          <Skeleton className="h-9 w-32" />
        </Card>
      </AppLayout>
    )
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Editar lançamento</h1>
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
          <div className="flex flex-wrap gap-3">
            <Button type="submit" variant="primary" loading={submitting}>Salvar</Button>
            <Button type="button" variant="danger" onClick={remove} disabled={submitting}>Excluir</Button>
          </div>
        </form>
      </Card>
    </AppLayout>
  )
}
