import { useEffect, useState } from 'react'
import AppLayout from '@/components/AppLayout'
import { reportService } from '@/services/reportService'
import Card from '@/components/ui/Card'
import FormSelect from '@/components/FormSelect'
import Skeleton from '@/components/ui/Skeleton'

const MONTHS_SHORT = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez']

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 })
}

export default function DreReportPage() {
  const now = new Date()
  const [year, setYear] = useState(String(now.getFullYear()))
  const [report, setReport] = useState(null)
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      const res = await reportService.getDre({ year })
      setReport(res.data)
    } catch {
      alert('Falha ao carregar o DRE')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [year])

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">DRE — Demonstrativo de Resultado</h1>

      <Card className="mb-6 max-w-xs">
        <FormSelect label="Ano" value={year} onChange={setYear}>
          {[now.getFullYear() - 1, now.getFullYear(), now.getFullYear() + 1].map(y => (
            <option key={y} value={y}>{y}</option>
          ))}
        </FormSelect>
      </Card>

      {loading ? (
        <Card><Skeleton className="h-64 w-full" /></Card>
      ) : report && (
        <Card className="p-0 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm whitespace-nowrap">
              <thead>
                <tr className="text-xs text-text-secondary uppercase border-b border-border">
                  <th className="text-left p-3 sticky left-0 bg-surface">Categoria</th>
                  {MONTHS_SHORT.map(m => <th key={m} className="text-right p-3">{m}</th>)}
                  <th className="text-right p-3">Total</th>
                </tr>
              </thead>
              <tbody>
                {report.incomeRows.map(row => (
                  <tr key={row.categoryGroupId} className="border-b border-border">
                    <td className="p-3 sticky left-0 bg-surface">{row.categoryGroupName}</td>
                    {row.monthlyValues.map((v, i) => <td key={i} className="text-right p-3 tabular-nums">{v ? formatCurrency(v) : '—'}</td>)}
                    <td className="text-right p-3 tabular-nums font-medium">{formatCurrency(row.total)}</td>
                  </tr>
                ))}
                <tr className="border-b border-border bg-primary-soft font-medium">
                  <td className="p-3 sticky left-0 bg-primary-soft">Total Receitas</td>
                  {report.monthlyIncomeTotals.map((v, i) => <td key={i} className="text-right p-3 tabular-nums text-primary">{formatCurrency(v)}</td>)}
                  <td className="text-right p-3 tabular-nums text-primary">{formatCurrency(report.yearIncomeTotal)}</td>
                </tr>

                {report.expenseRows.map(row => (
                  <tr key={row.categoryGroupId} className="border-b border-border">
                    <td className="p-3 sticky left-0 bg-surface">{row.categoryGroupName}</td>
                    {row.monthlyValues.map((v, i) => <td key={i} className="text-right p-3 tabular-nums">{v ? formatCurrency(v) : '—'}</td>)}
                    <td className="text-right p-3 tabular-nums font-medium">{formatCurrency(row.total)}</td>
                  </tr>
                ))}
                <tr className="border-b border-border bg-terracotta-wash font-medium">
                  <td className="p-3 sticky left-0 bg-terracotta-wash">Total Despesas</td>
                  {report.monthlyExpenseTotals.map((v, i) => <td key={i} className="text-right p-3 tabular-nums text-terracotta">{formatCurrency(v)}</td>)}
                  <td className="text-right p-3 tabular-nums text-terracotta">{formatCurrency(report.yearExpenseTotal)}</td>
                </tr>

                <tr className="font-semibold">
                  <td className="p-3 sticky left-0 bg-surface">Saldo</td>
                  {report.monthlyBalance.map((v, i) => (
                    <td key={i} className={`text-right p-3 tabular-nums ${v >= 0 ? 'text-primary' : 'text-terracotta'}`}>{formatCurrency(v)}</td>
                  ))}
                  <td className={`text-right p-3 tabular-nums ${report.yearBalance >= 0 ? 'text-primary' : 'text-terracotta'}`}>
                    {formatCurrency(report.yearBalance)}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </Card>
      )}

      {!loading && report && !report.incomeRows.length && !report.expenseRows.length && (
        <p className="mt-4 text-sm text-text-secondary">Nenhum lançamento encontrado nesse ano.</p>
      )}
    </AppLayout>
  )
}
