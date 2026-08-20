import { useEffect, useState } from 'react'
import Router from 'next/router'
import AppLayout from '@/components/AppLayout'
import { categoryService } from '@/services/categoryService'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

export default function NewCategoryPage() {
  const [name, setName] = useState('')
  const [type, setType] = useState('Expense')
  const [parentCategoryId, setParentCategoryId] = useState('')
  const [rootCategories, setRootCategories] = useState([])
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    categoryService.list({ includeInactive: false, page: 1, pageSize: 200 })
      .then(res => setRootCategories(res.data.items.filter(c => !c.parentCategoryId)))
      .catch(() => {})
  }, [])

  const selectedParent = rootCategories.find(c => String(c.id) === parentCategoryId)
  const effectiveType = selectedParent ? selectedParent.type : type

  const submit = async (e) => {
    e.preventDefault()
    if (!name) return setErrors({ name: 'Nome é obrigatório' })

    setSubmitting(true)
    try {
      await categoryService.create({
        name,
        type: effectiveType,
        parentCategoryId: parentCategoryId ? Number(parentCategoryId) : null,
      })
      Router.push('/categories')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao criar categoria' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Nova categoria</h1>
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

          <FormSelect
            label="Tipo"
            value={effectiveType}
            onChange={setType}
            disabled={!!selectedParent}
          >
            <option value="Income">Receita</option>
            <option value="Expense">Despesa</option>
          </FormSelect>
          {selectedParent && (
            <p className="text-xs text-text-secondary -mt-3 mb-4">
              O tipo é herdado automaticamente do grupo pai.
            </p>
          )}

          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <Button type="submit" variant="primary" loading={submitting}>
            {submitting ? 'Criando...' : 'Criar categoria'}
          </Button>
        </form>
      </Card>
    </AppLayout>
  )
}
