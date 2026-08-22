import { useEffect, useState } from 'react'
import AppLayout from '@/components/AppLayout'
import { reportService } from '@/services/reportService'
import Card from '@/components/ui/Card'
import Skeleton from '@/components/ui/Skeleton'

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

function formatDate(value) {
  const [y, m, d] = value.split('-')
  return `${d}/${m}/${y}`
}

function PendingList({ items }) {
  if (!items.length) {
    return <p className="text-sm text-text-secondary">Nenhum lançamento pendente.</p>
  }

  return (
    <div className="flex flex-col gap-2">
      {items.map(item => (
        <Card key={item.transactionId}>
          <div className="flex justify-between items-center gap-2">
            <div className="min-w-0">
              <p className="font-medium truncate">{item.description}</p>
              <p className={`text-xs mt-1 ${item.overdue ? 'text-terracotta font-medium' : 'text-text-secondary'}`}>
                {item.overdue ? 'Vencido em' : 'Vence em'} {formatDate(item.dueDate)} · {item.categoryName}
              </p>
            </div>
            <p className="font-medium shrink-0 tabular-nums">{formatCurrency(item.amount)}</p>
          </div>
        </Card>
      ))}
    </div>
  )
}

export default function PendingReportPage() {
  const [report, setReport] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    reportService.getPending()
      .then(res => setReport(res.data))
      .catch(() => alert('Falha ao carregar contas a pagar/receber'))
      .finally(() => setLoading(false))
  }, [])

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Contas a Pagar / Receber</h1>

      {loading ? (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-16 w-full" />)}
        </div>
      ) : report && (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-6">
            <Card className="bg-terracotta-wash">
              <p className="text-sm text-text-secondary mb-1">Total a pagar</p>
              <p className="text-2xl font-heading font-medium text-terracotta tabular-nums">{formatCurrency(report.totalPayable)}</p>
            </Card>
            <Card className="bg-primary-soft">
              <p className="text-sm text-text-secondary mb-1">Total a receber</p>
              <p className="text-2xl font-heading font-medium text-primary tabular-nums">{formatCurrency(report.totalReceivable)}</p>
            </Card>
          </div>

          <p className="text-sm text-text-secondary mb-2">A pagar</p>
          <div className="mb-6"><PendingList items={report.payable} /></div>

          <p className="text-sm text-text-secondary mb-2">A receber</p>
          <PendingList items={report.receivable} />
        </>
      )}
    </AppLayout>
  )
}
