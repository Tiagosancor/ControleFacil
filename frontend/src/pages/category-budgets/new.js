import { useEffect, useState } from 'react'
import Router from 'next/router'
import AppLayout from '@/components/AppLayout'
import { categoryBudgetService } from '@/services/categoryBudgetService'
import { categoryService } from '@/services/categoryService'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

export default function NewCategoryBudgetPage() {
  const now = new Date()
  const [groups, setGroups] = useState([])
  const [categoryId, setCategoryId] = useState('')
  const [year, setYear] = useState(String(now.getFullYear()))
  const [month, setMonth] = useState(String(now.getMonth() + 1))
  const [limitAmount, setLimitAmount] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    categoryService.list({ includeInactive: false, page: 1, pageSize: 200 }).then(res => {
      // Orçamento só se aplica a categoria-grupo (raiz) de despesa — bate com o backend.
      const expenseGroups = res.data.items.filter(c => !c.parentCategoryId && c.type === 'Expense')
      setGroups(expenseGroups)
      if (expenseGroups.length) setCategoryId(String(expenseGroups[0].id))
    })
  }, [])

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!categoryId) errs.categoryId = 'Selecione uma categoria'
    if (!limitAmount || Number(limitAmount) <= 0) errs.limitAmount = 'Informe um valor maior que zero'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      await categoryBudgetService.create({
        categoryId: Number(categoryId),
        year: Number(year),
        month: Number(month),
        limitAmount: Number(limitAmount),
      })
      Router.push('/category-budgets')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao criar orçamento' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Novo orçamento</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          {groups.length === 0 ? (
            <p className="text-sm text-text-secondary mb-4">
              Você ainda não tem nenhum grupo de categoria de despesa. Crie uma{' '}
              <a href="/categories/new" className="text-link">categoria</a> primeiro.
            </p>
          ) : (
            <FormSelect label="Categoria" value={categoryId} onChange={setCategoryId} error={errors.categoryId}>
              {groups.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </FormSelect>
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
            label="Valor limite"
            type="number"
            step="0.01"
            value={limitAmount}
            onChange={setLimitAmount}
            error={errors.limitAmount}
          />

          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <Button type="submit" variant="primary" loading={submitting} disabled={groups.length === 0}>
            {submitting ? 'Criando...' : 'Criar orçamento'}
          </Button>
        </form>
      </Card>
    </AppLayout>
  )
}
