import { useEffect, useMemo, useState } from 'react'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { transactionService } from '@/services/transactionService'
import { categoryService } from '@/services/categoryService'
import { bankAccountService } from '@/services/bankAccountService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import FormSelect from '@/components/FormSelect'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]
const STATUS_LABEL = { Pending: 'Não pago', Paid: 'Pago' }

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export default function TransactionsPage() {
  const now = new Date()
  const [items, setItems] = useState([])
  const [total, setTotal] = useState(0)
  const [categories, setCategories] = useState([])
  const [bankAccounts, setBankAccounts] = useState([])
  const [year, setYear] = useState(String(now.getFullYear()))
  const [month, setMonth] = useState(String(now.getMonth() + 1))
  const [categoryId, setCategoryId] = useState('')
  const [bankAccountId, setBankAccountId] = useState('')
  const [status, setStatus] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    categoryService.list({ includeInactive: true, page: 1, pageSize: 200 }).then(res => setCategories(res.data.items))
    bankAccountService.list({ includeInactive: true, page: 1, pageSize: 200 }).then(res => setBankAccounts(res.data.items))
  }, [])

  const categoryTypeById = useMemo(() => {
    const map = {}
    categories.forEach(c => { map[c.id] = c.type })
    return map
  }, [categories])

  const load = async () => {
    setLoading(true)
    try {
      const res = await transactionService.list({
        year: year || undefined,
        month: month || undefined,
        categoryId: categoryId || undefined,
        bankAccountId: bankAccountId || undefined,
        status: status || undefined,
        page: 1,
        pageSize: 200,
      })
      setItems(res.data.items)
      setTotal(res.data.total)
    } catch {
      alert('Falha ao carregar lançamentos')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [year, month, categoryId, bankAccountId, status])

  const removeOne = async (transaction) => {
    if (transaction.seriesId) {
      const cancelSeries = confirm(
        'Este lançamento faz parte de uma série parcelada.\n\nOK = cancelar a série inteira\nCancelar = apagar somente esta parcela'
      )
      if (cancelSeries) {
        await transactionService.removeSeries(transaction.seriesId)
        return load()
      }
    }
    if (!confirm('Excluir este lançamento?')) return
    await transactionService.remove(transaction.id)
    load()
  }

  return (
    <AppLayout>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-semibold">Lançamentos</h1>
        <Link href="/transactions/new">
          <Button variant="primary">Novo lançamento</Button>
        </Link>
      </div>

      <Card className="mb-6">
        <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
          <FormSelect label="Ano" value={year} onChange={setYear}>
            {[now.getFullYear() - 1, now.getFullYear(), now.getFullYear() + 1].map(y => (
              <option key={y} value={y}>{y}</option>
            ))}
          </FormSelect>
          <FormSelect label="Mês" value={month} onChange={setMonth}>
            <option value="">Todos</option>
            {MONTHS.map((m, i) => <option key={m} value={i + 1}>{m}</option>)}
          </FormSelect>
          <FormSelect label="Categoria" value={categoryId} onChange={setCategoryId}>
            <option value="">Todas</option>
            {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </FormSelect>
          <FormSelect label="Conta" value={bankAccountId} onChange={setBankAccountId}>
            <option value="">Todas</option>
            {bankAccounts.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
          </FormSelect>
          <FormSelect label="Status" value={status} onChange={setStatus}>
            <option value="">Todos</option>
            <option value="Pending">Não pago</option>
            <option value="Paid">Pago</option>
          </FormSelect>
        </div>
      </Card>

      <Card className="p-0 overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-xs text-text-secondary uppercase border-b border-border">
              <th className="text-left p-3">Data</th>
              <th className="text-left p-3">Categoria</th>
              <th className="text-left p-3">Descrição</th>
              <th className="text-left p-3">Conta</th>
              <th className="text-left p-3">Valor</th>
              <th className="text-left p-3">Status</th>
              <th className="text-left p-3"></th>
            </tr>
          </thead>
          <tbody>
            {items.map(t => (
              <tr key={t.id} className="border-b border-border hover:bg-background">
                <td className="p-3">{t.entryDate}</td>
                <td className="p-3">
                  {t.categoryName}
                  {t.totalInstallments && (
                    <span className="text-text-muted"> ({t.installmentNumber}/{t.totalInstallments})</span>
                  )}
                </td>
                <td className="p-3">{t.description}</td>
                <td className="p-3 text-text-secondary">{t.bankAccountName}</td>
                <td className={`p-3 ${categoryTypeById[t.categoryId] === 'Income' ? 'text-income' : 'text-expense'}`}>
                  {categoryTypeById[t.categoryId] === 'Income' ? '+' : '-'} {formatCurrency(t.amount)}
                </td>
                <td className="p-3">{STATUS_LABEL[t.status] || t.status}</td>
                <td className="p-3 whitespace-nowrap">
                  <Link href={`/transactions/${t.id}/edit`} className="text-accent mr-3">Editar</Link>
                  <button onClick={() => removeOne(t)} className="text-red-600">Excluir</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Card>

      {!loading && !items.length && <p className="mt-4 text-sm text-text-secondary">Nenhum lançamento encontrado.</p>}
      {!loading && !!items.length && <p className="mt-4 text-sm text-text-secondary">{total} lançamento(s)</p>}
    </AppLayout>
  )
}
