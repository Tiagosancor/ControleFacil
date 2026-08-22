import { useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { creditCardService } from '@/services/creditCardService'
import Card from '@/components/ui/Card'
import FormSelect from '@/components/FormSelect'
import Skeleton from '@/components/ui/Skeleton'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

function formatDate(value) {
  const [y, m, d] = value.split('-')
  return `${d}/${m}/${y}`
}

export default function CreditCardInvoicePage() {
  const router = useRouter()
  const { id } = router.query
  const now = new Date()

  const [year, setYear] = useState(String(now.getFullYear()))
  const [month, setMonth] = useState(String(now.getMonth() + 1))
  const [invoice, setInvoice] = useState(null)
  const [loading, setLoading] = useState(true)

  const load = async () => {
    if (!id) return
    setLoading(true)
    try {
      const res = await creditCardService.getInvoice(id, { year, month })
      setInvoice(res.data)
    } catch {
      alert('Falha ao carregar fatura')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [id, year, month])

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">
        Fatura {invoice ? `— ${invoice.creditCardName}` : ''}
      </h1>

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
        <Card><Skeleton className="h-24 w-full" /></Card>
      ) : invoice && (
        <>
          <Card className="mb-6 bg-gradient-to-b from-primary-soft to-surface">
            <p className="text-sm text-text-secondary mb-1">Total da fatura</p>
            <p className="text-3xl font-heading font-medium text-primary tabular-nums">{formatCurrency(invoice.total)}</p>
            <p className="text-xs text-text-secondary mt-1">
              Período: {formatDate(invoice.periodStart)} a {formatDate(invoice.periodEnd)} · Vencimento: {formatDate(invoice.dueDate)}
            </p>
          </Card>

          <div className="flex flex-col gap-2">
            {invoice.transactions.map(t => (
              <Card key={t.id}>
                <div className="flex justify-between items-center gap-2">
                  <div className="min-w-0">
                    <p className="font-medium truncate">{t.description}</p>
                    <p className="text-xs text-text-secondary">{formatDate(t.entryDate)} · {t.categoryName}</p>
                  </div>
                  <p className="font-medium shrink-0 tabular-nums">{formatCurrency(t.amount)}</p>
                </div>
              </Card>
            ))}
          </div>

          {!invoice.transactions.length && (
            <p className="mt-4 text-sm text-text-secondary">Nenhuma compra nesta fatura.</p>
          )}
        </>
      )}
    </AppLayout>
  )
}
