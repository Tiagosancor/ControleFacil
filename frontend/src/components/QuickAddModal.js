import { useEffect, useState } from 'react'
import { transactionService } from '@/services/transactionService'
import { categoryService } from '@/services/categoryService'
import { bankAccountService } from '@/services/bankAccountService'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import Button from '@/components/ui/Button'

function categoryLabel(c) {
  return c.parentCategoryName ? `${c.parentCategoryName} > ${c.name}` : c.name
}

const TODAY = () => new Date().toISOString().slice(0, 10)

// Formulário reduzido de propósito (Sprint H "registro rápido"): só Tipo, Valor,
// Categoria, Conta e Descrição opcional aparecem. Forma de pagamento, status e data
// ficam com valor padrão silencioso (À vista / Pago / hoje) — quem quer ajustar esses
// detalhes usa o formulário completo em /transactions/new.
export default function QuickAddModal({ open, onClose, onCreated }) {
  const [type, setType] = useState('Expense')
  const [categories, setCategories] = useState([])
  const [bankAccounts, setBankAccounts] = useState([])
  const [categoryId, setCategoryId] = useState('')
  const [bankAccountId, setBankAccountId] = useState('')
  const [amount, setAmount] = useState('')
  const [description, setDescription] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!open) return
    categoryService.list({ includeInactive: false, page: 1, pageSize: 200 }).then(res => setCategories(res.data.items))
    bankAccountService.list({ includeInactive: false, page: 1, pageSize: 200 }).then(res => {
      setBankAccounts(res.data.items)
      if (res.data.items.length) setBankAccountId(String(res.data.items[0].id))
    })
  }, [open])

  const categoriesForType = categories.filter(c => c.type === type)

  useEffect(() => {
    if (categoriesForType.length) setCategoryId(String(categoriesForType[0].id))
    else setCategoryId('')
  }, [type, categories])

  if (!open) return null

  const reset = () => {
    setType('Expense')
    setAmount('')
    setDescription('')
    setErrors({})
  }

  const close = () => {
    reset()
    onClose()
  }

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!categoryId) errs.categoryId = 'Selecione uma categoria'
    if (!bankAccountId) errs.bankAccountId = 'Selecione uma conta'
    if (!amount || Number(amount) <= 0) errs.amount = 'Informe um valor maior que zero'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      const today = TODAY()
      await transactionService.create({
        entryDate: today,
        categoryId: Number(categoryId),
        description: description || categoriesForType.find(c => String(c.id) === categoryId)?.name || 'Lançamento rápido',
        paymentMethod: 'Cash',
        bankAccountId: Number(bankAccountId),
        amount: Number(amount),
        paymentDate: today,
        status: 'Paid',
        totalInstallments: null,
      })
      onCreated?.()
      close()
    } catch (err) {
      const apiErrors = err?.response?.data?.errors
      setErrors({
        form: err?.response?.data?.error
          || (apiErrors && Object.values(apiErrors).flat().join(' '))
          || 'Falha ao criar lançamento',
      })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-end sm:items-center justify-center">
      <div className="fixed inset-0 bg-black/40" onClick={close} aria-hidden="true" />
      <div className="relative bg-surface rounded-t-xl sm:rounded-xl shadow-soft w-full sm:max-w-md p-5 max-h-[90vh] overflow-y-auto">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-lg font-heading font-semibold">Registro rápido</h2>
          <button onClick={close} aria-label="Fechar" className="text-text-secondary hover:text-text-primary text-xl leading-none">×</button>
        </div>

        <form onSubmit={submit}>
          <div className="grid grid-cols-2 gap-2 mb-4">
            <button
              type="button"
              onClick={() => setType('Expense')}
              className={`py-2 rounded-md text-sm font-medium border transition-colors ${type === 'Expense' ? 'bg-terracotta-wash border-terracotta text-terracotta' : 'border-border text-text-secondary'}`}
            >
              Despesa
            </button>
            <button
              type="button"
              onClick={() => setType('Income')}
              className={`py-2 rounded-md text-sm font-medium border transition-colors ${type === 'Income' ? 'bg-gold-wash border-gold text-gold' : 'border-border text-text-secondary'}`}
            >
              Receita
            </button>
          </div>

          <FormInput
            label="Valor"
            type="number"
            step="0.01"
            inputMode="decimal"
            autoFocus
            value={amount}
            onChange={setAmount}
            error={errors.amount}
          />

          {categoriesForType.length === 0 ? (
            <p className="text-sm text-text-secondary mb-4">
              Nenhuma categoria de {type === 'Expense' ? 'despesa' : 'receita'} cadastrada ainda.
            </p>
          ) : (
            <FormSelect label="Categoria" value={categoryId} onChange={setCategoryId} error={errors.categoryId}>
              {categoriesForType.map(c => <option key={c.id} value={c.id}>{categoryLabel(c)}</option>)}
            </FormSelect>
          )}

          <FormSelect label="Conta" value={bankAccountId} onChange={setBankAccountId} error={errors.bankAccountId}>
            {bankAccounts.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
          </FormSelect>

          <FormInput
            label="Descrição (opcional)"
            value={description}
            onChange={setDescription}
          />

          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <Button type="submit" variant="primary" className="w-full" loading={submitting} disabled={categoriesForType.length === 0}>
            {submitting ? 'Salvando...' : 'Salvar'}
          </Button>
        </form>
      </div>
    </div>
  )
}
