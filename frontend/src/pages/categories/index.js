import { useEffect, useState } from 'react'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { categoryService } from '@/services/categoryService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import Skeleton from '@/components/ui/Skeleton'
import { CategoryCircle } from '@/components/CategoryPicker'

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
        <h1 className="text-2xl font-heading font-semibold">Categorias</h1>
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

      {loading ? (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-14 w-full" />)}
        </div>
      ) : (
        <>
          {/* Tabela em telas md+; lista de cards em telas menores, onde uma
              tabela de 5 colunas não cabe sem cortar conteúdo. */}
          <Card className="hidden md:block p-0 overflow-hidden">
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
                    <td className="p-3">
                      <div className="flex items-center gap-2">
                        <CategoryCircle category={category} className="h-7 w-7" />
                        <span>{category.name}</span>
                        {category.isSystem && (
                          <span className="text-xs bg-primary-soft text-primary rounded-full px-2 py-0.5">Sistema</span>
                        )}
                      </div>
                    </td>
                    <td className="p-3">
                      <span className={category.type === 'Income' ? 'text-income' : 'text-expense'}>
                        {category.type === 'Income' ? 'Receita' : 'Despesa'}
                      </span>
                    </td>
                    <td className="p-3 text-text-secondary">{category.parentCategoryName || '—'}</td>
                    <td className="p-3">{category.isActive ? 'Ativa' : 'Inativa'}</td>
                    <td className="p-3">
                      {!category.isSystem && (
                        <Link href={`/categories/${category.id}/edit`} className="text-link">Editar</Link>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>

          <div className="flex flex-col gap-2 md:hidden">
            {items.map(category => {
              const content = (
                <Card>
                  <div className="flex justify-between items-start gap-2">
                    <div className="flex items-center gap-2 min-w-0">
                      <CategoryCircle category={category} className="h-8 w-8" />
                      <div className="min-w-0">
                        <p className="font-medium truncate">
                          {category.name}
                          {category.isSystem && (
                            <span className="ml-2 text-xs bg-primary-soft text-primary rounded-full px-2 py-0.5">Sistema</span>
                          )}
                        </p>
                        <p className="text-xs text-text-secondary mt-1">{category.parentCategoryName || 'Categoria raiz'}</p>
                      </div>
                    </div>
                    <span className={`text-sm shrink-0 ${category.type === 'Income' ? 'text-income' : 'text-expense'}`}>
                      {category.type === 'Income' ? 'Receita' : 'Despesa'}
                    </span>
                  </div>
                  {!category.isActive && <p className="text-xs text-text-muted mt-1">Inativa</p>}
                </Card>
              )
              return category.isSystem
                ? <div key={category.id}>{content}</div>
                : <Link key={category.id} href={`/categories/${category.id}/edit`}>{content}</Link>
            })}
          </div>
        </>
      )}

      {!loading && !items.length && <p className="mt-4 text-sm text-text-secondary">Nenhuma categoria encontrada.</p>}
    </AppLayout>
  )
}
