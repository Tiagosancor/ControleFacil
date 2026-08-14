import { useCallback, useState } from 'react';
import { FlatList, Pressable, Switch, Text, View } from 'react-native';
import { useFocusEffect, useRouter } from 'expo-router';
import { categoryService } from '@/services/categoryService';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';

export default function CategoriesScreen() {
  const router = useRouter();
  const [items, setItems] = useState([]);
  const [includeInactive, setIncludeInactive] = useState(false);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await categoryService.list({ includeInactive, page: 1, pageSize: 200 });
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
      <Button variant="primary" onPress={() => router.push('/categories/new')} className="mb-4">
        Nova categoria
      </Button>

      <View className="flex-row items-center mb-4">
        <Switch value={includeInactive} onValueChange={setIncludeInactive} />
        <Text className="text-sm text-text-secondary ml-2">Mostrar inativas</Text>
      </View>

      <FlatList
        data={items}
        keyExtractor={(item) => String(item.id)}
        refreshing={loading}
        onRefresh={load}
        ItemSeparatorComponent={() => <View className="h-2" />}
        ListEmptyComponent={!loading && (
          <Text className="text-sm text-text-secondary text-center mt-8">Nenhuma categoria encontrada.</Text>
        )}
        renderItem={({ item }) => (
          <Pressable onPress={() => router.push(`/categories/${item.id}`)}>
            <Card>
              <View className="flex-row justify-between items-center">
                <View className="flex-1">
                  <Text className="text-text-primary font-medium">{item.name}</Text>
                  <Text className="text-text-secondary text-xs mt-1">
                    {item.parentCategoryName || 'Categoria raiz'}
                  </Text>
                </View>
                <Text className={item.type === 'Income' ? 'text-income' : 'text-expense'}>
                  {item.type === 'Income' ? 'Receita' : 'Despesa'}
                </Text>
              </View>
              {!item.isActive && <Text className="text-text-muted text-xs mt-1">Inativa</Text>}
            </Card>
          </Pressable>
        )}
      />
    </View>
  );
}
