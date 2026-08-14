import { useState } from 'react';
import { Text, View } from 'react-native';
import { Link } from 'expo-router';
import { authService } from '@/services/authService';
import { useAuth } from '@/contexts/AuthContext';
import FormInput from '@/components/FormInput';
import Button from '@/components/ui/Button';
import Card from '@/components/ui/Card';

export default function LoginScreen() {
  const { login } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);

  const submit = async () => {
    const errs = {};
    if (!email) errs.email = 'Email é obrigatório';
    if (!password) errs.password = 'Senha é obrigatória';
    setErrors(errs);
    if (Object.keys(errs).length) return;

    setSubmitting(true);
    try {
      const res = await authService.login({ email, password });
      await login(res.data.token);
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Login falhou' });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <View className="flex-1 justify-center px-6 bg-background">
      <Text className="text-lg font-semibold text-accent text-center mb-6">ControleFacil</Text>
      <Card>
        <Text className="text-2xl font-semibold mb-6 text-text-primary">Entrar</Text>
        <FormInput
          label="Email"
          value={email}
          onChangeText={setEmail}
          error={errors.email}
          autoCapitalize="none"
          keyboardType="email-address"
        />
        <FormInput
          label="Senha"
          value={password}
          onChangeText={setPassword}
          error={errors.password}
          secureTextEntry
        />
        {errors.form && <Text className="text-red-600 text-sm mb-3">{errors.form}</Text>}
        <Button variant="primary" onPress={submit} disabled={submitting}>
          {submitting ? 'Entrando...' : 'Entrar'}
        </Button>
        <View className="flex-row justify-center mt-4">
          <Text className="text-sm text-text-secondary">Não tem conta? </Text>
          <Link href="/register" className="text-sm text-accent">Criar conta</Link>
        </View>
      </Card>
    </View>
  );
}
