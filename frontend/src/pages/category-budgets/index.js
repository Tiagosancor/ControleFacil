import { useEffect, useState } from 'react'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { categoryBudgetService } from '@/services/categoryBudgetService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import FormSelect from '@/components/FormSelect'
import Skeleton from '@/components/ui/Skeleton'
import BudgetProgressBar from '@/components/BudgetProgressBar'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

export default function CategoryBudgetsPage() {
  const now = new Date()
  const [year, setYear] = useState(String(now.getFullYear()))
  const [month, setMonth] = useState(String(now.getMonth() + 1))
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [removingId, setRemovingId] = useState(null)

  const load = async () => {
    setLoading(true)
    try {
      const res = await categoryBudgetService.list({ year, month })
      setItems(res.data)
    } catch {
      alert('Falha ao carregar orçamentos')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [year, month])

  const remove = async (budget) => {
    if (!confirm(`Excluir o orçamento de "${budget.categoryName}"?`)) return
    setRemovingId(budget.id)
    try {
      await categoryBudgetService.remove(budget.id)
      load()
    } finally {
      setRemovingId(null)
    }
  }

  return (
    <AppLayout>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-heading font-semibold">Orçamentos</h1>
        <Link href="/category-budgets/new">
          <Button variant="primary">Novo orçamento</Button>
        </Link>
      </div>

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
        <div className="flex flex-col gap-2">
          {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-20 w-full" />)}
        </div>
      ) : (
        <div className="flex flex-col gap-3">
          {items.map(budget => (
            <Card key={budget.id}>
              <BudgetProgressBar
                categoryName={budget.categoryName}
                spent={budget.spent}
                limitAmount={budget.limitAmount}
                percentage={budget.percentage}
              />
              <div className="flex gap-4 mt-3">
                <Link href={`/category-budgets/${budget.id}/edit`} className="text-link text-xs">Editar</Link>
                <button
                  onClick={() => remove(budget)}
                  disabled={removingId === budget.id}
                  className="text-red-600 text-xs disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {removingId === budget.id ? 'Excluindo...' : 'Excluir'}
                </button>
              </div>
            </Card>
          ))}
        </div>
      )}

      {!loading && !items.length && (
        <p className="mt-4 text-sm text-text-secondary">Nenhum orçamento definido para esse período.</p>
      )}
    </AppLayout>
  )
}
