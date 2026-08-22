import { useEffect, useState } from 'react'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { longTermGoalService } from '@/services/longTermGoalService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import Skeleton from '@/components/ui/Skeleton'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

function barColor(percentage) {
  if (percentage >= 1) return 'bg-primary'
  if (percentage >= 0.5) return 'bg-gold'
  return 'bg-terracotta'
}

export default function LongTermGoalsPage() {
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [removingId, setRemovingId] = useState(null)

  const load = async () => {
    setLoading(true)
    try {
      const res = await longTermGoalService.list()
      setItems(res.data)
    } catch {
      alert('Falha ao carregar metas de longo prazo')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const remove = async (goal) => {
    if (!confirm(`Excluir a meta "${goal.name}"?`)) return
    setRemovingId(goal.id)
    try {
      await longTermGoalService.remove(goal.id)
      load()
    } finally {
      setRemovingId(null)
    }
  }

  return (
    <AppLayout>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-heading font-semibold">Metas de Longo Prazo</h1>
        <Link href="/long-term-goals/new">
          <Button variant="primary">Nova meta</Button>
        </Link>
      </div>

      {loading ? (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-28 w-full" />)}
        </div>
      ) : (
        <div className="flex flex-col gap-3">
          {items.map(goal => {
            const widthPercent = Math.min(goal.progressPercentage, 1) * 100
            const reached = goal.progressPercentage >= 1
            return (
              <Card key={goal.id}>
                <div className="flex justify-between items-baseline gap-2 mb-1">
                  <span className="font-medium truncate">{goal.name}</span>
                  <span className="text-xs text-text-secondary shrink-0 tabular-nums">
                    {formatCurrency(goal.currentAmount)} / {formatCurrency(goal.targetAmount)} ({Math.round(goal.progressPercentage * 100)}%)
                  </span>
                </div>
                <div className="h-2 rounded-full bg-panel overflow-hidden mb-2">
                  <div className={`h-full rounded-full ${barColor(goal.progressPercentage)}`} style={{ width: `${widthPercent}%` }} />
                </div>
                <p className="text-xs text-text-secondary">
                  Prazo: {MONTHS[goal.targetMonth - 1]}/{goal.targetYear}
                  {goal.investmentCategoryName && ` · Vinculada a "${goal.investmentCategoryName}"`}
                </p>
                <p className={`text-sm mt-1 ${reached ? 'text-primary font-medium' : 'text-text-primary'}`}>
                  {reached ? 'Meta atingida!' : `Aporte mensal necessário: ${formatCurrency(goal.monthlyContributionNeeded)}`}
                </p>
                <div className="flex gap-4 mt-2">
                  <Link href={`/long-term-goals/${goal.id}/edit`} className="text-link text-xs">Editar</Link>
                  <button
                    onClick={() => remove(goal)}
                    disabled={removingId === goal.id}
                    className="text-red-600 text-xs disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {removingId === goal.id ? 'Excluindo...' : 'Excluir'}
                  </button>
                </div>
              </Card>
            )
          })}
        </div>
      )}

      {!loading && !items.length && (
        <p className="mt-4 text-sm text-text-secondary">Nenhuma meta de longo prazo cadastrada.</p>
      )}
    </AppLayout>
  )
}
