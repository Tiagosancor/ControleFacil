import { useEffect, useState } from 'react';
import { ScrollView, Text, View } from 'react-native';
import { useRouter } from 'expo-router';
import { categoryService } from '@/services/categoryService';
import FormInput from '@/components/FormInput';
import FormSelect, { SelectItem } from '@/components/FormSelect';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';

export default function NewCategoryScreen() {
  const router = useRouter();
  const [name, setName] = useState('');
  const [type, setType] = useState('Expense');
  const [parentCategoryId, setParentCategoryId] = useState('');
  const [rootCategories, setRootCategories] = useState([]);
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    categoryService.list({ includeInactive: false, page: 1, pageSize: 200 })
      .then(res => setRootCategories(res.data.items.filter(c => !c.parentCategoryId)))
      .catch(() => {});
  }, []);

  const selectedParent = rootCategories.find(c => String(c.id) === parentCategoryId);
  const effectiveType = selectedParent ? selectedParent.type : type;

  const submit = async () => {
    if (!name) return setErrors({ name: 'Nome é obrigatório' });

    setSubmitting(true);
    try {
      await categoryService.create({
        name,
        type: effectiveType,
        parentCategoryId: parentCategoryId ? Number(parentCategoryId) : null,
      });
      router.back();
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao criar categoria' });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <ScrollView
      className="flex-1 bg-background"
      contentContainerClassName="px-4 pt-4 pb-8"
      keyboardShouldPersistTaps="handled"
    >
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

        {errors.form && <Text className="text-red-600 text-sm mb-3">{errors.form}</Text>}
        <Button variant="primary" onPress={submit} disabled={submitting}>
          {submitting ? 'Criando...' : 'Criar categoria'}
        </Button>
      </Card>
    </ScrollView>
  );
}
