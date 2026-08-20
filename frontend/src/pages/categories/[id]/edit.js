import { useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { categoryService } from '@/services/categoryService'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'
import Skeleton from '@/components/ui/Skeleton'

export default function EditCategoryPage() {
  const router = useRouter()
  const { id } = router.query

  const [name, setName] = useState('')
  const [type, setType] = useState('Expense')
  const [parentCategoryId, setParentCategoryId] = useState('')
  const [isActive, setIsActive] = useState(true)
  const [rootCategories, setRootCategories] = useState([])
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!id) return
    Promise.all([
      categoryService.getById(id),
      categoryService.list({ includeInactive: false, page: 1, pageSize: 200 }),
    ]).then(([categoryRes, listRes]) => {
      const category = categoryRes.data
      setName(category.name)
      setType(category.type)
      setParentCategoryId(category.parentCategoryId ? String(category.parentCategoryId) : '')
      setIsActive(category.isActive)
      setRootCategories(listRes.data.items.filter(c => !c.parentCategoryId && c.id !== category.id))
      setLoading(false)
    }).catch(() => {
      alert('Categoria não encontrada')
      router.push('/categories')
    })
  }, [id])

  const selectedParent = rootCategories.find(c => String(c.id) === parentCategoryId)
  const effectiveType = selectedParent ? selectedParent.type : type

  const submit = async (e) => {
    e.preventDefault()
    if (!name) return setErrors({ name: 'Nome é obrigatório' })

    setSubmitting(true)
    try {
      await categoryService.update(id, {
        name,
        type: effectiveType,
        parentCategoryId: parentCategoryId ? Number(parentCategoryId) : null,
        isActive,
      })
      router.push('/categories')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar categoria' })
    } finally {
      setSubmitting(false)
    }
  }

  const remove = async () => {
    if (!confirm('Desativar esta categoria?')) return
    setSubmitting(true)
    try {
      await categoryService.remove(id)
      router.push('/categories')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <AppLayout>
        <Skeleton className="h-8 w-48 mb-6" />
        <Card className="max-w-lg">
          <Skeleton className="h-10 w-full mb-4" />
          <Skeleton className="h-10 w-full mb-4" />
          <Skeleton className="h-10 w-full mb-4" />
          <Skeleton className="h-9 w-32" />
        </Card>
      </AppLayout>
    )
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Editar categoria</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormInput label="Nome" value={name} onChange={setName} error={errors.name} />

          <FormSelect label="Grupo pai (opcional)" value={parentCategoryId} onChange={setParentCategoryId}>
            <option value="">Nenhum — esta será uma categoria raiz</option>
            {rootCategories.map(c => (
              <option key={c.id} value={c.id}>
                {c.name} ({c.type === 'Income' ? 'Receita' : 'Despesa'})
              </option>
            ))}
          </FormSelect>

          <FormSelect label="Tipo" value={effectiveType} onChange={setType} disabled={!!selectedParent}>
            <option value="Income">Receita</option>
            <option value="Expense">Despesa</option>
          </FormSelect>
          {selectedParent && (
            <p className="text-xs text-text-secondary -mt-3 mb-4">
              O tipo é herdado automaticamente do grupo pai.
            </p>
          )}

          <label className="flex items-center gap-2 text-sm text-text-secondary mb-4">
            <input type="checkbox" checked={isActive} onChange={e => setIsActive(e.target.checked)} />
            Ativa
          </label>

          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <div className="flex flex-wrap gap-3">
            <Button type="submit" variant="primary" loading={submitting}>Salvar</Button>
            <Button type="button" variant="danger" onClick={remove} disabled={submitting}>Desativar</Button>
          </div>
        </form>
      </Card>
    </AppLayout>
  )
}
