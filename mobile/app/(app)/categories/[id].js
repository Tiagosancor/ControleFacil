import { useEffect, useState } from 'react';
import { Alert, Switch, Text, View } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { categoryService } from '@/services/categoryService';
import FormInput from '@/components/FormInput';
import FormSelect, { SelectItem } from '@/components/FormSelect';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';

export default function EditCategoryScreen() {
  const { id } = useLocalSearchParams();
  const router = useRouter();

  const [name, setName] = useState('');
  const [type, setType] = useState('Expense');
  const [parentCategoryId, setParentCategoryId] = useState('');
  const [isActive, setIsActive] = useState(true);
  const [rootCategories, setRootCategories] = useState([]);
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!id) return;
    Promise.all([
      categoryService.getById(id),
      categoryService.list({ includeInactive: false, page: 1, pageSize: 200 }),
    ]).then(([categoryRes, listRes]) => {
      const category = categoryRes.data;
      setName(category.name);
      setType(category.type);
      setParentCategoryId(category.parentCategoryId ? String(category.parentCategoryId) : '');
      setIsActive(category.isActive);
      setRootCategories(listRes.data.items.filter(c => !c.parentCategoryId && c.id !== category.id));
      setLoading(false);
    }).catch(() => {
      Alert.alert('Erro', 'Categoria não encontrada');
      router.back();
    });
  }, [id]);

  const selectedParent = rootCategories.find(c => String(c.id) === parentCategoryId);
  const effectiveType = selectedParent ? selectedParent.type : type;

  const submit = async () => {
    if (!name) return setErrors({ name: 'Nome é obrigatório' });

    setSubmitting(true);
    try {
      await categoryService.update(id, {
        name,
        type: effectiveType,
        parentCategoryId: parentCategoryId ? Number(parentCategoryId) : null,
        isActive,
      });
      router.back();
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar categoria' });
    } finally {
      setSubmitting(false);
    }
  };

  const remove = () => {
    Alert.alert('Desativar categoria', 'Tem certeza?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Desativar',
        style: 'destructive',
        onPress: async () => {
          await categoryService.remove(id);
          router.back();
        },
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
    <View className="flex-1 bg-background px-4 pt-4">
      <Card>
        <FormInput label="Nome" value={name} onChangeText={setName} error={errors.name} />

        <FormSelect label="Grupo pai (opcional)" selectedValue={parentCategoryId} onValueChange={setParentCategoryId}>
          <SelectItem label="Nenhum — esta será uma categoria raiz" value="" />
          {rootCategories.map(c => (
            <SelectItem key={c.id} label={`${c.name} (${c.type === 'Income' ? 'Receita' : 'Despesa'})`} value={String(c.id)} />
          ))}
        </FormSelect>

        <FormSelect label="Tipo" selectedValue={effectiveType} onValueChange={setType} enabled={!selectedParent}>
          <SelectItem label="Receita" value="Income" />
          <SelectItem label="Despesa" value="Expense" />
        </FormSelect>
        {selectedParent && (
          <Text className="text-xs text-text-secondary -mt-3 mb-4">O tipo é herdado automaticamente do grupo pai.</Text>
        )}

        <View className="flex-row items-center mb-4">
          <Switch value={isActive} onValueChange={setIsActive} />
          <Text className="text-sm text-text-secondary ml-2">Ativa</Text>
        </View>

        {errors.form && <Text className="text-red-600 text-sm mb-3">{errors.form}</Text>}
        <View className="flex-row gap-3">
          <Button variant="primary" onPress={submit} disabled={submitting} className="flex-1">
            Salvar
          </Button>
          <Button variant="danger" onPress={remove} className="flex-1">
            Desativar
          </Button>
        </View>
      </Card>
    </View>
  );
}
