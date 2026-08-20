import { useState } from 'react'
import Router from 'next/router'
import AppLayout from '@/components/AppLayout'
import { bankAccountService } from '@/services/bankAccountService'
import FormInput from '@/components/FormInput'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

export default function NewBankAccountPage() {
  const [name, setName] = useState('')
  const [initialBalance, setInitialBalance] = useState('0')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  const submit = async (e) => {
    e.preventDefault()
    if (!name) return setErrors({ name: 'Nome é obrigatório' })

    setSubmitting(true)
    try {
      await bankAccountService.create({ name, initialBalance: Number(initialBalance) })
      Router.push('/bank-accounts')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao criar conta bancária' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Nova conta bancária</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormInput label="Nome" value={name} onChange={setName} error={errors.name} />
          <FormInput
            label="Saldo inicial"
            type="number"
            step="0.01"
            value={initialBalance}
            onChange={setInitialBalance}
          />
          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <Button type="submit" variant="primary" loading={submitting}>
            {submitting ? 'Criando...' : 'Criar conta'}
          </Button>
        </form>
      </Card>
    </AppLayout>
  )
}
