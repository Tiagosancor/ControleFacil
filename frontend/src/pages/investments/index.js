import { useEffect, useState } from 'react'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { investmentEntryService } from '@/services/investmentEntryService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import FormSelect from '@/components/FormSelect'
import Skeleton from '@/components/ui/Skeleton'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export default function InvestmentsPage() {
  const now = new Date()
  const [year, setYear] = useState(String(now.getFullYear()))
  const [month, setMonth] = useState(String(now.getMonth() + 1))
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [removingId, setRemovingId] = useState(null)

  const load = async () => {
    setLoading(true)
    try {
      const res = await investmentEntryService.list({ year, month })
      setItems(res.data)
    } catch {
      alert('Falha ao carregar investimentos')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [year, month])

  const total = items.reduce((sum, item) => sum + item.value, 0)

  const remove = async (entry) => {
    if (!confirm(`Excluir o valor lançado de "${entry.investmentCategoryName}"?`)) return
    setRemovingId(entry.id)
    try {
      await investmentEntryService.remove(entry.id)
      load()
    } finally {
      setRemovingId(null)
    }
  }

  return (
    <AppLayout>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-heading font-semibold">Investimentos</h1>
        <Link href="/investments/new">
          <Button variant="primary">Lançar valor</Button>
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

      {!loading && items.length > 0 && (
        <Card className="mb-6 bg-gradient-to-b from-primary-soft to-surface">
          <p className="text-sm text-text-secondary mb-1">Total investido no período</p>
          <p className="text-2xl font-heading font-medium text-primary tabular-nums">{formatCurrency(total)}</p>
        </Card>
      )}

      {loading ? (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-16 w-full" />)}
        </div>
      ) : (
        <div className="flex flex-col gap-3">
          {items.map(entry => (
            <Card key={entry.id}>
              <div className="flex justify-between items-center gap-2">
                <p className="font-medium truncate">{entry.investmentCategoryName}</p>
                <p className="font-medium shrink-0 tabular-nums">{formatCurrency(entry.value)}</p>
              </div>
              <div className="flex gap-4 mt-2">
                <Link href={`/investments/${entry.id}/edit`} className="text-link text-xs">Editar</Link>
                <button
                  onClick={() => remove(entry)}
                  disabled={removingId === entry.id}
                  className="text-red-600 text-xs disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {removingId === entry.id ? 'Excluindo...' : 'Excluir'}
                </button>
              </div>
            </Card>
          ))}
        </div>
      )}

      {!loading && !items.length && (
        <p className="mt-4 text-sm text-text-secondary">Nenhum valor lançado para esse período.</p>
      )}

      <p className="mt-6 text-xs text-text-secondary">
        <Link href="/investment-categories" className="text-link">Gerenciar categorias de investimento</Link>
      </p>
    </AppLayout>
  )
}
