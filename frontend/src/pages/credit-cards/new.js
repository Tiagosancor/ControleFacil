import { useState } from 'react'
import Router from 'next/router'
import AppLayout from '@/components/AppLayout'
import { creditCardService } from '@/services/creditCardService'
import FormInput from '@/components/FormInput'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

export default function NewCreditCardPage() {
  const [name, setName] = useState('')
  const [closingDay, setClosingDay] = useState('10')
  const [dueDay, setDueDay] = useState('17')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!name) errs.name = 'Nome é obrigatório'
    if (!closingDay || Number(closingDay) < 1 || Number(closingDay) > 31) errs.closingDay = 'Informe um dia entre 1 e 31'
    if (!dueDay || Number(dueDay) < 1 || Number(dueDay) > 31) errs.dueDay = 'Informe um dia entre 1 e 31'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      await creditCardService.create({ name, closingDay: Number(closingDay), dueDay: Number(dueDay) })
      Router.push('/credit-cards')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao criar cartão' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Novo cartão de crédito</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormInput label="Nome" placeholder="Ex: Nubank, Inter" value={name} onChange={setName} error={errors.name} />

          <div className="grid grid-cols-2 gap-4">
            <FormInput
              label="Dia de fechamento"
              type="number"
              min="1"
              max="31"
              value={closingDay}
              onChange={setClosingDay}
              error={errors.closingDay}
            />
            <FormInput
              label="Dia de vencimento"
              type="number"
              min="1"
              max="31"
              value={dueDay}
              onChange={setDueDay}
              error={errors.dueDay}
            />
          </div>

          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <Button type="submit" variant="primary" loading={submitting}>
            {submitting ? 'Criando...' : 'Criar cartão'}
          </Button>
        </form>
      </Card>
    </AppLayout>
  )
}
