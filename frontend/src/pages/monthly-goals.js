import { useEffect, useState } from 'react'
import AppLayout from '@/components/AppLayout'
import { monthlyGoalService } from '@/services/monthlyGoalService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import Skeleton from '@/components/ui/Skeleton'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

// Diferente de Categorias/Orçamentos, meta mensal é 1 registro por período — não faz
// sentido uma tela de lista + /new + /[id]/edit separadas. Uma página só, tipo
// "configuração do mês": troca Ano/Mês, carrega a meta se existir (e vira edição) ou
// mostra formulário vazio (e vira criação).
export default function MonthlyGoalsPage() {
  const now = new Date()
  const [year, setYear] = useState(String(now.getFullYear()))
  const [month, setMonth] = useState(String(now.getMonth() + 1))
  const [goal, setGoal] = useState(null)
  const [incomeGoal, setIncomeGoal] = useState('')
  const [expenseGoal, setExpenseGoal] = useState('')
  const [loading, setLoading] = useState(true)
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  const load = async () => {
    setLoading(true)
    setErrors({})
    try {
      const res = await monthlyGoalService.list({ year, month })
      const found = res.data[0] || null
      setGoal(found)
      setIncomeGoal(found ? String(found.incomeGoal) : '')
      setExpenseGoal(found ? String(found.expenseGoal) : '')
    } catch {
      alert('Falha ao carregar a meta do mês')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [year, month])

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!incomeGoal || Number(incomeGoal) <= 0) errs.incomeGoal = 'Informe um valor maior que zero'
    if (!expenseGoal || Number(expenseGoal) <= 0) errs.expenseGoal = 'Informe um valor maior que zero'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      const payload = { year: Number(year), month: Number(month), incomeGoal: Number(incomeGoal), expenseGoal: Number(expenseGoal) }
      if (goal) await monthlyGoalService.update(goal.id, payload)
      else await monthlyGoalService.create(payload)
      load()
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar a meta' })
    } finally {
      setSubmitting(false)
    }
  }

  const remove = async () => {
    if (!goal || !confirm('Excluir a meta desse mês?')) return
    setSubmitting(true)
    try {
      await monthlyGoalService.remove(goal.id)
      load()
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Meta mensal</h1>

      <Card className="mb-6">
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
      </Card>

      {loading ? (
        <Card className="max-w-lg">
          <Skeleton className="h-10 w-full mb-4" />
          <Skeleton className="h-10 w-full mb-4" />
          <Skeleton className="h-9 w-32" />
        </Card>
      ) : (
        <Card className="max-w-lg">
          <p className="text-sm text-text-secondary mb-4">
            {goal ? 'Editando a meta já definida pra esse mês.' : 'Nenhuma meta definida pra esse mês ainda.'}
          </p>
          <form onSubmit={submit}>
            <FormInput
              label="Meta de receita"
              type="number"
              step="0.01"
              value={incomeGoal}
              onChange={setIncomeGoal}
              error={errors.incomeGoal}
            />
            <FormInput
              label="Meta de despesa"
              type="number"
              step="0.01"
              value={expenseGoal}
              onChange={setExpenseGoal}
              error={errors.expenseGoal}
            />

            {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
            <div className="flex flex-wrap gap-3">
              <Button type="submit" variant="primary" loading={submitting}>
                {goal ? 'Salvar' : 'Criar meta'}
              </Button>
              {goal && (
                <Button type="button" variant="danger" onClick={remove} disabled={submitting}>
                  Excluir
                </Button>
              )}
            </div>
          </form>
        </Card>
      )}
    </AppLayout>
  )
}
