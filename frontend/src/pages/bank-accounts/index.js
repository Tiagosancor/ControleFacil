import { useEffect, useState } from 'react'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { bankAccountService } from '@/services/bankAccountService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import Skeleton from '@/components/ui/Skeleton'

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export default function BankAccountsPage() {
  const [items, setItems] = useState([])
  const [includeInactive, setIncludeInactive] = useState(false)
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      const res = await bankAccountService.list({ includeInactive, page: 1, pageSize: 200 })
      setItems(res.data.items)
    } catch {
      alert('Falha ao carregar contas bancárias')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [includeInactive])

  return (
    <AppLayout>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-heading font-semibold">Contas Bancárias</h1>
        <Link href="/bank-accounts/new">
          <Button variant="primary">Nova conta</Button>
        </Link>
      </div>

      <label className="flex items-center gap-2 text-sm text-text-secondary mb-6">
        <input type="checkbox" checked={includeInactive} onChange={e => setIncludeInactive(e.target.checked)} />
        Mostrar inativas
      </label>

      {loading ? (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-14 w-full" />)}
        </div>
      ) : (
        <>
          <Card className="hidden md:block p-0 overflow-hidden">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-xs text-text-secondary uppercase border-b border-border">
                  <th className="text-left p-3">Nome</th>
                  <th className="text-left p-3">Saldo inicial</th>
                  <th className="text-left p-3">Saldo atual</th>
                  <th className="text-left p-3">Status</th>
                  <th className="text-left p-3"></th>
                </tr>
              </thead>
              <tbody>
                {items.map(account => (
                  <tr key={account.id} className="border-b border-border hover:bg-background">
                    <td className="p-3">{account.name}</td>
                    <td className="p-3">{formatCurrency(account.initialBalance)}</td>
                    <td className="p-3 font-medium">{formatCurrency(account.currentBalance)}</td>
                    <td className="p-3">{account.isActive ? 'Ativa' : 'Inativa'}</td>
                    <td className="p-3">
                      <Link href={`/bank-accounts/${account.id}/edit`} className="text-link">Editar</Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>

          <div className="flex flex-col gap-2 md:hidden">
            {items.map(account => (
              <Link key={account.id} href={`/bank-accounts/${account.id}/edit`}>
                <Card>
                  <div className="flex justify-between items-center gap-2">
                    <p className="font-medium truncate">{account.name}</p>
                    <p className="font-medium shrink-0">{formatCurrency(account.currentBalance)}</p>
                  </div>
                  <p className="text-xs text-text-secondary mt-1">Saldo inicial: {formatCurrency(account.initialBalance)}</p>
                  {!account.isActive && <p className="text-xs text-text-muted mt-1">Inativa</p>}
                </Card>
              </Link>
            ))}
          </div>
        </>
      )}

      {!loading && !items.length && <p className="mt-4 text-sm text-text-secondary">Nenhuma conta encontrada.</p>}
    </AppLayout>
  )
}
