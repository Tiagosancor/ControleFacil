import { useEffect, useState } from 'react'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { creditCardService } from '@/services/creditCardService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import Skeleton from '@/components/ui/Skeleton'

export default function CreditCardsPage() {
  const [items, setItems] = useState([])
  const [includeInactive, setIncludeInactive] = useState(false)
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      const res = await creditCardService.list({ includeInactive })
      setItems(res.data)
    } catch {
      alert('Falha ao carregar cartões de crédito')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [includeInactive])

  return (
    <AppLayout>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-heading font-semibold">Cartões de Crédito</h1>
        <Link href="/credit-cards/new">
          <Button variant="primary">Novo cartão</Button>
        </Link>
      </div>

      <label className="flex items-center gap-2 text-sm text-text-secondary mb-6">
        <input type="checkbox" checked={includeInactive} onChange={e => setIncludeInactive(e.target.checked)} />
        Mostrar inativos
      </label>

      {loading ? (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-16 w-full" />)}
        </div>
      ) : (
        <div className="flex flex-col gap-2">
          {items.map(card => (
            <Card key={card.id}>
              <div className="flex justify-between items-center gap-2">
                <div>
                  <p className="font-medium truncate">{card.name}</p>
                  <p className="text-xs text-text-secondary mt-1">
                    Fecha dia {card.closingDay} · Vence dia {card.dueDay}
                    {!card.isActive && ' · Inativo'}
                  </p>
                </div>
                <div className="flex gap-4 shrink-0">
                  <Link href={`/credit-cards/${card.id}/invoice`} className="text-link text-xs">Ver fatura</Link>
                  <Link href={`/credit-cards/${card.id}/edit`} className="text-link text-xs">Editar</Link>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      {!loading && !items.length && (
        <p className="mt-4 text-sm text-text-secondary">Nenhum cartão de crédito cadastrado.</p>
      )}
    </AppLayout>
  )
}
