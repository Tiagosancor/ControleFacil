import { useEffect, useState } from 'react';
import { Alert, Switch, Text, View } from 'react-native';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { bankAccountService } from '@/services/bankAccountService';
import FormInput from '@/components/FormInput';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';

export default function EditBankAccountScreen() {
  const { id } = useLocalSearchParams();
  const router = useRouter();

  const [name, setName] = useState('');
  const [initialBalance, setInitialBalance] = useState('0');
  const [isActive, setIsActive] = useState(true);
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!id) return;
    bankAccountService.getById(id).then(res => {
      setName(res.data.name);
      setInitialBalance(String(res.data.initialBalance));
      setIsActive(res.data.isActive);
      setLoading(false);
    }).catch(() => {
      Alert.alert('Erro', 'Conta bancária não encontrada');
      router.back();
    });
  }, [id]);

  const submit = async () => {
    if (!name) return setErrors({ name: 'Nome é obrigatório' });

    setSubmitting(true);
    try {
      await bankAccountService.update(id, { name, initialBalance: Number(initialBalance) || 0, isActive });
      router.back();
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao salvar conta bancária' });
    } finally {
      setSubmitting(false);
    }
  };

  const remove = () => {
    Alert.alert('Desativar conta', 'Tem certeza?', [
      { text: 'Cancelar', style: 'cancel' },
      {
        text: 'Desativar',
        style: 'destructive',
        onPress: async () => {
          await bankAccountService.remove(id);
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
        <FormInput
          label="Saldo inicial"
          value={initialBalance}
          onChangeText={setInitialBalance}
          keyboardType="decimal-pad"
        />
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
