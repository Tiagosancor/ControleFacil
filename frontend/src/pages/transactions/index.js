import { useEffect, useMemo, useState } from 'react'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { transactionService } from '@/services/transactionService'
import { categoryService } from '@/services/categoryService'
import { bankAccountService } from '@/services/bankAccountService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import FormSelect from '@/components/FormSelect'
import FormInput from '@/components/FormInput'
import Skeleton from '@/components/ui/Skeleton'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

const STATUS_LABEL = { Pending: 'Não pago', Paid: 'Pago' }
const STATUS_STYLE = {
  Paid: 'bg-primary-soft text-primary',
  Pending: 'bg-gold-wash text-gold',
}

function pad(n) {
  return String(n).padStart(2, '0')
}

// Ano/Mês são só um atalho: convertidos aqui no primeiro/último dia do
// período pra alimentar os campos De/Até, que são o filtro real enviado à API.
function monthRange(year, month) {
  const y = Number(year)
  if (!month) return { start: `${y}-01-01`, end: `${y}-12-31` }
  const m = Number(month)
  const lastDay = new Date(y, m, 0).getDate()
  return { start: `${y}-${pad(m)}-01`, end: `${y}-${pad(m)}-${pad(lastDay)}` }
}

function StatusPill({ status }) {
  return (
    <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-semibold whitespace-nowrap ${STATUS_STYLE[status] || 'bg-panel text-text-secondary'}`}>
      {STATUS_LABEL[status] || status}
    </span>
  )
}

function CategoryDot({ isIncome }) {
  return <span className={`inline-block h-2 w-2 rounded-full shrink-0 ${isIncome ? 'bg-gold-soft' : 'bg-terracotta'}`} aria-hidden="true" />
}

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
  const initialRange = monthRange(now.getFullYear(), now.getMonth() + 1)
  const [startDate, setStartDate] = useState(initialRange.start)
  const [endDate, setEndDate] = useState(initialRange.end)
  const [categoryId, setCategoryId] = useState('')
  const [bankAccountId, setBankAccountId] = useState('')
  const [status, setStatus] = useState('')
  const [loading, setLoading] = useState(true)
  const [removingId, setRemovingId] = useState(null)

  useEffect(() => {
    categoryService.list({ includeInactive: true, page: 1, pageSize: 200 }).then(res => setCategories(res.data.items))
    bankAccountService.list({ includeInactive: true, page: 1, pageSize: 200 }).then(res => setBankAccounts(res.data.items))
  }, [])

  const categoryTypeById = useMemo(() => {
    const map = {}
    categories.forEach(c => { map[c.id] = c.type })
    return map
  }, [categories])

  const applyYear = (y) => {
    setYear(y)
    const range = monthRange(y, month)
    setStartDate(range.start)
    setEndDate(range.end)
  }

  const applyMonth = (m) => {
    setMonth(m)
    const range = monthRange(year, m)
    setStartDate(range.start)
    setEndDate(range.end)
  }

  const load = async () => {
    setLoading(true)
    try {
      const res = await transactionService.list({
        startDate: startDate || undefined,
        endDate: endDate || undefined,
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

  useEffect(() => { load() }, [startDate, endDate, categoryId, bankAccountId, status])

  const removeOne = async (transaction) => {
    if (transaction.seriesId) {
      const cancelSeries = confirm(
        'Este lançamento faz parte de uma série parcelada.\n\nOK = cancelar a série inteira\nCancelar = apagar somente esta parcela'
      )
      if (cancelSeries) {
        setRemovingId(transaction.id)
        try {
          await transactionService.removeSeries(transaction.seriesId)
          return load()
        } finally {
          setRemovingId(null)
        }
      }
    }
    if (!confirm('Excluir este lançamento?')) return
    setRemovingId(transaction.id)
    try {
      await transactionService.remove(transaction.id)
      load()
    } finally {
      setRemovingId(null)
    }
  }

  return (
    <AppLayout>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-heading font-semibold">Lançamentos</h1>
        <Link href="/transactions/new">
          <Button variant="primary">Novo lançamento</Button>
        </Link>
      </div>

      <Card className="mb-6">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 [&>div]:min-w-0">
          <FormSelect label="Ano" value={year} onChange={applyYear}>
            {[now.getFullYear() - 1, now.getFullYear(), now.getFullYear() + 1].map(y => (
              <option key={y} value={y}>{y}</option>
            ))}
          </FormSelect>
          <FormSelect label="Mês" value={month} onChange={applyMonth}>
            <option value="">Todos</option>
            {MONTHS.map((m, i) => <option key={m} value={i + 1}>{m}</option>)}
          </FormSelect>
          {/* Em telas pequenas, De/Até ocupam a linha inteira cada um: o Safari real do
              iOS não encolhe o <input type=date> abaixo de um mínimo intrínseco próprio
              (o valor por extenso, ex. '31 de ago. de 2026'), então dividir a linha ao
              meio deixa os dois campos colados, sem o gap entre eles. A partir de md
              sobra espaço de sobra e eles voltam a dividir a linha normalmente.
              overflow-hidden é cinto-e-suspensório: no Safari real o próprio <input
              type=date> às vezes se desenha mais largo que a caixa CSS calculada
              (w-full/box-sizing não é respeitado à risca nesse controle nativo em
              todo build do WebKit), então em vez de confiar só na largura calculada,
              cortamos fisicamente qualquer excesso na borda do wrapper — com o mesmo
              rounded-md do próprio input, pra o corte seguir a curva do canto em vez
              de cortar reto (senão o canto direito fica quadrado, já que a borda
              arredondada "de verdade" do input pertence à parte que foi cortada). */}
          <div className="col-span-2 md:col-span-1 overflow-hidden rounded-md">
            <FormInput
              label="De"
              type="date"
              value={startDate}
              onChange={setStartDate}
              max={endDate || undefined}
            />
          </div>
          <div className="col-span-2 md:col-span-1 overflow-hidden rounded-md">
            <FormInput
              label="Até"
              type="date"
              value={endDate}
              onChange={setEndDate}
              min={startDate || undefined}
            />
          </div>
        </div>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
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

      {loading ? (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 6 }).map((_, i) => <Skeleton key={i} className="h-16 w-full" />)}
        </div>
      ) : (
        <>
          {/* Tabela de 7 colunas não cabe numa tela de celular sem cortar
              conteúdo — em telas md+ mostra a tabela, abaixo disso, cards. */}
          <Card className="hidden md:block p-0 overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-xs text-text-secondary border-b border-border">
                  <th className="text-left p-3 font-normal">Data</th>
                  <th className="text-left p-3 font-normal">Categoria</th>
                  <th className="text-left p-3 font-normal">Descrição</th>
                  <th className="text-left p-3 font-normal">Conta</th>
                  <th className="text-right p-3 font-normal">Valor</th>
                  <th className="text-left p-3 font-normal">Status</th>
                  <th className="text-left p-3"></th>
                </tr>
              </thead>
              <tbody>
                {items.map(t => (
                  <tr key={t.id} className="border-b border-border hover:bg-background">
                    <td className="p-3">{t.entryDate}</td>
                    <td className="p-3">
                      <span className="inline-flex items-center gap-2">
                        <CategoryDot isIncome={categoryTypeById[t.categoryId] === 'Income'} />
                        {t.categoryName}
                        {t.totalInstallments && (
                          <span className="text-text-muted">({t.installmentNumber}/{t.totalInstallments})</span>
                        )}
                      </span>
                    </td>
                    <td className="p-3">{t.description}</td>
                    <td className="p-3 text-text-secondary">{t.bankAccountName}</td>
                    <td className={`p-3 text-right font-semibold tabular-nums ${categoryTypeById[t.categoryId] === 'Income' ? 'text-income' : 'text-expense'}`}>
                      {categoryTypeById[t.categoryId] === 'Income' ? '+' : '−'} {formatCurrency(t.amount)}
                    </td>
                    <td className="p-3"><StatusPill status={t.status} /></td>
                    <td className="p-3 whitespace-nowrap">
                      <Link href={`/transactions/${t.id}/edit`} className="text-link mr-3">Editar</Link>
                      <button
                        onClick={() => removeOne(t)}
                        disabled={removingId === t.id}
                        className="text-red-600 disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        {removingId === t.id ? 'Excluindo...' : 'Excluir'}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>

          <div className="flex flex-col gap-2 md:hidden">
            {items.map(t => (
              <Card key={t.id}>
                <div className="flex justify-between items-start gap-2">
                  <div className="min-w-0">
                    <p className="font-medium truncate flex items-center gap-2">
                      <CategoryDot isIncome={categoryTypeById[t.categoryId] === 'Income'} />
                      {t.description}
                    </p>
                    <p className="text-xs text-text-secondary mt-1">
                      {t.entryDate} · {t.categoryName}
                      {t.totalInstallments && ` (${t.installmentNumber}/${t.totalInstallments})`}
                    </p>
                    <p className="text-xs text-text-secondary mt-1">{t.bankAccountName}</p>
                  </div>
                  <div className="shrink-0 flex flex-col items-end gap-1">
                    <p className={`text-sm font-semibold tabular-nums ${categoryTypeById[t.categoryId] === 'Income' ? 'text-income' : 'text-expense'}`}>
                      {categoryTypeById[t.categoryId] === 'Income' ? '+' : '−'} {formatCurrency(t.amount)}
                    </p>
                    <StatusPill status={t.status} />
                  </div>
                </div>
                <div className="flex gap-4 mt-2">
                  <Link href={`/transactions/${t.id}/edit`} className="text-link text-xs">Editar</Link>
                  <button
                    onClick={() => removeOne(t)}
                    disabled={removingId === t.id}
                    className="text-red-600 text-xs disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {removingId === t.id ? 'Excluindo...' : 'Excluir'}
                  </button>
                </div>
              </Card>
            ))}
          </div>
        </>
      )}

      {!loading && !items.length && <p className="mt-4 text-sm text-text-secondary">Nenhum lançamento encontrado.</p>}
      {!loading && !!items.length && <p className="mt-4 text-sm text-text-secondary">{total} lançamento(s)</p>}
    </AppLayout>
  )
}
