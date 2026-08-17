import { useCallback, useEffect, useMemo, useState } from 'react';
import { Alert, FlatList, Pressable, Text, View } from 'react-native';
import { useFocusEffect, useRouter } from 'expo-router';
import { transactionService } from '@/services/transactionService';
import { categoryService } from '@/services/categoryService';
import { bankAccountService } from '@/services/bankAccountService';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';
import FormSelect, { SelectItem } from '@/components/FormSelect';

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
];
const STATUS_LABEL = { Pending: 'Não pago', Paid: 'Pago' };

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

export default function TransactionsScreen() {
  const router = useRouter();
  const now = new Date();
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [categories, setCategories] = useState([]);
  const [bankAccounts, setBankAccounts] = useState([]);
  const [year, setYear] = useState(String(now.getFullYear()));
  const [month, setMonth] = useState(String(now.getMonth() + 1));
  const [categoryId, setCategoryId] = useState('');
  const [bankAccountId, setBankAccountId] = useState('');
  const [status, setStatus] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    categoryService.list({ includeInactive: true, page: 1, pageSize: 200 }).then(res => setCategories(res.data.items));
    bankAccountService.list({ includeInactive: true, page: 1, pageSize: 200 }).then(res => setBankAccounts(res.data.items));
  }, []);

  const categoryTypeById = useMemo(() => {
    const map = {};
    categories.forEach(c => { map[c.id] = c.type; });
    return map;
  }, [categories]);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await transactionService.list({
        year: year || undefined,
        month: month || undefined,
        categoryId: categoryId || undefined,
        bankAccountId: bankAccountId || undefined,
        status: status || undefined,
        page: 1,
        pageSize: 200,
      });
      setItems(res.data.items);
      setTotal(res.data.total);
    } catch {
      // erro tratado visualmente pela lista vazia
    } finally {
      setLoading(false);
    }
  }, [year, month, categoryId, bankAccountId, status]);

  useFocusEffect(useCallback(() => { load(); }, [load]));

  const removeOne = (transaction) => {
    if (transaction.seriesId) {
      Alert.alert(
        'Lançamento parcelado',
        'Este lançamento faz parte de uma série.',
        [
          { text: 'Cancelar', style: 'cancel' },
          {
            text: 'Apagar só esta parcela',
            onPress: async () => { await transactionService.remove(transaction.id); load(); },
          },
          {
            text: 'Cancelar série inteira',
            style: 'destructive',
            onPress: async () => { await transactionService.removeSeries(transaction.seriesId); load(); },
          },
        ]
      );
      return;
    }
    Alert.alert('Excluir lançamento', 'Tem certeza?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Excluir',
        style: 'destructive',
        onPress: async () => { await transactionService.remove(transaction.id); load(); },
      },
    ]);
  };

  return (
    <View className="flex-1 bg-background px-4 pt-4">
      <FlatList
        className="flex-1"
        data={items}
        keyExtractor={(item) => String(item.id)}
        refreshing={loading}
        onRefresh={load}
        ItemSeparatorComponent={() => <View className="h-2" />}
        contentContainerClassName="pb-8"
        ListHeaderComponent={(
          <View>
            <Button variant="primary" onPress={() => router.push('/transactions/new')} className="mb-4">
              Novo lançamento
            </Button>

            <Card className="mb-4">
              <View className="flex-row gap-3">
                <View className="flex-1">
                  <FormSelect label="Ano" selectedValue={year} onValueChange={setYear}>
                    {[now.getFullYear() - 1, now.getFullYear(), now.getFullYear() + 1].map(y => (
                      <SelectItem key={y} label={String(y)} value={String(y)} />
                    ))}
                  </FormSelect>
                </View>
                <View className="flex-1">
                  <FormSelect label="Mês" selectedValue={month} onValueChange={setMonth}>
                    <SelectItem label="Todos" value="" />
                    {MONTHS.map((m, i) => <SelectItem key={m} label={m} value={String(i + 1)} />)}
                  </FormSelect>
                </View>
              </View>
              <FormSelect label="Categoria" selectedValue={categoryId} onValueChange={setCategoryId}>
                <SelectItem label="Todas" value="" />
                {categories.map(c => <SelectItem key={c.id} label={c.name} value={String(c.id)} />)}
              </FormSelect>
              <FormSelect label="Conta" selectedValue={bankAccountId} onValueChange={setBankAccountId}>
                <SelectItem label="Todas" value="" />
                {bankAccounts.map(b => <SelectItem key={b.id} label={b.name} value={String(b.id)} />)}
              </FormSelect>
              <FormSelect label="Status" selectedValue={status} onValueChange={setStatus}>
                <SelectItem label="Todos" value="" />
                <SelectItem label="Não pago" value="Pending" />
                <SelectItem label="Pago" value="Paid" />
              </FormSelect>
            </Card>

            {!loading && <Text className="text-sm text-text-secondary mb-3">{total} lançamento(s)</Text>}
          </View>
        )}
        ListEmptyComponent={!loading && (
          <Text className="text-sm text-text-secondary text-center mt-8">Nenhum lançamento encontrado.</Text>
        )}
        renderItem={({ item }) => (
          <Card>
            <Pressable onPress={() => router.push(`/transactions/${item.id}`)}>
              <View className="flex-row justify-between items-start">
                <View className="flex-1 pr-2">
                  <Text className="text-text-primary font-medium">{item.description}</Text>
                  <Text className="text-text-secondary text-xs mt-1">
                    {item.entryDate} · {item.categoryName}
                    {item.totalInstallments && ` (${item.installmentNumber}/${item.totalInstallments})`}
                  </Text>
                  <Text className="text-text-secondary text-xs">{item.bankAccountName} · {STATUS_LABEL[item.status] || item.status}</Text>
                </View>
                <Text className={categoryTypeById[item.categoryId] === 'Income' ? 'text-income' : 'text-expense'}>
                  {categoryTypeById[item.categoryId] === 'Income' ? '+' : '-'} {formatCurrency(item.amount)}
                </Text>
              </View>
            </Pressable>
            <Pressable onPress={() => removeOne(item)} className="mt-2 self-start">
              <Text className="text-red-600 text-xs">Excluir</Text>
            </Pressable>
          </Card>
        )}
      />
    </View>
  );
}
