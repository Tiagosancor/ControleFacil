import { useState } from 'react'
import Router from 'next/router'
import AppLayout from '@/components/AppLayout'
import { investmentCategoryService } from '@/services/investmentCategoryService'
import FormInput from '@/components/FormInput'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

export default function NewInvestmentCategoryPage() {
  const [name, setName] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  const submit = async (e) => {
    e.preventDefault()
    if (!name) return setErrors({ name: 'Nome é obrigatório' })

    setSubmitting(true)
    try {
      await investmentCategoryService.create({ name })
      Router.push('/investment-categories')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao criar categoria' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Nova categoria de investimento</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormInput
            label="Nome"
            placeholder="Ex: Renda Fixa, Ações, Fundos Imobiliários"
            value={name}
            onChange={setName}
            error={errors.name}
          />
          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <Button type="submit" variant="primary" loading={submitting}>
            {submitting ? 'Criando...' : 'Criar categoria'}
          </Button>
        </form>
      </Card>
    </AppLayout>
  )
}
