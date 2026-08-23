import { useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { transactionService } from '@/services/transactionService'
import { categoryService } from '@/services/categoryService'
import { bankAccountService } from '@/services/bankAccountService'
import { creditCardService } from '@/services/creditCardService'
import FormInput from '@/components/FormInput'
import FormSelect from '@/components/FormSelect'
import DateField from '@/components/DateField'
import CategoryPicker from '@/components/CategoryPicker'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'
import Skeleton from '@/components/ui/Skeleton'

const PAYMENT_METHODS = [
  { value: 'Cash', label: 'À vista' },
  { value: 'Debit', label: 'Débito' },
  { value: 'Credit', label: 'Crédito' },
  { value: 'Pix', label: 'Pix' },
  { value: 'BankTransfer', label: 'Transferência' },
]

export default function EditTransactionPage() {
  const router = useRouter()
  const { id } = router.query

  const [categories, setCategories] = useState([])
  const [bankAccounts, setBankAccounts] = useState([])
  const [creditCards, setCreditCards] = useState([])
  const [seriesInfo, setSeriesInfo] = useState(null)

  const [entryDate, setEntryDate] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [description, setDescription] = useState('')
  const [paymentMethod, setPaymentMethod] = useState('Cash')
  const [bankAccountId, setBankAccountId] = useState('')
  const [creditCardId, setCreditCardId] = useState('')
  const [amount, setAmount] = useState('')
  const [paymentDate, setPaymentDate] = useState('')
  const [status, setStatus] = useState('Pending')
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!id) return
    Promise.all([
      transactionService.getById(id),
      categoryService.list({ includeInactive: false, page: 1, pageSize: 200 }),
      bankAccountService.list({ includeInactive: false, page: 1, pageSize: 200 }),
      creditCardService.list({ includeInactive: false }),
    ]).then(([txRes, categoriesRes, bankAccountsRes, creditCardsRes]) => {
      const t = txRes.data
      setEntryDate(t.entryDate)
      setCategoryId(String(t.categoryId))
      setDescription(t.description)
      setPaymentMethod(t.paymentMethod)
      setBankAccountId(String(t.bankAccountId))
      setCreditCardId(t.creditCardId ? String(t.creditCardId) : '')
      setAmount(String(t.amount))
      setPaymentDate(t.paymentDate || '')
      setStatus(t.status)
      setSeriesInfo(t.seriesId ? { seriesId: t.seriesId, installmentNumber: t.installmentNumber, totalInstallments: t.totalInstallments } : null)
      setCategories(categoriesRes.data.items)
      setBankAccounts(bankAccountsRes.data.items)
      setCreditCards(creditCardsRes.data)
      setLoading(false)
    }).catch(() => {
      alert('Lançamento não encontrado')
      router.push('/transactions')
    })
  }, [id])

  const submit = async (e) => {
    e.preventDefault()
    const errs = {}
    if (!description) errs.description = 'Descrição é obrigatória'
    if (!amount || Number(amount) <= 0) errs.amount = 'Informe um valor maior que zero'
    setErrors(errs)
    if (Object.keys(errs).length) return

    setSubmitting(true)
    try {
      await transactionService.update(id, {
        entryDate,
        categoryId: Number(categoryId),
        description,
        paymentMethod,
        bankAccountId: Number(bankAccountId),
        amount: Number(amount),
        paymentDate: paymentDate || null,
        status,
        creditCardId: paymentMethod === 'Credit' && creditCardId ? Number(creditCardId) : null,
      })
      router.push('/transactions')
    } catch (err) {
      const apiErrors = err?.response?.data?.errors
      setErrors({
        form: err?.response?.data?.error
          || (apiErrors && Object.values(apiErrors).flat().join(' '))
          || 'Falha ao salvar lançamento',
      })
    } finally {
      setSubmitting(false)
    }
  }

  const removeOne = async () => {
    if (!confirm('Excluir este lançamento?')) return
    setSubmitting(true)
    try {
      await transactionService.remove(id)
      router.push('/transactions')
    } finally {
      setSubmitting(false)
    }
  }

  const removeSeries = async () => {
    if (!confirm('Cancelar a série inteira? Todas as parcelas serão excluídas.')) return
    setSubmitting(true)
    try {
      await transactionService.removeSeries(seriesInfo.seriesId)
      router.push('/transactions')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <AppLayout>
        <Skeleton className="h-8 w-52 mb-6" />
        <Card className="max-w-lg">
          {Array.from({ length: 7 }).map((_, i) => <Skeleton key={i} className="h-10 w-full mb-4" />)}
          <Skeleton className="h-9 w-32" />
        </Card>
      </AppLayout>
    )
  }

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Editar lançamento</h1>
      <Card className="max-w-lg">
        {seriesInfo && (
          <div className="bg-background border border-border rounded-md px-3 py-2 text-sm text-text-secondary mb-4">
            Parcela {seriesInfo.installmentNumber}/{seriesInfo.totalInstallments} de um lançamento parcelado.
          </div>
        )}

        <form onSubmit={submit}>
          <DateField label="Data do lançamento" value={entryDate} onChange={setEntryDate} />

          <CategoryPicker categories={categories} value={categoryId} onChange={setCategoryId} />

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

          <FormSelect label="Conta bancária" value={bankAccountId} onChange={setBankAccountId}>
            {bankAccounts.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
          </FormSelect>

          <FormInput label="Valor" type="number" step="0.01" value={amount} onChange={setAmount} error={errors.amount} />

          <DateField label="Data de pagamento (opcional)" value={paymentDate} onChange={setPaymentDate} />

          <FormSelect label="Status" value={status} onChange={setStatus}>
            <option value="Pending">Não pago</option>
            <option value="Paid">Pago</option>
          </FormSelect>

          {errors.form && <div className="text-red-600 text-sm mb-3">{errors.form}</div>}
          <div className="flex gap-3 flex-wrap">
            <Button type="submit" variant="primary" loading={submitting}>Salvar</Button>
            <Button type="button" variant="danger" onClick={removeOne} disabled={submitting}>Excluir esta parcela</Button>
            {seriesInfo && (
              <Button type="button" variant="danger" onClick={removeSeries} disabled={submitting}>Cancelar série inteira</Button>
            )}
          </div>
        </form>
      </Card>
    </AppLayout>
  )
}
