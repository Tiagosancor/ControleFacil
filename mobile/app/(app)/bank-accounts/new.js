import { useState } from 'react';
import { ScrollView, Text, View } from 'react-native';
import { useRouter } from 'expo-router';
import { bankAccountService } from '@/services/bankAccountService';
import FormInput from '@/components/FormInput';
import BankPicker from '@/components/BankPicker';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';

export default function NewBankAccountScreen() {
  const router = useRouter();
  const [name, setName] = useState('');
  const [initialBalance, setInitialBalance] = useState('0');
  const [bank, setBank] = useState(null);
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);

  const submit = async () => {
    if (!name) return setErrors({ name: 'Nome é obrigatório' });

    setSubmitting(true);
    try {
      await bankAccountService.create({ name, initialBalance: Number(initialBalance) || 0, bankIspb: bank?.ispb || null });
      router.back();
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Falha ao criar conta bancária' });
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
        <FormInput label="Nome" value={name} onChangeText={setName} error={errors.name} placeholder="Ex: Conta salário, Caixinha" />
        <BankPicker value={bank} onChange={setBank} />
        <FormInput
          label="Saldo inicial"
          value={initialBalance}
          onChangeText={setInitialBalance}
          keyboardType="decimal-pad"
        />
        {errors.form && <Text className="text-red-600 text-sm mb-3">{errors.form}</Text>}
        <Button variant="primary" onPress={submit} disabled={submitting}>
          {submitting ? 'Criando...' : 'Criar conta'}
        </Button>
      </Card>
    </ScrollView>
  );
}
