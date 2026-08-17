import { useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { bankAccountService } from '@/services/bankAccountService'
import FormInput from '@/components/FormInput'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export default function EditBankAccountPage() {
  const router = useRouter()
  const { id } = router.query

  const [name, setName] = useState('')
  const [initialBalance, setInitialBalance] = useState('0')
  const [currentBalance, setCurrentBalance] = useState(0)
  const [isActive, setIsActive] = useState(true)
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!id) return
    bankAccountService.getById(id).then(res => {
      setName(res.data.name)
      setInitialBalance(String(res.data.initialBalance))
      setCurrentBalance(res.data.currentBalance)
      setIsActive(res.data.isActive)
      setLoading(false)
    }).catch(() => {
      alert('Conta bancária não encontrada')
      router.push('/bank-accounts')
    })
  }, [id])

  const submit = async (e) => {
    e.preventDefault()
    if (!name) return setErrors({ name: 'Nome é obrigatório' })

    try {
      await bankAccountService.update(id, { name, initialBalance: Number(initialBalance), isActive })
      router.push('/bank-accounts')
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar conta bancária' })
    }
  }

  const remove = async () => {
    if (!confirm('Desativar esta conta bancária?')) return
    await bankAccountService.remove(id)
    router.push('/bank-accounts')
  }

  if (loading) return <AppLayout><p className="text-sm text-text-secondary">Carregando...</p></AppLayout>

  return (
    <AppLayout>
      <h1 className="text-2xl font-semibold mb-6">Editar conta bancária</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormInput label="Nome" value={name} onChange={setName} error={errors.name} />
          <div className="bg-background border border-border rounded-md px-3 py-2 mb-4">
            <p className="text-xs text-text-secondary">Saldo atual (inicial + lançamentos pagos)</p>
            <p className="font-medium">{formatCurrency(currentBalance)}</p>
          </div>
          <FormInput
            label="Saldo inicial"
            type="number"
            step="0.01"
            value={initialBalance}
            onChange={setInitialBalance}
          />
          <label className="flex items-center gap-2 text-sm text-text-secondary mb-4">
            <input type="checkbox" checked={isActive} onChange={e => setIsActive(e.target.checked)} />
            Ativa
          </label>
          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <div className="flex gap-3">
            <Button type="submit" variant="primary">Salvar</Button>
            <Button type="button" variant="danger" onClick={remove}>Desativar</Button>
          </div>
        </form>
      </Card>
    </AppLayout>
  )
}
