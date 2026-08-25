import { useState } from 'react'
import Router from 'next/router'
import AppLayout from '@/components/AppLayout'
import { investmentCategoryService } from '@/services/investmentCategoryService'
import { INVESTMENT_GROUPS, INVESTMENT_TYPES_BY_GROUP, GROUPS_WITH_INTEREST_RATE, BRAPI_TYPE_BY_INVESTMENT_TYPE } from '@/lib/investmentTypes'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import AssetAutocomplete from '@/components/AssetAutocomplete'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

export default function NewInvestmentCategoryPage() {
  const [name, setName] = useState('')
  const [group, setGroup] = useState('')
  const [type, setType] = useState('')
  const [appliedAmount, setAppliedAmount] = useState('')
  const [interestRate, setInterestRate] = useState('')
  const [monthlyContribution, setMonthlyContribution] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  const typeOptions = group ? INVESTMENT_TYPES_BY_GROUP[group] : []
  const showInterestRate = GROUPS_WITH_INTEREST_RATE.includes(group)
  const brapiType = BRAPI_TYPE_BY_INVESTMENT_TYPE[type]

  const applyGroup = (g) => {
    setGroup(g)
    setType('')
  }

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
      await investmentCategoryService.create({
        name,
        type,
        appliedAmount: Number(appliedAmount),
        interestRate: showInterestRate && interestRate ? Number(interestRate) : null,
        monthlyContribution: monthlyContribution ? Number(monthlyContribution) : null,
      })
      Router.push('/investment-categories')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao criar categoria' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Nova categoria de investimento</h1>
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
            <FormInput
              label="Nome"
              placeholder="Ex: CDB Banco Inter, Reserva de emergência"
              value={name}
              onChange={setName}
              error={errors.name}
            />
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

          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <Button type="submit" variant="primary" loading={submitting}>
            {submitting ? 'Criando...' : 'Criar categoria'}
          </Button>
        </form>
      </Card>
    </AppLayout>
  )
}
