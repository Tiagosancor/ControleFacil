import { useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { investmentEntryService } from '@/services/investmentEntryService'
import { investmentCategoryService } from '@/services/investmentCategoryService'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'
import Skeleton from '@/components/ui/Skeleton'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

export default function EditInvestmentEntryPage() {
  const router = useRouter()
  const { id } = router.query

  const [categories, setCategories] = useState([])
  const [investmentCategoryId, setInvestmentCategoryId] = useState('')
  const [year, setYear] = useState('')
  const [month, setMonth] = useState('')
  const [value, setValue] = useState('')
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  const now = new Date()

  useEffect(() => {
    if (!id) return
    Promise.all([
      investmentEntryService.getById(id),
      investmentCategoryService.list({ includeInactive: true }),
    ]).then(([entryRes, categoriesRes]) => {
      setInvestmentCategoryId(String(entryRes.data.investmentCategoryId))
      setYear(String(entryRes.data.year))
      setMonth(String(entryRes.data.month))
      setValue(String(entryRes.data.value))
      setCategories(categoriesRes.data)
      setLoading(false)
    }).catch(() => {
      alert('Lançamento não encontrado')
      router.push('/investments')
    })
  }, [id])

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!investmentCategoryId) errs.investmentCategoryId = 'Selecione uma categoria'
    if (value === '' || Number(value) < 0) errs.value = 'Informe um valor válido'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      await investmentEntryService.update(id, {
        investmentCategoryId: Number(investmentCategoryId),
        year: Number(year),
        month: Number(month),
        value: Number(value),
      })
      router.push('/investments')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar' })
    } finally {
      setSubmitting(false)
    }
  }

  const remove = async () => {
    if (!confirm('Excluir este lançamento?')) return
    setSubmitting(true)
    try {
      await investmentEntryService.remove(id)
      router.push('/investments')
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
          <Skeleton className="h-9 w-32" />
        </Card>
      </AppLayout>
    )
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Editar lançamento</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormSelect label="Categoria" value={investmentCategoryId} onChange={setInvestmentCategoryId} error={errors.investmentCategoryId}>
            {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </FormSelect>

          <div className="grid grid-cols-2 gap-4">
            <FormSelect label="Ano" value={year} onChange={setYear}>
              {[now.getFullYear() - 1, now.getFullYear(), now.getFullYear() + 1].map(y => (
                <option key={y} value={y}>{y}</option>
              ))}
            </FormSelect>
            <FormSelect label="Mês" value={month} onChange={setMonth}>
              {MONTHS.map((m, i) => <option key={m} value={i + 1}>{m}</option>)}
            </FormSelect>
          </div>

          <FormInput
            label="Valor atual"
            type="number"
            step="0.01"
            value={value}
            onChange={setValue}
            error={errors.value}
          />

          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <div className="flex flex-wrap gap-3">
            <Button type="submit" variant="primary" loading={submitting}>Salvar</Button>
            <Button type="button" variant="danger" onClick={remove} disabled={submitting}>Excluir</Button>
          </div>
        </form>
      </Card>
    </AppLayout>
  )
}
