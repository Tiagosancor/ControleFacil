import { useCallback, useState } from 'react';
import { FlatList, Image, Pressable, Switch, Text, View } from 'react-native';
import { useFocusEffect, useRouter } from 'expo-router';
import { bankAccountService } from '@/services/bankAccountService';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function BankBadge({ bank }) {
  if (!bank) return null;
  return (
    <View className="flex-row items-center gap-1.5 mt-1">
      {bank.logoUrl && (
        <Image source={{ uri: bank.logoUrl }} style={{ height: 14, width: 14, borderRadius: 7 }} resizeMode="contain" />
      )}
      <Text className="text-text-secondary text-xs">{bank.name}</Text>
    </View>
  );
}

export default function BankAccountsScreen() {
  const router = useRouter();
  const [items, setItems] = useState([]);
  const [includeInactive, setIncludeInactive] = useState(false);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await bankAccountService.list({ includeInactive, page: 1, pageSize: 200 });
      setItems(res.data.items);
    } catch {
      // erro tratado visualmente pela lista vazia
    } finally {
      setLoading(false);
    }
  }, [includeInactive]);

  useFocusEffect(useCallback(() => { load(); }, [load]));

  return (
    <View className="flex-1 bg-background px-4 pt-4">
      <Button variant="primary" onPress={() => router.push('/bank-accounts/new')} className="mb-4">
        Nova conta
      </Button>

      <View className="flex-row items-center mb-4">
        <Switch value={includeInactive} onValueChange={setIncludeInactive} />
        <Text className="text-sm text-text-secondary ml-2">Mostrar inativas</Text>
      </View>

      <FlatList
        className="flex-1"
        data={items}
        keyExtractor={(item) => String(item.id)}
        refreshing={loading}
        onRefresh={load}
        ItemSeparatorComponent={() => <View className="h-2" />}
        contentContainerClassName="pb-8"
        ListEmptyComponent={!loading && (
          <Text className="text-sm text-text-secondary text-center mt-8">Nenhuma conta encontrada.</Text>
        )}
        renderItem={({ item }) => (
          <Pressable onPress={() => router.push(`/bank-accounts/${item.id}`)}>
            <Card>
              <View className="flex-row justify-between items-center">
                <Text className="text-text-primary font-medium">{item.name}</Text>
                <Text className="text-text-primary font-medium">{formatCurrency(item.currentBalance)}</Text>
              </View>
              <BankBadge bank={item.bank} />
              <Text className="text-text-secondary text-xs mt-1">Saldo inicial: {formatCurrency(item.initialBalance)}</Text>
              {!item.isActive && <Text className="text-text-muted text-xs mt-1">Inativa</Text>}
            </Card>
          </Pressable>
        )}
      />
    </View>
  );
}
