import { useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { categoryBudgetService } from '@/services/categoryBudgetService'
import { categoryService } from '@/services/categoryService'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'
import Skeleton from '@/components/ui/Skeleton'
import BudgetProgressBar from '@/components/BudgetProgressBar'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

export default function EditCategoryBudgetPage() {
  const router = useRouter()
  const { id } = router.query
  const now = new Date()

  const [budget, setBudget] = useState(null)
  const [groups, setGroups] = useState([])
  const [categoryId, setCategoryId] = useState('')
  const [year, setYear] = useState('')
  const [month, setMonth] = useState('')
  const [limitAmount, setLimitAmount] = useState('')
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!id) return
    Promise.all([
      categoryBudgetService.getById(id),
      categoryService.list({ includeInactive: false, page: 1, pageSize: 200 }),
    ]).then(([budgetRes, categoriesRes]) => {
      setBudget(budgetRes.data)
      setCategoryId(String(budgetRes.data.categoryId))
      setYear(String(budgetRes.data.year))
      setMonth(String(budgetRes.data.month))
      setLimitAmount(String(budgetRes.data.limitAmount))
      setGroups(categoriesRes.data.items.filter(c => !c.parentCategoryId && c.type === 'Expense'))
      setLoading(false)
    }).catch(() => {
      alert('Orçamento não encontrado')
      router.push('/category-budgets')
    })
  }, [id])

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!categoryId) errs.categoryId = 'Selecione uma categoria'
    if (!limitAmount || Number(limitAmount) <= 0) errs.limitAmount = 'Informe um valor maior que zero'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      await categoryBudgetService.update(id, {
        categoryId: Number(categoryId),
        year: Number(year),
        month: Number(month),
        limitAmount: Number(limitAmount),
      })
      router.push('/category-budgets')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar orçamento' })
    } finally {
      setSubmitting(false)
    }
  }

  const remove = async () => {
    if (!confirm('Excluir este orçamento?')) return
    setSubmitting(true)
    try {
      await categoryBudgetService.remove(id)
      router.push('/category-budgets')
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
      <h1 className="text-2xl font-heading font-semibold mb-6">Editar orçamento</h1>
      <Card className="max-w-lg mb-6">
        <BudgetProgressBar
          categoryName={budget.categoryName}
          spent={budget.spent}
          limitAmount={budget.limitAmount}
          percentage={budget.percentage}
        />
      </Card>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormSelect label="Categoria" value={categoryId} onChange={setCategoryId} error={errors.categoryId}>
            {groups.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </FormSelect>

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
            label="Valor limite"
            type="number"
            step="0.01"
            value={limitAmount}
            onChange={setLimitAmount}
            error={errors.limitAmount}
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
