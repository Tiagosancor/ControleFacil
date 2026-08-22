import { useEffect, useState } from 'react';
import { Alert, ScrollView, Text, View } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { transactionService } from '@/services/transactionService';
import { categoryService } from '@/services/categoryService';
import { bankAccountService } from '@/services/bankAccountService';
import FormInput from '@/components/FormInput';
import FormSelect, { SelectItem } from '@/components/FormSelect';
import CategoryPicker from '@/components/CategoryPicker';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';

const PAYMENT_METHODS = [
  { value: 'Cash', label: 'À vista' },
  { value: 'Debit', label: 'Débito' },
  { value: 'Credit', label: 'Crédito' },
  { value: 'Pix', label: 'Pix' },
  { value: 'BankTransfer', label: 'Transferência' },
];

export default function EditTransactionScreen() {
  const { id } = useLocalSearchParams();
  const router = useRouter();

  const [categories, setCategories] = useState([]);
  const [bankAccounts, setBankAccounts] = useState([]);
  const [seriesInfo, setSeriesInfo] = useState(null);

  const [entryDate, setEntryDate] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [description, setDescription] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('Cash');
  const [bankAccountId, setBankAccountId] = useState('');
  const [amount, setAmount] = useState('');
  const [paymentDate, setPaymentDate] = useState('');
  const [status, setStatus] = useState('Pending');
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!id) return;
    Promise.all([
      transactionService.getById(id),
      categoryService.list({ includeInactive: false, page: 1, pageSize: 200 }),
      bankAccountService.list({ includeInactive: false, page: 1, pageSize: 200 }),
    ]).then(([txRes, categoriesRes, bankAccountsRes]) => {
      const t = txRes.data;
      setEntryDate(t.entryDate);
      setCategoryId(String(t.categoryId));
      setDescription(t.description);
      setPaymentMethod(t.paymentMethod);
      setBankAccountId(String(t.bankAccountId));
      setAmount(String(t.amount));
      setPaymentDate(t.paymentDate || '');
      setStatus(t.status);
      setSeriesInfo(t.seriesId ? { seriesId: t.seriesId, installmentNumber: t.installmentNumber, totalInstallments: t.totalInstallments } : null);
      setCategories(categoriesRes.data.items);
      setBankAccounts(bankAccountsRes.data.items);
      setLoading(false);
    }).catch(() => {
      Alert.alert('Erro', 'Lançamento não encontrado');
      router.back();
    });
  }, [id]);

  const submit = async () => {
    const errs = {};
    if (!description) errs.description = 'Descrição é obrigatória';
    if (!amount || Number(amount) <= 0) errs.amount = 'Informe um valor maior que zero';
    setErrors(errs);
    if (Object.keys(errs).length) return;

    setSubmitting(true);
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
      });
      router.back();
    } catch (err) {
      const apiErrors = err?.response?.data?.errors;
      setErrors({
        form: err?.response?.data?.error
          || (apiErrors && Object.values(apiErrors).flat().join(' '))
          || 'Falha ao salvar lançamento',
      });
    } finally {
      setSubmitting(false);
    }
  };

  const removeOne = () => {
    Alert.alert('Excluir lançamento', 'Tem certeza?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Excluir',
        style: 'destructive',
        onPress: async () => { await transactionService.remove(id); router.back(); },
      },
    ]);
  };

  const removeSeries = () => {
    Alert.alert('Cancelar série inteira', 'Todas as parcelas serão excluídas.', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Cancelar série',
        style: 'destructive',
        onPress: async () => { await transactionService.removeSeries(seriesInfo.seriesId); router.back(); },
      },
    ]);
  };

  if (loading) {
    return (
      <View className="flex-1 items-center justify-center bg-background">
        <Text className="text-text-secondary">Carregando...</Text>
      </View>
    );
  }

  return (
    <ScrollView
      className="flex-1 bg-background"
      contentContainerClassName="px-4 pt-4 pb-8"
      keyboardShouldPersistTaps="handled"
    >
      <Card>
        {seriesInfo && (
          <View className="bg-background border border-border rounded-md px-3 py-2 mb-4">
            <Text className="text-sm text-text-secondary">
              Parcela {seriesInfo.installmentNumber}/{seriesInfo.totalInstallments} de um lançamento parcelado.
            </Text>
          </View>
        )}

        <FormInput label="Data do lançamento (AAAA-MM-DD)" value={entryDate} onChangeText={setEntryDate} />

        <CategoryPicker categories={categories} selectedValue={categoryId} onValueChange={setCategoryId} />

        <FormInput label="Descrição" value={description} onChangeText={setDescription} error={errors.description} />

        <FormSelect label="Forma de pagamento" selectedValue={paymentMethod} onValueChange={setPaymentMethod}>
          {PAYMENT_METHODS.map(p => <SelectItem key={p.value} label={p.label} value={p.value} />)}
        </FormSelect>

        <FormSelect label="Conta bancária" selectedValue={bankAccountId} onValueChange={setBankAccountId}>
          {bankAccounts.map(b => <SelectItem key={b.id} label={b.name} value={String(b.id)} />)}
        </FormSelect>

        <FormInput label="Valor" value={amount} onChangeText={setAmount} error={errors.amount} keyboardType="decimal-pad" />

        <FormInput label="Data de pagamento (opcional, AAAA-MM-DD)" value={paymentDate} onChangeText={setPaymentDate} />

        <FormSelect label="Status" selectedValue={status} onValueChange={setStatus}>
          <SelectItem label="Não pago" value="Pending" />
          <SelectItem label="Pago" value="Paid" />
        </FormSelect>

        {errors.form && <Text className="text-red-600 text-sm mb-3">{errors.form}</Text>}

        <Button variant="primary" onPress={submit} disabled={submitting} className="mb-3">
          Salvar
        </Button>
        <Button variant="danger" onPress={removeOne} className="mb-3">
          Excluir esta parcela
        </Button>
        {seriesInfo && (
          <Button variant="danger" onPress={removeSeries}>
            Cancelar série inteira
          </Button>
        )}
      </Card>
    </ScrollView>
  );
}
