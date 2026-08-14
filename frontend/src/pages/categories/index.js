import { useEffect, useState } from 'react'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { categoryService } from '@/services/categoryService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'

export default function CategoriesPage() {
  const [items, setItems] = useState([])
  const [includeInactive, setIncludeInactive] = useState(false)
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      const res = await categoryService.list({ includeInactive, page: 1, pageSize: 200 })
      setItems(res.data.items)
    } catch {
      alert('Falha ao carregar categorias')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [includeInactive])

  return (
    <AppLayout>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-semibold">Categorias</h1>
        <Link href="/categories/new">
          <Button variant="primary">Nova categoria</Button>
        </Link>
      </div>

      <label className="flex items-center gap-2 text-sm text-text-secondary mb-6">
        <input
          type="checkbox"
          checked={includeInactive}
          onChange={e => setIncludeInactive(e.target.checked)}
        />
        Mostrar inativas
      </label>

      <Card className="p-0 overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-xs text-text-secondary uppercase border-b border-border">
              <th className="text-left p-3">Nome</th>
              <th className="text-left p-3">Tipo</th>
              <th className="text-left p-3">Grupo</th>
              <th className="text-left p-3">Status</th>
              <th className="text-left p-3"></th>
            </tr>
          </thead>
          <tbody>
            {items.map(category => (
              <tr key={category.id} className="border-b border-border hover:bg-background">
                <td className="p-3">{category.name}</td>
                <td className="p-3">
                  <span className={category.type === 'Income' ? 'text-income' : 'text-expense'}>
                    {category.type === 'Income' ? 'Receita' : 'Despesa'}
                  </span>
                </td>
                <td className="p-3 text-text-secondary">{category.parentCategoryName || '—'}</td>
                <td className="p-3">{category.isActive ? 'Ativa' : 'Inativa'}</td>
                <td className="p-3">
                  <Link href={`/categories/${category.id}/edit`} className="text-accent">Editar</Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </Card>

      {!loading && !items.length && <p className="mt-4 text-sm text-text-secondary">Nenhuma categoria encontrada.</p>}
    </AppLayout>
  )
}
