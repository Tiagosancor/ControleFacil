import { useEffect, useState } from 'react';
import { DeviceEventEmitter, Modal, Pressable, ScrollView, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { transactionService } from '@/services/transactionService';
import { categoryService } from '@/services/categoryService';
import { bankAccountService } from '@/services/bankAccountService';
import FormInput from '@/components/FormInput';
import FormSelect, { SelectItem } from '@/components/FormSelect';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';

function categoryLabel(c) {
  return c.parentCategoryName ? `${c.parentCategoryName} > ${c.name}` : c.name;
}

const today = () => new Date().toISOString().slice(0, 10);

// Formulário reduzido de propósito (Sprint H "registro rápido"): só Tipo, Valor,
// Categoria, Conta e Descrição opcional aparecem. Forma de pagamento, status e data
// ficam com valor padrão silencioso (À vista / Pago / hoje) — quem quer ajustar esses
// detalhes usa a tela completa em transactions/new.
export default function QuickAddFab() {
  const [open, setOpen] = useState(false);
  const [type, setType] = useState('Expense');
  const [categories, setCategories] = useState([]);
  const [bankAccounts, setBankAccounts] = useState([]);
  const [categoryId, setCategoryId] = useState('');
  const [bankAccountId, setBankAccountId] = useState('');
  const [amount, setAmount] = useState('');
  const [description, setDescription] = useState('');
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!open) return;
    categoryService.list({ includeInactive: false, page: 1, pageSize: 200 }).then(res => setCategories(res.data.items));
    bankAccountService.list({ includeInactive: false, page: 1, pageSize: 200 }).then(res => {
      setBankAccounts(res.data.items);
      if (res.data.items.length) setBankAccountId(String(res.data.items[0].id));
    });
  }, [open]);

  const categoriesForType = categories.filter(c => c.type === type);

  useEffect(() => {
    if (categoriesForType.length) setCategoryId(String(categoriesForType[0].id));
    else setCategoryId('');
  }, [type, categories]);

  const reset = () => {
    setType('Expense');
    setAmount('');
    setDescription('');
    setErrors({});
  };

  const close = () => {
    reset();
    setOpen(false);
  };

  const submit = async () => {
    const errs = {};
    if (!categoryId) errs.categoryId = 'Selecione uma categoria';
    if (!bankAccountId) errs.bankAccountId = 'Selecione uma conta';
    if (!amount || Number(amount) <= 0) errs.amount = 'Informe um valor maior que zero';
    setErrors(errs);
    if (Object.keys(errs).length) return;

    setSubmitting(true);
    try {
      const date = today();
      await transactionService.create({
        entryDate: date,
        categoryId: Number(categoryId),
        description: description || categoriesForType.find(c => String(c.id) === categoryId)?.name || 'Lançamento rápido',
        paymentMethod: 'Cash',
        bankAccountId: Number(bankAccountId),
        amount: Number(amount),
        paymentDate: date,
        status: 'Paid',
        totalInstallments: null,
      });
      // O modal fica fora do Tabs navigator (é filho direto do layout), então abrir/
      // fechar não passa pelo ciclo de foco do expo-router — useFocusEffect das telas
      // de Lançamentos/Dashboard não reagiria sozinho. Evento manual resolve.
      DeviceEventEmitter.emit('semeiagrana:transaction-created');
      close();
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
    <>
      <Pressable
        onPress={() => setOpen(true)}
        accessibilityLabel="Registro rápido"
        className="absolute bottom-24 right-5 h-14 w-14 rounded-full bg-accent items-center justify-center shadow-lg"
        style={{ elevation: 6 }}
      >
        <Ionicons name="add" size={28} color="#FFFFFF" />
      </Pressable>

      <Modal visible={open} animationType="slide" transparent onRequestClose={close}>
        <View className="flex-1 justify-end bg-black/40">
          <Pressable className="absolute inset-0" onPress={close} />
          <View className="bg-background rounded-t-2xl max-h-[90%]">
            <ScrollView contentContainerClassName="p-4 pb-8" keyboardShouldPersistTaps="handled">
              <View className="flex-row justify-between items-center mb-4">
                <Text className="text-lg font-semibold text-text-primary">Registro rápido</Text>
                <Pressable onPress={close} accessibilityLabel="Fechar">
                  <Ionicons name="close" size={24} color="#6B6960" />
                </Pressable>
              </View>

              <Card>
                <View className="flex-row gap-2 mb-4">
                  <Pressable
                    onPress={() => setType('Expense')}
                    className={`flex-1 py-2 rounded-md border items-center ${type === 'Expense' ? 'bg-expense/10 border-expense' : 'border-border'}`}
                  >
                    <Text className={type === 'Expense' ? 'text-expense font-medium' : 'text-text-secondary'}>Despesa</Text>
                  </Pressable>
                  <Pressable
                    onPress={() => setType('Income')}
                    className={`flex-1 py-2 rounded-md border items-center ${type === 'Income' ? 'bg-income/10 border-income' : 'border-border'}`}
                  >
                    <Text className={type === 'Income' ? 'text-income font-medium' : 'text-text-secondary'}>Receita</Text>
                  </Pressable>
                </View>

                <FormInput
                  label="Valor"
                  value={amount}
                  onChangeText={setAmount}
                  error={errors.amount}
                  keyboardType="decimal-pad"
                  autoFocus
                />

                {categoriesForType.length === 0 ? (
                  <Text className="text-sm text-text-secondary mb-4">
                    Nenhuma categoria de {type === 'Expense' ? 'despesa' : 'receita'} cadastrada ainda.
                  </Text>
                ) : (
                  <FormSelect label="Categoria" selectedValue={categoryId} onValueChange={setCategoryId} error={errors.categoryId}>
                    {categoriesForType.map(c => <SelectItem key={c.id} label={categoryLabel(c)} value={String(c.id)} />)}
                  </FormSelect>
                )}

                <FormSelect label="Conta" selectedValue={bankAccountId} onValueChange={setBankAccountId} error={errors.bankAccountId}>
                  {bankAccounts.map(b => <SelectItem key={b.id} label={b.name} value={String(b.id)} />)}
                </FormSelect>

                <FormInput label="Descrição (opcional)" value={description} onChangeText={setDescription} />

                {errors.form && <Text className="text-red-600 text-sm mb-3">{errors.form}</Text>}
                <Button variant="primary" onPress={submit} disabled={submitting || categoriesForType.length === 0}>
                  {submitting ? 'Salvando...' : 'Salvar'}
                </Button>
              </Card>
            </ScrollView>
          </View>
        </View>
      </Modal>
    </>
  );
}
