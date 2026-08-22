import { useEffect, useState } from 'react'
import Router from 'next/router'
import AppLayout from '@/components/AppLayout'
import { longTermGoalService } from '@/services/longTermGoalService'
import { investmentCategoryService } from '@/services/investmentCategoryService'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

const NO_CATEGORY = ''

export default function NewLongTermGoalPage() {
  const now = new Date()
  const [categories, setCategories] = useState([])
  const [name, setName] = useState('')
  const [targetAmount, setTargetAmount] = useState('')
  const [targetYear, setTargetYear] = useState(String(now.getFullYear() + 1))
  const [targetMonth, setTargetMonth] = useState(String(now.getMonth() + 1))
  const [investmentCategoryId, setInvestmentCategoryId] = useState(NO_CATEGORY)
  const [manualCurrentAmount, setManualCurrentAmount] = useState('0')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    investmentCategoryService.list({ includeInactive: false }).then(res => setCategories(res.data))
  }, [])

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!name) errs.name = 'Nome é obrigatório'
    if (!targetAmount || Number(targetAmount) <= 0) errs.targetAmount = 'Informe um valor maior que zero'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      await longTermGoalService.create({
        name,
        targetAmount: Number(targetAmount),
        targetYear: Number(targetYear),
        targetMonth: Number(targetMonth),
        investmentCategoryId: investmentCategoryId ? Number(investmentCategoryId) : null,
        manualCurrentAmount: investmentCategoryId ? 0 : Number(manualCurrentAmount || 0),
      })
      Router.push('/long-term-goals')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao criar meta' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Nova meta de longo prazo</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormInput
            label="Nome"
            placeholder="Ex: Carro, Casa, Viagem"
            value={name}
            onChange={setName}
            error={errors.name}
          />

          <FormInput
            label="Valor objetivo"
            type="number"
            step="0.01"
            value={targetAmount}
            onChange={setTargetAmount}
            error={errors.targetAmount}
          />

          <div className="grid grid-cols-2 gap-4">
            <FormSelect label="Ano alvo" value={targetYear} onChange={setTargetYear}>
              {Array.from({ length: 16 }, (_, i) => now.getFullYear() + i).map(y => (
                <option key={y} value={y}>{y}</option>
              ))}
            </FormSelect>
            <FormSelect label="Mês alvo" value={targetMonth} onChange={setTargetMonth}>
              {MONTHS.map((m, i) => <option key={m} value={i + 1}>{m}</option>)}
            </FormSelect>
          </div>

          <FormSelect label="Categoria de investimento (opcional)" value={investmentCategoryId} onChange={setInvestmentCategoryId}>
            <option value={NO_CATEGORY}>Nenhuma — informar valor manualmente</option>
            {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </FormSelect>

          {investmentCategoryId ? (
            <p className="text-xs text-text-secondary mb-4">
              O valor atual será o último valor lançado em{' '}
              <a href="/investments" className="text-link">Investimentos</a> para essa categoria.
            </p>
          ) : (
            <FormInput
              label="Valor já guardado atualmente"
              type="number"
              step="0.01"
              value={manualCurrentAmount}
              onChange={setManualCurrentAmount}
            />
          )}

          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <Button type="submit" variant="primary" loading={submitting}>
            {submitting ? 'Criando...' : 'Criar meta'}
          </Button>
        </form>
      </Card>
    </AppLayout>
  )
}
