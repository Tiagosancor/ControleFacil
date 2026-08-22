import { useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { longTermGoalService } from '@/services/longTermGoalService'
import { investmentCategoryService } from '@/services/investmentCategoryService'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'
import Skeleton from '@/components/ui/Skeleton'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

const NO_CATEGORY = ''

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export default function EditLongTermGoalPage() {
  const router = useRouter()
  const { id } = router.query
  const now = new Date()

  const [goal, setGoal] = useState(null)
  const [categories, setCategories] = useState([])
  const [name, setName] = useState('')
  const [targetAmount, setTargetAmount] = useState('')
  const [targetYear, setTargetYear] = useState('')
  const [targetMonth, setTargetMonth] = useState('')
  const [investmentCategoryId, setInvestmentCategoryId] = useState(NO_CATEGORY)
  const [manualCurrentAmount, setManualCurrentAmount] = useState('0')
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!id) return
    Promise.all([
      longTermGoalService.getById(id),
      investmentCategoryService.list({ includeInactive: true }),
    ]).then(([goalRes, categoriesRes]) => {
      setGoal(goalRes.data)
      setName(goalRes.data.name)
      setTargetAmount(String(goalRes.data.targetAmount))
      setTargetYear(String(goalRes.data.targetYear))
      setTargetMonth(String(goalRes.data.targetMonth))
      setInvestmentCategoryId(goalRes.data.investmentCategoryId ? String(goalRes.data.investmentCategoryId) : NO_CATEGORY)
      setManualCurrentAmount(String(goalRes.data.investmentCategoryId ? 0 : goalRes.data.currentAmount))
      setCategories(categoriesRes.data)
      setLoading(false)
    }).catch(() => {
      alert('Meta não encontrada')
      router.push('/long-term-goals')
    })
  }, [id])

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!name) errs.name = 'Nome é obrigatório'
    if (!targetAmount || Number(targetAmount) <= 0) errs.targetAmount = 'Informe um valor maior que zero'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      await longTermGoalService.update(id, {
        name,
        targetAmount: Number(targetAmount),
        targetYear: Number(targetYear),
        targetMonth: Number(targetMonth),
        investmentCategoryId: investmentCategoryId ? Number(investmentCategoryId) : null,
        manualCurrentAmount: investmentCategoryId ? 0 : Number(manualCurrentAmount || 0),
      })
      router.push('/long-term-goals')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar meta' })
    } finally {
      setSubmitting(false)
    }
  }

  const remove = async () => {
    if (!confirm('Excluir esta meta?')) return
    setSubmitting(true)
    try {
      await longTermGoalService.remove(id)
      router.push('/long-term-goals')
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
      <h1 className="text-2xl font-heading font-semibold mb-6">Editar meta</h1>
      <Card className="max-w-lg mb-6">
        <p className="text-xs text-text-secondary">Valor atual / aporte mensal necessário</p>
        <p className="font-medium">{formatCurrency(goal.currentAmount)} de {formatCurrency(goal.targetAmount)}</p>
        <p className="text-sm text-text-secondary mt-1">
          {goal.progressPercentage >= 1 ? 'Meta atingida!' : `Aporte necessário: ${formatCurrency(goal.monthlyContributionNeeded)}/mês (${goal.monthsRemaining} meses restantes)`}
        </p>
      </Card>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormInput label="Nome" value={name} onChange={setName} error={errors.name} />

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
          <div className="flex flex-wrap gap-3">
            <Button type="submit" variant="primary" loading={submitting}>Salvar</Button>
            <Button type="button" variant="danger" onClick={remove} disabled={submitting}>Excluir</Button>
          </div>
        </form>
      </Card>
    </AppLayout>
  )
}
