import { useEffect, useState } from 'react'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { investmentCategoryService } from '@/services/investmentCategoryService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import Skeleton from '@/components/ui/Skeleton'

export default function InvestmentCategoriesPage() {
  const [items, setItems] = useState([])
  const [includeInactive, setIncludeInactive] = useState(false)
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      const res = await investmentCategoryService.list({ includeInactive })
      setItems(res.data)
    } catch {
      alert('Falha ao carregar categorias de investimento')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [includeInactive])

  return (
    <AppLayout>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-heading font-semibold">Categorias de Investimento</h1>
        <Link href="/investment-categories/new">
          <Button variant="primary">Nova categoria</Button>
        </Link>
      </div>

      <label className="flex items-center gap-2 text-sm text-text-secondary mb-6">
        <input type="checkbox" checked={includeInactive} onChange={e => setIncludeInactive(e.target.checked)} />
        Mostrar inativas
      </label>

      {loading ? (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-14 w-full" />)}
        </div>
      ) : (
        <div className="flex flex-col gap-2">
          {items.map(category => (
            <Link key={category.id} href={`/investment-categories/${category.id}/edit`}>
              <Card>
                <div className="flex justify-between items-center gap-2">
                  <p className="font-medium truncate">{category.name}</p>
                  {!category.isActive && <p className="text-xs text-text-muted shrink-0">Inativa</p>}
                </div>
              </Card>
            </Link>
          ))}
        </div>
      )}

      {!loading && !items.length && (
        <p className="mt-4 text-sm text-text-secondary">Nenhuma categoria de investimento cadastrada.</p>
      )}
    </AppLayout>
  )
}
