import { useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { investmentCategoryService } from '@/services/investmentCategoryService'
import { INVESTMENT_GROUPS, INVESTMENT_TYPES_BY_GROUP, GROUPS_WITH_INTEREST_RATE, BRAPI_TYPE_BY_INVESTMENT_TYPE, groupOfType } from '@/lib/investmentTypes'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import AssetAutocomplete from '@/components/AssetAutocomplete'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'
import Skeleton from '@/components/ui/Skeleton'

export default function EditInvestmentCategoryPage() {
  const router = useRouter()
  const { id } = router.query

  const [name, setName] = useState('')
  const [group, setGroup] = useState('')
  const [type, setType] = useState('')
  const [appliedAmount, setAppliedAmount] = useState('')
  const [interestRate, setInterestRate] = useState('')
  const [monthlyContribution, setMonthlyContribution] = useState('')
  const [isActive, setIsActive] = useState(true)
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  const typeOptions = group ? INVESTMENT_TYPES_BY_GROUP[group] : []
  const showInterestRate = GROUPS_WITH_INTEREST_RATE.includes(group)
  const brapiType = BRAPI_TYPE_BY_INVESTMENT_TYPE[type]

  const applyGroup = (g) => {
    setGroup(g)
    setType('')
  }

  useEffect(() => {
    if (!id) return
    investmentCategoryService.getById(id).then(res => {
      setName(res.data.name)
      setIsActive(res.data.isActive)
      if (res.data.type) {
        setType(res.data.type)
        setGroup(res.data.group || groupOfType(res.data.type) || '')
      }
      setAppliedAmount(res.data.appliedAmount != null ? String(res.data.appliedAmount) : '')
      setInterestRate(res.data.interestRate != null ? String(res.data.interestRate) : '')
      setMonthlyContribution(res.data.monthlyContribution != null ? String(res.data.monthlyContribution) : '')
      setLoading(false)
    }).catch(() => {
      alert('Categoria de investimento não encontrada')
      router.push('/investment-categories')
    })
  }, [id])

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!name) errs.name = 'Nome é obrigatório'
    if (!group) errs.group = 'Categoria é obrigatória'
    if (!type) errs.type = 'Tipo é obrigatório'
    if (!appliedAmount || Number(appliedAmount) <= 0) errs.appliedAmount = 'Informe um valor maior que zero'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      await investmentCategoryService.update(id, {
        name,
        type,
        appliedAmount: Number(appliedAmount),
        interestRate: showInterestRate && interestRate ? Number(interestRate) : null,
        monthlyContribution: monthlyContribution ? Number(monthlyContribution) : null,
        isActive,
      })
      router.push('/investment-categories')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar categoria' })
    } finally {
      setSubmitting(false)
    }
  }

  const remove = async () => {
    if (!confirm('Desativar esta categoria de investimento?')) return
    setSubmitting(true)
    try {
      await investmentCategoryService.remove(id)
      router.push('/investment-categories')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <AppLayout>
        <Skeleton className="h-8 w-56 mb-6" />
        <Card className="max-w-lg">
          <Skeleton className="h-10 w-full mb-4" />
          <Skeleton className="h-9 w-32" />
        </Card>
      </AppLayout>
    )
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Editar categoria de investimento</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormSelect label="Categoria" value={group} onChange={applyGroup} error={errors.group}>
            <option value="">Selecione...</option>
            {INVESTMENT_GROUPS.map(g => <option key={g.value} value={g.value}>{g.label}</option>)}
          </FormSelect>

          <FormSelect label="Tipo" value={type} onChange={setType} disabled={!group} error={errors.type}>
            <option value="">{group ? 'Selecione...' : 'Escolha uma categoria primeiro'}</option>
            {typeOptions.map(t => <option key={t.value} value={t.value}>{t.label}</option>)}
          </FormSelect>

          {brapiType ? (
            <AssetAutocomplete
              label="Nome"
              placeholder="Digite o ticker ou nome, ex: PETR4"
              value={name}
              onChange={setName}
              assetType={brapiType}
              error={errors.name}
            />
          ) : (
            <FormInput label="Nome" value={name} onChange={setName} error={errors.name} />
          )}

          <FormInput
            label="Valor aplicado"
            type="number"
            step="0.01"
            value={appliedAmount}
            onChange={setAppliedAmount}
            error={errors.appliedAmount}
          />

          {showInterestRate && (
            <FormInput
              label="Taxa de juros (% ao ano, opcional)"
              type="number"
              step="0.01"
              value={interestRate}
              onChange={setInterestRate}
            />
          )}

          <FormInput
            label="Valor de investimento mensal (opcional)"
            type="number"
            step="0.01"
            value={monthlyContribution}
            onChange={setMonthlyContribution}
          />

          <label className="flex items-center gap-2 text-sm text-text-secondary mb-4">
            <input type="checkbox" checked={isActive} onChange={e => setIsActive(e.target.checked)} />
            Ativa
          </label>
          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <div className="flex flex-wrap gap-3">
            <Button type="submit" variant="primary" loading={submitting}>Salvar</Button>
            <Button type="button" variant="danger" onClick={remove} disabled={submitting}>Desativar</Button>
          </div>
        </form>
      </Card>
    </AppLayout>
  )
}
