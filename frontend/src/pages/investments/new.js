import { useEffect, useState } from 'react'
import Router from 'next/router'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { investmentEntryService } from '@/services/investmentEntryService'
import { investmentCategoryService } from '@/services/investmentCategoryService'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

export default function NewInvestmentEntryPage() {
  const now = new Date()
  const [categories, setCategories] = useState([])
  const [investmentCategoryId, setInvestmentCategoryId] = useState('')
  const [year, setYear] = useState(String(now.getFullYear()))
  const [month, setMonth] = useState(String(now.getMonth() + 1))
  const [value, setValue] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    investmentCategoryService.list({ includeInactive: false }).then(res => {
      setCategories(res.data)
      if (res.data.length) setInvestmentCategoryId(String(res.data[0].id))
    })
  }, [])

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!investmentCategoryId) errs.investmentCategoryId = 'Selecione uma categoria'
    if (value === '' || Number(value) < 0) errs.value = 'Informe um valor válido'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      await investmentEntryService.create({
        investmentCategoryId: Number(investmentCategoryId),
        year: Number(year),
        month: Number(month),
        value: Number(value),
      })
      Router.push('/investments')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao lançar valor' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Lançar valor de investimento</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          {categories.length === 0 ? (
            <p className="text-sm text-text-secondary mb-4">
              Você ainda não tem nenhuma categoria de investimento. Crie uma{' '}
              <Link href="/investment-categories/new" className="text-link">categoria</Link> primeiro.
            </p>
          ) : (
            <FormSelect label="Categoria" value={investmentCategoryId} onChange={setInvestmentCategoryId} error={errors.investmentCategoryId}>
              {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </FormSelect>
          )}

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
          <Button type="submit" variant="primary" loading={submitting} disabled={categories.length === 0}>
            {submitting ? 'Salvando...' : 'Salvar'}
          </Button>
        </form>
      </Card>
    </AppLayout>
  )
}
