import { useState } from 'react';
import { Text, View } from 'react-native';
import { Link, router } from 'expo-router';
import { authService } from '@/services/authService';
import FormInput from '@/components/FormInput';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';

export default function RegisterScreen() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);

  const submit = async () => {
    const errs = {};
    if (!name) errs.name = 'Nome é obrigatório';
    if (!email) errs.email = 'Email é obrigatório';
    if (!password) errs.password = 'Senha é obrigatória';
    if (password && password.length < 8) errs.password = 'Senha deve ter ao menos 8 caracteres';
    setErrors(errs);
    if (Object.keys(errs).length) return;

    setSubmitting(true);
    try {
      await authService.register({ name, email, password });
      router.replace('/login');
    } catch (err) {
      const apiErrors = err?.response?.data?.errors;
      setErrors({
        form: err?.response?.data?.error
          || (apiErrors && Object.values(apiErrors).flat().join(' '))
          || 'Registro falhou',
      });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <View className="flex-1 justify-center px-6 bg-background">
      <Text className="text-lg font-semibold text-accent text-center mb-6">ControleFacil</Text>
      <Card>
        <Text className="text-2xl font-semibold mb-6 text-text-primary">Criar conta</Text>
        <FormInput label="Nome" value={name} onChangeText={setName} error={errors.name} />
        <FormInput
          label="Email"
          value={email}
          onChangeText={setEmail}
          error={errors.email}
          autoCapitalize="none"
          keyboardType="email-address"
        />
        <FormInput label="Senha" value={password} onChangeText={setPassword} error={errors.password} secureTextEntry />
        {errors.form && <Text className="text-red-600 text-sm mb-3">{errors.form}</Text>}
        <Button variant="primary" onPress={submit} disabled={submitting}>
          {submitting ? 'Registrando...' : 'Registrar'}
        </Button>
        <View className="flex-row justify-center mt-4">
          <Text className="text-sm text-text-secondary">Já tem conta? </Text>
          <Link href="/login" className="text-sm text-accent">Entrar</Link>
        </View>
      </Card>
    </View>
  );
}
