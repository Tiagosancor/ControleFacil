import { useEffect, useState } from 'react';
import { Text, View } from 'react-native';
import { useRouter } from 'expo-router';
import { transactionService } from '@/services/transactionService';
import { categoryService } from '@/services/categoryService';
import { bankAccountService } from '@/services/bankAccountService';
import FormInput from '@/components/FormInput';
import FormSelect, { SelectItem } from '@/components/FormSelect';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';

const PAYMENT_METHODS = [
  { value: 'Cash', label: 'À vista' },
  { value: 'Debit', label: 'Débito' },
  { value: 'Credit', label: 'Crédito' },
  { value: 'Pix', label: 'Pix' },
  { value: 'BankTransfer', label: 'Transferência' },
];

function categoryLabel(c) {
  return c.parentCategoryName ? `${c.parentCategoryName} > ${c.name}` : c.name;
}

export default function NewTransactionScreen() {
  const router = useRouter();
  const [categories, setCategories] = useState([]);
  const [bankAccounts, setBankAccounts] = useState([]);

  const [entryDate, setEntryDate] = useState(new Date().toISOString().slice(0, 10));
  const [categoryId, setCategoryId] = useState('');
  const [description, setDescription] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('Cash');
  const [bankAccountId, setBankAccountId] = useState('');
  const [amount, setAmount] = useState('');
  const [paymentDate, setPaymentDate] = useState('');
  const [status, setStatus] = useState('Pending');
  const [totalInstallments, setTotalInstallments] = useState('');
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    categoryService.list({ includeInactive: false, page: 1, pageSize: 200 }).then(res => {
      setCategories(res.data.items);
      if (res.data.items.length) setCategoryId(String(res.data.items[0].id));
    });
    bankAccountService.list({ includeInactive: false, page: 1, pageSize: 200 }).then(res => {
      setBankAccounts(res.data.items);
      if (res.data.items.length) setBankAccountId(String(res.data.items[0].id));
    });
  }, []);

  const submit = async () => {
    const errs = {};
    if (!categoryId) errs.categoryId = 'Selecione uma categoria';
    if (!bankAccountId) errs.bankAccountId = 'Selecione uma conta';
    if (!description) errs.description = 'Descrição é obrigatória';
    if (!amount || Number(amount) <= 0) errs.amount = 'Informe um valor maior que zero';
    setErrors(errs);
    if (Object.keys(errs).length) return;

    setSubmitting(true);
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
      });
      router.back();
    } catch (err) {
      const apiErrors = err?.response?.data?.errors;
      setErrors({
        form: err?.response?.data?.error
          || (apiErrors && Object.values(apiErrors).flat().join(' '))
          || 'Falha ao criar lançamento',
      });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <View className="flex-1 bg-background px-4 pt-4">
      <Card>
        <FormInput
          label="Data do lançamento (AAAA-MM-DD)"
          value={entryDate}
          onChangeText={setEntryDate}
          placeholder="2026-01-31"
        />

        <FormSelect label="Categoria" selectedValue={categoryId} onValueChange={setCategoryId} error={errors.categoryId}>
          {categories.map(c => <SelectItem key={c.id} label={categoryLabel(c)} value={String(c.id)} />)}
        </FormSelect>

        <FormInput label="Descrição" value={description} onChangeText={setDescription} error={errors.description} />

        <FormSelect label="Forma de pagamento" selectedValue={paymentMethod} onValueChange={setPaymentMethod}>
          {PAYMENT_METHODS.map(p => <SelectItem key={p.value} label={p.label} value={p.value} />)}
        </FormSelect>

        <FormSelect label="Conta bancária" selectedValue={bankAccountId} onValueChange={setBankAccountId} error={errors.bankAccountId}>
          {bankAccounts.map(b => <SelectItem key={b.id} label={b.name} value={String(b.id)} />)}
        </FormSelect>

        <FormInput label="Valor" value={amount} onChangeText={setAmount} error={errors.amount} keyboardType="decimal-pad" />

        <FormInput
          label="Data de pagamento (opcional, AAAA-MM-DD)"
          value={paymentDate}
          onChangeText={setPaymentDate}
          placeholder="2026-01-31"
        />

        <FormSelect label="Status" selectedValue={status} onValueChange={setStatus}>
          <SelectItem label="Não pago" value="Pending" />
          <SelectItem label="Pago" value="Paid" />
        </FormSelect>

        <FormInput
          label="Parcelas (opcional — deixe em branco para lançamento único)"
          value={totalInstallments}
          onChangeText={setTotalInstallments}
          keyboardType="number-pad"
        />

        {errors.form && <Text className="text-red-600 text-sm mb-3">{errors.form}</Text>}
        <Button variant="primary" onPress={submit} disabled={submitting}>
          {submitting ? 'Criando...' : 'Criar lançamento'}
        </Button>
      </Card>
    </View>
  );
}
