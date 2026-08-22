import { useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { investmentCategoryService } from '@/services/investmentCategoryService'
import FormInput from '@/components/FormInput'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'
import Skeleton from '@/components/ui/Skeleton'

export default function EditInvestmentCategoryPage() {
  const router = useRouter()
  const { id } = router.query

  const [name, setName] = useState('')
  const [isActive, setIsActive] = useState(true)
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!id) return
    investmentCategoryService.getById(id).then(res => {
      setName(res.data.name)
      setIsActive(res.data.isActive)
      setLoading(false)
    }).catch(() => {
      alert('Categoria de investimento não encontrada')
      router.push('/investment-categories')
    })
  }, [id])

  const submit = async (e) => {
    e.preventDefault()
    if (!name) return setErrors({ name: 'Nome é obrigatório' })

    setSubmitting(true)
    try {
      await investmentCategoryService.update(id, { name, isActive })
      router.push('/investment-categories')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar categoria' })
    } finally {
      setSubmitting(false)
    }
  }

  const remove = async () => {
    if (!confirm('Desativar esta categoria de investimento?')) return
    setSubmitting(true)
    try {
      await investmentCategoryService.remove(id)
      router.push('/investment-categories')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <AppLayout>
        <Skeleton className="h-8 w-56 mb-6" />
        <Card className="max-w-lg">
          <Skeleton className="h-10 w-full mb-4" />
          <Skeleton className="h-9 w-32" />
        </Card>
      </AppLayout>
    )
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Editar categoria de investimento</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormInput label="Nome" value={name} onChange={setName} error={errors.name} />
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
