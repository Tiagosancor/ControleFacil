import { Stack } from 'expo-router';
import LogoutButton from '@/components/LogoutButton';

export default function BankAccountsLayout() {
  return (
    <Stack screenOptions={{ headerRight: () => <LogoutButton /> }}>
      <Stack.Screen name="index" options={{ title: 'Contas Bancárias' }} />
      <Stack.Screen name="new" options={{ title: 'Nova conta' }} />
      <Stack.Screen name="[id]" options={{ title: 'Editar conta' }} />
    </Stack>
  );
}
