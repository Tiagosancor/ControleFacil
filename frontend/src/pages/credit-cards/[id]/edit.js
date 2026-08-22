import { useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { creditCardService } from '@/services/creditCardService'
import FormInput from '@/components/FormInput'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'
import Skeleton from '@/components/ui/Skeleton'

export default function EditCreditCardPage() {
  const router = useRouter()
  const { id } = router.query

  const [name, setName] = useState('')
  const [closingDay, setClosingDay] = useState('')
  const [dueDay, setDueDay] = useState('')
  const [isActive, setIsActive] = useState(true)
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!id) return
    creditCardService.getById(id).then(res => {
      setName(res.data.name)
      setClosingDay(String(res.data.closingDay))
      setDueDay(String(res.data.dueDay))
      setIsActive(res.data.isActive)
      setLoading(false)
    }).catch(() => {
      alert('Cartão não encontrado')
      router.push('/credit-cards')
    })
  }, [id])

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
      await creditCardService.update(id, { name, closingDay: Number(closingDay), dueDay: Number(dueDay), isActive })
      router.push('/credit-cards')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar cartão' })
    } finally {
      setSubmitting(false)
    }
  }

  const remove = async () => {
    if (!confirm('Desativar este cartão?')) return
    setSubmitting(true)
    try {
      await creditCardService.remove(id)
      router.push('/credit-cards')
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
      <h1 className="text-2xl font-heading font-semibold mb-6">Editar cartão de crédito</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormInput label="Nome" value={name} onChange={setName} error={errors.name} />

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

          <label className="flex items-center gap-2 text-sm text-text-secondary mb-4">
            <input type="checkbox" checked={isActive} onChange={e => setIsActive(e.target.checked)} />
            Ativo
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
