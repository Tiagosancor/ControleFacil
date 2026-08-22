import { useEffect, useState } from 'react'
import Router from 'next/router'
import AppLayout from '@/components/AppLayout'
import { transactionService } from '@/services/transactionService'
import { categoryService } from '@/services/categoryService'
import { bankAccountService } from '@/services/bankAccountService'
import { creditCardService } from '@/services/creditCardService'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

const PAYMENT_METHODS = [
  { value: 'Cash', label: 'À vista' },
  { value: 'Debit', label: 'Débito' },
  { value: 'Credit', label: 'Crédito' },
  { value: 'Pix', label: 'Pix' },
  { value: 'BankTransfer', label: 'Transferência' },
]

function categoryLabel(c) {
  return c.parentCategoryName ? `${c.parentCategoryName} > ${c.name}` : c.name
}

export default function NewTransactionPage() {
  const [categories, setCategories] = useState([])
  const [bankAccounts, setBankAccounts] = useState([])
  const [creditCards, setCreditCards] = useState([])

  const [entryDate, setEntryDate] = useState(new Date().toISOString().slice(0, 10))
  const [categoryId, setCategoryId] = useState('')
  const [description, setDescription] = useState('')
  const [paymentMethod, setPaymentMethod] = useState('Cash')
  const [bankAccountId, setBankAccountId] = useState('')
  const [creditCardId, setCreditCardId] = useState('')
  const [amount, setAmount] = useState('')
  const [paymentDate, setPaymentDate] = useState('')
  const [status, setStatus] = useState('Pending')
  const [totalInstallments, setTotalInstallments] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    categoryService.list({ includeInactive: false, page: 1, pageSize: 200 }).then(res => {
      const items = res.data.items
      setCategories(items)
      if (items.length) setCategoryId(String(items[0].id))
    })
    bankAccountService.list({ includeInactive: false, page: 1, pageSize: 200 }).then(res => {
      setBankAccounts(res.data.items)
      if (res.data.items.length) setBankAccountId(String(res.data.items[0].id))
    })
    creditCardService.list({ includeInactive: false }).then(res => setCreditCards(res.data))
  }, [])

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!categoryId) errs.categoryId = 'Selecione uma categoria'
    if (!bankAccountId) errs.bankAccountId = 'Selecione uma conta'
    if (!description) errs.description = 'Descrição é obrigatória'
    if (!amount || Number(amount) <= 0) errs.amount = 'Informe um valor maior que zero'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      await transactionService.create({
        entryDate,
        categoryId: Number(categoryId),
        description,
        paymentMethod,
        bankAccountId: Number(bankAccountId),
        amount: Number(amount),
        paymentDate: paymentDate || null,
        status,
        totalInstallments: totalInstallments ? Number(totalInstallments) : null,
        creditCardId: paymentMethod === 'Credit' && creditCardId ? Number(creditCardId) : null,
      })
      Router.push('/transactions')
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
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Novo lançamento</h1>
      <Card className="max-w-lg">
        <form onSubmit={submit}>
          <FormInput label="Data do lançamento" type="date" value={entryDate} onChange={setEntryDate} />

          <FormSelect label="Categoria" value={categoryId} onChange={setCategoryId} error={errors.categoryId}>
            {categories.map(c => <option key={c.id} value={c.id}>{categoryLabel(c)}</option>)}
          </FormSelect>

          <FormInput label="Descrição" value={description} onChange={setDescription} error={errors.description} />

          <FormSelect label="Forma de pagamento" value={paymentMethod} onChange={setPaymentMethod}>
            {PAYMENT_METHODS.map(p => <option key={p.value} value={p.value}>{p.label}</option>)}
          </FormSelect>

          {paymentMethod === 'Credit' && creditCards.length > 0 && (
            <FormSelect label="Cartão de crédito (opcional)" value={creditCardId} onChange={setCreditCardId}>
              <option value="">Nenhum</option>
              {creditCards.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </FormSelect>
          )}

          <FormSelect label="Conta bancária" value={bankAccountId} onChange={setBankAccountId} error={errors.bankAccountId}>
            {bankAccounts.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
          </FormSelect>

          <FormInput label="Valor" type="number" step="0.01" value={amount} onChange={setAmount} error={errors.amount} />

          <FormInput
            label="Data de pagamento (opcional)"
            type="date"
            value={paymentDate}
            onChange={setPaymentDate}
          />

          <FormSelect label="Status" value={status} onChange={setStatus}>
            <option value="Pending">Não pago</option>
            <option value="Paid">Pago</option>
          </FormSelect>

          <FormInput
            label="Parcelas (opcional — deixe em branco para lançamento único)"
            type="number"
            min="1"
            max="60"
            value={totalInstallments}
            onChange={setTotalInstallments}
          />

          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <Button type="submit" variant="primary" loading={submitting}>
            {submitting ? 'Criando...' : 'Criar lançamento'}
          </Button>
        </form>
      </Card>
    </AppLayout>
  )
}
